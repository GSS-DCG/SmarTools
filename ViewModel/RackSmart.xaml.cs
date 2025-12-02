using SmarTools.APPS;
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

namespace SmarTools.ViewModel
{
    /// <summary>
    /// Lógica de interacción para RackSmart.xaml
    /// </summary>
    public partial class RackSmart : UserControl
    {
        public RackSmart()
        {
            InitializeComponent();
        }

        private void btnCambiarCombinaciones_Click(object sender, RoutedEventArgs e)
        {
            CambiarCombinacionesRackAPP App=new CambiarCombinacionesRackAPP();
            App.Show();
        }

        private void btnCambiarCargas_Click(object sender, RoutedEventArgs e)
        {
            CambiarCargasRackAPP App=new CambiarCargasRackAPP();
            App.Show();
        }

        private void btnDimensionamiento_Click(object sender, RoutedEventArgs e)
        {
            DimensionamientoRackAPP App= new DimensionamientoRackAPP();
            App.Show();
        }

        private void btnItalia_Click(object sender, RoutedEventArgs e)
        {
            ItaliaNTCRackAPP App= new ItaliaNTCRackAPP();
            App.Show();
        }

        private void btnCalcularFlechas_Click(object sender, RoutedEventArgs e)
        {
            ComprobacionFlechasRackAPP App = new ComprobacionFlechasRackAPP();
            App.Show();
        }

        private void btnComprobacionUniones_Click(object sender, RoutedEventArgs e)
        {
            ComprobacionUnionesRackAPP App = new ComprobacionUnionesRackAPP();
            App.Show();
        }

        private void btnReacciones_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnListadosCalculo_Click(object sender, RoutedEventArgs e)
        {
            ListadosDeCalculoAPP App = new ListadosDeCalculoAPP();
            App.Show();
        }
    }
}
