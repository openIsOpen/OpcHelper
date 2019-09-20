using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPCAutomation;

namespace OpcDaHelper
{
    public class Group
    {
        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public bool IsSubscribed        
        {
            get => _IsSubscribed;
            set => _IsSubscribed = value;
        }

        [DefaultValue(1024)]
        public int LocaleId { get; set; }

        public int TimeBias { get; set; }

        public float DeadBand { get; set; }

        public int UpdateRate { get; set; }

        private int ClientHandle = 0;

        public string Channel { get; set; }

        public string Device { get; set; }

        public string Name { get; set; }

        private OPCGroup _Instance;
        public OPCGroup Instance
        {
            get { return _Instance; }
            set
            {
                _Instance = value;
                _Instance.IsActive = this.IsActive;
                _Instance.IsSubscribed = IsSubscribed;
                _Instance.LocaleID = LocaleId;
                _Instance.TimeBias = TimeBias;
                _Instance.DeadBand = DeadBand;
                _Instance.UpdateRate = UpdateRate;
                _Instance.AsyncCancelComplete += _Instance_AsyncCancelComplete;
                _Instance.AsyncReadComplete += _Instance_AsyncReadComplete;
                _Instance.AsyncWriteComplete += _Instance_AsyncWriteComplete;
                _Instance.DataChange += _Instance_DataChange;
            }
        }

        public delegate void DataChange(Item item);
        public delegate void AsyncWriteComplete(Item item);
        public delegate void AsyncReadComplete(Item item);
        public delegate void AsyncCancelComplete(int cancelid);
        public event DataChange DataChangeEvent = null;
        public event AsyncWriteComplete AsyncWriteCompleteEvent = null;
        public event AsyncReadComplete AsyncReadCompleteEvent = null;
        public event AsyncCancelComplete AsyncCancelCompleteEvent = null;

        /// <inheritdoc />
        public Group()
        {
        }

        public Group(string channel, string devce, string name)
        {
            Channel = channel;
            Device = devce;
            Name = name;
        }

        public string GetGroupId()
        {
            if (string.IsNullOrEmpty(Name))
                return string.Format("{0}.{1}", Channel, Device);
            return string.Format("{0}.{1}.{2}", Channel, Device, Name);
        }

        private void _Instance_DataChange(int transactionid, int numitems, ref Array clienthandles, ref Array itemvalues, ref Array qualities, ref Array timestamps)
        {
            for (int i = 1; i <= numitems; i++)
            {
                int tmpClientHandle = (int)clienthandles.GetValue(i);
                object tmpItemValue = itemvalues.GetValue(i);
                int tmpQuality = (int)qualities.GetValue(i);
                object tmpTimestamps = timestamps.GetValue(i);
                if (DataChangeEvent != null)
                {
                    Item tmpItem = Items[tmpClientHandle];
                    tmpItem.Type = tmpItem.Instance.CanonicalDataType.ToString();
                    tmpItem.Value = tmpItemValue;
                    tmpItem.Quality = tmpQuality;
                    tmpItem.Timesnamp = tmpTimestamps;                   
                    DataChangeEvent(tmpItem);
                }
            }
        }


        private void _Instance_AsyncWriteComplete(int transactionid, int numitems, ref Array clienthandles, ref Array errors)
        {
            for (int i = 1; i <= numitems; i++)
            {
                int tmpClientHandle = (int)clienthandles.GetValue(i);
                int tmpError = (int)errors.GetValue(i);
                if (AsyncWriteCompleteEvent != null)
                {
                    Item tmpItem = Items[tmpClientHandle];
                    tmpItem.Error = tmpError;
                    AsyncWriteCompleteEvent(tmpItem);
                }
            }
        }



