using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPCAutomation;


namespace OpcDaHelper
{
    public class Item
    {
        private List<object> _Extension = new List<object>();

        private OPCItem _Instance;
        public OPCItem Instance
        {
            get { return _Instance; }
            set
            {
                _Instance = value;
                //_Instance.IsActive = IsActive;
            }
        }

        public string Id { get; set; }

        public int IsLoopRecord { get; set; }

        public int ServerHandle { get; set; }
        public Group Group { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public object Value { get; set; }

        public object ObjValue { get; set; }

        public bool IsActive { get; set; }

        public object Timesnamp { get; set; }

        public int Quality { get; set; }

        public string OtherName { get; set; }

        public string IsSend { get; set; }

        public List<object> Extension
        {
            get => _Extension;
            set => _Extension = value;
        }

        public object Extension1 { get; set; }

        public object Extension2 { get; set; }

        public object Extentsion3 { get; set; }

        public int Error
        { get; set; }

        /// <inheritdoc />
        public Item(string name)
        {
            Name = name;
        }

        public Item()
        {

        }

        public string GetItemId()
        {
            return Group.GetGroupId() + "." + Name;
        }

        public void Write_NoRes()
        {
            Group.Instance.OPCItems.GetOPCItem(ServerHandle).Write(ObjValue);
            // Instance.Write(this.ObjValue);
        }

        public Item Read_NoRes()
        {
            object tmpValue;
            object tmpQuality;
            object tmpTimesnamp;
            Group.Instance.OPCItems.GetOPCItem(ServerHandle).Read((short)OPCDataSource.OPCDevice, out tmpValue, out tmpQuality, out tmpTimesnamp);
            //            Instance.Read((short)OPCDataSource.OPCDevice, out tmpValue, out tmpQuality, out tmpTimesnamp);
            this.Value = tmpValue;
            this.Quality = Convert.ToInt32(tmpQuality);
            this.Timesnamp = tmpTimesnamp;
            return this;
        }

        public bool Write()
        {
            if (Group.Write(new List<Item> { this }).Count == 0)
                return true;
            else
                return false;
        }

        public bool Read()
        {
            if (Group.Read(new List<Item> { this }).Count == 0)
                return true;
            else
                return false;
        }
    }
}
