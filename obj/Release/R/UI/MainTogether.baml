   M S B A M L   `   `   `  ����   WGPM    WGPM.R.UI.MainTogether   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     c _Microsoft.Expression.Drawing, Version=4.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35=ed4http://schemas.microsoft.com/expression/2010/drawing  .��� -     �Name$  
LayoutRoot������  �� EndPoint$ 0.5,1&� � MappingMode$ RelativeToBoundingBox=� �� 
StartPoint$ 0.5,0&�.���  � Color$ White�� � Offset$ 0.004Q�� $ 	#FF008787��$ 1Q�� $
 Cyan��$	 0.5Q�.+��� -    ��Name$ 
CanvasMain��$
��Left=�$	��Top=�.+��� 
�������$��2q�$	��165��$
��1024���� -    ��Name$ 
txtSysTime�� �� TextWrapping$
 Wrap=�$��00:00��$
��Bold"�"	 �� VerticalScrollBarVisibility$	 Disabled=�$��29.333(�$��STXinwei)���.  
 �� TextAlignment$
 Center=� J� IsHitTestVisible .   �� Left$ 170.661��
 �� Top$	 2.1��
���  ����Z� ��Z� �� -    ��Name$ TextBlockRoomPlanPush��$
��Left=�$��计划推焦炉号：��$��21.333(�$ 40.977��$ 41.472��$��STLiti)�
���E��$
��Bold"��� -   $ txtPushPlanRoomNum��$
 Wrap=�$	��000��$
��Bold"�$	 Disabled=�$��29.333(�$��STXinwei)���.  $
 Center=� .  $ 190.381��$ 34.572��
���  ����Z� ��Z� �� -   $ TextBlockTimePlanPush��$
��Wrap=�$��计划推焦时间：��$��21.333(�$ 40.977��$ 67.472��$��STLiti)�
���E��$
��Bold"��� -   $ txtPushPlanTime��$
 Wrap=�$��00:00��$	 Disabled=�
���  ��$��29.333(�$��STXinwei)���.  $
 Center=� .  $ 176.655��$ 60.572��$
��Bold"���Z� ��Z� �� -   $ TextBlockRoomPlanCoal��$��计划装煤炉号：��$��21.333(�$ 40.977��$ 99.472��$��STLiti)�
���E��$
��Bold"��� -	   $ txtStokingPlanRoomNum��$
 Wrap=�$	��000��$	��Top=�$
��Bold"�$	 Disabled=�
���  ��$��29.333(�$��STXinwei)���.  $
 Center=� .  $ 190.381��$ 92.572����Z� ��Z� �� -
   $ TextBlockTimePlanCoal��$��计划装煤时间：��$��21.333(�$ 40.977��$ 125.472��$��STLiti)�
���E��$
��Bold"��� -   $ txtStokingPlanTime��$
 Wrap=�$��00:00��$	��Top=�$
��Bold"�$	 Disabled=�
���  ��$��29.333(�$��STXinwei)���.  $
 Center=� .  $ 176.655��$ 118.572����Z� ��Z� �� -   $ TextBlockTimeSystem��$��系统时间：��$��21.333(�$ 62.737��$ 9��$��STLiti)�
���E��$
��Bold"�A� $��3��
���   �$	��156��$ 35.666������ $ 0.5,1&�$ 0.5,0&�.��� $ White��$ 1Q�� $ Black��$	 0.5Q�� $ White���� -   $ TextBoxTjcMark��$
 Wrap=�$��	推焦车��$
 Center=����	#FF323232
����  �$��
STFangsong)�
�������$��70�� .  ��.  $��18.667(�$	 318��$ 7��$��25��$
��Bold"�.�� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     	Direction$	 272Q�.���� -   $ TextBoxLjcMark��$
 Wrap=�$��	拦焦车��$
 Center=����	#FF323232
����  �$��
STFangsong)�
�������$��70�� .  ��.  $��18.667(�$	 318��$ 36��$��25��$
��Bold"�.��   $	 272Q�.���� -   $ TextBoxXjcMark��$
 Wrap=�$��	熄焦车��$
 Center=�
����  �$��
STFangsong)�
�������$��70�� .  ��.  $��18.667(�$	��Top=�$	 318��$ 65�����	#FF323232$��25��$
��Bold"�.��   $	 272Q�.���� -   $ TextBoxZmcMark��$
 Wrap=�$��	装煤车��$
 Center=����	#FF323232
����  �$��
STFangsong)�
�������$��70�� .  ��.  $��18.667(�$	��Top=�$	 318��$ 94��$��25��$
��Bold"�.��   $	 272Q�.���� -   $ TextBoxDycMark��$
 Wrap=�$��	导烟车��$
 Center=����	#FF323232
�������$
��Bold"�$��Arial)�
�������$��70�� .  ��.  $��14.667(�$	��Top=�$	 318��$	 123��$y�	Collapsed=�A� $��3��
���   �$	��156��$	 265������ $ 0.5,1&�$ 0.5,0&�.��� $ White��$ 1Q�� $ Black��$	 0.5Q�� $ White���� -    ��Name$ RectangleTjcDirection��$��25��$	��104��$	 453��$ 7��
���   �
����  �   $	 272Q�,  &Microsoft.Expression.Shapes.BlockArrow  -     Name$ tArrow01��
��� � �$��15��   Orientation$ Right=� � UseLayoutRounding .  $	��9.5��$	 457��$ 12��
�������  -   $ 	tArrow011��
��� � �$��15��$ Right=� .  $��4��$ 468.75��$ 12��
�������  -   $ tArrow02��
��� � �$��15��$ Right=� .  $	��9.5��$	 481��$ 12��
�������A� -    A�Name$ tReady��$��21��$��22��$	 494��$ 9��
����������� $ 0.5,1&�$ 0.5,0&�.��� $ 	#FF323232��$ 1Q�� $ 	#FFA3A3A3��� $ 	#FF161616��$	 0.3Q�  -   $ tArrow03��
��� � �$��15��$
 left=� .  $	��9.5�� J� RenderTransformOrigin$ 0.5,0.5&�$ 519.5��$ 12��
�������  -   $ tArrow04��
��� � �$��15��$
 left=� .  $��4��$ 0.5,0.5&�$ 537.326��$ 12��
�������  -   $ 	tArrow041��
��� � �$��15��$
 left=� .  $	��9.5��$ 0.5,0.5&�$ 543.412��$ 12��
��������� -   $ RectangleLjcDirection��$��25��$	��104��$	 453��$ 36��
���   �
����  �   $	 272Q�  -   $ lArrow01��
��� � �$��15��$ Right=� .  $	��9.5��$	 457��$ 41��
�������  -   $ 	lArrow011��
��� � �$��15��$ Right=� .  $��4��$ 468.75��$ 41��
�������  -   $ lArrow02��
��� � �$��15��$ Right=� .  $	��9.5��$	 481��$ 41��
�������A� -   $ lReady��$��21��$��22��$	 494��$ 38��
����������� $ 0.5,1&�$ 0.5,0&�.��� $ 	#FF00BE00��$ 1Q�� $ White��� $ 	#FF009600��$	 0.3Q�  -   $ lArrow03��
��� � �$��15��$
 Left=� .  $	��9.5��$ 0.5,0.5&�$ 519.5��$ 41��
�������  -    $ lArrow04��
��� � �$��15��$
 Left=� .  $��4��$ 0.5,0.5&�$ 537.326��$ 41��
�������  -!   $ 	lArrow041��
��� � �$��15��$
 Left=� .  $	��9.5��$ 0.5,0.5&�$ 543.412��$ 41��
��������� -"   $ RectangleXjcDirection��
����  �$	��104��$��25��$	 453��$ 65��
���   �   $	 272Q�  -#   $ xArrow01��
��� � �$ Right=� .  $	��9.5��$��15��$	 457��$ 70��
�������  -$   $ 	xArrow011��
��� � �$ Right=� .  $��4��$��15��$ 468.75��$ 70��
�������  -%   $ xArrow02��
��� � �$ Right=� .  $	��9.5��$��15��$	 481��$ 70��
�������A� -&   $ xReady��$��22��$��21��$	 494��$ 67��
��� � �
�������  -'   $ xArrow03��
��� � �$
 Left=� .  $	��9.5��$ 0.5,0.5&�$��15��$ 519.5��$ 70��
�������  -(   $ xArrow04��
��� � �$
 Left=� .  $��4��$ 0.5,0.5&�$��15��$ 537.326��$ 70��
�������  -)   $ 	xArrow041��
��� � �$
 Left=� .  $	��9.5��$ 0.5,0.5&�$��15��$ 543.412��$ 70��
��������� -*   $ RectangleZmcDirection��
����  �$	��104��$��25��$	 453��$ 94��
���   �   $	 272Q�  -+   $ mArrow01��
��� � �$ Right=� .  $	��9.5��$��15��$	 457��$ 99��
�������  -,   $ 	mArrow011��
��� � �$ Right=� .  $��4��$��15��$ 468.75��$ 99��
�������  --   $ mArrow02��
��� � �$ Right=� .  $	��9.5��$��15��$	 481��$ 99��
�������A� -.   $ mReady��
��� � �$��22��$��21��$	 494��$ 96��
�������  -/   $ mArrow03��
��� � �$
 Left=� .  $	��9.5��$ 0.5,0.5&�$��15��$ 519.5��$ 99��
�������  -0   $ mArrow04��
��� � �$
 Left=� .  $��4��$ 0.5,0.5&�$��15��$ 537.326��$ 99��
�������  -1   $ 	mArrow041��
��� � �$
 Left=� .  $	��9.5��$ 0.5,0.5&�$��15��$ 543.412��$ 99��
��������� -2   $ RectangleDycDirection��
���   �
�������$	��104��$��25��$	 453��$	 123��$y�	Collapsed=�  -3   $ BlockArrowDyc5Direction��
��� � �$ Right=� .  $	��9.5��
�������$��15��$	 457��$	 128��$y�	Collapsed=�  -4   $ BlockArrowDyc3Direction��
��� � �$ Right=� .  $��4��
�������$��15��$ 468.75��$	 128��$y�	Collapsed=�  -5   $ BlockArrowDyc1Direction��
��� � �$ Right=� .  $	��9.5��
�������$��15��$	 481��$	 128��$y�	Collapsed=�A� -6   $ EllipseDycDirection��
��� � �
�������$��22��$��21��$	 494��$	 125��$y�	Collapsed=�  -7   $ BlockArrowDyc2Direction��
��� � �$
 Left=� .  $	��9.5��
�������$ 0.5,0.5&�$��15��$ 519.5��$	 128��$y�	Collapsed=�  -8   $ BlockArrowDyc4Direction��
��� � �$
 Left=� .  $��4��
�������$ 0.5,0.5&�$��15��$ 537.326��$	 128��$y�	Collapsed=�  -9   $ BlockArrowDyc6Direction��
��� � �$
 Left=� .  $	��9.5��
�������$ 0.5,0.5&�$��15��$ 543.412��$	 128��$y�	Collapsed=��� -:   $ txtTjcDoorOpen��$
 Wrap=�$��炉门已摘��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�$��25��
����  �$��95��$	 587��$ 7��$
��Bold"�.������ $ 0.5,1&�$ 0.5,0&�.��� $ 	#FF323232��$ 1Q�� $ 	#FFA3A3A3��� $ 	#FF161616��$	 0.3Q�.��   $	 272Q�.���� -;   $ txtLjcTroughLocked��$
 Wrap=�$��焦槽锁闭��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�$��25��
����  �$��95��$	 587��$ 36��$
��Bold"�.������ $ 0.5,1&�$ 0.5,0&�.��� $ 	#FF007800��$ 1Q�� $ White��� $ 	DarkGreen��$	 0.3Q�.��   $	 272Q�.���� -<   $ txtCanReady��$
 Wrap=�$��车门关闭��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�
����  �
��� � �$��95��$��25��$	 587��$ 65��$
��Bold"�.��   $	 272Q�.���� -=   $ txtMcSleeve��$
 Wrap=�$��导套到位��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�
����  �$��25��$��95��$	 587��$ 94�����	#FF323232$
��Bold"�.��   $	 272Q�.���� ->   $ TextBoxDycLockM��$
 Wrap=�$��闸板打开��$
 Center=�$��Times New Roman)�
������� .  ��.  $��16(�
����  ����	#FF323232$��25��$��95��$	 587��$	 123��$y�	Collapsed=��� -?   $ txtFstAllow��$
 Wrap=�$��一级允推��$
 Center=�$��YouYuan)�
������� .  ��.  $��32(�
��� � �
����  �$��76�� J� 	AllowDrop . $��76��$	 712��$
 10.5��$
��Bold"�.��   $	 272Q�.���� -@   $ txtAllowStoking��$
 Wrap=�$��允许装煤��$
 Center=�$��YouYuan)�
������� .  ��.  $��24(�
����  �$��60�����	#FF323232$��60��$	 942��$ 92��$
��Bold"�.��   $	 272Q�.���� -A   $ txtTimeAllow��$
 Wrap=�$��允许时间��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�
��� � �$��25��
����  �$��95��$	 818��$ 7��$
��Bold"�.��   $	 272Q�.���� -B   $ txtLjcAllowPush��$
 Wrap=�$��人工允推��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�$��25��
����  �
��� � �$��95��$	 818��$ 36��$
��Bold"�.��   $	 272Q�.���� -C   $ txtXjcAllowPush��$
 Wrap=�$��人工允推��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�
����  �
��� � �$��95��$��25��$	 818��$ 65��$
��Bold"�.��   $	 272Q�.���� -D   $ txtSecAllow��$
 Wrap=�$��二级允推��$
 Center=�$��YouYuan)�
������� .  ��.  $��32(�
��� � �
����  �$��76�� . $��76��$	 943��$ 11��$
��Bold"�.��   $	 272Q�.���� -E   $ rectLinkTjcDoorOpen��
��� � �$��5��
�������$	 557��$ 17��$��30��   $	 272Q��� -F   $ rectLinkLjcTrough��
��� � �$��5��
�������$	 557��$ 46��$��30��   $	 272Q��� -G   $ rectLinkXjcCan��
��� � �
�������$��5��$	 557��$ 75��$��30��   $	 272Q��� -H   $ rectLinkMcSleeve��
��� � �
�������$��5��$	 557��$	 104��$��30��   $	 272Q��� -I   $ rectTLinkFstAllow��
��� � �$��5��
�������$��30��$	 682��$ 17��   $	 272Q��� -J   $ rectLLinkFstAllow��
��� � �$��5��
�������$��30��$	 682��$ 46��   $	 272Q��� -K   $ rectXLinkFstAllow��
��� � �
�������$��30��$��5��$	 682��$ 75��   $	 272Q��� -L   $ rectMLinkAllowPing��
��� � �
�������$��5��$	��260��$	 682��$ 98��   $	 272Q��� -M   $ rectTimeAllowLink��
��� � �$��5��
�������$��30��$	 913��$ 17��   $	 272Q��� -N   $ rectFstAllowLink��
��� � �$��5��
�������$��30��$	 788��$ 46��   $	 272Q��� -O   $ rectXLinkSecAllow��
��� � �
�������$��30��$��5��$	 913��$ 75��   $	 272Q��� -P   $ rectLLinkSecAllow��
��� � �$��5��
�������$��30��$	 913��$ 46��   $	 272Q��� -Q   $ txtPushTogethor��$
 Wrap=�$��推焦联锁��$��35��$	��112��$��YouYuan)�$��24(����	#FF323232
����  �
�������$
 Center=�$	 276��$ 7��$
��Bold"���.   .  .��   $	 272Q�.���� -R   $ txtStrokingTogether��$
 Wrap=�$��装煤联锁��$��35��$��35��$��YouYuan)����	#FF323232
����  �
�������$
 Center=�$	 337��$	 123����.  $
��Bold"� .  $y�Hidden=�.��   $	 272Q�.���� -S   $ 
txtTJobCar��$��25��$	 393��$
 Wrap=�$��0#0000��$ 7��$��55��$
 Center=�
�������$��YouYuan)� .  $��13.333(���Z� .��   $	 272Q�   ShadowDepth$ 3Q�.���� -T   $ 
txtLJobCar��$��25��$	 393��$
 Wrap=�$��0#0000��$ 36��$��55��$
 Center=�
�������$��YouYuan)�$��13.333(� .  ��Z� .��   $	 272Q�$ 3Q�.���� -U   $ 
txtXJobCar��$��25��$	 393��$
 Wrap=�$��0#0000��$ 65��$��55��$
 Center=�
�������$��YouYuan)�$��13.333(� .  ��Z� .��   $	 272Q�$ 3Q�.���� -V   $ 
txtMJobCar��$��25��$	 393��$
 Wrap=�$��0#0000��$ 94��$��55��$
 Center=�
�������$��YouYuan)�$��13.333(� .  ��Z� .��   $	 272Q�$ 3Q�   Opacity$ 0.995Q�.���� -W   $ TextBoxRoomNumD��$��25��$	 395��$
 Wrap=�$��00000��$	 123��$��50��$
 Center=���.  $y�	Collapsed=��� -X   $ txtTDoorClosed��$
 Wrap=�$��推炉门关��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�
����  ����	#FF323232$��25��$��95��$	 702��$	 105��$
��Bold"�.��   $	 272Q�.���� -Y   $ txtLDoorClosed��$
 Wrap=�$��拦炉门关��$
 Center=�$��YouYuan)�
������� .  ��.  $��18.667(�
����  ����	#FF323232$��25��$��95��$	 702��$	 131��$
��Bold"�.��   $	 272Q�.���� -Z   $ txtTPingReady��$
 Wrap=�$��推平煤到位��$
 Center=�$��YouYuan)�
������� .  ��.  $��16(�
����  ����	#FF323232$��25��$��95��$	 817��$	 110��$
��Bold"�.��   $	 272Q�.���� -[   $ rectLinkTPingReady�����	#FF323232
�������$��5��$��20��$	 797��$	 115��   $	 272Q��� -\   $ rectLLinkAllowStoking�����	#FF323232
�������$��5��$	��145��$	 797��$	 141��   $	 272Q��� -]   $$ rectTPingReadyLinkAllowStoking��
��� � �
�������$��5��$��30��$	 912��$	 120��   $	 272Q��� -^   $ TextBoxCommuniction��
���  ��
�������$��14.667(�
������� .  ��.  $
 Center=�$
 Wrap=�$	 Disabled=�$��通信异常��$ 4��$
��LiSu)�$ 5��$��74��$��28��