#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：EtcdClientUls
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
using System.Collections.Generic;
using System.Text;

namespace etcd.Provider.Service
{
   
    /// <summary>
    /// etcd信息
    /// </summary>
  public  class EtcdClientUrls
    {

        /// <summary>
        /// 集群客户端
        /// </summary>
      public  List<string> Urls = new List<string>();
    }
}
