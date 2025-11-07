using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic;
using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.Model.Repository;
using SmarTools.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmarTools.Model.Applications
{
    public class Resultados
    {
        public string UNION { get; set; }
        public string ELEMENTO { get; set; }
        public string ESPESOR { get; set; }
        public string MATERIAL { get; set; }
        public string AXIL { get; set; }
        public string Vy { get; set; }
        public string Vz { get; set; }
        public string RESULTANTE { get; set; }
        public string MAXADM { get; set; }
        public string CHECK { get; set; }
    }

    class ComprobacionUnionesRack
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        public static void ComprobarUniones(ComprobacionUnionesRackAPP vista)
        {
            var loadingWindow = new Status();

            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();
                SAP.AnalysisSubclass.RunModel(mySapModel);

                mySapModel.SetPresentUnits(eUnits.kN_m_C);

                List<Resultados> resultados = new List<Resultados>();

                EsfuerzosVigaCorrea(vista, loadingWindow, resultados);

            }
            finally
            {
                try
                {
                    loadingWindow.Close();
                }
                catch
                {
                    var ventana = new Incidencias();
                    ventana.ConfigurarIncidencia("Se ha producido un error", TipoIncidencia.Error);
                    ventana.ShowDialog();
                }
            }
        }

        public static void EsfuerzosVigaCorrea(ComprobacionUnionesRackAPP vista, Status loadingwindow,List<Resultados> resultados)
        {

        }
    }
}
