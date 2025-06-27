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
using SmarTools.APPS;

namespace SmarTools.ViewModel
{
    /// <summary>
    /// Lógica de interacción para TracSmart2V.xaml
    /// </summary>
    public partial class TracSmart2V : UserControl
    {
        public TracSmart2V()
        {
            InitializeComponent();
            MainView.Globales._producto = "2V";
        }
        private void btnCambiarCombinaciones_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCambiarCargas_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDimensionamiento_Click(object sender, RoutedEventArgs e)
        {
            Dimensionamiento2VAPP App= new Dimensionamiento2VAPP();
            App.Show();
        }

        private void btnRefuerzoSecundaria_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnItalia_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCalcularFlechas_Click(object sender, RoutedEventArgs e)
        {
            ComprobacionFlechasTracker2VAPP App=new ComprobacionFlechasTracker2VAPP();
            App.Show();
        }

        private void btnComprobacionUniones_Click(object sender, RoutedEventArgs e)
        {
            ComprobacionUniones2VAPP App=new ComprobacionUniones2VAPP();
            App.Show();
        }

        private void btnReacciones_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnListadosCalculo_Click(object sender, RoutedEventArgs e)
        {
            ListadosDeCalculo2VAPP App = new ListadosDeCalculo2VAPP();
            App.Show();
        }
    }
}
