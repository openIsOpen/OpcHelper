using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Siemens.UAClientHelper;

namespace Zlw.OpcClient
{
    public class OpcUaClient : global::Zlw.OpcClient.OpcClient
    {
        private UAClientHelperAPI uAClient = null;
        private Subscription _Subscription = null;
        private Dictionary<string, TagItem> _SubscriptionTags = new Dictionary<string, TagItem>();
        private Dictionary<string, MonitoredItem> _MonitorItem = new Dictionary<string, MonitoredItem>();
        private Dictionary<string, TagItem> _AsyncReadTags = new Dictionary<string, TagItem>();
        private Dictionary<string, TagItem> _AsyncWriteTags = new Dictionary<string, TagItem>();
        private Dictionary<string, TagItem> _Tags = new Dictionary<string, TagItem>();
        private Dictionary<string, EndpointDes> _EndpointDescriptions = new Dictionary<string, EndpointDes>();
        private NavModal _Nav = null;                             //保存服务端Node树状结构
        private List<TagItem> _ServerTags = new List<TagItem>();    //保存服务端所有变量
        private List<TagItem> _ServerUserTags = new List<TagItem>();    //保存服务端所有变量
        private List<TagItem> _ServerSysTags = new List<TagItem>();    //保存服务端所有变量

        public OpcUaClient()
        {
            uAClient = new UAClientHelperAPI();
            uAClient.CertificateValidationNotification = CertificateValidation;  //验证证书
            uAClient.ItemChangedNotification = ItemChanged;                       //订阅数据改变
            uAClient.KeepAliveNotification = KeepAlive;                                 //保持连接
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool Connect(OpcServer server)
        {
            var uaServer = server as OpcUaServer;
            if (uaServer == null) return false;
            uAClient.Connect((EndpointDescription)uaServer.EndpointDesCription.OpcEndpointDescription,
                uaServer.UserAuth, uaServer.UserName, uaServer.Password);
            _Subscription = uAClient.Subscribe(uaServer.SubscriptionPublishingInterval);
            GetAllNode();
            GetAllTags(_Nav);
            GetUserTags(_Nav);
            GetSysTags(_Nav);
            IsConnected = true;
            return true;
        }

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        /// <summary>
        /// 配置连接
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public bool Config(Configuration cfg)
        {
            return false;
        }

        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool AddSubscription(TagItem tag)
        {
            if (_SubscriptionTags.ContainsKey(tag.ServerId)) return true;
            MonitoredItem item = uAClient.AddMonitoredItem(_Subscription, tag.GetNodeId(), tag.Name, tag.SampleRate);
            _SubscriptionTags.Add(tag.ServerId, tag);
            _MonitorItem.Add(tag.ServerId, item);
            if (item.Created)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 批量添加订阅
        /// </summary>
        /// <param name="tags">要添加订阅的变量</param>
        /// <returns>成功添加的变量</returns>
        public IList<TagItem> AddSubscription(IList<TagItem> tags)
        {
            List<TagItem> sucessed = new List<TagItem>();
            foreach (var tag in tags)
            {
                if (AddSubscription(tag))
                {
                    sucessed.Add(tag);
                }
            }

            return sucessed;
        }

        /// <summary>
        /// 订阅内移除变量
        /// </summary>
        /// <param name="tag">要移除的变量</param>
        /// <returns></returns>
        public bool RemoveSubscription(TagItem tag)
        {

            MonitoredItem monitoredItem = uAClient.RemoveMonitoredItem(_Subscription, _MonitorItem[tag.ServerId]);
            if (_MonitorItem[tag.ServerId].Created == false)
            {
                _SubscriptionTags.Remove(tag.ServerId);
                _MonitorItem.Remove(tag.ServerId);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 订阅内批量移除变量
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IList<TagItem> RemoveSubscription(IList<TagItem> tags)
        {

            List<TagItem> sucessed = new List<TagItem>();
            foreach (var tag in tags)
            {
                if (RemoveSubscription(tag))
                {
                    sucessed.Add(tag);
                }
            }

            return sucessed;
        }

        /// <summary>
        /// 移除所有变量的订阅
        /// </summary>
        /// <returns></returns>
        public bool RemoveSubscription()
        {
            Dictionary<string, TagItem>.ValueCollection values = _SubscriptionTags.Values;
            var enumerator = values.GetEnumerator();
            if (enumerator.MoveNext())
            {
                RemoveSubscription(enumerator.Current);
            }
            enumerator.Dispose();
            return true;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            RemoveSubscription();
            uAClient.RemoveSubscription(_Subscription);
            uAClient.Disconnect();
            IsConnected = false;
            return true;
        }

        /// <summary>
        /// 读一个变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public TagItem Read(TagItem tag)
        {

            DataValue value = uAClient.ReadValue(tag.GetNodeId());
            tag.Value = value.Value;
            tag.Quality = value.StatusCode.ToString();
            tag.Timestamp = value.SourceTimestamp;
            return tag;
        }

        /// <summary>
        /// 读多个变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>返回状态为GOOD的变量</returns>
        public IList<TagItem> Read(IList<TagItem> tag)
        {
            List<string> nodeids = new List<string>();
            foreach (var item in tag)
            {
                nodeids.Add(item.GetNodeId());
            }

            List<string> values = uAClient.ReadValues(nodeids);
            for (int i = 0; i < values.Count; i++)
            {
                tag[i].Value = values[i];
            }
            return tag;
        }

        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="tag"></param>
        public void AsyncRead(TagItem tag)
        {
            List<TagItem> tags = new List<TagItem>();
            tags.Add(tag);
            AsyncRead(tags);
        }

        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="tag"></param>
        public void AsyncRead(IList<TagItem> tag)
        {
            List<string> ss = new List<string>();
            foreach (var item in tag)
            {
                ss.Add(item.GetNodeId());
                if(!_AsyncReadTags.ContainsKey(item.ServerId))
                _AsyncReadTags.Add(item.ServerId, item);
            }
            uAClient.AsyncReadValue(ss, CallBack_Read);
        }


        /// <summary>
        /// 写变量
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Write(TagItem tag)
        {
            uAClient.WriteValues(new List<string>() { tag.ValueToWrite?.ToString() }, new List<string>() { tag.GetNodeId() });
            return true;
        }

        /// <inheritdoc />
        public bool Write(IList<TagItem> tags)
        {
            List<string> values = new List<string>();
            List<string> nodeIdStrs = new List<string>();
            foreach (var tag in tags)
            {
                values.Add(tag.ValueToWrite?.ToString());
                nodeIdStrs.Add(tag.GetNodeId());
            }

            uAClient.WriteValues(values, nodeIdStrs);
            return true;
        }

        /// <inheritdoc />
        public bool AsyncWrite(TagItem tag)
        {
            AsyncWrite(new List<TagItem>() { tag });
            return true;
        }

        /// <inheritdoc />
        public bool AsyncWrite(IList<TagItem> tags)
        {
            List<string> nodeIdStrs = new List<string>();
            List<string> values = new List<string>();
            for (int i = 0; i < tags.Count; i++)
            {
                nodeIdStrs.Add(tags[i].GetNodeId());
                values.Add(tags[i].ValueToWrite.ToString());
                if (!_AsyncWriteTags.ContainsKey(tags[i].ServerId))
                    _AsyncWriteTags.Add(tags[i].ServerId, tags[i]);
            }

            uAClient.AsyncWriteValue(nodeIdStrs, values, CallBack_Write);
            return true;
        }

        /// <inheritdoc />
        public bool AddTag(TagItem tag)
        {
            _Tags.Add(tag.ServerId, tag);
            return true;
        }

        /// <inheritdoc />
        public IList<TagItem> AddTag(IList<TagItem> tags)
        {
            foreach (var tag in tags)
            {
                _Tags.Add(tag.ServerId, tag);
            }

            return tags;
        }

        /// <inheritdoc />
        public bool RemoveTag(TagItem tag)
        {
            _Tags.Remove(tag.ServerId);
            return true;
        }

        /// <inheritdoc />
        public IList<TagItem> RemoveTag(IList<TagItem> tags)
        {
            foreach (var tag in tags)
            {
                _Tags.Remove(tag.ServerId);
            }
            return tags;
        }

        #region 回调函数
        private void KeepAlive(Session session, KeepAliveEventArgs e)
        {
            //检查会话状态
            if (!ServiceResult.IsGood(e.Status))
            {
                // 重新连接
                uAClient.Session.Reconnect();
            }
        }

        /// <summary>
        /// 异回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void CallBack_Read(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                TagItem tag = null;
                string tagServerIndex = string.Empty;
                DataValueCollection results;
                DiagnosticInfoCollection diagnosticinfos;
                ReadValueIdCollection tmpReadIds = ar.AsyncState as ReadValueIdCollection;
                ResponseHeader responseHeader = uAClient.Session.EndRead(ar, out results, out diagnosticinfos);
                ClientBase.ValidateResponse(results, tmpReadIds);
                ClientBase.ValidateDiagnosticInfos(diagnosticinfos, tmpReadIds);
                for (int i = 0; i < results.Count; i++)
                {
                    tagServerIndex = tmpReadIds[i].NodeId.Identifier.ToString();
                    if (!_AsyncReadTags.ContainsKey(tagServerIndex)) continue;
                    tag = _AsyncReadTags[tmpReadIds[i].NodeId.Identifier.ToString()];
                    tag.CBType = TagItem.CallBackType.AsyncRead;
                    tag.Quality = results[i].StatusCode.ToString();
                    tag.Value = results[i].Value;
                    tag.Timestamp = results[i].SourceTimestamp;
                    tag.CallBack?.Invoke(tag);
                    _AsyncReadTags.Remove(tmpReadIds[i].NodeId.Identifier.ToString());
                }
            }
        }

        private void CallBack_Write(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                TagItem tag = null;
                string tagServerIndex = string.Empty;
                DataValueCollection results;
                DiagnosticInfoCollection diagnosticinfos;
                WriteValueCollection tmpReadIds = ar.AsyncState as WriteValueCollection;
                ResponseHeader responseHeader = uAClient.Session.EndRead(ar, out results, out diagnosticinfos);
                ClientBase.ValidateResponse(results, tmpReadIds);
                ClientBase.ValidateDiagnosticInfos(diagnosticinfos, tmpReadIds);
                for (int i = 0; i < results.Count; i++)
                {
                    tagServerIndex = tmpReadIds[i].NodeId.Identifier.ToString();
                    if (!_AsyncWriteTags.ContainsKey(tagServerIndex)) continue;
                    tag = _AsyncWriteTags[tmpReadIds[i].NodeId.Identifier.ToString()];
                    tag.CBType = TagItem.CallBackType.AsyncRead;
                    tag.Quality = results[i].StatusCode.ToString();
                    tag.Value = results[i].Value;
                    tag.Timestamp = results[i].SourceTimestamp;
                    tag.CallBack?.Invoke(tag);
                    _AsyncWriteTags.Remove(tmpReadIds[i].NodeId.Identifier.ToString());
                }
            }
        }

        private void ItemChanged(MonitoredItem monitoreditem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
            if (notification != null)
            {
                string nodeid = monitoreditem.StartNodeId.Identifier.ToString();
                if (!_SubscriptionTags.ContainsKey(nodeid))
                    throw (new Exception(string.Format("订阅回调触发,但是_SubscriptionTags不包含键;key:{0}", nodeid)));
                TagItem item = _SubscriptionTags[nodeid];
                if (item == null)
                    throw (new Exception(string.Format("订阅回调触发,但是未找到键对应TagItem对象;key:{0},value:null", nodeid)));
                if (item.CallBack == null) return;
                item.CBType = TagItem.CallBackType.Subscription;
                item.Value = notification.Value.Value;
                item.Timestamp = notification.Value.SourceTimestamp;
                item.Quality = notification.Value.StatusCode.ToString();
                item.CallBack(item);

            }
        }

        private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            //在本地查抄服务器证书，如果已经存在，直接接受
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509CertificateCollection certCol = store.Certificates.Find(X509FindType.FindByThumbprint, e.Certificate.Thumbprint, true);
            store.Close();
            if (certCol.Capacity > 0)
            {
                e.Accept = true;
            }

            //不存在
            else
            {
                if (!e.Accept)
                {
                    //获取证书数据
                    //证书信息先备用，这里暂时直接接受证书并存储
                    string[] row1 = new string[] { "Issuer Info", e.Certificate.IssuerName.Name };
                    string[] row2 = new string[] { "Valid From", e.Certificate.NotBefore.ToString() };
                    string[] row3 = new string[] { "Valit To", e.Certificate.NotAfter.ToString() };
                    string[] row4 = new string[] { "Serial Number", e.Certificate.SerialNumber };
                    string[] row5 = new string[] { "Signature Algorithm", e.Certificate.SignatureAlgorithm.FriendlyName };
                    string[] row6 = new string[] { "Cipher Strength", e.Certificate.PublicKey.Key.KeySize.ToString() };
                    string[] row7 = new string[] { "Thumbprint", e.Certificate.Thumbprint };
                    string[] row8 = new string[] { "URI", e.Certificate.GetNameInfo(X509NameType.UrlName, false) };
                    string[] row9 = new string[] { "Subject Alternative Name", "" };

                    foreach (X509Extension ext in e.Certificate.Extensions)
                    {
                        AsnEncodedData asnData = new AsnEncodedData(ext.Oid, ext.RawData);
                        String tempString = asnData.Format(true);
                        if (tempString.Contains("URL") || tempString.Contains("IP") || tempString.Contains("DNS"))
                        {
                            row9 = new string[] { "Subject Alternative Name", tempString };
                        }
                    }
                    object[] rows = new object[] { row1, row2, row3, row4, row5, row6, row7, row8, row9 };

                    e.Accept = true;    //接受服务端证书
                                        //存储服务端证书
                    store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(e.Certificate);
                    store.Close();
                }
            }
        }
        #endregion

