   M S B A M L   `   `   `  ����   WGPM    WGPM.R.UI.PlanEdit   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .��� -     �Name$  GridMain��$
��1024��$��Center=�.+��� -    ��Name$ 
CanvasMain��.+�&   System.Windows.Controls.DataGrid  -     Name$ dgPlan��$	��700��
 �� Top$ 0��$	��768�� �� Left$ 0��   
IsReadOnly . $
��Bold"�$��16(�   AutoGenerateColumns .     Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   CanUserSort .  	   Width5  /System.Windows.Controls.DataGridLengthConverter$
	 auto 
   Header$
 时段��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *	StrPeriod+   .  $
	 auto $
 班组�� �� *
StrGroup+   .  $
	 auto $
 炉号�� �� *
StrRoomNum+   .  $
	 auto $
 计划推焦时间�� �� *StrPushTime+�� $
��Wrap=�$��	日期：�� �� TextAlignment$ Center=�$
��Bold"�$��16(�$	 789��$ 27.441��(  "System.Windows.Controls.DatePicker  -     Name$ dpDate��$��28��$��16(�   SelectedDateFormat$ Short=�$	 841��$	��164��$ 24��$
��Bold"�
���   ��� $
��Wrap=�$��	计划：��$ Center=�$
��Bold"�$��16(�$	 789��$ 171.441���� -    ��Name$ cboPlanArea��$��28��$	 841��$	��164��$	 168��$��16(�$
��Bold"�.@��� $��全部计划��$
��Left=� �� 
IsSelected . $��Center=��� $��
1-2#炉区��$
��Left=�$��Center=��� $��
3-4#炉区��$
��Left=�$��Center=��� $
��Wrap=�$��	班组：��$ Center=�$
��Bold"�$��16(�$	 789��$ 123.441���� -   $ cboGroup��$��28��$	��164��$	 841��$	 120��$��16(�$
��Bold"�.@��� $��全部班组��$
��Left=�$��Center=� . �� $��甲班��$
��Left=�$��Center=��� $��乙班��$
��Left=�$��Center=��� $��丙班��$
��Left=�$��Center=��� $��丁班��$
��Left=�$��Center=��� $	 789��$ 75.441��$��16(�$
��Bold"�$��	时段：��$ Center=�$
��Wrap=��� -   $ 	cboPeriod��$	 841��$ 72��$��28��$	��164��$��16(�$
��Bold"�$
��Left=�$��Center=�.@��� $��全部时段��$
��Left=�$��Center=� . �� $��白班��$
��Left=�$��Center=��� $��夜班��$
��Left=�$��Center=��� $	 789��$
��Wrap=�$ 219.441��$��16(�$
��Bold"�$ Center=���  � Language$ zh-cn�$�计划炭化室号：���� -    ��Name$ txtPlanRoom��$��28��$	 901�� �� TextWrapping$
 Wrap=�$	��000��$	 216��$��88��$��16(�$
��Bold"� �� TextAlignment$ Center=�{�.  �� $	 789��$
��Wrap=�$ 267.441��$��16(�$
��Bold"�$ Center=��� $ zh-cn�$�计划推焦时间：���� -	   $ txtPlanPushTime��$��28��$	 901��$
 Wrap=�$��00:00��$	 264��$��88��$��16(�$
��Bold"�$ Center=�
 � Tag$ 1��{�.  �� $	 789��$
��Wrap=�$ 315.441��$��16(�$
��Bold"�$ Center=��� $ zh-cn�$�计划结焦时间：���� -
   $ txtPlanBurnTime��$��28��$	 901��$
 Wrap=�$��00:00��$	 312��$��88��$��16(�$
��Bold"�$ Center=�$ 2��{�.  �� -    ��Name$ CheckBoxPlanBurnTime��$��15��$
 1000��$ 318.5��$��15�� d� 	IsChecked$
 TrueY��� $	 789��$
��Wrap=�$ 363.441��$��16(�$
��Bold"�$ Center=��� $ zh-cn�$�规定结焦时间：���� -   $ txtStandardBurnTime��$��28��$	 901��$
 Wrap=�$��00:00��$	 360��$��88��$��16(�$
��Bold"�$ Center=�$ 3��{�.  �� -   $ CheckBoxStandardBurnTime��$��15��$
 1000��$ 366.5��$��15���� -    ��Name$ btnEdit��$��编辑计划��$��32��$	 789��$	 408��$	��100��$��18.667(�$
��Bold"��� -   $ 	btnEnsure��$��确定��$	 906��$	 408��$	��100��$��18.667(�$
��Bold"�$��32��{�.  �� -   $ btnSave��$��保存计划��$��32��$	 906��$	 460��$	��100��$��18.667(�$
��Bold"��� -   $ btnPrint��$��打印时段计划��$��32��$	 789��$	 512��$	��217��$��18.667(�$
��Bold"��� -   $ 	btnDelete��$	 789��$	 460��$��删除计划��$��18.667(�$
��Bold"�$��32��$	��100���� -   $ TextBoxPlanUpdateT��$��32��$	 789��$
 Wrap=�$	 564��$��88��$��18.667(�$
��Bold"�$ Center=�$	��000���� -   $ 	btnUpdate��$	 881��$	 564��$��更新推焦计划��$��18.667(�$
��Bold"�$��32��$	��124��$ 0��