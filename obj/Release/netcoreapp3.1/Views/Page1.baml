   M S B A M L   `   `   `  ���� =  9C3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:C3.ViewsC3.Views      C3.Views.Page1   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     (localclr-namespace:C3.Views       N� Title$  Page1��.���  � RowDefinitions �� $��40.72 ��� $��* ��� $��61 � � ColumnDefinitions �� $��22.1* ��� $��671.31* ��� $��22.1* �.+��� $��0��$��1��$��Stretch=����	#FF32A0AB.+�� .+��� $
��Left=�$	��Top=�$��19.6,8q�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -    ��Name$ TextTitleRegisterNew��$-�'Register New Video Copyright Protection��� -   $��GridVideoPlayer��$��1��$��1�����	#3B32A0AB �� $��* ��� $��50 �.+�r� -   $��VideoPlayer�� J� Opacity$	 0.3Q�$��2��$��Stretch=�$��Stretch=� r� LoadedBehavior$ Manual=� r� UnloadedBehavior$
 Stop=� r� ScrubbingEnabled$
 True��� -   $��GridVideoDescription��$��2��$y�	Collapsed=� �� $	��120 ��� $��* �.+��� $��0��.+��� $
��Left=�$	��Top=�$��	15,20,0,0q�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextContentTitle��$�Title���� $
��Left=�$	��Top=�$��	15,19,0,0q�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextRegisterer��$�
Registerer���� $
��Left=�$	��Top=�$��	15,19,0,0q�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ 
TextMadeAt��$�Made At���� $
��Left=�$	��Top=�$��	15,19,0,0q�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextKeyword��$�Keywords���� $
��Left=�$	��Top=�$��	15,19,0,0q�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -	   $ TextDescription��$�Description���� $
��Left=�$	��Top=�$��	15,19,0,0q�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -
   $ TextOriginalLink��$�Original Link���� $��1��.+��� -    ��Name$ TextBox_ContentTitle��$��34��$��10, 15,15,0q�$��Stretch=�$	��Top=�	 �� 
CaretBrush
	 �����
������ $��2q�$��Center=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextBox_Registerer��$��34��$��10, 10,15,0q�$��Stretch=�$	��Top=�
	 �����
������ $��2q�$��Center=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(�� $��34��$��
10,10,15,0q�$��Stretch=�$	��Top=�.+��� $
��Left=�$	��Top=�$��10,5,0,0q����	#40FFFFFF$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextMadeAtHint��$�
YYYY.MM.DD���� -   $ TextBox_MadeAt��$��34��$��0,0,0,0q�$��Stretch=�$	��Top=�
	 �����
������ $��2q�$��Center=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(�� $��34��$��
10,10,15,0q�$��Stretch=�$	��Top=�.+��� $
��Left=�$	��Top=�$��10,5,0,0q����	#40FFFFFF$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextKeywordHint��$)�#Seperate each keyword by comma(',')���� -   $ TextBox_Keyword��$��34��$��0,0,0,0q�$��Stretch=�$	��Top=�
	 �����
������ $��2q�$��Center=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextBox_Description��$��34��$��10, 10,15,0q�$��Stretch=�$	��Top=�
	 �����
������ $��2q�$��Center=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��� -   $ TextBox_OriginalLink��$��34��$��10, 10,15,0q�$��Stretch=�$	��Top=�
	 �����
������ $��2q�$��Center=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��18(��  �� $	��62* ��� $	��120 ��� $
��117* �.+��� -   $��RectLargePlus1��$	��120��$��16�����	#FF32A0AB$��1��$��Center=�$��Center=��� -   $��RectLargePlus2��$��16��$	��120�����	#FF32A0AB$��1��$��Center=�$��Center=��� -   $��TextAddVideoFile��$��2��$��1��$��Center=�$	��Top=�$��
0,15.7,0,0q����	#FF32A0AB$'��!../Resources/#AppleSDGothicNeoH00)�$��21.97(��� -   $ TextAddVideoFile_Text��$�Add a video file.���� -   $��TextAddedFileName��$y�	Collapsed=�$��1��$��1��$��Center=�$��Bottom=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��37.87(��� -   $ TextAddedFileName_Text��$�TestVideo123.Mov���� -   $��TextAddedFileInfo��$y�	Collapsed=�$��2��$��1��$��Center=�$	��Top=�
�������$'��!../Resources/#AppleSDGothicNeoB00)�$��23.85(��� -   $ TextAddedFileInfo_Text��$�802.12mb / 10:08���� -   
 ��Name$
 Slider�� � Minimum$ 0Q� � Maximum$	 100Q� � Value$ 0Q�$
��5.03��$��3��$��1��$��Stretch=�$	��Top=�$��22.5,0,22.5,0q�$��1q����	#3BFFFFFF   Horizontal_Slider#��[   �� IsMoveToPointEnabled$
 True���� -   $��BtnVideoPlayerPlay��$��16.08��$��16.08��$��3��$��1��$
��Left=�$��Bottom=�$��22.5,0,0,21.9q�  BtnVideoPlayerControl#��[ .���� .+�� $��14.08��$��14.08��.+��� $��14.08��$��14.08��$#��../Images/btn_player_play.png���� -   $��BtnVideoPlayerPause��$y�	Collapsed=�$��16.08��$��16.08��$��3��$��1��$
��Left=�$��Bottom=�$��22.5,0,0,21.9q�#��[ .���� .+�� $��14.08��$��14.08��.+��� ���	#3BFFFFFF$��5��$��14.08��$
��Left=��� 
������ $
��4.08��$��16.08��$��Center=��� ���	#3BFFFFFF$��5��$��14.08��$��Right=��� -   $��BtnVideoPlayerStop��$��16.08��$��16.08��$��3��$��1��$
��Left=�$��Bottom=�$��49.4,0,0,21.9q�#��[ .���� .+�� $��14.08��$��14.08��.+��� ���	#3BFFFFFF$��14.08��$��14.08��$��Center=�� -   $��ProgressRegistering��$y�	Collapsed=�$ 0Q�$ 0Q�$	 100Q�$��60.21��$��2��$��1��$��0q�$��Stretch=�$��Stretch=�$��	0,2.5,0,0q����	#FF32A0AB���	#B3000000�� -    $��BtnRegisterThisFile��$��60.21��$��2��$��1��$��Center=�$��	0,2.5,0,0q�${�False��  BtnRegisterThisFile#��[ .���� .+�� .+��� -!    ��Name$ TextRegisterThisFile��$��Center=�$��Center=����	#11FFFFFF$'��!../Resources/#AppleSDGothicNeoB00)�$��21(��� -"   $ TextRegisterThisFile_Text��$-�'Register Video Copyright with This File��