   M S B A M L   `   `   `  ����?  ;WGPM, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:WGPMWGPM  5   	       
WGPM.LogIn   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   $localclr-namespace:WGPM     5   	     � Title$  LogIn��5   	   $	��360��6   $	��400��6$   $��20(�60    � WindowState$ Normal=�6>    � WindowStartupLocation$ CenterScreen=�6S    � 
ResizeMode$ NoResize=�5	   	   .��5
      � ��5   
   �� 5       �� ImageSource$0 */WGPM;component/R/image/Background/Sky.jpg��6    � ColumnDefinitions 5   
   �� 5      $
��129* �6   �� 5      $
��163* �6    � RowDefinitions 5   
   �� 5      $	��81* �6   �� 5      $	��41* �6   �� 5      $
��147* �6   .+�5   
   �� -   5         ��Name$ label��5      $��18(�6   $��选择炉区：��6-   $��Right=�6=   $��37��6Y   $��Center=�6e   $	��129��6�   $��1��6�   ���	#FF18D3E6$��2q�6�   $��Center=�6�   $��Center=�6�   ��6�   Z� 6�   �� 5   
   -   5         ��Name$ cboCokeArea��5      $��1��6(   $��Center=�68   $��37��6U   $��10,0,0,0q�6a   $��1��6s   $��Center=�6�   $	��125��6�   $��Center=�6�   $��Center=�6�   $��2q�6  ���	#FF1E198F��6�   Z� .@�5      �� $��1、2#炉区��6   	 �� 
IsSelected	 . 6,   �� 5      $��3、4#炉区��6   5      �� 5   
   -   5        
 ��Name$
 button��5      $��确定��6!   $��1��6.   $��Center=�6>   $��38��6[   $��	10,10,0,0q�6g   $��2��6z   $	��Top=�6�   $	��125��6�   $��1q�6�   
���   ����	#FF1437D3��6�   Z� 6�   �� 5   
   -   5         ��Name$ 	chkServer��5      $��服务器端��6&   $
��Left=�65   $��37��6P   $��	45,10,0,0q�6\   $��2��6o   $	��Top=�6|   $	��129��6�   $��18(�6�   ���	#FF18D3E6$��2q�6�   $y�Hidden=�6�   ��6�   Z� 6�   5      5       