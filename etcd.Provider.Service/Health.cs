#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：Health
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

    /// <summary>
    /// 服务信息
    /// </summary>
    public  class Health
    {
        /// <summary>
        /// 当前使用的客户端；
        /// 可以使用不同集群的
        /// </summary>
        public List<EtcdClient> Clients { get; set; }

        /// <summary>
        /// 保持更新时间
        /// </summary>
        public Dictionary<string,long> KeyTTL { get; set; }

        /// <summary>
        /// 集群地址
        /// </summary>
        public EtcdClientUrls Urls { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public Dictionary<string,long> LastKeep { get; set; }

        /// <summary>
        /// 同步ID
        /// </summary>
        public Dictionary<string, long> LeaseID { get; set; }
    }
}
