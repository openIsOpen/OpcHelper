/*
 * 版本V1.0.0
 * 说明：
 *1.在本工具中，所有DataSource全部默认为device
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OpcDaHelper;
using OPCAutomation;

namespace OPCDAAUTO
{
    /// <summary>
    /// 获取服务端条目时,条目类型
    /// </summary>
    public enum FildType
    {
        All,
        User,
        Sys
    }

    /// <summary>
    /// 获取服务端条目时,表示是变量还是导航元素
    /// </summary>
    public enum ItemType
    {
        Tag,
        Nav,
    }

    public class OpcDAClientHelper
    {
        private OPCServer Server = null;
        private OPCBrowser Browser = null;
        private OPCGroups OPCGroupsIns = null;
        private Dictionary<string, Group> _groups = new Dictionary<string, Group>();

        public Dictionary<string, Group> Groups
        {
            get { return _groups; }
        }

        public string Node { get; set; }

        public string Prog { get; set; }

        /// <inheritdoc />
        public OpcDAClientHelper(string node, string prog)
        {
            Node = node;
            Prog = prog;
        }

        public bool Connect()
        {
            Server = new OPCServer();
            Server.Connect(Prog, Node);
            Browser = Server.CreateBrowser();
            OPCGroupsIns = Server.OPCGroups;
            //默认属性这里先填写，以后可能会改写到配置文件中。
            //OPCGroupsIns.DefaultGroupDeadband = 0;                  //组默认死区
            //OPCGroupsIns.DefaultGroupIsActive = true;                 //组默认激活状态
            //OPCGroupsIns.DefaultGroupLocaleID = 0;                    //组默认区域ID*/
            //OPCGroupsIns.DefaultGroupTimeBias = 0;                    //组默认时间基*/
            //OPCGroupsIns.DefaultGroupUpdateRate = 10;              //组默认更新频率  ms
            return true;
        }

        public bool DisConnect()
        {
            if (Server != null)
                Server.Disconnect();
            return true;
        }


        /// <inheritdoc />
        public OpcDAClientHelper()
        {
            Node = "127.0.0.1";
            Prog = "Kepware.KEPServerEX.V6";
        }

        public Group AddGroup(Group group)
        {
            OPCGroup tmpGroup = OPCGroupsIns.Add(group.GetGroupId());
            Groups.Add(group.GetGroupId(), group);
            group.Instance = tmpGroup;
            return group;
        }

        public bool RemoveGroup(Group group)
        {
            OPCGroupsIns.Remove(group.Instance.ServerHandle);
            Groups.Remove(group.GetGroupId());
            return true;
        }

        public bool RmoveAllGroup()
        {
            OPCGroupsIns.RemoveAll();
            Groups.Clear();
            return true;
        }

        public BLModal Filds = new BLModal();

        public BLModal GetAllNodeTags()
        {
            Filds.Children.Clear();
            GetItem("", Filds,FildType.All);
            return Filds;
        }
        public BLModal GetSysNodeTags()
        {
            Filds.Children.Clear();
            GetItem("", Filds, FildType.Sys);
            return Filds;
        }
        public BLModal GetUserNodeTags()
        {
            Filds.Children.Clear();
            GetItem("", Filds, FildType.User);
            return Filds;
        }

        public BLModal GetUserNodeTags(string node,FildType type)
        {
            GetItem(node, Filds,type);
            return Filds;
        }

        private void GetItem(string node, BLModal bl, FildType type)
        {
            List<string> branch = this.GetBranch(node);
            List<string> leafs = GetLeafs(node);

            //具体变量
            for (int i = 0; i < leafs.Count; i++)
            {
                if (type == FildType.User)
                {
                    if (!leafs[i].StartsWith("_"))
                    {
                        BLModal tmpBl = new BLModal() { Name = leafs[i], Parent = bl,ItemType = ItemType.Tag};
                        bl.Children.Add(tmpBl);
                    }
                }
                else if (type == FildType.Sys)
                {
                    if (leafs[i].StartsWith("_"))
                    {
                        BLModal tmpBl = new BLModal() { Name = leafs[i], Parent = bl ,ItemType = ItemType.Tag};
                        bl.Children.Add(tmpBl);
                    }
                }
                else if (type == FildType.All)
                {
                    BLModal tmpBl = new BLModal() { Name = leafs[i], Parent = bl ,ItemType = ItemType.Tag};
                    bl.Children.Add(tmpBl);
                }
            }

            //导航元素
            for (int i = 0; i < branch.Count; i++)
            {
                if (type == FildType.User)
                {
                    if (!branch[i].StartsWith("_"))
                    {
                        BLModal tmpBl = new BLModal() { Name = branch[i], Parent = bl ,ItemType = ItemType.Nav};
                        bl.Children.Add(tmpBl);
                        GetItem(tmpBl.GetID(), tmpBl,FildType.User);
                    }
                }else if (type == FildType.Sys)
                {
                    if (branch[i].StartsWith("_"))
                    {
                        BLModal tmpBl = new BLModal() { Name = branch[i], Parent = bl ,ItemType = ItemType.Nav};
                        bl.Children.Add(tmpBl);
                        GetItem(tmpBl.GetID(), tmpBl, FildType.Sys);
                    }
                }else if (type == FildType.All)
                {
                    BLModal tmpBl = new BLModal() { Name = branch[i], Parent = bl ,ItemType = ItemType.Nav};
                    bl.Children.Add(tmpBl);
                    GetItem(tmpBl.GetID(), tmpBl, FildType.All);
                }
            }

        }

        public List<string> GetBranch(string position)
        {
            List<string> branchs = new List<string>();
            string[] desName = position.ToString().Split('.');
            Browser.MoveTo(desName);
            Browser.ShowBranches();
            IEnumerator enumerator = Browser.GetEnumerator();
            //树
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null)
                {
                    branchs.Add(enumerator.Current.ToString());
                }
            }

            Browser.MoveToRoot();
            return branchs;
        }

        public List<string> GetLeafs(string position)
        {
            List<string> leafs = new List<string>();
            string[] desName = position.ToString().Split('.');
            Browser.MoveTo(desName);
            Browser.ShowLeafs();
            IEnumerator enumerator = Browser.GetEnumerator();
            //树
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null)
                {
                    leafs.Add(enumerator.Current.ToString());
                }
            }

            Browser.MoveToRoot();
            return leafs;
        }

        public static string[] GetOpcServer(string nodeId = "127.0.0.1")
        {
            string[] tmpStr;
            OPCServer server = new OPCServer();
            object servers = server.GetOPCServers(nodeId);
            //object servers = server.GetOPCServers();
            Array serversStr = (Array)servers;
            tmpStr = new string[serversStr.Length];
            for (int i = 0; i < serversStr.Length; i++)
            {
                tmpStr[i] = (string)serversStr.GetValue(i + 1);
            }

            return tmpStr;
        }
    }
}
