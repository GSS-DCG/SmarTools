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

        private void VERIFICAR_ESTRUCTURA(object sender, RoutedEventArgs e)
        {
            ItaliaNTC2018.ComprobarNTC(CABEZA_MOTOR,CABEZA_GENERAL,PILAR,PILAR_MOTOR,VIGA_PRINCIPAL,VIGA_SECUNDARIA);
        }
    }
}
