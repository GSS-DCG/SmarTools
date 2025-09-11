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
using SAP2000v1;
using SmarTools.Model.Applications;

namespace SmarTools.APPS.ReaccionesTrackerAPP
{
    /// <summary>
    /// Lógica de interacción para RellenarPOT_Tracker.xaml
    /// </summary>
    public partial class RellenarPOT_Tracker : System.Windows.Controls.Page
    {
        public RellenarPOT_Tracker()
        {
            InitializeComponent();
        }


        // Instanciamos las clases necesarias
        RellenarPOT_TrackerClass RellenarPOT_TrackerInstance = new RellenarPOT_TrackerClass();


        private void SearchFolder2str_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.SearchFolder2str(this);
        }


        private void SearchFolder1ymedstr_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.SearchFolder1ymedstr(this);
        }


        private void SearchFolder1str_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.SearchFolder1str(this);
        }


        private void SearchFolder0ymedstr_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.SearchFolder0ymedstr(this);
        }


        private void SearchPOTFile_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.SearchPOTFile(this);
        }


        private void SearchPOTFolder_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.SearchPOTFolder(this);
        }


        private void SearchESMARoute_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.SearchESMARoute(this);
        }


        private void FillPOT_Click(object sender, RoutedEventArgs e)
        {
            RellenarPOT_TrackerInstance.FillPOT(this);
        }

    }
}
