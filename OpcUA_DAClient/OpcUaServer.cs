using Opc.Ua;

namespace Zlw.OpcClient
{
    public class OpcUaServer:OpcServer
    {
        private MessageSecurityMode _MsgSecurityMode;

        /// <summary>
        /// Server endpoint Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 连接时要使用的安全策略
        /// </summary>
        public string SecurityPolicy { get; set; }

        /// <summary>
        /// 消息安全模式
        /// </summary>
        public MessageSecurityMode MsgSecurityMode
        {
            get => _MsgSecurityMode;
            set => _MsgSecurityMode = value;
        }

        /// <summary>
        /// 是否启用用户验证
        /// </summary>
        public bool UserAuth { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 订阅发布时间
        /// </summary>
        public int SubscriptionPublishingInterval { get; set; }

    }
}
