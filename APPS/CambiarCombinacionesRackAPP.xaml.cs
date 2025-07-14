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
    /// Lógica de interacción para CambiarCombinacionesRack.xaml
    /// </summary>
    public partial class CambiarCombinacionesRackAPP : Window
    {
        public CambiarCombinacionesRackAPP()
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

        private void btnGenerarCombinaciones_Click(object sender, RoutedEventArgs e)
        {
            CambiarCombinacionesRack.GenerarCombinaciones(this);
        }

        private void btnAplicarCombinaciones_Click(object sender, RoutedEventArgs e)
        {
            CambiarCombinacionesRack.AplicarCombinaciones(this);
        }

        private void Normativa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var seleccion=(Normativa.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var visibilidad_EU = (seleccion == "Eurocódigo") || (seleccion == "NTC-2018") ? Visibility.Visible : Visibility.Collapsed;
            var visibilidad_ASCE = (seleccion == "ASCE7-05") || (seleccion == "ASCE7-16") ? Visibility.Visible : Visibility.Collapsed;

            var elementos_ASCE = new UIElement[]
            {
                ELU_ASCE,
                StackPanel_ELU_ASCE,
                ELS_ASCE,
                StackPanel_ELS_ASCE
            };

            var elementos_EU = new UIElement[]
            {
                ELU_EU,
                StackPanel_ELU_EU,
                CoefSimult_EU,
                StackPanel_Simult_EU,
                ELS_EU,
                StackPanel_ELS_EU
            };

            foreach (var elemento in elementos_EU)
            {
                if (elemento == null)
                {
                    continue;
                }

                elemento.Visibility = visibilidad_EU;
            }

            foreach (var elemento in elementos_ASCE)
            {
                if (elemento == null)
                {
                    continue;
                }

                elemento.Visibility = visibilidad_ASCE;
            }

            var coeficientes = CambiarCombinacionesRack.Coeficientes(this, seleccion);

            foreach(var (valor,caja) in coeficientes)
            {
                if (caja != null)
                {
                    caja.Text = valor.ToString("0.00");
                }
            }
        }
    }
}
