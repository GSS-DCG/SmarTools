using ModernUI.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmarTools.ViewModel;
using SmarTools.APPS;

namespace SmarTools.ViewModel
{
    /// <summary>
    /// Lógica de interacción para TracSmart1V.xaml
    /// </summary>
    public partial class TracSmart1V : UserControl
    {
        public TracSmart1V()
        {
            InitializeComponent();
            MainView.Globales._producto = "1V";
        }

        private void btnNumeroPilares_Click(object sender, RoutedEventArgs e)
        {
            NumeroPilaresAPP App= new NumeroPilaresAPP();
            App.Show();
        }

        private void btnCambiarCombinaciones_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCambiarCargas_Click(object sender, RoutedEventArgs e)
        {
            CambiarCargas1VAPP App=new CambiarCargas1VAPP();
            App.Show();
        }

        private void btnDimensionamiento_Click(object sender, RoutedEventArgs e)
        {
            Dimensionamiento1VAPP App = new Dimensionamiento1VAPP();
            App.Show();
        }

        private void btnItalia_Click(object sender, RoutedEventArgs e)
        {
            ItaliaNTC1VAPP App = new ItaliaNTC1VAPP();
            App.Show();
        }

        private void btnCalcularFlechas_Click(object sender, RoutedEventArgs e)
        {
            ComprobacionFlechasTrackerAPP App = new ComprobacionFlechasTrackerAPP();
            App.Show();
        }

        private void btnComprobacionUniones_Click(object sender, RoutedEventArgs e)
        {
            ComprobacionUnionesAPP App = new ComprobacionUnionesAPP();
            App.Show();
        }

        private void btnReacciones_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnListadosCalculo_Click(object sender, RoutedEventArgs e)
        {
            ListadosDeCalculo1VAPP App = new ListadosDeCalculo1VAPP();
            App.Show();
        }
    }
}
