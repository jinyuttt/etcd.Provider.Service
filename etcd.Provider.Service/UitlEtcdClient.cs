#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：Uitletcd.Provider.ServiceClient
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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace etcd.Provider.Service
{
  
   
    /// <summary>
    /// 注册更新
    /// </summary>
    public  class Utiletcd
    {

        private const int DefaultTTL = 30;//默认上报时间

        /// <summary>
        /// 同一集群内的服务信息
        /// </summary>
        readonly ConcurrentDictionary<ulong, Health> dic = new ConcurrentDictionary<ulong, Health>();

        private long minTTL = DefaultTTL;//30s
        private const long STicks = 10000000;
       
        //记录更新的数据
        private readonly ConcurrentDictionary<string, long> udateID = new ConcurrentDictionary<string, long>();
        private readonly static Lazy<Utiletcd> Instance = new Lazy<Utiletcd>();
        private long LastKeep = DateTime.Now.Ticks;//上次更新时间
        private  long ExecuteTiks = 0;//提前的时间间隔
        private long FulshTicks = DefaultTTL*STicks;//执行的间隔时间
        private bool isUpdate = true;//Update正常允许

        string username = "";
        string password = "";
        string caCert = "";
        string clientCert = "";
        string clientKey = "";
        bool publicRootCa = false;

        public static Utiletcd Sinlgeton
        {
            get { return Instance.Value; }
        }

        public string Username { get { return username; } set { username = value; } }

        public string Password { get { return password; } set { password = value; } }

        public string CaCert { get { return caCert; } set { caCert = value; } }

        public string ClientCert { get { return clientCert; } set { clientCert = value; } }

        public string ClientKey { get { return clientKey; } set { clientKey = value; } }

        public bool PublicRootCa { get { return publicRootCa; } set { publicRootCa = value; } }

        public Utiletcd()
        {
            Flush();
            Update();
           
        }

        /// <summary>
        /// 保持服务活动
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="key">服务Key</param>
        /// <param name="ttl">服务更新时间</param>
        /// <param name="ID">申请ID</param>
        public async Task AddKeepAliveAsync(EtcdClient client, string key, long ttl, long ID)
        {
            //获取集群列表

            MemberListRequest request = new MemberListRequest();
            var rsp = await client.MemberListAsync(request);
            if (minTTL > ttl)
            {
                if (minTTL == DefaultTTL)
                {
                    //第一次修改
                    ExecuteTiks = (DefaultTTL - ttl) * STicks;//需要提前时间
                }
                else
                {
                    //需要提前的时间减去已经运行的时间
                    ExecuteTiks = (minTTL - ttl) * STicks - (LastKeep - DateTime.Now.Ticks);
                }
                FulshTicks = FulshTicks - ExecuteTiks;//针对上次运行任务还剩余的时间
                minTTL = ttl;
                isUpdate = false;//无法正常允许了
            }
            if (!dic.ContainsKey(rsp.Header.ClusterId))
            {
                Health health = new Health
                {
                    Clients = new List<EtcdClient>(),
                    KeyTTL = new Dictionary<string, long>(),
                  
                    LastKeep = new Dictionary<string, long>(),
                    LeaseID = new Dictionary<string, long>()
                };
                foreach (var kv in rsp.Members)
                {
                    EtcdClientUrls urls = new EtcdClientUrls();
                    foreach (var c in kv.ClientURLs)
                    {
                        urls.Urls.Add(c);
                    }
                    health.Urls=urls;
                }
                //
                health.Clients.Add(client);
                health.KeyTTL[key] = ttl;
                health.LeaseID[key] = ID;
                health.LastKeep[key] = DateTime.Now.Ticks;
                dic[rsp.Header.ClusterId] = health;
                Console.WriteLine("加入刷新");
            }
            else
            {
                Health health = dic[rsp.Header.ClusterId];
                lock (health)
                {
                    health.Clients.Add(client);
                    health.KeyTTL[key] = ttl;
                    health.LastKeep[key] = DateTime.Now.Ticks;
                    health.LeaseID[key] = ID;
                }
            }

        }

        /// <summary>
        /// 定时间隔更新
        /// </summary>
        private void Update()
        {
             Thread update = new Thread(() =>
             {
                 while (true)
                 {
                     Thread.Sleep((int)(minTTL-1) * 1000);
                     if(DateTime.Now.Ticks-LastKeep<minTTL*STicks)
                     {
                         //说明已经刷新了，还没有一个周期的等待
                         continue;
                     }
                     Console.WriteLine("UpdateKeep");
                     CheckKeepAlive();
                     isUpdate = true;
                 }
             });
            update.IsBackground = true;
            update.Name = "Update";
            update.Start();
        }

        /// <summary>
        /// 根据时间变化立即刷新
        /// </summary>
        private void Flush()
        {
            Thread flush = new Thread(() =>
            {
                int num = 0;
                while (true)
                {
                    Thread.Sleep(2000);//2秒计算
                    num++;
                    if(num>DefaultTTL/2)
                    {
                        num = 0;
                    }
                    if (FulshTicks/STicks <= num*2+1&&!isUpdate)
                    {
                        Console.WriteLine("FlushKeep");
                        //等待的周期少于计算，立即执行
                        CheckKeepAlive();
                        num = 0;
                    }
                }
            });
            flush.IsBackground = true;
            flush.Name = "Flush";
            flush.Start();
        }

        /// <summary>
        /// 检测修改
        /// </summary>
        private void CheckKeepAlive()
        {
           
            long ticks = DateTime.Now.Ticks;//s
            LastKeep = ticks;//当前执行时间
            bool isSucess = false;
            foreach (var kv in dic)
            {
                lock(kv.Value)
                {
                    foreach (var ttl in kv.Value.KeyTTL)
                    {
                        
                        long cur = kv.Value.LastKeep[ttl.Key] / STicks+ttl.Value;//下次需要执行的时间
                        long next = ticks / STicks + minTTL;//下次任务时间
                        if (next>cur)
                        {
                            //如果等不到了就更新
                            for (int i = 0; i < kv.Value.Clients.Count; i++)
                            {
                                var c = kv.Value.Clients[i];
                                if (KeepAlive(c, kv.Value.LeaseID[ttl.Key]))
                                {
                                    kv.Value.LastKeep[ttl.Key] = DateTime.Now.Ticks;
                                    isSucess = true;
                                    break;
                                }
                                else
                                {
                                    //不成功则移除
                                    kv.Value.Clients.RemoveAt(i);
                                    c.Dispose();
                                    i--;
                                }
                            }
                            if (!isSucess)
                            {
                               
                                    var urls = kv.Value.Urls;
                                    for (int j = 0; j < urls.Urls.Count; j++)
                                    {
                                        EtcdClient client = null;
                                        string[] addr = urls.Urls[j].Split(new char[] {':','/' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (addr.Length == 2)
                                        {
                                            client = new EtcdClient(addr[0], int.Parse(addr[1]),caCert,clientCert,clientKey,publicRootCa);
                                        }
                                        else if (addr.Length == 3)
                                        {
                                            client = new EtcdClient(addr[1], int.Parse(addr[2]),  caCert, clientCert, clientKey, publicRootCa);
                                        }
                                        if (client != null)
                                        {
                                        
                                            if (KeepAlive(client, kv.Value.LeaseID[ttl.Key]))
                                            {
                                                kv.Value.LastKeep[ttl.Key] = DateTime.Now.Ticks;
                                                UpdateCluster(client);
                                               
                                                break;//成功退出
                                            }
                                        }
                                    }
                                
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新集群列表
        /// </summary>
        /// <param name="client"></param>
        private  void UpdateCluster(EtcdClient client)
        {
           
             MemberListRequest request = new MemberListRequest();
            var rsp = client.MemberList(request);
            if (dic.ContainsKey(rsp.Header.ClusterId))
            {
                Health health = dic[rsp.Header.ClusterId];
                health.Clients.Add(client);
                health.Urls.Urls.Clear();
                foreach (var kv in rsp.Members)
                {
                    EtcdClientUrls urls = new EtcdClientUrls();
                    foreach (var c in kv.ClientURLs)
                    {
                        urls.Urls.Add(c);
                    }
                    health.Urls = urls;
                }
                health.Clients.Add(client);
            }
               
            

        }
      
        /// <summary>
        /// 发送更新
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        private bool KeepAlive(EtcdClient client,long id)
        {
            try
            {
                CancellationToken token = new CancellationToken();
                client.LeaseKeepAlive(new LeaseKeepAliveRequest() { ID = id }, Watch, token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="response"></param>
        private  void Watch(LeaseKeepAliveResponse response)
        {
           // response.Header.
            //刷新返回了
            Console.WriteLine("刷新：" + response.ID);
            udateID[response.ID.ToString() + "_" + response.Header.ClusterId.ToString()] = DateTime.Now.Ticks;
        }

       
    }
}
