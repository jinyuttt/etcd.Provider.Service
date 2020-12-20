#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：AgentServiceCheck
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

using System;

namespace etcd.Provider.Service
{

    /// <summary>
    /// 监控信息
    /// </summary>
    public class AgentServiceCheck
    {
        /// <summary>
        /// 延迟注册时间
        /// </summary>
        public TimeSpan DeregisterCriticalServiceAfter { get; set; }

        /// <summary>
        /// 监控检测时间
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// 检测方式
        /// </summary>
        public string TCP { get; set; }

        /// <summary>
        /// 监控超时
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}
