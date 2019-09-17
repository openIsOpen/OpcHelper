/*
 *最好所有变量添加订阅
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpcDaHelper;
using OPCDAAUTO;

namespace Zlw.OpcClient
{
    public class OpcDaClient : OpcClient
    {
        private OpcDAClientHelper _DaClient = null;
        private Group _OpcDaGroup = null;
        private Dictionary<string, TagItem> _TagItems = new Dictionary<string, TagItem>(); 

        /// <inheritdoc />
        public bool Connect(OpcServer server)
        {
            if (server == null) throw (new ArgumentNullException());
            OpcDaServer daServer = server as OpcDaServer;
            _DaClient = new OPCDAAUTO.OpcDAClientHelper(daServer.Ip, daServer.ProgId);
            _DaClient.Connect();
            _OpcDaGroup = _DaClient.AddGroup(new Group()
            {
                Channel="C",
                Device = "D",
            });
            _OpcDaGroup.DataChangeEvent += _OpcDaGroup_DataChangeEvent;
            _OpcDaGroup.AsyncReadCompleteEvent += _OpcDaGroup_AsyncReadCompleteEvent;
            _OpcDaGroup.AsyncWriteCompleteEvent += _OpcDaGroup_AsyncWriteCompleteEvent;
            _OpcDaGroup.AsyncCancelCompleteEvent += _OpcDaGroup_AsyncCancelCompleteEvent;
            return true;
        }



        /// <summary>
        /// 配置，暂未实现
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
            if (_TagItems.ContainsKey(tag.ServerId)) return true; //重复添加订阅，认为成功
            _TagItems.Add(tag.ServerId,tag);
            return _OpcDaGroup.AddItem(new Item()
            {
                Name = tag.ServerId       
            });
        }

        /// <summary>
        /// 批量添加订阅
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IList<TagItem> AddSubscription(IList<TagItem> tags)
        {
            List<Item> items = new List<Item>();
            foreach (var tag in tags)
            {
                if (_TagItems.ContainsKey(tag.ServerId)) continue;
                _TagItems.Add(tag.ServerId,tag);
                items.Add(new Item()
                {
                    Name = tag.ServerId
                });
            }

            List<Item> addItem = _OpcDaGroup.AddItem(items);

            IList<TagItem> result = new List<TagItem>();
            foreach (var item in addItem)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (tags[i].ServerId == item.Name)
                    {
                        result.Add(tags[i]);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool RemoveSubscription(TagItem tag)
        {
            _TagItems.Remove(tag.ServerId);
            return  _OpcDaGroup.RemoveItem(_OpcDaGroup.ItemsEx[tag.ServerId]).Count ==1;
        }

        /// <summary>
        /// 批量取消订阅
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IList<TagItem> RemoveSubscription(IList<TagItem> tags)
        {
            List<Item> items = new List<Item>();
            foreach (var tag in tags)
            {
                _TagItems.Remove(tag.ServerId);
                items.Add(_OpcDaGroup.ItemsEx[tag.ServerId]);
            }

            List<Item> removeItem = _OpcDaGroup.RemoveItem(items);
            IList<TagItem> result = new List<TagItem>();
            foreach (var item in removeItem)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (tags[i].ServerId == item.Name)
                    {
                        result.Add(tags[i]);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 暂未实现
        /// </summary>
        /// <returns></returns>
        public bool RemoveSubscription()
        {
            return false;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            _DaClient.RemoveGroup(_OpcDaGroup);
            return _DaClient.DisConnect();
        }

       /// <summary>
       /// 同步读
       /// </summary>
       /// <param name="tag"></param>
       /// <returns></returns>
        public TagItem Read(TagItem tag)
        {
            TagItem tagItem = _TagItems[tag.ServerId];
            Item item = _OpcDaGroup.ItemsEx[tag.ServerId];
            if (item == null)
            {
                AddSubscription(tag);
                item = _OpcDaGroup.ItemsEx[tag.ServerId];
                item.Read();
                tagItem.Value = item.Value;
                RemoveSubscription(tag);
            }
            else
            {
                item.Read();
                tagItem.Value = item.Value;
            }

            return tagItem;
        }

        /// <summary>
        /// 同步读
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public IList<TagItem> Read(IList<TagItem> tag)
        {
            List<TagItem> tagItems = new List<TagItem>();
            foreach (var t in tag)
            {
                tagItems.Add(Read(t));
            }

            return tagItems;
        }

        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="tag"></param>
        public void AsyncRead(TagItem tag)
        {
            AsyncRead(new List<TagItem>{tag});
        }

        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="tag"></param>
        public void AsyncRead(IList<TagItem> tag)
        {
            int cancelId;
            //异步读时,变量必须已经添加了订阅，否则DA会报错。
            List<Item> items = new List<Item>();
            foreach (var t in tag)
            {
                items.Add(_OpcDaGroup.ItemsEx[t.ServerId]);
            }

            _OpcDaGroup.AsyncRead(items, 0, out cancelId);
        }

        /// <summary>
        /// 同步写
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Write(TagItem tag)
        {
            TagItem tagItem = _TagItems[tag.ServerId];
            Item item = _OpcDaGroup.ItemsEx[tag.ServerId];
            if (item == null)
            {
                AddSubscription(tag);
                item = _OpcDaGroup.ItemsEx[tag.ServerId];
                item.ObjValue = tag.ValueToWrite;
                item.Write_NoRes();
                RemoveSubscription(tag);
            }
            else
            {
                item.ObjValue = tag.ValueToWrite;
                item.Write_NoRes();
            }
            return true;
        }

        /// <summary>
        /// 同步写
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool Write(IList<TagItem> tags)
        {

            List<TagItem> tagItems = new List<TagItem>();
            foreach (var t in tags)
            {
                Read(t);
            }
            return true;
        }

        /// <summary>
        /// 异步写
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool AsyncWrite(TagItem tag)
        {
            return false;
        }

        /// <summary>
        /// 异步写
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool AsyncWrite(IList<TagItem> tags)
        {
            int cancelId;
            //异步读时,变量必须已经添加了订阅，否则DA会报错。
            List<Item> items = new List<Item>();
            foreach (var t in tags)
            {
                items.Add(_OpcDaGroup.ItemsEx[t.ServerId] );
                _OpcDaGroup.ItemsEx[t.ServerId].ObjValue = t.ValueToWrite;
            }
            _OpcDaGroup.AsyncWrite(items, 0, out cancelId);
            return true;
        }

        /// <summary>
        /// 等同于添加订阅
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool AddTag(TagItem tag)
        {
            return AddSubscription(tag);
        }

       /// <summary>
       /// 等同于添加订阅
       /// </summary>
       /// <param name="tags"></param>
       /// <returns></returns>
        public IList<TagItem> AddTag(IList<TagItem> tags)
        {
            return AddSubscription(tags);
        }

       /// <summary>
       /// 等同于移除订阅
       /// </summary>
       /// <param name="tag"></param>
       /// <returns></returns>
        public bool RemoveTag(TagItem tag)
        {
            return RemoveSubscription(tag);
        }

        /// <summary>
        /// 等同于移除订阅
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IList<TagItem> RemoveTag(IList<TagItem> tags)
        {
            return RemoveSubscription(tags);
        }

        /// <summary>
        /// 发现服务器
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <returns></returns>
        public List<EndpointDes> FindServer(string serverUrl)
        {
            List<EndpointDes> endpointDes = new List<EndpointDes>();
            foreach (var s in OpcDAClientHelper.GetOpcServer())
            {
                endpointDes.Add(new EndpointDes()
                {
                    ID = Guid.NewGuid().ToString(),
                    Description = s
                });
            }

            return endpointDes;
        }

        #region 回调函数

        private void _OpcDaGroup_AsyncCancelCompleteEvent(int cancelid)
        {
            //暂时什么都不干
        }

        private void _OpcDaGroup_AsyncWriteCompleteEvent(Item item)
        {
            TagItem tag = _TagItems[item.Name];
            if (tag == null) return;
            if (tag.CallBack == null) return;
            tag.Value = item.Value;
            tag.Quality = item.Quality.ToString();
            tag.Timestamp = DateTime.Parse(item.Timesnamp.ToString());
            tag.CBType = TagItem.CallBackType.AsyncWrite;
            tag.CallBack(tag);
        }

        private void _OpcDaGroup_AsyncReadCompleteEvent(Item item)
        {
            TagItem tag = _TagItems[item.Name];
            if (tag == null) return;
            if (tag.CallBack == null) return;
            tag.Value = item.Value;
            tag.Quality = item.Quality.ToString();
            tag.Timestamp = DateTime.Parse(item.Timesnamp.ToString());
            tag.CBType = TagItem.CallBackType.AsyncRead;
            tag.CallBack(tag);
        }

        private void _OpcDaGroup_DataChangeEvent(Item item)
        {
            TagItem tag = _TagItems[item.Name];
            if (tag == null) return;
            if (tag.CallBack == null) return;
            tag.Value = item.Value;
            tag.Quality = item.Quality.ToString();
            tag.Timestamp = DateTime.Parse(item.Timesnamp.ToString());
            tag.CBType = TagItem.CallBackType.Subscription;
            tag.CallBack(tag);
        }

        #endregion

    }
}