        private void _Instance_AsyncReadComplete(int transactionid, int numitems, ref Array clienthandles, ref Array itemvalues, ref Array qualities, ref Array timestamps, ref Array errors)
        {
            for (int i = 1; i <= numitems; i++)
            {
                int tmpClientHandle = (int)clienthandles.GetValue(i);
                object tmpItemValue = itemvalues.GetValue(i);
                int tmpQuality = (int)qualities.GetValue(i);
                object tmpTimestamps = timestamps.GetValue(i);
                if (AsyncReadCompleteEvent != null)
                {
                    Item tmpItem = Items[tmpClientHandle];
                    tmpItem.Value = tmpItemValue;
                    tmpItem.Quality = tmpQuality;
                    tmpItem.Timesnamp = tmpTimestamps;
                    AsyncReadCompleteEvent(tmpItem);
                }
            }
        }


        private void _Instance_AsyncCancelComplete(int cancelid)
        {
            if (AsyncCancelCompleteEvent != null)
            {
                AsyncCancelCompleteEvent(cancelid);
            }
        }


        private Dictionary<int, Item> _Items = new Dictionary<int, Item>();
        private Dictionary<string, Item> _ItemsEx = new Dictionary<string, Item>();
        private bool _IsActive = true;
        private bool _IsSubscribed = true;

        public Dictionary<int, Item> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        public Dictionary<string, Item> ItemsEx
        {
            get => _ItemsEx;
            set => _ItemsEx = value;
        }

        public bool AddItem(Item item)
        {
            item.Group = this;
            OPCItem tmpItem = this.Instance.OPCItems.AddItem(item.Name, ClientHandle++);
            item.ServerHandle = tmpItem.ServerHandle;
            item.Instance = tmpItem;
            Items.Add(tmpItem.ClientHandle, item);
            ItemsEx.Add(item.Name,item);
            return true;
        }

        public List<Item> AddItem(List<Item> items)
        {
            if (items.Count < 1) return items;
            List<Item> result = new List<Item>();
            int numItems = items.Count;
            Array itemId = new string[numItems + 1];
            Array clientHandle = new int[numItems + 1];
            Array serverHandle = null;
            Array errors = null;
            for (int i = 1; i <= numItems; i++)
            {
                items[i - 1].Group = this;
                itemId.SetValue(items[i - 1].Name, i);
                clientHandle.SetValue(ClientHandle++, i);
            }

            Instance.OPCItems.AddItems(numItems, ref itemId, ref clientHandle, out serverHandle, out errors);

            for (int i = 1; i <= numItems; i++)
            {
                if ((int)errors.GetValue(i) == 0)
                {
                    // OPCItem tmpItem = Instance.OPCItems.Item((int)clientHandle.GetValue(i));
                    //OPCItem tmpItem = Instance.OPCItems.GetOPCItem((int)serverHandle.GetValue(i));
                    int tmpServerHandle = (int)serverHandle.GetValue(i);
                    items[i - 1].ServerHandle = tmpServerHandle;
                    //2019.09.19
                    items[i - 1].Instance = items[i - 1].Group.Instance.OPCItems.GetOPCItem(tmpServerHandle);
                    Items.Add((int)clientHandle.GetValue(i),
                        items[i - 1]);
                    ItemsEx.Add(items[i - 1].Name, items[i - 1]);
                }
                else
                {
                    ItemsEx.Add(items[i - 1].Name, items[i - 1]);
                    result.Add(items[i - 1]);
                }
            }
            Instance.IsActive = true;

            return result;
        }

        public List<Item> RemoveItem(Item item)
        {
            return RemoveItem(new List<Item>() { item });
        }

        public List<Item> RemoveItem(List<Item> items)
        {
            List<Item> result = new List<Item>();
            int itemNums = items.Count;
            List<int> clientHandle = new List<int>();
            Array serverHandle = new int[itemNums + 1];
            Array errors = null;
            for (int i = 1; i <= itemNums; i++)
            {
                serverHandle.SetValue(items[i - 1].ServerHandle, i);
                clientHandle.Add(Instance.OPCItems.GetOPCItem(items[i - 1].ServerHandle).ClientHandle);
            }
            Instance.OPCItems.Remove(itemNums, ref serverHandle, out errors);
            for (int i = 1; i <= itemNums; i++)
            {
                if ((int)errors.GetValue(i) == 0)
                {
                    Items.Remove(clientHandle[i - 1]);
                    ItemsEx.Remove(Items[clientHandle[i-1]].Name);
                    items[i - 1].ServerHandle = 0;
                }
                else
                {
                    result.Add(items[i - 1]);
                }
            }

            return result;
        }

