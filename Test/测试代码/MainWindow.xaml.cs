using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zlw.OpcClient;

namespace Test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum ConType
        {
            UA,
            DA
        }

        private ConType _SelectedType;
        private OpcClient _OpcClient = null;
        private OpcServer _OpcServer = null;
        private EndpointDes _SelectedEndpointDes = null;
        private List<string> _ServerInfo = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
        }

        private void OpcUa_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == OpcUa)
            {
                _OpcClient = new OpcUaClient();
                _OpcServer = new OpcUaServer();
            }else if (radioButton == OpcDa)
            {
                _OpcClient = new OpcDaClient();
                _OpcServer = new OpcDaServer();
            }
            else
            {

            }
        }

        private void BtnBrowseServer_Click(object sender, RoutedEventArgs e)
        {
            string url = TbUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Url为空,请输入正确的Url");
                return;
            }
            List<EndpointDes> endpointDeses = _OpcClient.FindServer(url);
            LbServer.ItemsSource = endpointDeses;
        }

        private void LbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1) return;
            _SelectedEndpointDes = (EndpointDes)e.AddedItems[0];
            _OpcServer.EndpointDesCription = _SelectedEndpointDes;
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Content.ToString() == "连接")
            {
                if (_SelectedEndpointDes == null)
                {
                    Msg("请选择要连接的终结点");
                    return;
                }

                if (sender == BtnConnect)
                {
                    if (_OpcClient.Connect(_OpcServer))
                    {
                        Msg("连接成功！");
                        btn.Content = "断开连接";
                    }
                    else
                    {
                        Msg("连接失败!");
                    }
                }
                else
                {
                    OpcUaServer uaServer = _OpcServer as OpcUaServer;
                    if (RbNoneLogin.IsChecked == true)
                    {
                        //匿名登录

                        uaServer.UserAuth = false;
                    }
                    else
                    {
                        //使用用户名密码登录
                        uaServer.UserAuth = true;
                        uaServer.UserName = TbUser.Text.Trim();
                        uaServer.Password = TbPass.Password;
                    }

                    if (_OpcClient.Connect(uaServer))
                    {
                        btn.Content = "断开连接";
                        Msg("连接成功！");
                    }
                    else
                    {
                        Msg("连接失败!");
                    }
                }
            }

            else if (btn.Content.ToString() == "断开连接")
            {
                _OpcClient.Disconnect();
                btn.Content = "连接";
                ClearUi();
            }
            else
            {
                Msg("未知情况");
            }
        }

        private void Msg(string msg)
        {
            MessageBox.Show(msg);
        }

        List<TagInfoModal> tagModals;
        private void BtnAddAll_Click(object sender, RoutedEventArgs e)
        {
            if (!_OpcClient.IsConnected) {
                Msg("请先连接服务器");
                return;
            }
            tagModals = new List<TagInfoModal>();
            List<TagItem> items = _OpcClient.GetUserItems();
            foreach (var i in items)
            {
                i.CallBack = TagCallBack;
                tagModals.Add(new TagInfoModal()
                {
                    ID= i.Id,
                    ServerID = i.ServerId,
                    TagName= i.Name,
                    Value = i.Value?.ToString(),
                    Quality = i.Quality
                    //Timesnamp = i.Timestamp?.ToString("yyyy-mm-dd HH:mm:ss:fff")
                });
            }

            DgTagInfo.ItemsSource = tagModals;
            _OpcClient.AddSubscription(items);
        }

        private void TagCallBack(TagItem obj)
        {
            if (obj.CBType == TagItem.CallBackType.AsyncRead)
            {
                //异步读回调
                Msg("异步读成功");
                LbReadValue.Dispatcher.Invoke(new Action(() => { LbReadValue.Content = obj.Value; }));
            }else if (obj.CBType == TagItem.CallBackType.AsyncWrite)
            {
                //异步写完成回调
                Msg("异步写成功");
            }
            else
            {
                //订阅回调
                foreach (var item in tagModals)
                {
                    if (item.ServerID == obj.ServerId)
                    {
                        item.Value = obj.Value.ToString();
                        item.Quality = obj.Quality;
                        item.Timesnamp = obj.Timestamp.ToString("yyyy-mm-dd HH:mm:ss:fff");
                    }
                }
            }
        }

        private void ClearUi()
        {
            DgTagInfo.Items.Clear();
        }

        private void BtnOperate_Click(object sender, RoutedEventArgs e)
        {
            if (!_OpcClient.IsConnected)
            {
                Msg("请先连接服务器");
                return;
            }

            string serverId = TbTagServerId.Text.Trim();
            string writeValue = LbWriteValue.Text.Trim();
            if (string.IsNullOrEmpty(serverId))
            {
                Msg("请输入Tag ServerID");
                TbTagServerId.Focus();
                return;
            }

            Button btn = sender as Button;
            TagItem tagItem = new TagItem()
            {
                ServerId = serverId,
                ValueToWrite = writeValue,
                NameSpace = "2",
                CallBack = this.TagCallBack
            };
            switch (btn?.Tag?.ToString())
            {
                case "1":
                    //同步读
                    TagItem item = _OpcClient.Read(tagItem);
                    LbReadValue.Content = item.Value;
                    break;
                case "2":
                    //异步读
                    _OpcClient.AsyncRead(tagItem);
                    break;
                case "3":
                    //同步写
                    _OpcClient.Write(tagItem);
                    break;
                case "4":
                    //异步写
                    _OpcClient.AsyncWrite(tagItem);
                    break;
                case "5":
                    //添加订阅
                    _OpcClient.AddSubscription(tagItem);
                    break;
            }
        }

        private void DgTagInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
                return;
            TagInfoModal tag = e.AddedItems[0] as TagInfoModal;
            TbTagServerId.Text = tag.ServerID;
        }

        [DllImport("kernel32.dll")]
        public static extern void AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern void FreeConsole();
    }
}
