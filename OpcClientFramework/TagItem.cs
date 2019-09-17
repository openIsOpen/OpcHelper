using System;

namespace Zlw.OpcClient
{
    public  class TagItem
    {

        public enum CallBackType
        {
            Subscription,
            AsyncRead,
            AsyncWrite
        }

        /// <summary>
        /// 变量ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// UA连接服务端命名空间
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// 变量在Server上注册所需id
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// 变量名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 变量值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 要写入的值
        /// </summary>
        public object ValueToWrite { get; set; }  

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 质量
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// 采样频率
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// 订阅事件
        /// </summary>
        public Action<TagItem> CallBack { get; set; }

        /// <summary>
        /// 回调函数类型
        /// </summary>
        public CallBackType CBType { get; set; }

        public string GetNodeId()
        {
            return string.Format("ns={0};{1}",NameSpace,ServerId);
        }
    }
}
