#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：ServiceEntry
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
* 功能描述：ServiceEntry 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class ServiceEntry
    {
        /// <summary>
        /// 服务唯一ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; }
    }
}
