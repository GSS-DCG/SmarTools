   M S B A M L   `   `   `  ����D  @SmarTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ModernUI.ViewModernUI.View  5   	   9!clr-namespace:SmarTools.ViewModelSmarTools.ViewModel  5   	       ModernUI.View.MainView   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ModernUI.View     5   	   X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5   	   1	controles!clr-namespace:SmarTools.ViewModel  5   	     � Title$  MainView��5
   	   $	��700��6   $
��1300��6'    � WindowStyle$
 None=�5   	   
������  � AllowsTransparency$
 True��5   	    � WindowStartupLocation$ CenterScreen=�5   	   .��5      ��  �� CornerRadius$ 12��6   ��5   
   �� 5       �� ImageSource$a [https://www.gsolarsteel.com/wp-content/uploads/2025/05/pfv_SolarSteel_Trackers-1024x576.png��6    j� Stretch$ UniformToFill=�5      ._�5   
   �� $ 10��6   $��2q�5       J� Opacity$
 0.95Q�5      ��5      �� 5       �� 
StartPoint$	 0,0&�6&   	 �� EndPoint$		 1,1&�67   .��5      �    windowBorderColor1
 � Color#
 [  6#    � Offset$ 0Q�6O   6\   � 5        windowBorderColor2#
 [ 6#   $	 0.5Q�6O   6^   � 5        windowBorderColor3#
 [ 6#   $ 1Q�6O   6\   5      ._�5$      �  � ColumnDefinitions 5%      �� 5&      $	��250 �6'   65   �� 5'      $��* �6'   63   .+�5+      �� $ 	10,0,0,10��6   $��0��5,      ��5-      �� 5.      $	 0,0&�6.   $		 1,1&�6?   .��5/      �   primaryBackColor1#
 [ 6+   $ 0Q�6V   6c   � 50        primaryBackColor2#
 [ 6+   $
 0.75Q�6V   6f   51      53      �� 55      .+�57      ��  �� Orientation$ 
Horizontal=�6!   $��50��58   !   $��	15,25,0,0q�59   !   $��center=�5:   !   .+�5;      �� $;��5https://f7asturias.mygol.es/upload/D9/F7/wfhdvase.png��6    $��50��6_   6m   5<      �� 5>      $��	SmarTools��6    
�������$��Center=�5@       $��Center=�5A       $��40(�5B       $��Medium"�5C       $��Neo Tech Std)�5D       $��5,0,0,25q�5E       5F      	� 5H      -   5          
menuButton#��[ 5H   "     General4
 � Tag# [ 5I   "   .��5K      �� $ 
Horizontal=�6%   .+�5L      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$
 Home=�6+     menuButtonIcon#��[ 67   �� 5M      $��Inicio��6(     menuButtonText#��[ 66   5N      5O      	� 5Q      -   5        #��[ 5Q   "   # [ 5R   "   .��5T      �� $ 
Horizontal=�6%   .+�5U        $ 
SolarPanel=�6+   #��[ 6=   �� 5V      $��TracSmart+ 1P��6(   #��[ 6=   5W      5X      	� 5Z      -   5        #��[ 5Z   "   # [ 5[   "   .��5]      �� $ 
Horizontal=�6%   .+�5^        $ 
SolarPanel=�6+   #��[ 6=   �� 5_      $��TracSmart+ 2P��6(   #��[ 6=   5`      5a      	� 5c      -   5        #��[ 5c   "   # [ 5d   "   .��5f      �� $ 
Horizontal=�6%   .+�5g        $ 
SolarPanel=�6+   #��[ 6=   �� 5h      $��	RackSmarT��6(   #��[ 69   5i      5j      	� 5l      -   5        #��[ 5l   "   # [ 5m   "   .��5o      �� $ 
Horizontal=�6%   .+�5p        $
 Gear=�6+   #��[ 67   �� 5q      $��Ajustes��6(   #��[ 67   5r      5s      5u      �� 5w      $ 	0,10,10,0��6   $��1��5x      ��5y      �� 5z      $	 1,1&�6.   $		 0,0&�6?   .��5{      � #
 [ 6+   $ 0Q�6V   6c   � 5|      #
 [ 6+   $
 0.75Q�6V   6f   5}      ._�5�      �  � RowDefinitions 5�      �� 5�      $��25 �6,   �� 5�      $��50 �6,   �� 5�      $��* �6,   .+�5�      �� -   5         ��Name$ pnlControlBar��5�   %   $��0��5�   %   $ 
Horizontal=�5�   %   $��RightToLeft=�5�   %   
������ $��0,0,5,0q�5�   %   .+�5�      �� -   5         ��Name$ btnClose��5�   %    	 ControlButton#��[	 5�   %   # [ 5�   %   .��5�   "     $ Xmark=�6/    
 ControlButtonIcon#��[
 6<   6i   5�      �� 5�      -   5        $ btnMaximize��5�   %   #��[	 5�   %   # [ 5�   %   .��5�   "     $ WindowMaximize=�6/   #��[
 6E   6r   5�      �� 5�      -	   5        $ btnMinimize��5�   %   #��[	 5�   %   # [ 5�   %   .��5�   "     $ WindowMinimize=�6/   #��[
 6E   6r   5�      5�      � 5�      $��1��6    5�      �� 5�   "   �� 5�   "   .+�5�      �� $��0��6)   $ 
Horizontal=�5�   )   $��Center=�5�   )   .+�5�   "     -
   5          Name$ Caption_Icon��5�   /   $	 Ban=�5�   /   $��25��5�   /   $��25��5�   /      
Foreground
 �����$��	35,0,10,0q�5�   /   �� 5�   "   -   5         ��Name$ Caption_Text��5�   ,   $��Error��5�   ,   
�������$��20(�5�   ,   $��Neo Tech Std)�5�   ,   $��Center=�5�   ,   5�      �� 5�      $��1��6)   $ 
Horizontal=�5�   )   $��RightToLeft=�5�   )   $��Center=�5�   )   $��0,0,10,0q�5�   )   .+�5�   "   �� -   5        $ SettingsBtn��5�   )     
IconButton#��[ 5�   )   # [ 5�   )   .��5�   &     $
 Gear=�63   $��25��5�   3   $��25��5�   3   #��[
 5�   3   5�   #   �� 5�   "   -   5        $ BellBtn��5�   )   #��[ 5�   )   # [ 5�   )   .��5�   &     $
 Bell=�63   $��25��5�   3   $��25��5�   3   #��[
 5�   3   5�   '   5�   #   5�      � 5�      $��1��6#   $��LeftToRight=�5�   #   $��Center=�5�   #   $��0,0,50,0q�5�   #   $��25��5�   #   $��Right=�5�   #    5�   "   �� 5�   &   �� 5�   &    5�   "   �� 5�   &   �� 5�   &   .+�5�   "     -   5        $ BellNotification��5�   /   $ 
CircleInfo=�5�   %    �#C72021$��0��5�   %   $��1��5�   %   $y�Visible=�5�   /   6F   5�      5�      � 5�      -   5         �Name$ MainViewContentControl��5�      $��2��5�      $��25q�5      5     5     5     5     5     5	     