        public List<EndpointDes> FindServer(string serverUrl)
        {
            List<EndpointDes> endpointListView = new List<EndpointDes>();
            _EndpointDescriptions.Clear();
            ApplicationDescriptionCollection servers = uAClient.FindServers(serverUrl);
            foreach (ApplicationDescription ad in servers)
            {
                foreach (string url in ad.DiscoveryUrls)
                {
                    EndpointDescriptionCollection endpoints = uAClient.GetEndpoints(url);
                    foreach (EndpointDescription ep in endpoints)
                    {
                        string securityPolicy = ep.SecurityPolicyUri.Remove(0, 42);
                        EndpointDes endpointDes = new EndpointDes()
                        {
                            ID = Guid.NewGuid().ToString(),
                            Description = "[" + ad.ApplicationName + "] " + " [" + ep.SecurityMode + "] " + " [" + securityPolicy + "] " + " [" + ep.EndpointUrl + "]",
                            OpcEndpointDescription = ep
                        };
                        endpointListView.Add(endpointDes);
                        _EndpointDescriptions.Add(endpointDes.ID, endpointDes);
                    }
                }
            }
            return endpointListView;
        }

        /// <summary>
        /// 获取服务端所有变量
        /// </summary>
        /// <returns></returns>
        List<TagItem> OpcClient.GetItems()
        {
            return _ServerTags;
        }

