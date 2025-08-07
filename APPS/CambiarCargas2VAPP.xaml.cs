using DocumentFormat.OpenXml.Math;
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
    /// Lógica de interacción para CambiarCargas2VAPP.xaml
    /// </summary>
    public partial class CambiarCargas2VAPP : Window
    {
        public CambiarCargas2VAPP()
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

        private void btnCargarDatos_Click(object sender, RoutedEventArgs e)
        {
            CambiarCargas2V.CargarDatos(this);
        }

        private void btnAsignarCargas_Click(object sender, RoutedEventArgs e)
        {
            CambiarCargas2V.AsignarCargas(this);
        }

        private void Normativa_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                string normativa = rb.Content.ToString();

                //Ocultamos todo
                Friccion_text.Visibility = Visibility.Collapsed;
                Friccion.Visibility = Visibility.Collapsed;
                Friccion_unidades.Visibility = Visibility.Collapsed;
                Friccion_Check.Visibility = Visibility.Collapsed;

                Presion_text.Visibility = Visibility.Collapsed;
                Presion.Visibility = Visibility.Collapsed;
                Presion_unidades.Visibility = Visibility.Collapsed;
                Presion_Check.Visibility = Visibility.Collapsed;

                Succion_text.Visibility = Visibility.Collapsed;
                Succion.Visibility = Visibility.Collapsed;
                Succion_unidades.Visibility = Visibility.Collapsed;
                Succion_Check.Visibility = Visibility.Collapsed;

                G_text.Visibility = Visibility.Collapsed;
                G.Visibility = Visibility.Collapsed;
                G_unidades.Visibility = Visibility.Collapsed;
                G_Check.Visibility = Visibility.Collapsed;

                //Mostramos según la opción elegida
                if (normativa == "Eurocódigo" || normativa == "NTC-2018")
                {
                    Friccion_text.Visibility = Visibility.Visible;
                    Friccion.Visibility = Visibility.Visible;
                    Friccion_unidades.Visibility = Visibility.Visible;
                    Friccion_Check.Visibility = Visibility.Visible;
                }
                else if (normativa == "ASCE7-05" || normativa == "ASCE7-16")
                {
                    Presion_text.Visibility = Visibility.Visible;
                    Presion.Visibility = Visibility.Visible;
                    Presion_unidades.Visibility = Visibility.Visible;
                    Presion_Check.Visibility = Visibility.Visible;

                    Succion_text.Visibility = Visibility.Visible;
                    Succion.Visibility = Visibility.Visible;
                    Succion_unidades.Visibility = Visibility.Visible;
                    Succion_Check.Visibility = Visibility.Visible;

                    G_text.Visibility = Visibility.Visible;
                    G.Visibility = Visibility.Visible;
                    G_unidades.Visibility = Visibility.Visible;
                    G_Check.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
