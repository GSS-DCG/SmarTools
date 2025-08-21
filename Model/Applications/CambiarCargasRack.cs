using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using System.Windows;
using ClosedXML.Excel;
using OfficeOpenXml;
using SmarTools.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmarTools.Model.Repository;
using System.Reflection.Emit;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Drawing;
using System.Text.RegularExpressions;

namespace SmarTools.Model.Applications
{
    class CambiarCargasRack
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        public static void CargarDatos(CambiarCargasRackAPP vista)
        {
            var loadingWindow = new Status();

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                string rutaArchivo = WindowsFunctions.SearchFile();

                var textboxs = new TextBox[]
                {
                    vista.PesoPropio_Panel,
                    vista.PesoPropio_Cable,
                    vista.Carga_Nieve,
                    vista.Carga_NieveAccidental,
                    vista.Presion_Sup,
                    vista.Presion_Inf,
                    vista.Succion_Sup,
                    vista.Succion_Inf,
                    vista.Friccion,
                    vista.Presion_Pico
                };
                foreach (var textbox in textboxs)
                {
                    textbox.Text=string.Empty;
                }

                using (ExcelPackage package = new ExcelPackage(rutaArchivo))
                {
                    //Obtenemos los datos y los pegamos directamente en la ventana
                    vista.PesoPropio_Panel.Text = LeerCeldaPorNombre(rutaArchivo, "Peso_Propio_Panel").ToString("F3");
                    vista.PesoPropio_Cable.Text = LeerCeldaPorNombre(rutaArchivo, "Carga_Cable").ToString("F3");
                    vista.Carga_Nieve.Text = LeerCeldaPorNombre(rutaArchivo, "Sfinal").ToString("F3");
                    vista.Carga_NieveAccidental.Text = LeerCelda(rutaArchivo,"Cargas","K14").ToString("F3");
                    if (vista.Expuesto.IsChecked == true)
                    {
                        vista.Presion_Sup.Text = LeerCeldaPorNombre(rutaArchivo, "F_1_Sup_Presion").ToString("F3");
                        vista.Presion_Inf.Text = LeerCeldaPorNombre(rutaArchivo, "F_1_Inf_Presion").ToString("F3");
                        vista.Succion_Sup.Text = LeerCeldaPorNombre(rutaArchivo, "F_1_Sup_Succion").ToString("F3");
                        vista.Succion_Inf.Text = LeerCeldaPorNombre(rutaArchivo, "F_1_Inf_Succion").ToString("F3");
                    }
                    else if (vista.Resguardo.IsChecked == true)
                    {
                        vista.Presion_Sup.Text = LeerCeldaPorNombre(rutaArchivo, "F_2_Sup_Presion").ToString("F3");
                        vista.Presion_Inf.Text = LeerCeldaPorNombre(rutaArchivo, "F_2_Inf_Presion").ToString("F3");
                        vista.Succion_Sup.Text = LeerCeldaPorNombre(rutaArchivo, "F_2_Sup_Succion").ToString("F3");
                        vista.Succion_Inf.Text = LeerCeldaPorNombre(rutaArchivo, "F_2_Inf_Succion").ToString("F3");
                    }
                    vista.Friccion.Text = LeerCelda(rutaArchivo, "Cargas", "G34").ToString("F3");
                    vista.Presion_Pico.Text=LeerCeldaPorNombre(rutaArchivo,"Presion_Pico").ToString("F3");
                }
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

        public static void AsignarCargas(CambiarCargasRackAPP vista)
        {
            Herramientas.AbrirArchivoSAP2000();
            var loadingWindow = new Status();
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();
                double D = DimensionPanel();

                if (vista.PesoPropio_Check.IsChecked == true && double.TryParse(vista.PesoPropio_Panel.Text, out var val1))
                    PesoPropioPaneles(vista, val1, D);

                if (vista.PesoCable_Check.IsChecked == true && double.TryParse(vista.PesoPropio_Cable.Text, out var val2))
                    PesoPropioCable(val2);

                if (vista.CargaNieve_Check.IsChecked == true && double.TryParse(vista.Carga_Nieve.Text, out var val3))
                    CargaNieve(vista, val3, D);

                if (vista.CargaNieveAccidental_Check.IsChecked==true && double.TryParse(vista.Carga_NieveAccidental.Text, out var val4))
                    CargaNieveAccidental(vista, val4, D);

                if (vista.PresionSup_Check.IsChecked == true && double.TryParse(vista.Presion_Sup.Text, out var val5))
                    PresionSuperior(vista, val5,D);

                if (vista.PresionInf_Check.IsChecked == true && double.TryParse(vista.Presion_Inf.Text, out var val6))
                    PresionInferior(vista, val6, D);

                if (vista.SuccionSup_Check.IsChecked == true && double.TryParse(vista.Succion_Sup.Text, out var val7))
                    SuccionSuperior(vista, val7, D);

                if (vista.SuccionInf_Check.IsChecked == true && double.TryParse(vista.Succion_Inf.Text, out var val8))
                    SuccionInferior (vista, val8, D);

                if (vista.Friccion_Check.IsChecked == true && double.TryParse(vista.Friccion.Text, out var val9))
                    Friccion(val9, D);

                if (vista.PresionPico_Check.IsChecked == true && double.TryParse(vista.Presion_Pico.Text, out var val10))
                    PresionPico(val10, D);
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

        public static double LeerCelda(string rutaArchivo, string nombreHoja, string direccionCelda)
        {
            using (var workbook = new XLWorkbook(rutaArchivo))
            {
                var hoja = workbook.Worksheet(nombreHoja);
                if (hoja == null)
                {
                    throw new Exception($"No se encontró la hoja '{nombreHoja}' en el archivo.");
                }

                var celda = hoja.Cell(direccionCelda);
                if (celda.IsEmpty())
                {
                    throw new Exception($"La celda '{direccionCelda}' está vacía.");
                }

                return celda.GetDouble();
            }
        }

        public static double LeerCeldaPorNombre(string rutaArchivo, string nombreCelda)
        {
            using (var workbook = new XLWorkbook(rutaArchivo))
            {
                var rangoNombrado = workbook.DefinedName(nombreCelda);
                if (rangoNombrado == null)
                {
                    throw new Exception($"No se encontró un rango con el nombre '{nombreCelda}'.");
                }

                var celda = rangoNombrado.Ranges.First().FirstCell();
                if (celda.IsEmpty())
                {
                    throw new Exception($"La celda con nombre '{nombreCelda}' está vacía.");
                }

                return celda.GetDouble();
            }
        }

        public static double DimensionPanel()
        {
            double X = 0;
            double Y = 0;
            double Z = 0;
            double[] CoordX = new double[2];
            double[] CoordZ = new double[2];

            mySapModel.PointObj.GetCoordCartesian("np_0i", ref X, ref Y, ref Z);

            CoordX[0] = X;
            CoordZ[0] = Z;

            mySapModel.PointObj.GetCoordCartesian("np_1i", ref X, ref Y, ref Z);

            CoordX[1] = X;
            CoordZ[1] = Z;

            double D = Math.Round(Math.Sqrt(Math.Pow(CoordZ[1] - CoordZ[0], 2) + Math.Pow(CoordX[1] - CoordX[0], 2)), 3);

            return D;

        }

        public static void PesoPropioPaneles(CambiarCargasRackAPP vista, double PesoPropio_Panel, double D)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);
            string[] correas=SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);

