   M S B A M L   `   `   `  ����D  @SmarTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:SmarTools.ViewModelSmarTools.ViewModel  5      %    SmarTools.ViewModel.TracSmart1V   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      3local!clr-namespace:SmarTools.ViewModel     5      X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5      .��5
      �   � ColumnDefinitions  5   
   �� 5      6!   �� 5      6!   �� 5      6!   �� 5      6!   �� 5      6!    � RowDefinitions 5   
   �� 5      $��50 �6   6*   �� 5      $��* �6   6)   �� 5      $��50 �6   6*   �� 5      $��* �6   6)   �� 5      $��50 �6   6*   �� 5      $��* �6   6)   .+�5   
   �� $��0��6   $��0��6   $��4��6/   ._�5      �� $!��Propiedades Modelo Cálculo��6   
�������$��Neo Tech Std)�6N   $
��Bold"�6h   $��20(�6z   $��Center=�6�   $��20,0,0,0q�6�   6�   5      	� 5    
   -   5           SelectorButton#��[  5         	Resaltado
 � Tag# [ 5!      $��1��5"      $��0��5#      .��5%      ��  �� Orientation$ Vertical=�6   .+�5&      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$
 Bars=�6     SelectorButtonIcon#��[ 6+   �� 5'      $��Numero Pilares��6     selectorButtonText#��[ 62   5(      5)      	� 5+   
   -   5        #��[  5+      # [ 5,      $��1��5-      $��1��5.      .��50      �� $ Vertical=�6   .+�51        $ RulerCombined=�6   #��[ 64   �� 52      $��Cambiar
Combinaciones��6   #��[ 6>    �� TextAlignment$ center=�6j   53      54      	� 56   
   -   5        #��[  56      # [ 57      $��1��58      $��2��59      .��5;      �� $ Vertical=�6   .+�5<        $ FaceGrinBeam=�6   #��[ 63   �� 5=      $��Cambiar cargas��6   #��[ 62   5>      5?      �� 5B   
   $��2��6   $��0��6   $��4��6/   ._�5C      �� $��Comprbaciones Cálculo��6   
�������$��Neo Tech Std)�6I   $
��Bold"�6c   $��20(�6u   $��Center=�6�   $��20,0,0,0q�6�   6�   5D      	� 5F   
   -   5        #��[  5F      # [ 5G      $��3��5H      $��0��5I      .��5K      �� $ Vertical=�6   .+�5L        $ Check=�6   #��[ 6,   �� 5M      $��Dimensionamiento��6   #��[ 64   5N      5O      	� 5Q   
   -   5        #��[  5Q      # [ 5R      $��3��5S      $��1��5T      .��5V      �� $ Vertical=�6   .+�5W        $
 Flag=�6   #��[ 6+   �� 5X      $��Italia NTC_2018��6   #��[ 63   5Y      5Z      	� 5\   
   -   5        #��[  5\      # [ 5]      $��3��5^      $��2��5_      .��5a      �� $ Vertical=�6   .+�5b        $ Check=�6   #��[ 6,   �� 5c      $��Cálculo Flechas��6   #��[ 63   5d      5e      	� 5g   
   -   5        #��[  5g      # [ 5h      $��3��5i      $��3��5j      .��5l      �� $ Vertical=�6   .+�5m        $ Wrench=�6   #��[ 6-   �� 5n      $��Comprobacion Uniones��6   #��[ 68   5o      5p      	� 5r   
   -   5        #��[  5r      # [ 5s      $��5��5t      $��0��5u      .��5w      �� $ Vertical=�6   .+�5x        $ Wrench=�6   #��[ 6-   �� 5y      $��
Reacciones��6   #��[ 6.   5z      5{      �� 5}   
   $��4��6   $��0��6   $��4��6/   ._�5~      �� $��Extraccion documentos��6   
�������$��Neo Tech Std)�6I   $
��Bold"�6c   $��20(�6u   $��Center=�6�   $��20,0,0,0q�6�   6�   5      	� 5�   
   -	   5        #��[  5�      # [ 5�      $��5��5�      $��1��5�      .��5�      �� $ Vertical=�6   .+�5�        $ Wrench=�6   #��[ 6-   �� 5�      $��Listados Cálculo��6   #��[ 64   5�      5�      5�      5�      