        /// <summary>
        /// 获取服务端用户变量
        /// </summary>
        /// <returns></returns>
        public List<TagItem> GetUserItems()
        {
            return _ServerUserTags;
        }

        /// <summary>
        /// 获取服务端系统变量
        /// </summary>
        /// <returns></returns>
        public List<TagItem> GetSysItems()
        {
            return _ServerSysTags;
        }



        private void GetAllTags(NavModal modal)
        {
            if (modal == null) throw new ArgumentNullException();
            if (modal.Node?.NodeClass == NodeClass.Variable)            //这是一个变量
            {
                if (!modal.Node.NodeId.ToString().StartsWith("i"))       //过滤掉i=8 开头的
                {
                    _ServerTags.Add(new TagItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ServerId = modal.Node.NodeId.Identifier.ToString(),
                        NodeId = modal.Node.NodeId.ToString(),
                        NameSpace = modal.Node.NodeId.NamespaceIndex.ToString(),
                        Name = modal.Node.DisplayName.Text
                    });
                }
            }
            if (modal.Children.Count > 0)
            {
                foreach (var m in modal.Children)
                {
                    GetAllTags(m);
                }
            }
        }

        private void GetUserTags(NavModal modal)
        {
            if (modal == null) throw new ArgumentNullException();
            if (modal.Node?.NodeClass == NodeClass.Variable)            //这是一个变量
            {
                if (!modal.Node.NodeId.ToString().StartsWith("i"))       //过滤掉i=8 开头的
                {
                    if (!modal.Node.DisplayName.Text.StartsWith("_"))   //过滤掉系统变量
                    {
                        _ServerUserTags.Add(new TagItem()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ServerId = modal.Node.NodeId.Identifier.ToString(),
                            NodeId = modal.Node.NodeId.ToString(),
                            NameSpace = modal.Node.NodeId.NamespaceIndex.ToString(),
                            Name = modal.Node.DisplayName.Text
                        });
                    }
                }
            }
            if (modal.Children.Count > 0)
            {
                foreach (var m in modal.Children)
                {
                    GetUserTags(m);
                }
            }
        }

        private void GetSysTags(NavModal modal)
        {
            if (modal == null) throw new ArgumentNullException();
            if (modal.Node?.NodeClass == NodeClass.Variable)            //这是一个变量
            {
                if (!modal.Node.NodeId.ToString().StartsWith("i"))       //过滤掉i=8 开头的
                {
                    if (modal.Node.DisplayName.Text.StartsWith("_"))   //过滤掉系统变量
                    {
                        _ServerSysTags.Add(new TagItem()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ServerId = modal.Node.NodeId.Identifier.ToString(),
                            NodeId = modal.Node.NodeId.ToString(),
                            NameSpace = modal.Node.NodeId.NamespaceIndex.ToString(),
                            Name = modal.Node.DisplayName.Text
                        });
                    }
                }
            }
            if (modal.Children.Count > 0)
            {
                foreach (var m in modal.Children)
                {
                    GetSysTags(m);
                }
            }
        }

        /// <summary>
        /// 获取所有节点,包括变量
        /// </summary>
        private void GetAllNode()
        {
            _Nav = new NavModal();
            ReferenceDescriptionCollection referenceDescriptionCollection = uAClient.BrowseRoot();
            GetNodes(referenceDescriptionCollection, _Nav);
        }


        private void GetNodes(ReferenceDescriptionCollection refDes, NavModal parent)
        {
            //parent 上一层的modal
            for (int i = 0; i < refDes.Count; i++)
            {
                NavModal thisModal = new NavModal()
                {
                    Id = Guid.NewGuid().ToString(),
                    Node = new Node(refDes[i]),
                    Parent = parent
                };
                parent.Children.Add(thisModal);
                if (refDes[i].NodeClass == NodeClass.Variable) continue;
                ReferenceDescriptionCollection collection = uAClient.BrowseNode(refDes[i]);
                if (collection.Count > 0)
                {
                    GetNodes(collection, thisModal);
                }
            }
        }

    }
}
