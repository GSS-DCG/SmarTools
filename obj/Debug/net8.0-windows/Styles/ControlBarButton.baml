   M S B A M L   `   `   `  ����D  @SmarTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:Ventana_TEST.StylesVentana_TEST.Styles  5      *    $Ventana_TEST.Styles.ControlBarButton   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      3local!clr-namespace:Ventana_TEST.Styles     5      X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5      .��5      � $��RightToLeft=�6     � ColumnDefinitions  5   
   �� 5      $
��auto �6   �� 5      $��* �6   �� 5      $
��auto �6   .+�5   
   �� -   5         ��Name$ pnlControlBar��5      $��0��5       �� Orientation$ 
Horizontal=�5      $��RightToLeft=�5      
������ $��0,0,5,0q�5      $��Right=�5      $	��Top=�5      $��25��5      .+�5      �� -   5         ��Name$ btnClose��5          ControlButton#��[  5!       
 Blue03
 � Tag# [ 5"      .��5$      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$ Xmark=�6     ControlButtonIcon#��[ 6,   6Y   5%      �� 5'      -   5        $ btnMaximize��5(      #��[  5)      # [ 5*      .��5,        $ WindowMaximize=�6   #��[ 65   6b   5-      �� 5/      -   5        $ btnMinimize��50      #��[  51      # [ 52      .��54        $ WindowMinimize=�6   #��[ 65   6b   55      56      �� 58   
   -   5        $��1��58      
������ $��25��6:   $	��Top=�6F   6�   �� 5:   
   -   5        $ pnlControlBarText��5;      $��2��5<      $ 
Horizontal=�5=      $��LeftToRight=�5>      
������ $��10,0,0,0q�5@      $
��Left=�5B      $	��Top=�5C      $��25��5E      .+�5F      �� -   5         ��Name$ ControlBarTxt��5G      
�������$��Center=�5I      $
��13.5(�5J      $��Medium"�5K      $��Neo Tech Std)�5L      $��10,0,0,0q�5M      5O      5P      5Q      5R      