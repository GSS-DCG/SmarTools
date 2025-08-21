using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using OfficeOpenXml;
using System.Windows.Controls;
using ClosedXML.Excel;
using SmarTools.Model.Repository;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using Microsoft.Office.Interop.Word;

namespace SmarTools.Model.Applications
{
    internal class ComprobacionUniones1V
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static string ruta = @"Z:\300SmarTools\03 Uniones\Uniones 1VR5_"+MainView.Globales._revisionUniones1V+".xlsx";
        public static void ComprobarUniones1V(ComprobacionUnionesAPP vista)
        {
            var loadingWindow = new Status();
            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                UnionBS(vista);
                UnionMS(vista);
                UnionBC(vista);
                UnionSB(vista);
                UnionMC(vista);
            }
            finally
            {
                try
                {
                    loadingWindow.Close();
                }
                catch
                {
                    MessageBox.Show("Se ha producido un error","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
        }

        public static Dictionary<string, double[]> CargarDesdeExcel(string rutaArchivo)
        {
            var datos = new Dictionary<string, double[]>();

            using (var workbook = new XLWorkbook(rutaArchivo))
            {
                var hoja = workbook.Worksheet(1); // Primera hoja
                var filas = hoja.RangeUsed().RowsUsed();

                foreach (var fila in filas.Skip(1)) // Saltar encabezado
                {
                    string nombre = fila.Cell(1).GetString();
                    double[] valores = new double[6];

                    for (int i = 0; i < 6; i++)
                    {
                        valores[i] = fila.Cell(i + 2).GetDouble();
                    }

                    datos[nombre] = valores;
                }
            }

            return datos;

        }

        public static void UnionBS(ComprobacionUnionesAPP vista)
        {
            //Datos de la unión
            var uniones = CargarDesdeExcel(ruta);
            double[] esfuerzos_BS = uniones["BS"];

            //Obtenemos los esfuerzos en las cabezas de los pilares
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            double X = 0, Y = 0, Z = 0;
            mySapModel.PointElm.GetCoordCartesian("mps",ref X,ref Y,ref Z);
            double[] esfuerzos_BS_n = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", SAP.ElementFinderSubclass.TrackerSubclass.NorthPiles(mySapModel), Z);
            double[] esfuerzos_BS_s = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", SAP.ElementFinderSubclass.TrackerSubclass.SouthPiles(mySapModel), Z);

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_BS, vista.Pmax_BS, vista.V2max_BS, vista.V3max_BS, vista.Tmax_BS, vista.M2max_BS, vista.M3max_BS
            };

            labels_max[0].Content = "BS-1VR5";
            for (int i = 1;i<= 6;i++)
            {
                labels_max[i].Content = esfuerzos_BS[i-1];
            }

            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_BS, vista.P_BS, vista.V2_BS, vista.V3_BS, vista.T_BS, vista.M2_BS, vista.M3_BS
            };
            labels_esfuerzos[0].Content = "55º";
            for (int i = 1; i <= 6; i++)
            {
                labels_esfuerzos[i].Content = Math.Max(esfuerzos_BS_n[i-1], esfuerzos_BS_s[i-1]).ToString("F3");
            }

            //Cogemos los valores que necesitamos
            double.TryParse(vista.P_BS.Content?.ToString(), out double P);
            double.TryParse(vista.V2_BS.Content?.ToString(), out double V2);
            double.TryParse(vista.M2_BS.Content?.ToString(), out double M2);
            double.TryParse(vista.M3_BS.Content?.ToString(), out double M3);
            double.TryParse(vista.Pmax_BS.Content?.ToString(), out double Pmax);
            double.TryParse(vista.V2max_BS.Content?.ToString(), out double V2max);
            double.TryParse(vista.M2max_BS.Content?.ToString(), out double M2max);
            double.TryParse(vista.M3max_BS.Content?.ToString(), out double M3max);

            //Coloreamos todos los labels en verde por defecto
            var labelsVerificar = new[] { vista.P_BS, vista.V2_BS, vista.V3_BS, vista.T_BS, vista.M2_BS, vista.M3_BS };
            foreach (var label in labelsVerificar)
            {
                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
            }

            // Evaluar condiciones
            bool cumpleP = P <= Pmax;
            bool cumpleV2 = V2 <= V2max;
            bool cumpleM2 = M2 <= M2max;
            bool cumpleM3 = M3 <= M3max;

            vista.RecuadroBS.Background = (cumpleP && cumpleV2 && cumpleM2 && cumpleM3) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleP) vista.P_BS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleV2) vista.V2_BS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM2) vista.M2_BS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM3) vista.M3_BS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
        }

        public static void UnionMS(ComprobacionUnionesAPP vista)
        {
            //Datos de la unión
            var uniones = CargarDesdeExcel(ruta);
            double[] esfuerzos_MS = uniones["MS"];

            //Obtenemos los esfuerzos en la cabeza del pilar motor
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            double X = 0, Y = 0, Z = 0;
            mySapModel.PointElm.GetCoordCartesian("mps", ref X, ref Y, ref Z);
            string[] pilar_MP = new string[] {"Column_0" };
            double[] esfuerzos_MS_modelo = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", pilar_MP, Z);

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_MS, vista.Pmax_MS, vista.V2max_MS, vista.V3max_MS, vista.Tmax_MS, vista.M2max_MS, vista.M3max_MS
            };

            labels_max[0].Content = "MS-1VR5";
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_MS[i - 1];
            }

            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_MS, vista.P_MS, vista.V2_MS, vista.V3_MS, vista.T_MS, vista.M2_MS, vista.M3_MS
            };
            labels_esfuerzos[0].Content = "55º";
            for (int i = 1; i <= 6; i++)
            {
                labels_esfuerzos[i].Content = esfuerzos_MS_modelo[i-1].ToString("F3");
            }

            //Cogemos los valores que necesitamos
            double.TryParse(vista.P_MS.Content?.ToString(), out double P);
            double.TryParse(vista.V2_MS.Content?.ToString(), out double V2);
            double.TryParse(vista.M2_MS.Content?.ToString(), out double M2);
            double.TryParse(vista.M3_MS.Content?.ToString(), out double M3);
            double.TryParse(vista.Pmax_MS.Content?.ToString(), out double Pmax);
            double.TryParse(vista.V2max_MS.Content?.ToString(), out double V2max);
            double.TryParse(vista.M2max_MS.Content?.ToString(), out double M2max);
            double.TryParse(vista.M3max_MS.Content?.ToString(), out double M3max);

            //Coloreamos todos los labels en verde por defecto
            var labelsVerificar = new[] { vista.P_MS, vista.V2_MS, vista.V3_MS, vista.T_MS, vista.M2_MS, vista.M3_MS };
            foreach (var label in labelsVerificar)
            {
                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
            }

            // Evaluar condiciones
            bool cumpleP = P <= Pmax;
            bool cumpleV2 = V2 <= V2max;
            bool cumpleM3 = M3 <= M3max;

            vista.RecuadroMS.Background = (cumpleP && cumpleV2 && cumpleM3) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleP) vista.P_MS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleV2) vista.V2_MS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM3) vista.M3_MS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
        }

        public static void UnionBC(ComprobacionUnionesAPP vista)
        {
            //Datos de la unión
            var uniones = CargarDesdeExcel(ruta);
            double[] esfuerzos_BC = uniones["BC"];

            //Obtenemos los esfuerzos de la BC
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            string[] vigas_n = SAP.ElementFinderSubclass.TrackerSubclass.NorthBeams(mySapModel);
            string[] vigas_s = SAP.ElementFinderSubclass.TrackerSubclass.SouthBeams(mySapModel);
            double[] maximosNorte = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel, vigas_n);
            double[] maximosSur = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel,vigas_s);

            double[] esfuerzos_BC_modelo = maximosNorte
             .Zip(maximosSur, (n, s) => Math.Max(Math.Abs(n), Math.Abs(s)))
             .ToArray();

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_BC, vista.Pmax_BC, vista.V2max_BC, vista.V3max_BC, vista.Tmax_BC, vista.M2max_BC, vista.M3max_BC
            };

            labels_max[0].Content = "BC-1VR5";
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_BC[i - 1];
            }

            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_BC, vista.P_BC, vista.V2_BC, vista.V3_BC, vista.T_BC, vista.M2_BC, vista.M3_BC
            };
            labels_esfuerzos[0].Content = "55º";
            for (int i = 1; i <= 6; i++)
            {
                labels_esfuerzos[i].Content = esfuerzos_BC_modelo[i - 1].ToString("F3");
            }

            //Cogemos los valores que necesitamos
            double.TryParse(vista.V2_BC.Content?.ToString(), out double V2);
            double.TryParse(vista.T_BC.Content?.ToString(), out double T);
            double.TryParse(vista.M3_BC.Content?.ToString(), out double M3);
            double.TryParse(vista.V2max_BC.Content?.ToString(), out double V2max);
            double.TryParse(vista.Tmax_BC.Content?.ToString(), out double Tmax);
            double.TryParse(vista.M3max_BC.Content?.ToString(), out double M3max);

            //Coloreamos todos los labels en verde por defecto
            var labelsVerificar = new[] { vista.P_BC, vista.V2_BC, vista.V3_BC, vista.T_BC, vista.M2_BC, vista.M3_BC };
            foreach (var label in labelsVerificar)
            {
                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
            }

            // Evaluar condiciones
            bool cumpleV2 = V2 <= V2max;
            bool cumpleT = T <= Tmax;
            bool cumpleM3 = M3 <= M3max;

            vista.RecuadroBC.Background = (cumpleV2 && cumpleT && cumpleM3) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleV2) vista.V2_BC.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleT) vista.T_BC.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM3) vista.M3_BC.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
        }

        public static void UnionSB(ComprobacionUnionesAPP vista)
        {
            //Obtenemos el perfil y espesor de la secundaria del modelo
            string[] secundarias_norte_sup = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, true);
            string[] secundarias_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, false);
            string PropName = "", SAuto = "";
            mySapModel.FrameObj.GetSection(secundarias_norte_inf[1], ref PropName, ref SAuto);
            double espesor = double.Parse(PropName.Split('/')[0].Split('-')[1].Split('x').Last().Trim());

            //Datos de la unión
            var uniones = CargarDesdeExcel(ruta);
            double[] esfuerzos_SB=new double[6];
            switch(espesor)
            {
                case 1.3:
                    esfuerzos_SB = uniones["SB_1,3"];
                    break;

                case 1.5:
                    esfuerzos_SB = uniones["SB_1,5"];
                    break;

                case 1.6:
                    esfuerzos_SB = uniones["SB_1,6"];
                    break;

                case 1.8:
                    esfuerzos_SB = uniones["SB_1,8"];
                    break;

                case 2:
                    esfuerzos_SB = uniones["SB_2"];
                    break;
            }

            //Obtenemos los esfuerzos de la unión y rellenamos los datos de la tabla
            double[] esfuerzos_SB_sup = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", secundarias_norte_sup, 0);
            double[] esfuerzos_SB_inf = SAP.AnalysisSubclass.GetFrameForces(mySapModel,"ULS",secundarias_norte_inf, 0);
            Label[] label_esfuerzos = {vista.Ang_SB, vista.P_SB, vista.V2_SB, vista.V3_SB, vista.T_SB, vista.M2_SB, vista.M3_SB};
            label_esfuerzos[0].Content = "55º";
            bool[] suma = { true, true, false, false, false, false };
            for (int i = 1; i <= 6; i++)
            {
                double resultado = suma[i-1]
                    ? Math.Abs(esfuerzos_SB_sup[i-1] + esfuerzos_SB_inf[i-1])
                    : Math.Abs(esfuerzos_SB_sup[i-1] - esfuerzos_SB_inf[i-1]);
                label_esfuerzos[i].Content = resultado.ToString("F3");
            }

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_SB, vista.Pmax_SB, vista.V2max_SB, vista.V3max_SB, vista.Tmax_SB, vista.M2max_SB, vista.M3max_SB
            };

            labels_max[0].Content = "e="+espesor;
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_SB[i - 1];
            }

            //Cogemos los valores que necesitamos
            double.TryParse(vista.V3_SB.Content?.ToString(), out double V3);
            double.TryParse(vista.M2_SB.Content?.ToString(), out double M2);
            double.TryParse(vista.V3max_SB.Content?.ToString(), out double V3max);
            double.TryParse(vista.M2max_SB.Content?.ToString(), out double M2max);

            //Coloreamos todos los labels en verde por defecto
            var labelsVerificar = new[] { vista.P_SB, vista.V2_SB, vista.V3_SB, vista.T_SB, vista.M2_SB, vista.M3_SB };
            foreach (var label in labelsVerificar)
            {
                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
            }

            // Evaluar condiciones
            bool cumpleV3 = V3 <= V3max;
            bool cumpleM2 = M2 <= M2max;

            vista.RecuadroSB.Background = (cumpleV3 && cumpleM2) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleV3) vista.V3_SB.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM2) vista.M2_SB.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

        }

        public static void UnionMC(ComprobacionUnionesAPP vista)
        {
            //Datos de la unión
            var uniones = CargarDesdeExcel(ruta);
            double[] esfuerzos_MC = uniones["MC"];

            //Obtenemos los esfuerzos de la MC
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            string[] vigas_n = SAP.ElementFinderSubclass.TrackerSubclass.NorthBeams(mySapModel);
            string[] vigas_s = SAP.ElementFinderSubclass.TrackerSubclass.SouthBeams(mySapModel);
            double[] maximosNorte = SAP.AnalysisSubclass.ObtenerEsfuerzosEnExtremo(mySapModel, vigas_n[0], 0);
            double[] maximosSur = SAP.AnalysisSubclass.ObtenerEsfuerzosEnExtremo(mySapModel, vigas_s[0], 0);

            double[] esfuerzos_MC_modelo = maximosNorte
             .Zip(maximosSur, (n, s) => Math.Max(Math.Abs(n), Math.Abs(s)))
             .ToArray();

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_MC, vista.Pmax_MC, vista.V2max_MC, vista.V3max_MC, vista.Tmax_MC, vista.M2max_MC, vista.M3max_MC
            };

            labels_max[0].Content = "MC-1VR5";
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_MC[i - 1];
            }

            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_MC, vista.P_MC, vista.V2_MC, vista.V3_MC, vista.T_MC, vista.M2_MC, vista.M3_MC
            };
            labels_esfuerzos[0].Content = "55º";
            for (int i = 1; i <= 6; i++)
            {
                labels_esfuerzos[i].Content = esfuerzos_MC_modelo[i - 1].ToString("F3");
            }

            //Cogemos los valores que necesitamos
            double.TryParse(vista.V2_MC.Content?.ToString(), out double V2);
            double.TryParse(vista.T_MC.Content?.ToString(), out double T);
            double.TryParse(vista.M3_MC.Content?.ToString(), out double M3);
            double.TryParse(vista.V2max_MC.Content?.ToString(), out double V2max);
            double.TryParse(vista.Tmax_MC.Content?.ToString(), out double Tmax);
            double.TryParse(vista.M3max_MC.Content?.ToString(), out double M3max);

            //Coloreamos todos los labels en verde por defecto
            var labelsVerificar = new[] { vista.P_MC, vista.V2_MC, vista.V3_MC, vista.T_MC, vista.M2_MC, vista.M3_MC };
            foreach (var label in labelsVerificar)
            {
                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
            }

            // Evaluar condiciones
            bool cumpleV2 = V2 <= V2max;
            bool cumpleT = T <= Tmax;
            bool cumpleM3 = M3 <= M3max;

            vista.RecuadroMC.Background = (cumpleV2 && cumpleT && cumpleM3) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleV2) vista.V2_MC.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleT) vista.T_MC.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM3) vista.M3_MC.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
        }
    }
}