        public List<Item> Write(List<Item> items)
        {
            List<Item> result = new List<Item>();
            int itemNum = items.Count;
            Array serverHandle = new int[itemNum + 1];
            Array value = new object[itemNum + 1];
            Array errors;
            for (int i = 1; i <= itemNum; i++)
            {
                serverHandle.SetValue(items[i - 1].ServerHandle, i);
                value.SetValue(items[i - 1].ObjValue, i);
            }

            Instance.SyncWrite(itemNum, ref serverHandle, ref value, out errors);
            for (int i = 1; i <= itemNum; i++)
            {
                if ((int)errors.GetValue(i) != 0)
                {
                    result.Add(items[i - 1]);
                }
            }

            return result;
        }

        public List<Item> Read(List<Item> items)
        {
            List<Item> result = new List<Item>();
            int itemNum = items.Count;
            Array serverHandle = new int[itemNum + 1];
            Array value = null;
            Array errors = null;
            object qualities = null;
            object timeSnamps = null;
            for (int i = 1; i <= itemNum; i++)
            {
                serverHandle.SetValue(items[i - 1].ServerHandle, i);
            }

            Instance.SyncRead((short)OPCDataSource.OPCDevice, itemNum, ref serverHandle, out value, out errors, out qualities,
                out timeSnamps);
            for (int i = 1; i <= itemNum; i++)
            {
                items[i - 1].Value = value.GetValue(i);
                items[i - 1].Error = (int)errors.GetValue(i);
                items[i - 1].Quality = (short)(qualities as Array).GetValue(i);
                items[i - 1].Timesnamp = (timeSnamps as Array).GetValue(i);
                if (items[i - 1].Error != 0)
                {
                    result.Add(items[i - 1]);
                }
            }

            return result;
        }

        public List<Item> AsyncWrite(List<Item> items, int transactionid, out int cancleid)
        {
            List<Item> result = new List<Item>();
            int itemNum = items.Count;
            Array serverHandle = new int[itemNum + 1];
            Array value = new object[itemNum + 1];
            Array errors;
            int tmpCancleid;
            for (int i = 1; i <= itemNum; i++)
            {
                serverHandle.SetValue(items[i - 1].ServerHandle, i);
                value.SetValue(items[i - 1].ObjValue, i);
            }

            Instance.AsyncWrite(itemNum, ref serverHandle, ref value, out errors, transactionid, out cancleid);

            for (int i = 1; i <= itemNum; i++)
            {
                if (errors.GetValue(i) != null)
                {
                    result.Add(items[i - 1]);
                }
            }

            return result;
        }

        public List<Item> AsyncRead(List<Item> items, int transactionid, out int cancleid)
        {
            List<Item> result = new List<Item>();
            int itemNum = items.Count;
            Array serverHandle = new int[itemNum + 1];
            Array value = null;
            Array errors = null;
            object qualities = null;
            object timeSnamps = null;
            for (int i = 1; i <= itemNum; i++)
            {
                serverHandle.SetValue(items[i - 1].ServerHandle, i);
            }

            Instance.AsyncRead(itemNum, ref serverHandle, out errors, transactionid, out cancleid);

            for (int i = 1; i <= itemNum; i++)
            {
                if (errors.GetValue(i) != null)
                {
                    result.Add(items[i - 1]);
                }
            }

            return result;
        }

        public void AsyncCancle(int cancleid)
        {
            Instance.AsyncCancel(cancleid);
        }

        public void AsyncRefresh(int transactionid, out int cancleid)
        {
            Instance.AsyncRefresh((short)OPCDataSource.OPCDevice, transactionid, out cancleid);
        }

    }
}