            //Asignamos las cargas al modelo en función de si es PV o PH
            if (vista.Configuracion_PV.IsChecked == true)
            {
                for (int i = 0; i < correas.Length; i++)
                {
                    mySapModel.FrameObj.SetLoadDistributed(correas[i], "PP PANELES", 1, 10, 0, 1, PesoPropio_Panel * D / 2, PesoPropio_Panel * D / 2);
                }
            }
            else if (vista.Configuracion_PH.IsChecked == true)
            {
                for(int i = 0;i < correas.Length; i++)
                {
                    if (correas[i].Contains("Purlin_1") || correas[i].Contains("Purlin_"+n_cm))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correas[i], "PP PANELES", 1, 10, 0, 1, PesoPropio_Panel * D / 2, PesoPropio_Panel * D / 2);
                    }
                    else
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correas[i], "PP PANELES", 1, 10, 0, 1, PesoPropio_Panel * D, PesoPropio_Panel * D);
                    }
                }
            }
        }

        public static void PesoPropioCable(double PesoPropio_Cable)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);

            //Comprobamos que el modelo tiene el mismo número de hipótesis de carga muerta que de correas, sino, las agregamos
            int NumberNames = 0;
            string[] LoadPatterns = new string[1];
            mySapModel.LoadPatterns.GetNameList(ref NumberNames, ref LoadPatterns);
            for (int i = 1; i <= n_cm; i++)
            {
                string hipotesis = "CM" + i;
                if (LoadPatterns.Any(p => p == hipotesis))
                {
                    continue;
                }
                else
                {
                    mySapModel.LoadPatterns.Add(hipotesis, eLoadPatternType.Dead);
                }
            }

            //Reemplazamos las cargas del cable por las nuevas
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);
            
            for(int i=0;i<correas.Length;i++)
            {
                for(int j=1;j<=n_cm;j++)
                {
                    string hipotesis = "CM" + j;
                    if (correas[i].Contains("Purlin_"+j))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correas[i], hipotesis, 1, 10, 0, 1, PesoPropio_Cable, PesoPropio_Cable);
                    }
                }
            }
        }

        public static void CargaNieve(CambiarCargasRackAPP vista, double CargaNieve, double D)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);

            //Asignamos las cargas al modelo en función de si es PV o PH
            if (vista.Configuracion_PV.IsChecked == true)
            {
                for (int i = 0; i < correas.Length; i++)
                {
                    mySapModel.FrameObj.SetLoadDistributed(correas[i], "Snow", 1, 10, 0, 1, CargaNieve * D / 2, CargaNieve * D / 2);
                }
            }
            else if (vista.Configuracion_PH.IsChecked == true)
            {
                for (int i = 0; i < correas.Length; i++)
                {
                    if (correas[i].Contains("Purlin_1") || correas[i].Contains("Purlin_" + n_cm))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correas[i], "Snow", 1, 10, 0, 1, CargaNieve * D / 2, CargaNieve * D / 2);
                    }
                    else
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correas[i], "Snow", 1, 10, 0, 1, CargaNieve * D, CargaNieve * D);
                    }
                }
            }
        }

        public static void CargaNieveAccidental(CambiarCargasRackAPP vista, double CargaNieveAccidental, double D)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);

            //Comprobamos que el modelo tiene definida la hipótesis de carga de nieve accidental
            int NumberNames = 0;
            string[] LoadPatterns = new string[1];
            mySapModel.LoadPatterns.GetNameList(ref NumberNames, ref LoadPatterns);
            if (LoadPatterns.Any(p => p =="Accidental_Snow"))
            {
                mySapModel.LoadPatterns.Add("Accidental_Snow", eLoadPatternType.Snow);
            }

            //Asignamos las cargas al modelo en función de si es PV o PH
            if (vista.Configuracion_PV.IsChecked == true)
            {
                for (int i = 0; i < correas.Length; i++)
                {
                    mySapModel.FrameObj.SetLoadDistributed(correas[i], "Accidental_Snow", 1, 10, 0, 1, CargaNieveAccidental * D / 2, CargaNieveAccidental * D / 2);
                }
            }
            else if (vista.Configuracion_PH.IsChecked == true)
            {
                for (int i = 0; i < correas.Length; i++)
                {
                    if (correas[i].Contains("Purlin_1") || correas[i].Contains("Purlin_" + n_cm))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correas[i], "Accidental_Snow", 1, 10, 0, 1, CargaNieveAccidental * D / 2, CargaNieveAccidental * D / 2);
                    }
                    else
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correas[i], "Accidental_Snow", 1, 10, 0, 1, CargaNieveAccidental * D, CargaNieveAccidental * D);
                    }
                }
            }
        }

        public static void PresionSuperior(CambiarCargasRackAPP vista, double PresionSuperior, double D)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);
            
            //Quitamos todas las cargas de presión del modelo
            for (int i = 0;i<correas.Length;i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(correas[i], "W1_Press", 1, 2, 0, 1, 0, 0,"Local");
            }

            //Generamos el prefijo de las correas que aplican al paño superior
            int n_correasValidas = 0;
            string[] correasValidas = null;
            int j = 0;
            if (n_cm%2==0)
            {
                n_correasValidas=n_cm/2;
                correasValidas = new string[n_correasValidas];
                for (int i = n_correasValidas + 1; i <= n_cm; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }
            else
            {
                n_correasValidas=(n_cm/2)+1;
                correasValidas = new string[n_correasValidas];
                for (int i = n_correasValidas; i <= n_cm; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }

            //Filtramos las correas necesarias
            var correasSup=correas
                .Where(correa=>correasValidas.Any(valida=>correa.Contains(valida)))
                .ToArray();

            //Asignamos las cargas al modelo en función de si es PV o PH
            if (vista.Configuracion_PV.IsChecked == true || n_cm==3)
            {
                for (int i = 0; i < correasSup.Length; i++)
                {
                    mySapModel.FrameObj.SetLoadDistributed(correasSup[i], "W1_Press", 1, 2, 0, 1, PresionSuperior * D / 2, PresionSuperior * D / 2,"Local");
                }
            }
            else if (vista.Configuracion_PH.IsChecked == true)
            {
                for (int i = 0; i < correasSup.Length; i++)
                {
                    if (correasSup[i].Contains("Purlin_1") || correasSup[i].Contains("Purlin_" + n_cm))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasSup[i], "W1_Press", 1, 2, 0, 1, PresionSuperior * D / 2, PresionSuperior * D / 2, "Local");
                    }
                    else
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasSup[i], "W1_Press", 1, 2, 0, 1, PresionSuperior * D, PresionSuperior * D, "Local");
                    }
                }
            }
        }

        public static void PresionInferior(CambiarCargasRackAPP vista, double PresionInferior, double D)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);

            //Generamos el prefijo de las correas que aplican al paño superior
            int n_correasValidas = 0;
            string[] correasValidas = null;
            int j = 0;
            if (n_cm % 2 == 0)
            {
                n_correasValidas=n_cm / 2;
                correasValidas = new string[n_correasValidas];
                for (int i = 1; i <= n_correasValidas; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }
            else
            {
                n_correasValidas = (n_cm / 2)+1;
                correasValidas = new string[n_correasValidas];
                for (int i = 1; i <= n_correasValidas; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }

            //Filtramos las correas necesarias
            var correasInf = correas
                .Where(correa => correasValidas.Any(valida => correa.Contains(valida)))
                .ToArray();

            //Asignamos las cargas al modelo en función de si es PV o PH
            if (vista.Configuracion_PV.IsChecked == true || n_cm == 3)
            {
                for (int i = 0; i < correasInf.Length; i++)
                {
                    mySapModel.FrameObj.SetLoadDistributed(correasInf[i], "W1_Press", 1, 2, 0, 1, PresionInferior * D / 2, PresionInferior * D / 2, "Local", true,false);
                }
            }
            else if (vista.Configuracion_PH.IsChecked == true)
            {
                for (int i = 0; i < correasInf.Length; i++)
                {
                    if (correasInf[i].Contains("Purlin_1") || correasInf[i].Contains("Purlin_" + n_cm))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasInf[i], "W1_Press", 1, 2, 0, 1, PresionInferior * D / 2, PresionInferior * D / 2, "Local",true, false);
                    }
                    else
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasInf[i], "W1_Press", 1, 2, 0, 1, PresionInferior * D, PresionInferior * D, "Local",true, false);
                    }
                }
            }
        }

        public static void SuccionSuperior(CambiarCargasRackAPP vista, double SuccionSuperior, double D)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);

            //Quitamos todas las cargas de presión del modelo
            for (int i = 0; i < correas.Length; i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(correas[i], "W2_Suct", 1, 2, 0, 1, 0, 0, "Local");
            }

            //Generamos el prefijo de las correas que aplican al paño superior
            int n_correasValidas = 0;
            string[] correasValidas = null;
            int j = 0;
            if (n_cm % 2 == 0)
            {
                n_correasValidas = n_cm / 2;
                correasValidas = new string[n_correasValidas];
                for (int i = n_correasValidas + 1; i <= n_cm; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }
            else
            {
                n_correasValidas = (n_cm / 2) + 1;
                correasValidas = new string[n_correasValidas];
                for (int i = n_correasValidas; i <= n_cm; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }

            //Filtramos las correas necesarias
            var correasSup = correas
                .Where(correa => correasValidas.Any(valida => correa.Contains(valida)))
                .ToArray();

            //Asignamos las cargas al modelo en función de si es PV o PH
            if (vista.Configuracion_PV.IsChecked == true)
            {
                for (int i = 0; i < correasSup.Length; i++)
                {
                    mySapModel.FrameObj.SetLoadDistributed(correasSup[i], "W2_Suct", 1, 2, 0, 1, SuccionSuperior * D / 2, SuccionSuperior * D / 2, "Local");
                }
            }
            else if (vista.Configuracion_PH.IsChecked == true)
            {
                for (int i = 0; i < correasSup.Length; i++)
                {
                    if (correasSup[i].Contains("Purlin_1") || correasSup[i].Contains("Purlin_" + n_cm))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasSup[i], "W2_Suct", 1, 2, 0, 1, SuccionSuperior * D / 2, SuccionSuperior * D / 2, "Local");
                    }
                    else
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasSup[i], "W2_Suct", 1, 2, 0, 1, SuccionSuperior * D, SuccionSuperior * D, "Local");
                    }
                }
            }
        }

        public static void SuccionInferior(CambiarCargasRackAPP vista, double SuccionInferior, double D)
        {
            //Obtenemos el número y el nombre de correas del modelo
            int n_cm = SAP.ElementFinderSubclass.FixedSubclass.NumeroCorreas(mySapModel);
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);

            //Generamos el prefijo de las correas que aplican al paño superior
            int n_correasValidas = 0;
            string[] correasValidas = null;
            int j = 0;
            if (n_cm % 2 == 0)
            {
                n_correasValidas = n_cm / 2;
                correasValidas = new string[n_correasValidas];
                for (int i = 1; i <= n_correasValidas; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }
            else
            {
                n_correasValidas = (n_cm / 2) + 1;
                correasValidas = new string[n_correasValidas];
                for (int i = 1; i <= n_correasValidas; i++)
                {
                    correasValidas[j++] = "Purlin_" + i;
                }
            }

            //Filtramos las correas necesarias
            var correasInf = correas
                .Where(correa => correasValidas.Any(valida => correa.Contains(valida)))
                .ToArray();

            //Asignamos las cargas al modelo en función de si es PV o PH
            if (vista.Configuracion_PV.IsChecked == true)
            {
                for (int i = 0; i < correasInf.Length; i++)
                {
                    mySapModel.FrameObj.SetLoadDistributed(correasInf[i], "W2_Suct", 1, 2, 0, 1, SuccionInferior * D / 2, SuccionInferior * D / 2, "Local", true, false);
                }
            }
            else if (vista.Configuracion_PH.IsChecked == true)
            {
                for (int i = 0; i < correasInf.Length; i++)
                {
                    if (correasInf[i].Contains("Purlin_1") || correasInf[i].Contains("Purlin_" + n_cm))
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasInf[i], "W2_Suct", 1, 2, 0, 1, SuccionInferior * D / 2, SuccionInferior * D / 2, "Local", true, false);
                    }
                    else
                    {
                        mySapModel.FrameObj.SetLoadDistributed(correasInf[i], "W2_Suct", 1, 2, 0, 1, SuccionInferior * D, SuccionInferior * D, "Local", true, false);
                    }
                }
            }
        }

        public static void Friccion(double Friccion,double D)
        {
            //Obtenemos el nombre de correas del modelo
            string[] correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);

            int ret = mySapModel.LoadPatterns.Add("W3_90º", eLoadPatternType.Wind, 0, true);
            ret = mySapModel.LoadPatterns.Add("W4_270º", eLoadPatternType.Wind, 0, true);

            for (int i = 0;i < correas.Length;i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(correas[i], "W3_90º", 1, 5, 0, 1, Friccion, Friccion);
                mySapModel.FrameObj.SetLoadDistributed(correas[i],"W4_270º",1,5,0,1,(-Friccion),(-Friccion));
            }
        }

        public static void PresionPico(double PresionPico,double D)
        {
            string[] pilares=SAP.ElementFinderSubclass.FixedSubclass.ListaPilares(mySapModel);
            string[] pilaresDelanteros=SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
            string[] pilaresTraseros=SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);
            string[] vigas=SAP.ElementFinderSubclass.FixedSubclass.ListaVigas(mySapModel);
            (string[], string[]) diagonales=SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);

            int ret = mySapModel.LoadPatterns.Add("W3_90º", eLoadPatternType.Wind, 0, true);
            ret = mySapModel.LoadPatterns.Add("W4_270º", eLoadPatternType.Wind, 0, true);

            if (pilares.Length!=0)
            {
                string[] seccion = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, pilares[0]);
                Match ancho = Regex.Match(seccion[0], @"C-(\d{2,3})x");
                int Ancho = int.Parse(ancho.Groups[1].Value);
                double Fw = PresionPico * Ancho * 1.8 / 1000;
                mySapModel.FrameObj.SetLoadDistributed(pilares[0], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(pilares[0], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
                mySapModel.FrameObj.SetLoadDistributed(pilares.Last(), "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(pilares.Last(), "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }

            if (pilaresDelanteros.Length != 0)
            {
                string[] seccion = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, pilaresDelanteros[0]);
                Match ancho = Regex.Match(seccion[0], @"C-(\d{2,3})x");
                int Ancho = int.Parse(ancho.Groups[1].Value);
                double Fw = PresionPico * Ancho * 1.8 / 1000;
                mySapModel.FrameObj.SetLoadDistributed(pilaresDelanteros[0], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(pilaresDelanteros[0], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
                mySapModel.FrameObj.SetLoadDistributed(pilaresDelanteros.Last(), "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(pilaresDelanteros.Last(), "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }

            if (pilaresTraseros.Length != 0)
            {
                string[] seccion = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, pilaresTraseros[0]);
                Match ancho = Regex.Match(seccion[0], @"C-(\d{2,3})x");
                int Ancho = int.Parse(ancho.Groups[1].Value);
                double Fw = PresionPico * Ancho * 1.8 / 1000;
                mySapModel.FrameObj.SetLoadDistributed(pilaresTraseros[0], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(pilaresTraseros[0], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
                mySapModel.FrameObj.SetLoadDistributed(pilaresTraseros.Last(), "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(pilaresTraseros.Last(), "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }

            if (vigas.Length != 0)
            {
                string[] seccion = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, vigas[0]);
                Match ancho = Regex.Match(seccion[0], @"C-(\d{2,3})x");
                int Ancho = int.Parse(ancho.Groups[1].Value);
                double Fw = PresionPico * Ancho * 1.8 / 1000;
                mySapModel.FrameObj.SetLoadDistributed(vigas[0], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(vigas[0], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
                mySapModel.FrameObj.SetLoadDistributed(vigas.Last(), "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(vigas.Last(), "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }

            if (diagonales.Item1.Length != 0)
            {
                string[] seccion = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, diagonales.Item1[0]);
                Match ancho = Regex.Match(seccion[0], @"U-(\d{2,3})x");
                int Ancho = int.Parse(ancho.Groups[1].Value);
                double Fw = PresionPico * Ancho * 1.8 / 1000;
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item1[0], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item1[0], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item1.Last(), "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item1.Last(), "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }

            if (diagonales.Item2.Length != 0)
            {
                string[] seccion = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, diagonales.Item2[0]);
                Match ancho = Regex.Match(seccion[0], @"U-(\d{2,3})x");
                int Ancho = int.Parse(ancho.Groups[1].Value);
                double Fw = PresionPico * Ancho * 1.8 / 1000;
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item2[0], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item2[0], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item2.Last(), "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(diagonales.Item2.Last(), "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }
        }
    }
}
