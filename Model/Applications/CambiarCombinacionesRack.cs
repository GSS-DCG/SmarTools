using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.Model.Repository;
using SmarTools.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SmarTools.Model.Applications
{
    internal class CambiarCombinacionesRack
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        public static void GenerarCombinaciones(CambiarCombinacionesRackAPP vista)
        {
            var loadingWindow = new Status();
            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);

                bool[] HipotesisCarga =
                {
                    vista.Aplicar_Dead.IsChecked==true,
                    vista.Aplicar_PPaneles.IsChecked==true,
                    vista.Aplicar_Presion.IsChecked==true,
                    vista.Aplicar_Succion.IsChecked==true,
                    vista.Aplicar_Nieve.IsChecked==true,
                    vista.Aplicar_NieveAccidental.IsChecked==true,
                    vista.Aplicar_SismoX.IsChecked==true,
                    vista.Aplicar_SismoY.IsChecked==true
                };

                double[,] CoefELU = new double[,]
                {
                    {double.Parse(vista.Permanente_Persistente_Favorable.Text),double.Parse(vista.Permanente_Persistente_Desfavorable.Text),double.Parse(vista.Permanente_Accidental_Favorable.Text),double.Parse(vista.Permanente_Accidental_Desfavorable.Text) },
                    {double.Parse(vista.Permanente_NoCte_Persistente_Favorable.Text),double.Parse(vista.Permanente_NoCte_Persistente_Desfavorable.Text),double.Parse(vista.Permanente_NoCte_Accidental_Favorable.Text),double.Parse(vista.Permanente_NoCte_Accidental_Desfavorable.Text) },
                    {double.Parse(vista.Variable_Persistente_Favorable.Text),double.Parse(vista.Variable_Persistente_Desfavorable.Text),double.Parse(vista.Variable_Accidental_Favorable.Text),double.Parse(vista.Variable_Accidental_Desfavorable.Text) },
                    {double.Parse(vista.Accidental_Persistente_Favorable.Text),double.Parse(vista.Accidental_Persistente_Desfavorable.Text),double.Parse(vista.Accidental_Accidental_Favorable.Text),double.Parse(vista.Accidental_Accidental_Desfavorable.Text) },
                };

                double[,] CoefSimultaneidad;

                if (vista.Nieve_Menos1000_Check.IsChecked == true)
                {
                    CoefSimultaneidad = new double[,]
                    {
                         {
                         double.Parse(vista.Psi0_Menos1000.Text),
                         double.Parse(vista.Psi1_Menos1000.Text),
                         double.Parse(vista.Psi2_Menos1000.Text)
                         },
                         {
                         double.Parse(vista.Psi0_Viento.Text),
                         double.Parse(vista.Psi1_Viento.Text),
                         double.Parse(vista.Psi2_Viento.Text)
                         }
                    };
                }
                else if (vista.Nieve_Mas1000_Check.IsChecked == true)
                {
                    CoefSimultaneidad = new double[,]
                    {
                         {
                         double.Parse(vista.Psi0_Mas1000.Text),
                         double.Parse(vista.Psi1_Mas1000.Text),
                         double.Parse(vista.Psi2_Mas1000.Text)
                         },
                         {
                         double.Parse(vista.Psi0_Viento.Text),
                         double.Parse(vista.Psi1_Viento.Text),
                         double.Parse(vista.Psi2_Viento.Text)
                         }
                    };
                }

                double[,] CoefSLS = new double[,]
                {
                    {double.Parse(vista.Permanente_Favorable_SLS.Text),double.Parse(vista.Permanente_Desfavorable_SLS.Text)},
                    {double.Parse(vista.Permanente_NoCte_Favorable_SLS.Text),double.Parse(vista.Permanente_NoCte_Desfavorable_SLS.Text)},
                    {double.Parse(vista.Variable_Favorable_SLS.Text),double.Parse(vista.Variable_Desfavorable_SLS.Text)}
                };
            }
            finally
            {
                try
                {
                    loadingWindow.Close();
                }
                catch
                {
                    MessageBox.Show("Se ha producido un error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static void AplicarCombinaciones(CambiarCombinacionesRackAPP vista)
        {

        }
    }
}
