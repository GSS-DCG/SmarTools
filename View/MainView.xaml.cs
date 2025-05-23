using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using SmarTools.ViewModel;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace ModernUI.View
{
    /// <summary>
    /// Lógica de interacción para MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public static class Globales
        {
            public static string _version = "Version_0.0";

            public static Inicio _inicio = new Inicio();
            public static TracSmart1V _TracSmart1V = new TracSmart1V();
            public static TracSmart2V _TracSmart2V = new TracSmart2V();
            public static RackSmart _RackSmart = new RackSmart();
            public static Ajustes _Ajustes = new Ajustes();
            public static FontAwesome.Sharp.IconImage _BellNotification = new FontAwesome.Sharp.IconImage();
        }
        public MainView()
        {
            InitializeComponent();
            Caption_Text.Text = "Inicio";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Home;
            MainViewContentControl.Children.Add(Globales._inicio);
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            Globales._BellNotification = BellNotification;
            Herramientas.NotificacionCampana();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnlcControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal) { this.WindowState = WindowState.Maximized; } else { this.WindowState = WindowState.Normal; }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnSettingsIcon_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "Ajustes";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Gear;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._Ajustes);
        }

        private System.Drawing.Color backColor;
        private System.Drawing.Color borderColor;
        private System.Drawing.Color menuItemBorderColor;
        private System.Drawing.Color menuItemSelectedColor;

        private void btnBellIcon_Click(object sender, RoutedEventArgs e)
        {
            backColor = System.Drawing.Color.FromArgb(0,0,0);
            backColor = System.Drawing.Color.FromArgb(255,0,0);
            backColor = System.Drawing.Color.FromArgb(0,255,0);
            backColor = System.Drawing.Color.FromArgb(0,0,255);

            
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "Inicio";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Home;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._inicio);
            SmarTools.ViewModel.Inicio.InfoVersion();
        }

        private void btnTracSmart1V_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "TracSmart+ 1P";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.SolarPanel;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._TracSmart1V);
        }

        private void btnTracSmart2V_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "TracSmart+ 2P";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.SolarPanel;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._TracSmart2V);
        }

        private void btnRacksmart_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "RackSmarT";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.SolarPanel;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._RackSmart);
        }

        private void btnAjustes_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "Ajustes";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Gear;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._Ajustes);
        }
    }
    public class Herramientas
    {
        public static void NotificacionCampana()
        {
            string ruta = @"Z:\300Logos\Version.txt";
            string version = File.ReadAllText(ruta);

            if (version != ModernUI.View.MainView.Globales._version)
            {
                ModernUI.View.MainView.Globales._BellNotification.Visibility = Visibility.Visible;
            }
            else
            {
                ModernUI.View.MainView.Globales._BellNotification.Visibility = Visibility.Hidden;
            }
        }
    }
}
