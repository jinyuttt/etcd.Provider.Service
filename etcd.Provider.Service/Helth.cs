#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：Helth
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
using System.Collections.Generic;

namespace etcd.Provider.Service
{
    /* ============================================================================== 
* 功能描述：Helth 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class Helth
    {
        /// <summary>
        /// 刷新Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 生命周期
        /// </summary>
        public long TTL { get; set; }

        /// <summary>
        /// 使用的客户端
        /// </summary>
        public EtcdClient Client{get;set;}

        Dictionary<string, EtcdClientUrls> Cluster { get; set; }

        public ulong ID { get; set; }

        public ulong ClusterID { get; set; }
    }
}
