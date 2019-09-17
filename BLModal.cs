using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPCDAAUTO;

namespace OpcDaHelper
{
    public class BLModal
    {
        private List<BLModal> _Children = new List<BLModal>();
        public string Name { get; set; }

        public string DataType { get; set; }

        public BLModal Parent { get; set; }

        public List<BLModal> Children
        {
            get => _Children;
            set => _Children = value;
        }

        public ItemType ItemType { get; set; }

        public string GetID()
        {
            string result = string.Empty;
            result = Parent == null ? Name:Parent?.GetID() + "." + Name;
            result = result?.Trim('.');
            return result;
        }
    }
}
