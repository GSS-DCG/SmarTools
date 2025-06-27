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
    class ComprobacionUniones2V
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static string ruta = @"Z:\300Logos\03 Uniones\Uniones 2VR4.xlsx";

        public static void ComprobarUniones2V(ComprobacionUniones2VAPP vista)
        {
            var loadingWindow = new Status();
            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                UnionBS(vista);
                UnionMS(vista);
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
                    MessageBox.Show("Se ha producido un error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static Dictionary<string, double[]> CargarDesdeExcel()
        {
            var datos = new Dictionary<string, double[]>();

            using (var workbook = new XLWorkbook(ruta))
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

        public static void UnionBS(ComprobacionUniones2VAPP vista)
        {
            //Identificar los perfiles de los pilares generales
            string[] GP_norte=SAP.ElementFinderSubclass.TrackerSubclass.NorthPiles(mySapModel);
            string[] GP_sur=SAP.ElementFinderSubclass.TrackerSubclass.SouthPiles(mySapModel);
            string[] seccion_tipo = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, GP_norte[0]);

            //Datos de la unión
            var uniones = CargarDesdeExcel();
            string union = "";
            bool cambiarBS = false;

            if (seccion_tipo[0].StartsWith("C-195") || seccion_tipo[0].StartsWith("W8"))
            {
                union = "BS-22A";
            }
            if (seccion_tipo[0].StartsWith("C-175")|| seccion_tipo[0].StartsWith("W6"))
            {
                union = "BS-22B/C";
                cambiarBS=true;
            }
            double[] esfuerzos_BS = uniones[union];

            //Obtenemos los esfuerzos en las cabezas de los pilares
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            double X = 0, Y = 0, Z = 0;
            mySapModel.PointElm.GetCoordCartesian("mps", ref X, ref Y, ref Z);
            double[] esfuerzos_BS_n = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", GP_norte, Z);
            double[] esfuerzos_BS_s = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", GP_sur, Z);

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_BS, vista.Pmax_BS, vista.V2max_BS, vista.V3max_BS, vista.Tmax_BS, vista.M2max_BS, vista.M3max_BS
            };

            labels_max[0].Content = union;
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_BS[i - 1];
            }

            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_BS, vista.P_BS, vista.V2_BS, vista.V3_BS, vista.T_BS, vista.M2_BS, vista.M3_BS
            };
            labels_esfuerzos[0].Content = "Envolvente";
            for (int i = 1; i <= 6; i++)
            {
                labels_esfuerzos[i].Content = Math.Max(esfuerzos_BS_n[i - 1], esfuerzos_BS_s[i - 1]).ToString("F3");
            }

            //Cogemos los valores que necesitamos
            double.TryParse(vista.P_BS.Content?.ToString(), out double P);
            double.TryParse(vista.V2_BS.Content?.ToString(), out double V2);
            double.TryParse(vista.M2_BS.Content?.ToString(), out double M2);
            double.TryParse(vista.Pmax_BS.Content?.ToString(), out double Pmax);
            double.TryParse(vista.V2max_BS.Content?.ToString(), out double V2max);
            double.TryParse(vista.M2max_BS.Content?.ToString(), out double M2max);

            bool cumpleP = P <= Pmax;
            bool cumpleV2 = V2 <= V2max;
            bool cumpleM2 = M2 <= M2max;

            //Si no cumple y se puede cambiar la BS
            if(cambiarBS==true && (cumpleP&&cumpleV2&&cumpleM2)==false)
            {

                union = "BS-22A";
                esfuerzos_BS = uniones[union];

                // Actualizar tabla de máximos
                labels_max[0].Content = union;
                for (int i = 1; i <= 6; i++)
                {
                    labels_max[i].Content = esfuerzos_BS[i - 1];
                }

                // Reevaluar condiciones
                Pmax = esfuerzos_BS[0];
                V2max = esfuerzos_BS[1];
                M2max = esfuerzos_BS[5];

                cumpleP = P <= Pmax;
                cumpleV2 = V2 <= V2max;
                cumpleM2 = M2 <= M2max;

            }

            //Coloreamos todos los labels en verde por defecto
            var labelsVerificar = new[] { vista.P_BS, vista.V2_BS, vista.V3_BS, vista.T_BS, vista.M2_BS, vista.M3_BS };
            foreach (var label in labelsVerificar)
            {
                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
            }

            vista.RecuadroBS.Background = (cumpleP && cumpleV2 && cumpleM2) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleP) vista.P_BS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleV2) vista.V2_BS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM2) vista.M2_BS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
        }

        public static void UnionMS(ComprobacionUniones2VAPP vista)
        {
            //Identificar los perfiles del pilar motor
            string[] pilar_MP = new string[] { "Column_0" };
            string[] seccion_tipo = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, pilar_MP[0]);

            //Datos de la unión
            var uniones = CargarDesdeExcel();
            string union = "";

            if (seccion_tipo[0].Contains("W6"))
            {
                union = "MS-04";
            }
            if (seccion_tipo[0].Contains("W8"))
            {
                union = "MS-02";
            }
            //if (seccion_tipo[0].Contains("W8X24"))
            //{
            //    union = "MS-03";
            //}
            double[] esfuerzos_MS = uniones[union];

            //Obtenemos los esfuerzos en la cabeza del pilar motor
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            double X = 0, Y = 0, Z = 0;
            mySapModel.PointElm.GetCoordCartesian("mps", ref X, ref Y, ref Z);
            double[] esfuerzos_MS_modelo = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", pilar_MP, Z);

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_MS, vista.Pmax_MS, vista.V2max_MS, vista.V3max_MS, vista.Tmax_MS, vista.M2max_MS, vista.M3max_MS
            };

            labels_max[0].Content = union;
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_MS[i - 1];
            }

            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_MS, vista.P_MS, vista.V2_MS, vista.V3_MS, vista.T_MS, vista.M2_MS, vista.M3_MS
            };
            labels_esfuerzos[0].Content = "Envolvente";
            for (int i = 1; i <= 6; i++)
            {
                labels_esfuerzos[i].Content = esfuerzos_MS_modelo[i - 1].ToString("F3");
            }

            //Cogemos los valores que necesitamos
            double.TryParse(vista.P_MS.Content?.ToString(), out double P);
            double.TryParse(vista.V2_MS.Content?.ToString(), out double V2);
            double.TryParse(vista.M3_MS.Content?.ToString(), out double M3);
            double.TryParse(vista.Pmax_MS.Content?.ToString(), out double Pmax);
            double.TryParse(vista.V2max_MS.Content?.ToString(), out double V2max);
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

            if((union=="MS-02")==true && (cumpleP && cumpleV2 && cumpleM3)==false)
            {
                union = "MS-03";
                esfuerzos_MS = uniones[union];

                //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
                labels_max = new Label[]
                {
                vista.Tipo_MS, vista.Pmax_MS, vista.V2max_MS, vista.V3max_MS, vista.Tmax_MS, vista.M2max_MS, vista.M3max_MS
                };

                labels_max[0].Content = union;
                for (int i = 1; i <= 6; i++)
                {
                    labels_max[i].Content = esfuerzos_MS[i - 1];
                }
                //Rellenamos la parte de la tabla de esfuerzos del modelo
                labels_esfuerzos = new Label[]
                {
                vista.Ang_MS, vista.P_MS, vista.V2_MS, vista.V3_MS, vista.T_MS, vista.M2_MS, vista.M3_MS
                };
                labels_esfuerzos[0].Content = "Envolvente";
                for (int i = 1; i <= 6; i++)
                {
                    labels_esfuerzos[i].Content = esfuerzos_MS_modelo[i - 1].ToString("F3");
                }

                //Cogemos los valores que necesitamos
                double.TryParse(vista.P_MS.Content?.ToString(), out P);
                double.TryParse(vista.V2_MS.Content?.ToString(), out V2);
                double.TryParse(vista.M3_MS.Content?.ToString(), out M3);
                double.TryParse(vista.Pmax_MS.Content?.ToString(), out Pmax);
                double.TryParse(vista.V2max_MS.Content?.ToString(), out V2max);
                double.TryParse(vista.M3max_MS.Content?.ToString(), out M3max);

                // Evaluar condiciones
                cumpleP = P <= Pmax;
                cumpleV2 = V2 <= V2max;
                cumpleM3 = M3 <= M3max;
            }

            vista.RecuadroMS.Background = (cumpleP && cumpleV2 && cumpleM3) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleP) vista.P_MS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleV2) vista.V2_MS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM3) vista.M3_MS.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
        }

        public static void UnionSB(ComprobacionUniones2VAPP vista)
        {
            //Obtenemos el perfil y espesor de la secundaria del modelo
            string[] secundarias_norte_sup = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel, true);
            string[] secundarias_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel, false);
            string PropName = "", SAuto = "";
            mySapModel.FrameObj.GetSection(secundarias_norte_inf[1], ref PropName, ref SAuto);
            double espesor = double.Parse(PropName.Split('/')[0].Split('-')[1].Split('x').Last().Trim());
            double altura = double.Parse(PropName.Split('/')[0].Split('-')[1].Split('x')[0].Trim());
            string material = PropName.Split('/')[1].Trim();
            string tipo = "";
            if(vista.Refuerzo_Secundaria.SelectedIndex == 0)
            {
                tipo = "A";
            }
            if (vista.Refuerzo_Secundaria.SelectedIndex == 1)
            {
                tipo = "B";
            }

            //Datos de la unión
            var uniones = CargarDesdeExcel();
            double[] esfuerzos_SB = new double[6];
            
            switch ((altura,espesor,material,tipo))
            {
                case (75,1.6,"S350GD","B"):
                    esfuerzos_SB = uniones["2V-75x1,6 S350 (B)"];
                    break;

                case (75, 1.6, "S350GD_R", "B"):
                    esfuerzos_SB = uniones["2V-75x1,6 S350 (B)"];
                    break;

                case (75, 1.6, "S420GD", "B"):
                    esfuerzos_SB = uniones["2V-75x1,6 S420 (B)"];
                    break;

                case (75, 1.6, "S420GD_R", "B"):
                    esfuerzos_SB = uniones["2V-75x1,6 S420 (B)"];
                    break;

                case (90, 1.6, "S420GD", "B"):
                    esfuerzos_SB = uniones["2V-90x1,6 S420 (B)"];
                    break;

                case (90, 1.6, "S420GD_R", "B"):
                    esfuerzos_SB = uniones["2V-90x1,6 S420 (B)"];
                    break;

                case (90, 1.6, "S420GD", "A"):
                    esfuerzos_SB = uniones["2V-90x1,6 S420 (A)"];
                    break;

                case (90, 1.6, "S420GD_R", "A"):
                    esfuerzos_SB = uniones["2V-90x1,6 S420 (A)"];
                    break;

                case (90, 1.8, "S420GD", "A"):
                    esfuerzos_SB = uniones["2V-90x1,8 S420 (A)"];
                    break;

                case (90, 1.8, "S420GD_R", "A"):
                    esfuerzos_SB = uniones["2V-90x1,8 S420 (A)"];
                    break;

                case (90, 2, "S420GD", "A"):
                    esfuerzos_SB = uniones["2V-90x2,0 S420 (A)"];
                    break;

                case (90, 2, "S420GD_R", "A"):
                    esfuerzos_SB = uniones["2V-90x2,0 S420 (A)"];
                    break;

                default:
                    MessageBox.Show("No se encuentra la combinación de secundaria y refuerzo de secundaria", "Aviso");
                    break;
            }

            //Obtenemos los esfuerzos de la unión y rellenamos los datos de la tabla
            double[] esfuerzos_SB_sup = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", secundarias_norte_sup, 0);
            double[] esfuerzos_SB_inf = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", secundarias_norte_inf, 0);
            Label[] label_esfuerzos = { vista.Ang_SB, vista.P_SB, vista.V2_SB, vista.V2inf_SB, vista.T_SB, vista.M2_SB, vista.M3_SB };
            label_esfuerzos[0].Content = "Envolvente";
            bool[] suma = { true, true, false, true, true, true };
            for (int i = 1; i <= 6; i++)
            {
                double resultado;
                if(i==6)
                {
                    resultado=Math.Abs(esfuerzos_SB_sup[i - 1] - esfuerzos_SB_inf[i - 1]);
                    label_esfuerzos[i].Content = resultado.ToString("F3");
                }
                else
                {
                    resultado = suma[i - 1]
                    ? Math.Abs(esfuerzos_SB_sup[i - 1] + esfuerzos_SB_inf[i - 1])
                    : Math.Abs(esfuerzos_SB_inf[i - 2]);
                    label_esfuerzos[i].Content = resultado.ToString("F3");
                }
            }

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_SB, vista.Pmax_SB, vista.V2max_SB, vista.V2infmax_SB, vista.Tmax_SB, vista.M2max_SB, vista.M3max_SB
            };

            labels_max[0].Content = altura+"x"+espesor+"("+material+")";
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_SB[i - 1];
            }

            //Cogemos los valores que necesitamos

            double.TryParse(vista.V2_SB.Content?.ToString(), out double V2);
            double.TryParse(vista.V2inf_SB.Content?.ToString(), out double V2inf);
            double.TryParse(vista.M3_SB.Content?.ToString(), out double M3);
            double.TryParse(vista.V2max_SB.Content?.ToString(), out double V2max);
            double.TryParse(vista.V2infmax_SB.Content?.ToString(), out double V2infmax);
            double.TryParse(vista.M3max_SB.Content?.ToString(), out double M3max);

            //Coloreamos todos los labels en verde por defecto
            var labelsVerificar = new[] { vista.P_SB, vista.V2_SB, vista.V2inf_SB, vista.T_SB, vista.M3_SB, vista.M3_SB };
            foreach (var label in labelsVerificar)
            {
                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
            }

            // Evaluar condiciones
            bool cumpleV2 = V2 <= V2max;
            bool cumpleV2inf = V2inf <= V2infmax;
            bool cumpleM3 = M3 <= M3max;

            vista.RecuadroSB.Background = (cumpleV2&&cumpleV2inf && cumpleM3) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

            if (!cumpleV2) vista.V2_SB.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleV2inf) vista.V2inf_SB.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            if (!cumpleM3) vista.M3_SB.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

        }

        public static void UnionMC(ComprobacionUniones2VAPP vista)
        {
            //Datos de la unión
            var uniones = CargarDesdeExcel();
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

            labels_max[0].Content = "MC-2VR4";
            for (int i = 1; i <= 6; i++)
            {
                labels_max[i].Content = esfuerzos_MC[i - 1];
            }

            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_MC, vista.P_MC, vista.V2_MC, vista.V3_MC, vista.T_MC, vista.M2_MC, vista.M3_MC
            };
            labels_esfuerzos[0].Content = "3º";
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
