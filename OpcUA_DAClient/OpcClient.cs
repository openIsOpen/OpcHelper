using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zlw.OpcClient
{
    public interface OpcClient
    {
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        bool Connect(OpcServer server);

         bool IsConnected { get; set; }

        /// <summary>
        /// 配置访问参数
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        bool Config(Configuration cfg);

        /// <summary>
        /// 添加指定订阅
        /// </summary>
        /// <param name="tag">要添加订阅的变量</param>
        /// <returns></returns>
        bool AddSubscription(TagItem tag);

        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="tags">要添加订阅的变量</param>
        /// <returns></returns>
         IList<TagItem> AddSubscription(IList<TagItem> tags);

        /// <summary>
        /// 指定变量移除订阅
        /// </summary>
        /// <param name="tag">要移除订阅的变量</param>
        /// <returns></returns>
         bool RemoveSubscription(TagItem tag);

        /// <summary>
        /// 指定变量移除订阅
        /// </summary>
        /// <param name="tags">要移除订阅的变量</param>
        /// <returns>成功移除订阅的变量</returns>
         IList<TagItem> RemoveSubscription(IList<TagItem> tags);

        /// <summary>
        /// 移除所有哦变量订阅
        /// </summary>
        /// <returns></returns>
         bool RemoveSubscription();

        /// <summary>
        /// 断开与服务器的连接
        /// </summary>
        /// <returns></returns>
        bool Disconnect();
        
        /// <summary>
        /// 读变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        TagItem Read(TagItem tag);

        /// <summary>
        /// 读多个变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        IList<TagItem> Read(IList<TagItem> tag);

        /// <summary>
        /// 同步读变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        void AsyncRead(TagItem tag);

        /// <summary>
        /// 同步读多个变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        void AsyncRead(IList<TagItem> tag);

        /// <summary>
        /// 写变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool Write(TagItem tag);

        /// <summary>
        /// 写多个变量
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        bool Write(IList<TagItem> tags);

        /// <summary>
        /// 异步写变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool AsyncWrite(TagItem tag);

        /// <summary>
        /// 异步写多个变量
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        bool AsyncWrite(IList<TagItem> tags);

        /// <summary>
        /// 添加变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool AddTag(TagItem tag);

        /// <summary>
        /// 添加多个变量
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        IList<TagItem> AddTag(IList<TagItem> tags);

        /// <summary>
        /// 移除变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool RemoveTag(TagItem tag);

        /// <summary>
        /// 移除多个变量
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        IList<TagItem> RemoveTag(IList<TagItem> tags);

        List<EndpointDes> FindServer(string serverUrl);

        /// <summary>
        /// 获取服务端所有变量
        /// </summary>
        /// <returns></returns>
        List< TagItem> GetItems();

        /// <summary>
        /// 获取服务端所有用户变量
        /// </summary>
        /// <returns></returns>
        List< TagItem> GetUserItems();

        /// <summary>
        /// 获取服务端所有系统变量
        /// </summary>
        /// <returns></returns>
        List< TagItem> GetSysItems();

    }

}