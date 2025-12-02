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
        public static ComprobacionUniones2VAPP vista = new ComprobacionUniones2VAPP();
        public static string revision = vista.Revisión.Text;
        public static string ruta = @"Z:\300SmarTools\03 Uniones\Uniones "+revision+"_"+MainView.Globales._revisionUniones1V+".xlsx";

        public static void ComprobarUniones2V(ComprobacionUniones2VAPP vista)
        {
            var loadingWindow = new Status();
            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                revision = vista.Revisión.Text;
                ruta = @"Z:\300SmarTools\03 Uniones\Uniones " + revision + "_" + MainView.Globales._revisionUniones1V + ".xlsx";

                UnionBS(vista);
                UnionMS(vista);
                UnionSB(vista);
                UnionMC(vista);
                UnionBC(vista);
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
            if(vista.Num_String.Text=="1")
            {
                if (seccion_tipo[0].StartsWith("C-195") || seccion_tipo[0].StartsWith("W8"))
                {
                    union = "BS-22A";
                }
                if (seccion_tipo[0].StartsWith("C-175") || seccion_tipo[0].StartsWith("W6"))
                {
                    union = "BS-22B/C";
                    cambiarBS = true;
                }
            }
            else if (vista.Num_String.Text == "2")
            {
                union = "BS-22B/C";
                cambiarBS = true;
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
            string unionFunc = "";
            if(vista.Num_String.SelectedIndex==0)//1ST
            {
                if (seccion_tipo[0].Contains("W6"))
                {
                    union = "T5MS-04 DEF";
                    unionFunc = "T5MS-04 FUN";
                }
                if (seccion_tipo[0].Contains("W8"))
                {
                    union = "T5MS-02 DEF";
                    unionFunc = "T5MS-02 FUN";
                }
            }
            else if (vista.Num_String.SelectedIndex == 1)
            {
                union = "T4MS-04 DEF";
                unionFunc = "T4MS-04 FUN";
            }
            double[] esfuerzos_MS = uniones[union];
            double[] esfuerzos_MSFunc = uniones[unionFunc];

            //Obtenemos los esfuerzos en la cabeza del pilar motor
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            double X = 0, Y = 0, Z = 0;
            string succion = BuscarCombinacionMaxSuccion();
            mySapModel.PointElm.GetCoordCartesian("mps", ref X, ref Y, ref Z);
            double[] esfuerzos_MS_modelo = SAP.AnalysisSubclass.GetFrameForces(mySapModel, "ULS", pilar_MP, Z);
            double[] esfuerzos_MSFunc_modelo = SAP.AnalysisSubclass.GetFrameForces(mySapModel, succion, pilar_MP, Z);

            //Rellenamos la parte de la tabla de esfuerzos máximos admisibles
            var labels_max = new Label[]
            {
                vista.Tipo_MS, vista.Pmax_MS, vista.V2max_MS, vista.V3max_MS, vista.Tmax_MS, vista.M2max_MS, vista.M3max_MS
            };
            var labels_maxFunc = new Label[]
            {
                vista.Tipo_MSFunc, vista.Pmax_MSFunc, vista.V2max_MSFunc, vista.V3max_MSFunc, vista.Tmax_MSFunc, vista.M2max_MSFunc, vista.M3max_MSFunc
            };
            if (vista.Posicion.Text=="Defensa")
            {
                labels_max[0].Content = union;
                for (int i = 1; i <= 6; i++)
                {
                    labels_max[i].Content = esfuerzos_MS[i - 1];
                }

                labels_maxFunc[0].Content = unionFunc;
                for (int i = 1; i <= 6; i++)
                {
                    labels_maxFunc[i].Content = "-";
                }
            }
            else if(vista.Posicion.Text=="Funcionamiento")
            {
                labels_max[0].Content = union;
                for (int i = 1; i <= 6; i++)
                {
                    labels_max[i].Content = "-";
                }

                labels_maxFunc[0].Content = unionFunc;
                for (int i = 1; i <= 6; i++)
                {
                    labels_maxFunc[i].Content = esfuerzos_MSFunc[i - 1];
                }
            }
            //Rellenamos la parte de la tabla de esfuerzos del modelo
            var labels_esfuerzos = new Label[]
            {
                vista.Ang_MS, vista.P_MS, vista.V2_MS, vista.V3_MS, vista.T_MS, vista.M2_MS, vista.M3_MS
            };
            var labels_esfuerzosFunc = new Label[]
            {
                vista.Ang_MSFunc, vista.P_MSFunc, vista.V2_MSFunc, vista.V3_MSFunc, vista.T_MSFunc, vista.M2_MSFunc, vista.M3_MSFunc
            };

            if (vista.Posicion.Text=="Defensa")
            {
                labels_esfuerzos[0].Content = "Envolvente";
                for (int i = 1; i <= 6; i++)
                {
                    labels_esfuerzos[i].Content = esfuerzos_MS_modelo[i - 1].ToString("F3");
                }
                labels_esfuerzosFunc[0].Content = "Func. Succión";
                for (int i = 1; i <= 6; i++)
                {
                    labels_esfuerzosFunc[i].Content = "-";
                }
            }
            else if(vista.Posicion.Text=="Funcionamiento")
            {
                labels_esfuerzos[0].Content = "Envolvente";
                for (int i = 1; i <= 6; i++)
                {
                    labels_esfuerzos[i].Content = "-";
                }
                labels_esfuerzosFunc[0].Content = succion;
                for (int i = 1; i <= 6; i++)
                {
                    labels_esfuerzosFunc[i].Content = esfuerzos_MSFunc_modelo[i - 1].ToString("F3");
                }
            }

            //Cogemos los valores que necesitamos
            if (vista.Posicion.Text == "Defensa")
            {
                double.TryParse(vista.P_MS.Content?.ToString(), out double P);
                double.TryParse(vista.V2_MS.Content?.ToString(), out double V2);
                double.TryParse(vista.M3_MS.Content?.ToString(), out double M3);
                double.TryParse(vista.Pmax_MS.Content?.ToString(), out double Pmax);
                double.TryParse(vista.V2max_MS.Content?.ToString(), out double V2max);
                double.TryParse(vista.M3max_MS.Content?.ToString(), out double M3max);

                //Coloreamos todos los labels en verde por defecto
                var labelsVerificar = new[] { vista.P_MS, vista.V2_MS, vista.V3_MS, vista.T_MS, vista.M2_MS, vista.M3_MS, vista.P_MSFunc, vista.V2_MSFunc, vista.V3_MSFunc, vista.T_MSFunc, vista.M2_MSFunc, vista.M3_MSFunc };
                foreach (var label in labelsVerificar)
                {
                    label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
                }

                // Evaluar condiciones
                bool cumpleP = P <= Pmax;
                bool cumpleV2 = V2 <= V2max;
                bool cumpleM3 = M3 <= M3max;

                if ((union == "MS-02") == true && (cumpleP && cumpleV2 && cumpleM3) == false)
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
            else if (vista.Posicion.Text == "Funcionamiento")
            {
                double.TryParse(vista.P_MSFunc.Content?.ToString(), out double PFunc);
                double.TryParse(vista.V2_MSFunc.Content?.ToString(), out double V2Func);
                double.TryParse(vista.M3_MSFunc.Content?.ToString(), out double M3Func);
                double.TryParse(vista.Pmax_MSFunc.Content?.ToString(), out double PmaxFunc);
                double.TryParse(vista.V2max_MSFunc.Content?.ToString(), out double V2maxFunc);
                double.TryParse(vista.M3max_MSFunc.Content?.ToString(), out double M3maxFunc);

                //Coloreamos todos los labels en verde por defecto
                var labelsVerificar = new[] { vista.P_MS, vista.V2_MS, vista.V3_MS, vista.T_MS, vista.M2_MS, vista.M3_MS, vista.P_MSFunc, vista.V2_MSFunc, vista.V3_MSFunc, vista.T_MSFunc, vista.M2_MSFunc, vista.M3_MSFunc };
                foreach (var label in labelsVerificar)
                {
                    label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79));
                }

                // Evaluar condiciones
                bool cumpleP = PFunc <= PmaxFunc;
                bool cumpleV2 = V2Func <= V2maxFunc;
                bool cumpleM3 = M3Func <= M3maxFunc;

                if ((union == "MS-02") == true && (cumpleP && cumpleV2 && cumpleM3) == false)
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
                    double.TryParse(vista.P_MS.Content?.ToString(), out PFunc);
                    double.TryParse(vista.V2_MS.Content?.ToString(), out V2Func);
                    double.TryParse(vista.M3_MS.Content?.ToString(), out M3Func);
                    double.TryParse(vista.Pmax_MS.Content?.ToString(), out PmaxFunc);
                    double.TryParse(vista.V2max_MS.Content?.ToString(), out V2maxFunc);
                    double.TryParse(vista.M3max_MS.Content?.ToString(), out M3maxFunc);

                    // Evaluar condiciones
                    cumpleP = PFunc <= PmaxFunc;
                    cumpleV2 = V2Func <= V2maxFunc;
                    cumpleM3 = M3Func <= M3maxFunc;
                }

                vista.RecuadroMS.Background = (cumpleP && cumpleV2 && cumpleM3) ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 199, 79)) : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));

                if (!cumpleP) vista.P_MSFunc.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
                if (!cumpleV2) vista.V2_MSFunc.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
                if (!cumpleM3) vista.M3_MSFunc.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 32, 32));
            }
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
            double[] esfuerzos_MC=new double[6];
            if(revision=="2VR4")
            {
                esfuerzos_MC = uniones["MC"];
            }
            else if(revision=="2VR5")
            {
                if (vista.Num_String.Text == "1")
                {
                    esfuerzos_MC = uniones["MC (2V-1ST) DEF"];
                }
                else if (vista.Num_String.Text == "2")
                {
                    esfuerzos_MC = uniones["MC (2V-2ST) DEF"];
                }
            }
            

            //Obtenemos los esfuerzos de la MC
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            string[] vigas_n = SAP.ElementFinderSubclass.TrackerSubclass.NorthBeams(mySapModel);
            string[] vigas_s = SAP.ElementFinderSubclass.TrackerSubclass.SouthBeams(mySapModel);
            double[] maximosNorte = SAP.AnalysisSubclass.ObtenerEsfuerzosEnExtremo(mySapModel, vigas_n[0], 0, "ULS");
            double[] maximosSur = SAP.AnalysisSubclass.ObtenerEsfuerzosEnExtremo(mySapModel, vigas_s[0], 0, "ULS");

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

        public static void UnionBC(ComprobacionUniones2VAPP vista)
        {
            //Datos de la unión
            var uniones = CargarDesdeExcel();
            double[] esfuerzos_BC = uniones["T4BC-31A"];

            //Obtenemos los esfuerzos de la BC
            SAP.AnalysisSubclass.RunModel(mySapModel);
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            string[] vigas_n = SAP.ElementFinderSubclass.TrackerSubclass.NorthBeams(mySapModel);
            string[] vigas_s = SAP.ElementFinderSubclass.TrackerSubclass.SouthBeams(mySapModel);
            double[] maximosNorte = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel, vigas_n, "ULS");
            double[] maximosSur = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel, vigas_s, "ULS");

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

        public static string BuscarCombinacionMaxSuccion()
        {
            // Obtener lista de combinaciones
            int numCombos = 0;
            string[] combos = null;
            mySapModel.RespCombo.GetNameList(ref numCombos, ref combos);
            string resultado = null;
            double maxSuccion = double.MinValue;

            foreach (var combo in combos)
            {
                // Obtener casos y factores de la combinación
                int numCases = 0;
                string[] caseNames = null;
                eCNameType[] CNameType = null;
                double[] factors = null;
                mySapModel.RespCombo.GetCaseList(combo, ref numCases,ref CNameType, ref caseNames, ref factors);

                // Validar que solo tenga los tres casos requeridos
                var casosRequeridos = new[] { "DEAD", "PP PANELES", "W1_Neg_Cfmin" };
                if (numCases == 3 && casosRequeridos.All(c => caseNames.Contains(c, StringComparer.OrdinalIgnoreCase)))
                {
                    // Obtener factores
                    double fDead = factors[Array.IndexOf(caseNames, "DEAD")];
                    int index=Array.FindIndex(caseNames, c=>c.Trim().Equals("PP PANELES",StringComparison.OrdinalIgnoreCase));
                    double fPP = factors[index];
                    double fSuccion = factors[Array.IndexOf(caseNames, "W1_Neg_Cfmin")];

                    // Validar condiciones
                    if (Math.Abs(fDead - 1.0) < 0.0001 && Math.Abs(fPP - 1.0) < 0.0001)
                    {
                        // Buscar la combinación con mayor succión
                        if (fSuccion > maxSuccion)
                        {
                            maxSuccion = fSuccion;
                            resultado = combo;
                        }
                    }
                }
            }

            return resultado;
        }
    }
}
