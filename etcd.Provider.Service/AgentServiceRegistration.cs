#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：AgentServiceRegistration
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


using System.Collections.Generic;

namespace etcd.Provider.Service
{
    /* ============================================================================== 
* 功能描述：AgentServiceRegistration 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public  class AgentServiceRegistration
    {
        /// <summary>
        /// 监测
        /// </summary>
        public AgentServiceCheck[] Checks { get; set; }

        /// <summary>
        /// 服务ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 服务版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Tags { get; set; }
    }
}
