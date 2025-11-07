using SmarTools.Model.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmarTools.APPS
{
    /// <summary>
    /// Lógica de interacción para ComprobacionFlechasRackAPP.xaml
    /// </summary>
    public partial class ComprobacionFlechasRackAPP : Window
    {
        public ComprobacionFlechasRackAPP()
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

        private void btnCalcularFlecha_Click(object sender, RoutedEventArgs e)
        {
            ComprobacionFlechasRack.ComprobarFlechas(this);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBoxMarcado = sender as CheckBox;

            // Grupo de tipo de poste
            if (checkBoxMarcado == Monoposte)
            {
                Biposte.IsChecked = false;
            }
            else if (checkBoxMarcado == Biposte)
            {
                Monoposte.IsChecked = false;
            }
            // Grupo de diagonales
            else if (checkBoxMarcado == SinDiagonal)
            {
                UnaDiagonal.IsChecked = false;
                DosDiagonal.IsChecked = false;
            }
            else if (checkBoxMarcado == UnaDiagonal)
            {
                SinDiagonal.IsChecked = false;
                DosDiagonal.IsChecked = false;
            }
            else if (checkBoxMarcado == DosDiagonal)
            {
                SinDiagonal.IsChecked = false;
                UnaDiagonal.IsChecked = false;
            }
        }
    }
}
