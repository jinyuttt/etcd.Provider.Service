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
    /* ============================================================================== 
* 功能描述：Utiletcd.Provider.ServiceClient 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
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
                var lease = await client.LeaseGrantAsync(new LeaseGrantRequest() { ID = 0, TTL = (long)registration.Checks[0].Interval.TotalSeconds });
                var rsp = await client.PutAsync(new PutRequest() { Key = key.ToGoogleString(), Value = val.ToGoogleString(), Lease = lease.ID });
                client.LeaseKeepAlive(new LeaseKeepAliveRequest() { ID = lease.ID }, Watch, token);
                await AddConfigAsync(client, systemName,registration);
                await Uitletcd.Sinlgeton.AddKeepAliveAsync(client, key, (long)registration.Checks[0].Interval.TotalSeconds, lease.ID);
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
        public static void SetInfo(string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            Uitletcd.Sinlgeton.Username = username;
            Uitletcd.Sinlgeton.Password = password;
            Uitletcd.Sinlgeton.CaCert = caCert;
            Uitletcd.Sinlgeton.ClientCert = clientCert;
            Uitletcd.Sinlgeton.ClientKey = clientKey;
            Uitletcd.Sinlgeton.PublicRootCa = publicRootCa;
        }

        /// <summary>
        /// 系统配置注册
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sysName"></param>
        /// <param name="registration"></param>
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
                var lst = new List<AgentServiceRegistration>();
                lst.Add(registration);
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
        /// <param name="client"></param>
        /// <param name="sytemName"></param>
        private static void Init(EtcdClient  client, string sytemName)
        {
            client.Watch(sytemName, new Action<WatchResponse>(p =>
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
                        Console.WriteLine("移除：" + e.Kv.Key.FromGoogleString());
                    }
                }

            }));
        }



    }
}
