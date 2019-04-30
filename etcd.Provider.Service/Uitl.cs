#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：etcd.Provider.Service
* 项目描述 ：
* 类 名 称 ：Uitl
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

using Google.Protobuf;
using System.Text;

namespace etcd.Provider.Service
{
    /* ============================================================================== 
* 功能描述：Uitl 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public static class Uitl
    {

        /// <summary>
        /// 字符串转成ByteString
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ByteString ToGoogleString(this string str)
        {
          return  ByteString.CopyFrom(str, Encoding.Default);
        }

        /// <summary>
        /// ByteString转成字符串
        /// </summary>
        /// <param name="googleString"></param>
        /// <returns></returns>
        public static string FromGoogleString(this ByteString googleString)
        {
          return  googleString.ToString(Encoding.Default);
        }

        /// <summary>
        /// 转成byte[]
        /// </summary>
        /// <param name="googleString"></param>
        /// <returns></returns>
        public static byte[] FromGoogleStringBytes(this ByteString googleString)
        {
          return  googleString.ToByteArray();
        }

        /// <summary>
        /// 转换ByteString
        /// </summary>
        /// <param name="googleBytes"></param>
        /// <returns></returns>
        public static ByteString FromGoogleStringBytes(this byte[] googleBytes)
        {
            return ByteString.CopyFrom(googleBytes, 0, googleBytes.Length);
        }

    }
}
