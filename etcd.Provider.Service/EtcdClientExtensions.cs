#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：Utiletcd.Provider.ServiceClient
* 类 描 述 ：
* 命名空间 ：etcd.Provider.Service
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion

using dotnet_etcd;
using Etcdserverpb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace etcd.Provider.Service
{

   
    /// <summary>
    /// 注册
    /// </summary>
   
    public static class EtcdClientExtensions
    {

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="systemName">系统名称(默认：Agent)</param>
        /// <param name="registration">注册信息</param>
        /// <returns></returns>
        public static async Task<bool> RegisterAsync(this EtcdClient  client,  AgentServiceRegistration registration, string systemName = "Agent")
        {
            try
            {
                // /系统/Services/srvname/srvid
                ServiceEntry entry = new ServiceEntry()
                {
                    Host = registration.Address,
                    Port = registration.Port,
                    Name = registration.Name,
                    Id = registration.ID,
                    Tags = registration.Tags,
                    Version = registration.Version
                };
                if(registration.Checks==null||registration.Checks.Length==0)
                {
                    registration.Checks = new AgentServiceCheck[1];
                    registration.Checks[0] = new AgentServiceCheck();
                    registration.Checks[0].Interval = new TimeSpan(0, 0, 5);
                }
                var val = JsonConvert.SerializeObject(entry);
                string key ="/"+systemName + "/Services/" + entry.Name + "/" + entry.Id;
                CancellationToken token = new CancellationToken();

                if (!string.IsNullOrEmpty(Utiletcd.Sinlgeton.Password) || !string.IsNullOrEmpty(Utiletcd.Sinlgeton.Username))
                {
                    await client.AuthenticateAsync(new AuthenticateRequest()
                    {
                        Name= Utiletcd.Sinlgeton.Username,
                         Password= Utiletcd.Sinlgeton.Password
                    });
                }

                //申请TTL的ID
                var lease = await client.LeaseGrantAsync(new LeaseGrantRequest() { ID = 0, TTL = (long)registration.Checks[0].Interval.TotalSeconds });
                
                //加入节点
                var rsp = await client.PutAsync(new PutRequest() { Key = key.ToGoogleString(), Value = val.ToGoogleString(), Lease = lease.ID });
                
                //保持激活
                await client.LeaseKeepAlive(new LeaseKeepAliveRequest() { ID = lease.ID }, Watch, token);

                //添加配置
                await AddConfigAsync(client, systemName,registration);

                //服务加入更新列表
                await Utiletcd.Sinlgeton.AddKeepAliveAsync(client, key, (long)registration.Checks[0].Interval.TotalSeconds, lease.ID);
                Init(client, key);
                return true;
            }catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="systemName">系统名称(默认：Agent)</param>
        /// <param name="configJson">注册信息JSON字符串</param>
        /// <returns></returns>
        public static async Task<bool> RegisterAsync(this EtcdClient client,  string configJson, string systemName= "Agent")
        {
                // /Ocelot/Services/srvname/srvid
                var registration = JsonConvert.DeserializeObject<AgentServiceRegistration>(configJson);
                return await RegisterAsync(client,registration,systemName);
        }

        /// <summary>
        /// 获取系统服务
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sysName"></param>
        /// <returns></returns>
        public static async Task<List<ServiceEntry>> GetServicesAsync(this EtcdClient client, string sysName)
        {
            string key = "/" + sysName + "/Services/";
            var rsp =await client.GetRangeAsync(key);
            List<ServiceEntry> lst = new List<ServiceEntry>();
            foreach (var s in rsp.Kvs)
            {
                string serverKey = s.Key.FromGoogleString();
                string serverV = s.Value.FromGoogleString();
                string[] entityKey = serverKey.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (entityKey.Length == 4)
                {
                    var ser = JsonConvert.DeserializeObject<ServiceEntry>(serverV);
                    lst.Add(ser);
                }
            }
            return lst;
        }

        /// <summary>
        /// 设置客户端连接使用的信息
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="caCert"></param>
        /// <param name="clientCert"></param>
        /// <param name="clientKey"></param>
        /// <param name="publicRootCa"></param>
        public static void SetInfo(this EtcdClient client,string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            Utiletcd.Sinlgeton.Username = username;
            Utiletcd.Sinlgeton.Password = password;
            Utiletcd.Sinlgeton.CaCert = caCert;
            Utiletcd.Sinlgeton.ClientCert = clientCert;
            Utiletcd.Sinlgeton.ClientKey = clientKey;
            Utiletcd.Sinlgeton.PublicRootCa = publicRootCa;
        }

        /// <summary>
        /// 系统配置注册
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="sysName">系统名称</param>
        /// <param name="registration">配置信息</param>
        /// <returns></returns>
        private static async Task AddConfigAsync(EtcdClient client, string sysName, AgentServiceRegistration registration)
        {
            string key = "/" + sysName + "/config";
            string timeKey = "/" + sysName + "/Time/" + registration.Name + "/" + registration.ID;
            var leasersp = await client.LeaseGrantAsync(new LeaseGrantRequest() { ID = 0, TTL = 10 });
            var lockrsp = await client.LockAsync(new V3Lockpb.LockRequest() { Lease = leasersp.ID, Name = (key+"1").ToGoogleString() });
            var rsp = await client.GetAsync(key);
            EtcdConfig config = new EtcdConfig();
            if (rsp.Kvs.Count == 0)
            {
                var lst = new List<AgentServiceRegistration>
                {
                    registration
                };
                config.Services = lst;
            }
            else
            {
                var val = JsonConvert.DeserializeObject<EtcdConfig>(rsp.Kvs[0].Value.FromGoogleString());
                List<AgentServiceRegistration> lst = new List<AgentServiceRegistration>(val.Services);
                lst.Add(registration);
                config.Services = lst;
            }
            string cof = JsonConvert.SerializeObject(config);
            await client.PutAsync(key, cof);
            await client.PutAsync(timeKey, DateTime.Now.ToString("yyyy-MM-dd mm:HH:ss"));
      
            await client.UnlockAsync(key+"1");
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="response"></param>
        private static void Watch(LeaseKeepAliveResponse response)
        {
            //刷新返回了
            Console.WriteLine("注册：" + response.ID);
        }

        /// <summary>
        /// 监视Key
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="key">服务Key</param>
        private static void Init(EtcdClient  client, string key)
        {
            client.Watch(key, new Action<WatchResponse>(p =>
            {
                foreach (var e in p.Events)
                {
                    if (e.Type == Mvccpb.Event.Types.EventType.Delete)
                    {
                       
                        //服务异常
                        Console.WriteLine("移除：" + e.Kv.Key.FromGoogleString());
                    }
                    else if (e.Type == Mvccpb.Event.Types.EventType.Put)
                    {
                        //服务加入
                        Console.WriteLine("加入：" + e.Kv.Key.FromGoogleString());
                    }
                }

            }));
        }



    }
}
