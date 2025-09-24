using FontAwesome.Sharp;
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

namespace SmarTools.View
{
    /// <summary>
    /// Lógica de interacción para Incidencias.xaml
    /// </summary>
    public partial class Incidencias : Window
    {
        public Incidencias()
        {
            InitializeComponent();
        }

        public void ConfigurarIncidencia(string mensaje, TipoIncidencia tipo, double fontSize = 13)
        {
            Incidencia.Text = mensaje;
            Incidencia.FontSize = fontSize;

            switch (tipo)
            {
                case TipoIncidencia.Informacion:
                    Icono.Icon = IconChar.InfoCircle;
                    break;

                case TipoIncidencia.Advertencia:
                    Icono.Icon = IconChar.ExclamationTriangle;
                    break;

                case TipoIncidencia.Error:
                    Icono.Icon = IconChar.TimesCircle;
                    break;

                case TipoIncidencia.Exito:
                    Icono.Icon = IconChar.CheckCircle;
                    break;

                case TipoIncidencia.Pregunta:
                    Icono.Icon = IconChar.QuestionCircle;
                    break;

                default:
                    Icono.Icon = IconChar.ExclamationCircle;
                    break;
            }
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        // Métodos estáticos para uso fácil como MessageBox
        public static bool? Mostrar(string mensaje, TipoIncidencia tipo = TipoIncidencia.Informacion, double fontSize = 13)
        {
            var ventana = new Incidencias();
            ventana.ConfigurarIncidencia(mensaje, tipo, fontSize);
            return ventana.ShowDialog();
        }

        public static bool? MostrarError(string mensaje, double fontSize = 13)
        {
            return Mostrar(mensaje, TipoIncidencia.Error, fontSize);
        }

        public static bool? MostrarAdvertencia(string mensaje, double fontSize = 13)
        {
            return Mostrar(mensaje, TipoIncidencia.Advertencia, fontSize);
        }

        public static bool? MostrarExito(string mensaje, double fontSize = 13)
        {
            return Mostrar(mensaje, TipoIncidencia.Exito, fontSize);
        }

        public static bool? MostrarInformacion(string mensaje, double fontSize = 13)
        {
            return Mostrar(mensaje, TipoIncidencia.Informacion, fontSize);
        }
    }
    public enum TipoIncidencia
    {
        Informacion,
        Advertencia,
        Error,
        Exito,
        Pregunta
    }
}
