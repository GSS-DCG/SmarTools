using Microsoft.VisualBasic;
using Microsoft.Win32;
using ModernUI.View;
using SAP2000v1;
using SmarTools.Model.Applications;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmarTools.APPS
{
    /// <summary>
    /// Lógica de interacción para ItaliaNTC1VAPP.xaml
    /// </summary>
    public partial class ItaliaNTC1VAPP : Window
    {

        public ItaliaNTC1VAPP()
        {
            InitializeComponent();
            
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
            this.Close();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal) { this.WindowState = WindowState.Maximized; } else { this.WindowState = WindowState.Normal; }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void VERIFICAR_ESTRUCTURA(object sender, RoutedEventArgs e)
        {
            ItaliaNTC2018.ComprobarNTC(CABEZA_MOTOR,CABEZA_GENERAL,PILAR,PILAR_MOTOR,VIGA_PRINCIPAL,VIGA_SECUNDARIA);
        }
    }
}
