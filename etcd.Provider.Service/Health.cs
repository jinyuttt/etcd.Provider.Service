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
using System;
using System.Collections.Generic;
using System.Text;

namespace etcd.Provider.Service
{
    /* ============================================================================== 
* 功能描述：Health 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
  public  class Health
    {
        public List<EtcdClient> Clients { get; set; }
        public Dictionary<string,long> KeyTTL { get; set; }
        public List<EtcdClientUrls> Urls { get; set; }
        public Dictionary<string,long> LastKeep { get; set; }
        public Dictionary<string, long> LeaseID { get; set; }
    }
}
