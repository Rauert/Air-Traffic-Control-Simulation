   M S B A M L   `   `   `  ����E  AATCGUI_WPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ATCGUI_WPF
ATCGUI_WPF  5   	       ATCGUI_WPF.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   *localclr-namespace:ATCGUI_WPF     5   	   -   5          � Title$  Air Traffic Control��5   	   $	��500��6%   $
��1019��62    � 	Resources 5         ATCGUI_WPF.CharConverter   charFormatter   ATCGUI_WPF.DblConverter  dblFormatter   ATCGUI_WPF.RouteConverter  routeFormatter!   ATCGUI_WPF.AirportConverter  airportFormatter   ATCGUI_WPF.IntConverter  arIDFormatter��%�   &	        &	      &	 &     &	 9     &	 L       5   
   6     5   
   6     5	   
   6     5
   
   6!     5   
   6   5      .��5      � .+�5   
   �� -   5        $��lvwAirports��5   N   $	��202��6   $
��Left=�6    $��	10,25,0,0q�6;   $	��Top=�6a   $	��130��6y    �� SelectionMode$ Single=�6�    �� View 5      �� 5      .P�5      �� $��ID��6P    �� Width$ 20��6\    �� DisplayMemberBinding 6%   �� *	AirportID+6\   �� 5      $��Airport��6K   $	 100��6\    6%   �� *Name+6\   5      5      �� 5   
   -   5        $��btnStep��5   \   $��->��6   $��25��6!   $
��Left=�6-   $��
59,345,0,0q�6H   $	��Top=�6k   $��33��6�    � DataContext 6�   �� 6�   �� 5   
   -   5        $��
lvwInbound��5   5   $	��202��6   $��146,25,12,0q�6    $	��Top=�6G   $ Single=�6_    5      �� 5      .P�5      �� $��ID��6Q   $ 30��6]    6%   �� *
AirplaneID+6]   �� 5      $��State description��6u   $	 100��6�    6%   �� *State+ �� 	Converter# [  6�   �� 5      $��Airplane type��6P   $ 90��6g    6%   �� *	PlaneType+6g   �� 5      $��Fuel remaining��6s   $ 90��6�    6%   �� *Fuel+# [ 6�   �� 5      $��Speed��6R   $ 45��6a    6%   �� *CruisingKPH+6a   �� 5      $��AR ID��6�   $ 40��6�    6%   �� *CurrentAirRouteID+# [ 6�   �� 5       $��	Air Route��6�   $	 140��6�    6%   �� *CurrentAirRouteID+# [ 6�   �� 5!      $��Distance along route��6Y   $	 115��6w    6%   �� *DistanceAlongRoute+6w   �� 5"      $��Airport name��6�   $ 90��6�    6%   �� *CurrentAirportID+# [ 6�   �� 5#      $��Time Landed��6Q   $ 75��6f    6%   �� *
TimeLanded+6f   5$      5&      �� 5'   
   -   5        $��lvwOutbound��5'   6   $	��202��6   $��146,254,12,0q�6    $ Single=�6I   $	��Top=�6`    5(      �� 5)      .P�5*      �� $��ID��6Q   $ 30��6]    6%   �� *
AirplaneID+6]   �� 5+      $��State description��6u   $	 100��6�    6%   �� *State+# [  6�   �� 5,      $��Airplane type��6P   $ 90��6g    6%   �� *	PlaneType+6g   �� 5-      $��Fuel remaining��6s   $ 90��6�    6%   �� *Fuel+# [ 6�   �� 5.      $��Speed��6R   $ 45��6a    6%   �� *CruisingKPH+6a   �� 5/      $��AR ID��6�   $ 40��6�    6%   �� *CurrentAirRouteID+# [ 6�   �� 50      $��	Air Route��6�   $	 140��6�    6%   �� *CurrentAirRouteID+# [ 6�   �� 51      $��Distance along route��6Y   $	 115��6w    6%   �� *DistanceAlongRoute+6w   �� 52      $��Airport name��6�   $ 90��6�    6%   �� *CurrentAirportID+# [ 6�   �� 53      $��Time Landed��6Q   $ 75��6f    6%   �� *
TimeLanded+6f   54      56      �� 57   
   -   5        $��lblAirports��57   [   $��Airports��6   $��28��6#   $
��Left=�6/   $��9,2,0,0q�6J   $	��Top=�6n   �� 58   
   -   5        $��
lblInbound��58   f   $��Inbound Airplanes��6   $��28��6,   $
��Left=�68   $��	144,2,0,0q�6S   $	��Top=�6x   �� 59   
   -   5        $��lblOutbound��59   i   $��Outbound Airplanes��6   $��28��6-   $
��Left=�69   $��144,231,0,0q�6T   $	��Top=�6|   5:      5;      