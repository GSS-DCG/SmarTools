using System;
using System.Collections.Generic;
using System.IO.Packaging;
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
using System.IO;

namespace SmarTools.ViewModel
{
    /// <summary>
    /// Lógica de interacción para Inicio.xaml
    /// </summary>
    public partial class Inicio : UserControl
    {
        public Inicio()
        {
            InitializeComponent();
            InfoVersionView.Text = InfoVersion();
        }

        public static string InfoVersion()
        {
            string ruta = @"Z:\300Logos\Cambios_Version.txt";
            string texto = File.ReadAllText(ruta);
            return texto;
        }
    }
}
