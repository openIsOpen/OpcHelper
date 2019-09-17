using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;

namespace Zlw.OpcClient
{
    public class NavModal
    {
        private List<NavModal> _Children = new List<NavModal>();
        public string Id { get; set; }

        public Node Node { get; set; }

        public string Description { get; set; }

        public List<NavModal> Children
        {
            get => _Children;
            set => _Children = value;
        }

        public NavModal Parent { get; set; }
    }
}
