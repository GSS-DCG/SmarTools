using System;
using System.Collections.Generic;
using System.IO;
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
using SAP2000v1;
using SmarTools.Model.Applications;

namespace SmarTools.APPS.ReaccionesTrackerAPP
{
    /// <summary>
    /// Lógica de interacción para ObtenerExcels.xaml
    /// </summary>
    public partial class ObtenerExcels : System.Windows.Controls.Page
    {
        public ObtenerExcels()
        {
            InitializeComponent();
        }


        // Instanciamos las clases necesarias
        ObtenerExcelsClass ObtenerExcelsInstance = new ObtenerExcelsClass();


        private void SearchSAPFiles_Click(object sender, RoutedEventArgs e)
        {
            ObtenerExcelsInstance.SearchSAPFiles(this);
        }


        private void SearchSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            ObtenerExcelsInstance.SearchSaveFolder(this);
        }


        private void ObtainExcels_Click(object sender, RoutedEventArgs e)
        {
            ObtenerExcelsInstance.ObtainExcels(this);
        }

    }
}
