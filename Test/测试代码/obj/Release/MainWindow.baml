   M S B A M L   `   `   `  ���� ?  ;Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:TestTest      Test.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     $localclr-namespace:Test       � Title$  测试程序��$	��450��$	��800��.���  � RowDefinitions �� $��30 ��� $��80 ��� $	��230 ��� .+���  �� Orientation$ 
Horizontal=�.+��� .��选择OPC类型:	� -   $��OpcUa��$��5q� 	� 	GroupName$ opcType��.��OPC UA	� -   $��OpcDa��$��5q�$ opcType�� d� 	IsChecked$
 TrueY�.��OPC DA�� $��15,3,5,3q�.��URL:�� -   $��TbUrl��$	��400��$��5q�.��opc.tcp://zhangliwen-PC:49320�� -   $��BtnBrowseServer��$��5q�.��浏览�� -   $��
BtnConnect��$��5q�$��50��y���  �� ElementName$ OpcDa�� �� Path$ 	IsChecked�   BoolToVisibilityConverter �� 	Converter# [  .��连接� $��1��.+��� -   $��LbServer�� �� DisplayMemberPath$ Description��� $��2��	 � ColumnDefinitions	 �� $��4* ��� $��* �.+��  �� $��30 ��� $��25 ��� .+��� -   $��	BtnAddAll��.��添加服务端所有变量�� $��1��$ 
Horizontal=�.+��� .��FDTag ServerID(格式为:通道.设备.文件夹.文件夹.变量名):�� -   $��TbTagServerId��$��3q�� $��2��	 �� �� �� .+��� $��	读变量��.���� .+��� -	   $��
BtnOperate��

 � Tag$
 1��.��	同步读�� -
   $
 2��.��	异步读�� -   $��LbReadValue���� $��1��$��	写变量��.���� .+��� -   $
 3��.��	同步写�� -   $
 4��.��	异步写�� -   $��LbWriteValue���� $��2��$��订阅��.���� -   $
 5��.��添加订阅� $��1��$��5q� � ShowGridLines . y��� $ OpcUa��$ 	IsChecked�# [  .+��� $��用户信息��.���� $ Vertical=�.+�	� -   $��RbNoneLogin��$ RbLoginStyle��$
 TrueY�.��匿名登录	� -   $��RbUserLogin��$ RbLoginStyle��.��用户名密码登录�� $ 
Horizontal=�$��5q�.+��� .��
用户名:�� -   $��TbUser��$	��100��{��� $ RbUserLogin��$ 	IsChecked��� $��8q�$ 
Horizontal=�.+��� .��	密 码 :G� -   $��TbPass��$	��100��{��� $ RbUserLogin��$ 	IsChecked��� -   $��BtnUaConnect��$��10q�.��连接� $��3��.+�&   System.Windows.Controls.DataGrid  -   $��	DgTagInfo��   ColumnWidth5  /System.Windows.Controls.DataGridLengthConverter$ *    AutoGenerateColumns .     CanUserAddRows .     Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ ID��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� $ ID�  $ TagName�� �� $ TagName�  $ ServerId�� �� $ ServerID�  $ Value�� �� $ Value�  $ Quality�� �� $ Quality�  $ 
Timessnamp�� �� $ 	Timesnamp