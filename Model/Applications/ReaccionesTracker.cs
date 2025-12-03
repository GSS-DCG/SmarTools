using System;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Microsoft.Win32;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.APPS.ReaccionesTrackerAPP;
using ModernUI.View;
using SmarTools.View;


namespace SmarTools.Model.Applications
{
    public class RellenarPOT_TrackerClass()
    {
        // Ponemos las instancias globales
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        // Atributos de clase necesarios
        string[] SAPstrFilesRoutes = new string[24]; // Rutas de los archivos SAP de cada string (necesario para pestaña Rellenar POT)
        string[] ESMARoutes = new string[4]; // Almacena la ruta de excel de la ESMA de cada tipo de tracker
        bool[] ESMATrackerType = new bool[4]; // Almacena si en esa ESMA es el tracker tipo 1 (true) o el tipo 2 (false)
        string[] NewExcelRoutes = new string[4]; // Almacena las rutas de los excel guardados
        int[] GPcount = new int[8]; // Almacena el numero de pilares generales del tracker (toma solo el primero de los SAP)
        string[] MPSection = new string[8]; // Almacena el perfil de PG de cada estrategia (expuesto/resguardo)
        string[] GPSection = new string[8]; // Almacena el perfil de PG de cada estrategia (expuesto/resguardo)



        // Instanciamos las clases necesarias
        MyMethods MyMethods = new MyMethods();



        // --------------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------------

        // Funciones "Rellenar POT"

        // --------------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------------



        // Examinar archivos


        public void SearchFolder2str(RellenarPOT_Tracker vista)
        {
            if (vista.Exp2.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 0, "expuesto");
            }
            if (vista.Res2.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 3, "resguardo");
            }
        }


        public void SearchFolder1ymedstr(RellenarPOT_Tracker vista)
        {
            if (vista.Exp1ymed.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 6, "expuesto");
            }
            if (vista.Res1ymed.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 9, "resguardo");
            }
        }


        public void SearchFolder1str(RellenarPOT_Tracker vista)
        {
            if (vista.Exp1.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 12, "expuesto");
            }
            if (vista.Res1.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 15, "resguardo");
            }
        }


        public void SearchFolder0ymedstr(RellenarPOT_Tracker vista)
        {
            if (vista.Exp0ymed.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 18, "expuesto");
            }
            if (vista.Res0ymed.IsChecked == true)
            {
                MyMethods.File.StoreFileRoutes(ref SAPstrFilesRoutes, 21, "resguardo");
            }
        }


        public void SearchPOTFile(RellenarPOT_Tracker vista)
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string defaultPath = System.IO.Path.Combine(userProfile, "Gonvarri", "SSE Proyectos - Documentos", "20_RECURSOS", "20_20_HOJAS_CALCULO", "04-CARGAS HINCADO");

            vista.POTOriginalRoute.Text = MyMethods.File.SearchFile(defaultPath, "Archivos de Excel (*.xlsx;*.xlsm)|*.xlsx;*.xlsm");
        }


        public void SearchPOTFolder(RellenarPOT_Tracker vista)
        {
            vista.POTSaveRoute.Text = MyMethods.File.SearchFolder();
        }


        public void SearchESMARoute(RellenarPOT_Tracker vista)
        {
            // Inicializamos el tracker elegido en cada ESMA
            ESMATrackerType[0] = vista.str2tracker1.IsChecked == true ? true : false;
            ESMATrackerType[1] = vista.str1ymedtracker1.IsChecked == true ? true : false;
            ESMATrackerType[2] = vista.str1tracker1.IsChecked == true ? true : false;
            ESMATrackerType[3] = vista.str0ymedtracker1.IsChecked == true ? true : false;

            System.Windows.Controls.CheckBox[] str = { vista.ESMA_2str, vista.ESMA_1ymedstr, vista.ESMA_1str, vista.ESMA_0ymedstr };

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i].IsChecked == true) { ESMARoutes[i] = MyMethods.File.SearchFile(filter: "Archivos de Excel (*.xlsx;*.xlsm)|*.xlsx;*.xlsm"); }
            }
        }


        // --------------------------------------------------------------------------------------------------


        // Función principal de Rellenar excel POT


        public void FillPOT(RellenarPOT_Tracker vista)
        {
            // Instanciamos los checkboxes y hacemos un array con las posiciones del array de cada tracker y estrategia
            System.Windows.Controls.CheckBox[] checkBoxes = { vista.str2, vista.str1ymed, vista.str1, vista.str0ymed };
            System.Windows.Controls.CheckBox[] checkBoxesStrat = { vista.Exp2, vista.Res2, vista.Exp1ymed, vista.Res1ymed, 
                                                                    vista.Exp1, vista.Res1, vista.Exp0ymed, vista.Res0ymed };

            int[][] CheckBoxListPositions = {
            new int[] { 0, 1, 2 },  // Posiciones de ruta en el array para 2Str Exp
            new int[] { 3, 4, 5 },  // Posiciones de ruta en el array para 2Str Res
            new int[] { 6, 7, 8 },  // Posiciones de ruta en el array para 1.5Str Exp
            new int[] { 9, 10, 11 }, // Posiciones de ruta en el array para 1.5Str Res
            new int[] { 12, 13, 14 },  // Posiciones de ruta en el array para 1Str Exp
            new int[] { 15, 16, 17 },  // Posiciones de ruta en el array para 1Str Res
            new int[] { 18, 19, 20 },  // Posiciones de ruta en el array para 0.5Str Exp
            new int[] { 21, 22, 23 } // Posiciones de ruta en el array para 0.5Str Res
            };

            // Comprobamos posibles errores
            bool check = CheckDataFields(checkBoxes, checkBoxesStrat, CheckBoxListPositions, vista);
            if (check == false) { return; }

            // Mostramos ventana de cargando
            var status = new Status();
            status.Show();

            // Copiamos los excel POT en la ruta especificada
            NewExcelRoutes = MyMethods.File.CopyPOTExcel(checkBoxes, vista.POTSaveRoute, vista.POTOriginalRoute);

            // Copiamos las reacciones de los apoyos en los excel creados
            FillReactionsWorksheet(mySapModel, NewExcelRoutes);

            // Rellenamos la configuracion de la ESMA si se ha elegido un archivo
            FillAllESMAData();

            // Limpiamos las variables
            DeselectAll(checkBoxes, checkBoxesStrat);
            RestartArray(SAPstrFilesRoutes);
            RestartArray(NewExcelRoutes);
            RestartArray(ESMARoutes);
            RestartArray(MPSection);
            RestartArray(GPSection);


            // Cerramos la ventana de cargando y mostramos la ventana de proceso concluido
            status.Dispatcher.Invoke(() => status.Close());
            var ventana = new Incidencias();
            ventana.ConfigurarIncidencia("Proceso concluido correctamente", TipoIncidencia.Error);
            ventana.ShowDialog();
        }


        private void FillReactionsWorksheet(cSapModel mySapModel, string[] NewExcelRoutes)
        {
            foreach (var (str, index) in SAPstrFilesRoutes.Select((str, index) => (str, index)))
            {
                if (str == "") { continue; }
                int StrategyIndex = index / 6;

                MyMethods.SAP.LoadModels(mySapModel, str);
                object[,] reactions = MyMethods.SAP.ObtainReactionsInSAP(mySapModel);
                object[,] MotorPileReactions = MyMethods.SAP.FilterPileReactions(reactions, true);
                object[,] GeneralPileReactions = MyMethods.SAP.FilterPileReactions(reactions, false);

                MyMethods.Excel.FillWorksheet(index, StrategyIndex, NewExcelRoutes[StrategyIndex], reactions, 0);
                MyMethods.Excel.FillWorksheet(index, StrategyIndex, NewExcelRoutes[StrategyIndex], MotorPileReactions, 1);
                MyMethods.Excel.FillWorksheet(index, StrategyIndex, NewExcelRoutes[StrategyIndex], GeneralPileReactions, 2);

                int GPindex = index / 3;
                GPcount[GPindex] = GeneralPileReactions.GetLength(0) / 2;

                // Almacenamos los perfiles de PM y PG
                MPSection[GPindex] = MyMethods.SAP.GetFrameSection(mySapModel, true);
                GPSection[GPindex] = MyMethods.SAP.GetFrameSection(mySapModel, false);

                // Filtramos los nombres y los ajustamos
                MyMethods.SAP.RenameSections(ref MPSection, GPindex);
                MyMethods.SAP.RenameSections(ref GPSection, GPindex);
            }
        }


        private void FillAllESMAData()
        {
            for (int i = 0; i < NewExcelRoutes.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(NewExcelRoutes[i]) && !string.IsNullOrWhiteSpace(ESMARoutes[i]))
                {
                    MyMethods.Excel.FillESMAData(ESMARoutes[i], NewExcelRoutes[i], ESMATrackerType[i], MPSection, GPSection, GPcount, i);
                }
            }
        }


        // --------------------------------------------------------------------------------------------------


        // Control de posibles errores e inicialización de variables


        private bool CheckDataFields(System.Windows.Controls.CheckBox[] checkBoxes, System.Windows.Controls.CheckBox[] checkBoxesStrat, int[][] CheckBoxListPositions, RellenarPOT_Tracker vista)
        {
            // Deseleccionamos los checkbox de estrategias de aquellos trackers desmarcados y limpiamos sus posiciones del array de archivos SAP
            for (int i = 0; i < checkBoxes.Length; i++)
            {
                DeselectStrategyCheckbox(checkBoxes, checkBoxesStrat, CheckBoxListPositions, i);
                CleanStrategyPositions(checkBoxes, checkBoxesStrat, CheckBoxListPositions, i);
            }

            // Comprobamos que al menos esté seleccionado un tipo de tracker
            if (checkBoxes[0].IsChecked == false && checkBoxes[1].IsChecked == false && checkBoxes[2].IsChecked == false && checkBoxes[3].IsChecked == false)
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Selecciona algún string para obtener sus reacciones", TipoIncidencia.Error);
                ventana.ShowDialog();
                return false;
            }

            // Comprobamos que si están marcados los checkboxes de tracker (2Str, 1str...) se haya seleccionado la estrategia expuesto y/o resguardo
            for (int i = 0; i < checkBoxes.Length; i++)
            {
                if (checkBoxes[i].IsChecked == true && checkBoxesStrat[2 * i].IsChecked == false && checkBoxesStrat[2 * i + 1].IsChecked == false)
                {
                    var ventana = new Incidencias();
                    ventana.ConfigurarIncidencia($"Selecciona expuesto y/o resguardo para {checkBoxes[i].Name}", TipoIncidencia.Error);
                    ventana.ShowDialog();
                    return false;
                }
            }

            // Verificamos que si está seleccionado algun exp/res se hayan almacenado correctamente sus SAP
            for (int i = 0; i < checkBoxesStrat.Length; i++)
            {
                if (checkBoxesStrat[i].IsChecked == true)
                {
                    foreach (int position in CheckBoxListPositions[i])
                    {
                        if (string.IsNullOrEmpty(SAPstrFilesRoutes[position]))
                        {
                            var ventana = new Incidencias();
                            ventana.ConfigurarIncidencia($"El CheckBox {checkBoxesStrat[i].Name} está seleccionado, pero sus archivos SAP no han sido seleccionados", TipoIncidencia.Error);
                            ventana.ShowDialog();
                            return false;
                        }
                    }
                }
            }

            // Comprobamos que la ruta del archivo POT esté seleccionado correctamente
            if (vista.POTOriginalRoute.Text == "")
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Selecciona el archivo de excel para rellenar con las reacciones", TipoIncidencia.Error);
                ventana.ShowDialog();
                return false;
            }

            // Comprobamos que la ruta de la carpeta de guardado esté seleccionada correctamente
            if (vista.POTSaveRoute.Text == "")
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Selecciona la carpeta donde guardar el excel de reacciones", TipoIncidencia.Error);
                ventana.ShowDialog();
                return false;
            }

            return true;
        }


        private void DeselectStrategyCheckbox(System.Windows.Controls.CheckBox[] checkBoxes, System.Windows.Controls.CheckBox[] checkBoxesStrat, int[][] CheckBoxListPositions, int i)
        {
            // Si el checkbox general (2Str, 1Str...) está desmarcado, se desmarcan y borran los de exp/res
            if (checkBoxes[i].IsChecked == false)
            {
                checkBoxesStrat[2 * i].IsChecked = false;
                checkBoxesStrat[2 * i + 1].IsChecked = false;


                foreach (int position in CheckBoxListPositions[2 * i])
                {
                    SAPstrFilesRoutes[position] = "";
                }
                foreach (int position in CheckBoxListPositions[2 * i + 1])
                {
                    SAPstrFilesRoutes[position] = "";
                }
            }
        }


        private void CleanStrategyPositions(System.Windows.Controls.CheckBox[] checkBoxes, System.Windows.Controls.CheckBox[] checkBoxesStrat, int[][] CheckBoxListPositions, int i)
        {
            // Si el checkbox general (2Str, 1Str...) está marcado y el de la estrategia exp/res está desmarcado, se limpian sus posiciones del array
            if (checkBoxes[i].IsChecked == true)
            {
                if (checkBoxesStrat[2 * i].IsChecked == false)
                {
                    foreach (int position in CheckBoxListPositions[2 * i])
                    {
                        SAPstrFilesRoutes[position] = "";
                    }
                }
                if (checkBoxesStrat[2 * i + 1].IsChecked == false)
                {
                    foreach (int position in CheckBoxListPositions[2 * i + 1])
                    {
                        SAPstrFilesRoutes[position] = "";
                    }
                }
            }
        }


        private void RestartArray(string[] Array)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = string.Empty;
            }

        }


        private void DeselectAll(System.Windows.Controls.CheckBox[] checkBoxes, System.Windows.Controls.CheckBox[] checkBoxesStrat)
        {
            foreach (System.Windows.Controls.CheckBox checkBox in checkBoxes) { checkBox.IsChecked = false; }
            foreach (System.Windows.Controls.CheckBox checkBox in checkBoxesStrat) { checkBox.IsChecked = false; }
        }
    }


    public class ObtenerExcelsClass()
    {
        // Ponemos las instancias globales
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        // Ponemos los atributos de clase necesarios
        string ExcelPlantilla = ""; // Ruta del excel vacio para copiar y rellenar



        // Instanciamos las clases necesarias
        MyMethods MyMethods = new MyMethods();



        // --------------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------------

        // Funciones "Obtener Excels"

        // --------------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------------



        // Examinar archivos

        public void SearchSAPFiles(ObtenerExcels vista)
        {
            vista.SAPFilesRoute.Text = MyMethods.File.SearchFolder();
        }


        public void SearchSaveFolder(ObtenerExcels vista)
        {
            vista.ReactionsSaveRoute.Text = MyMethods.File.SearchFolder();
        }


        // --------------------------------------------------------------------------------------------------


        // Función principal de Obtener excels


        public void ObtainExcels(ObtenerExcels vista)
        {
            // Buscamos la ruta del excel base
            ExcelPlantilla = "Z://300SmarTools//06 Plantilla excel//Libro1.xlsx";

            string SAPFolderRoute = vista.SAPFilesRoute.Text;
            string ExcelFolderRoute = vista.ReactionsSaveRoute.Text;
            object[,] header = new object[3, 10] // Encabezado de la tabla a pegar en el excel para hacerlo similar al output de SAP
                {
                { "TABLE:  Joint Reactions", "", "", "", "", "", "", "", "", "" },
                { "Joint", "OutputCase", "CaseType", "StepType", "F1", "F2", "F3", "M1", "M2", "M3" },
                { "Text", "Text", "Text", "Text", "KN", "KN", "KN", "KN-m", "KN-m", "KN-m" }
                };

            // Gestionamos los posibles errores por no introducir carpetas válidas
            DirectoryFailControl(SAPFolderRoute, ExcelFolderRoute);

            // Bucamos los archivos de SAP en la carpeta dada
            List<string> SAPFilesRoutes = MyMethods.SAP.SearchSAPFiles(SAPFolderRoute);

            // Mostramos ventana de cargando
            var status = new Status();
            status.Show();

            // Ejecutamos el proceso de obtención de excels de reacciones
            if (SAPFilesRoutes.Count != 0)
            {
                List<string> ExcelFilesRoutes = MyMethods.Excel.EstablishExcelRoutes(SAPFilesRoutes, SAPFolderRoute, ExcelFolderRoute);

                for (int i = 0; i < SAPFilesRoutes.Count; i++)
                {
                    MyMethods.SAP.LoadModels(mySapModel, SAPFilesRoutes[i]);
                    object[,] reactions = MyMethods.SAP.ObtainReactionsInSAP(mySapModel);
                    File.Copy(ExcelPlantilla, ExcelFilesRoutes[i], true);
                    MyMethods.Excel.PrintReactionsToExcel(ExcelFilesRoutes[i], header, "Hoja1", "A1");
                    MyMethods.Excel.PrintReactionsToExcel(ExcelFilesRoutes[i], reactions, "Hoja1", "A4");
                    MyMethods.Excel.AddHeaderFormat(ExcelFilesRoutes[i], "Hoja1", "A1");
                }
                
                // Cerramos la ventana de cargando y mostramos la ventana de proceso concluido
                status.Dispatcher.Invoke(() => status.Close());
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Proceso concluido", TipoIncidencia.Informacion);
                ventana.ShowDialog();
            }
            else 
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("No existen archivos SAP en la carpeta seleccionada", TipoIncidencia.Error);
                ventana.ShowDialog();
            }
        }


        // --------------------------------------------------------------------------------------------------


        // Control de posibles errores e inicialización de variables


        private void DirectoryFailControl(string SAPFolderRoute, string ExcelFolderRoute)
        {
            if (SAPFolderRoute.Length == 0 | Directory.Exists(SAPFolderRoute) == false)
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Por favor, selecciona una carpeta existente donde existan archivos de SAP", TipoIncidencia.Advertencia);
                ventana.ShowDialog();
                return;
            }

            if (ExcelFolderRoute.Length == 0 | Directory.Exists(ExcelFolderRoute) == false)
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Por favor, selecciona una carpeta existente para guardar los archivos de Excel", TipoIncidencia.Advertencia);
                ventana.ShowDialog();
                return;
            }
        }
    }



    public class MyMethods
    {
        public SAPMethod SAP = new SAPMethod();
        public ExcelMethod Excel = new ExcelMethod();
        public FileMethod File = new FileMethod();
    }

    public class ExcelMethod
    {
        public List<string> EstablishExcelRoutes(List<string> SAPFilesRoutes, string SAPFolderRoute, string ExcelFolderRoute)
        {
            List<string> ExcelFilesRoutes = new List<string>();

            foreach (string route in SAPFilesRoutes)
            {
                string ExcelRoute = route.Replace(SAPFolderRoute, ExcelFolderRoute);
                ExcelRoute = System.IO.Path.ChangeExtension(ExcelRoute, ".xlsx");
                ExcelFilesRoutes.Add(ExcelRoute);
            }

            return ExcelFilesRoutes;
        }


        public void PrintReactionsToExcel(string ExcelFileRoute, object[,] reactions, string Sheet, string PasteRange)
        {
            FileInfo archivoExcel = new FileInfo(ExcelFileRoute);

            using (ExcelPackage paquete = new ExcelPackage(archivoExcel))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[Sheet];  // Selecciona la hoja

                int filas = reactions.GetLength(0);
                int columnas = reactions.GetLength(1);

                // Convertir el rango de inicio en coordenadas
                var inicio = hoja.Cells[PasteRange].Start;

                for (int i = 0; i < filas; i++)
                {
                    for (int j = 0; j < columnas; j++)
                    {
                        hoja.Cells[inicio.Row + i, inicio.Column + j].Value = reactions[i, j];
                    }
                }

                paquete.Save();
            }
        }


        public void AddHeaderFormat(string ExcelFileRoute, string Sheet, string PasteRange)
        {
            FileInfo archivoExcel = new FileInfo(ExcelFileRoute);

            using (ExcelPackage paquete = new ExcelPackage(archivoExcel))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[Sheet];
                var inicio = hoja.Cells[PasteRange].Start;

                int startRow = inicio.Row;
                int startCol = inicio.Column;
                int columnas = 10;

                var rangoFila1 = hoja.Cells[startRow, startCol, startRow, startCol + columnas - 1];
                rangoFila1.Merge = true;
                rangoFila1.Style.Font.Bold = true;
                rangoFila1.Style.Fill.PatternType = ExcelFillStyle.Solid;
                rangoFila1.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(51, 204, 204));

                for (int i = 0; i < columnas; i++)
                {
                    var celda = hoja.Cells[startRow + 1, startCol + i];
                    celda.Style.Font.Bold = true;
                    celda.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    celda.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(204, 255, 255));
                }

                for (int i = 0; i < columnas; i++)
                {
                    var celda = hoja.Cells[startRow + 2, startCol + i];
                    celda.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    celda.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(204, 255, 255));
                }

                paquete.Save();
            }
        }


        public void FillWorksheet(int ArrayIndex, int StrategyIndex, string ExcelRoute, object[,] reactions, int action)
        {
            string[] InputSheet;
            string InputPosition;

            switch (action)
            {
                case 1:
                    InputSheet = new string[] { "DEF_mot_EXP", "INTER_mot_EXP", "FUNC_mot_EXP", "DEF_mot_RESG", "INTER_mot_RESG", "FUNC_mot_RESG" };
                    InputPosition = "B5";
                    break;
                case 2:
                    InputSheet = new string[] { "DEF_gen_EXP", "INTER_gen_EXP", "FUNC_gen_EXP", "DEF_gen_RESG", "INTER_gen_RESG", "FUNC_gen_RESG" };
                    InputPosition = "B5";
                    break;
                default:
                    InputSheet = new string[] { "DATOS_DEF_EXP", "DATOS_INTER_EXP", "DATOS_FUNC_EXP", "DATOS_DEF_RESG", "DATOS_INTER_RESG", "DATOS_FUNC_RESG" };
                    InputPosition = "A2";
                    break;
            }

            int sheetIndex = ArrayIndex - StrategyIndex * 6;
            PrintReactionsToExcel(ExcelRoute, reactions, InputSheet[sheetIndex], InputPosition);
        }


        public void FillESMAData(string ESMARoute, string POTExcelRoute, bool TrackerType, string[] MPSection, string[] GPSection, int[] GPcount, int index)
        {
            object[] data = GetESMAData(ESMARoute, TrackerType);

            FileInfo archivoExcel = new FileInfo(POTExcelRoute);

            using (ExcelPackage paquete = new ExcelPackage(archivoExcel))
            {

                // Ponemos los datos de la obra
                PasteESMAData(paquete, "DATOS", "B2", data[0]);
                PasteESMAData(paquete, "DATOS", "B3", data[2]);
                PasteESMAData(paquete, "DATOS", "B4", data[3]);

                // Ponemos los datos de panel y tracker
                PasteESMAData(paquete, "DATOS", "B6", data[4]);
                PasteESMAData(paquete, "DATOS", "B7", data[5]);
                PasteESMAData(paquete, "DATOS", "B8", data[6]);
                PasteESMAData(paquete, "DATOS", "B9", data[7]);
                PasteESMAData(paquete, "DATOS", "B10", data[8]);
                PasteESMAData(paquete, "DATOS", "B11", data[9]);
                PasteESMAData(paquete, "DATOS", "B13", data[11]);

                // Ponemos el numero de pilares
                PasteESMAData(paquete, "DATOS", "B14", GPcount[2 * index]);
                PasteESMAData(paquete, "DATOS", "D14", GPcount[2 * index + 1]);

                // Ponemos la inclinacion maxima del tracker
                if (Convert.ToInt32(data[10]) == 60) { PasteESMAData(paquete, "DATOS", "B12", "±60º"); }
                else { PasteESMAData(paquete, "DATOS", "B12", "±55º"); }

                // Ponemos la normativa correcta
                if (data[12].ToString() == "ASCE7-05" || data[12].ToString() == "ASCE7-16") { PasteESMAData(paquete, "DATOS", "E2", "ASCE"); }
                else { PasteESMAData(paquete, "DATOS", "E2", "EU"); }

                // Ponemos el idioma correcto (inglés en la ESMA lleva tilde y en POT no)
                if (data[13].ToString() == "Inglés") { PasteESMAData(paquete, "DATOS", "E3", "Ingles"); }
                else { PasteESMAData(paquete, "DATOS", "E3", "Español"); }

                // Ponemos el nº strings
                PasteESMAData(paquete, "DATOS", "E6", data[14]);
                PasteESMAData(paquete, "DATOS", "E7", data[14]);

                // Ponemos los perfiles
                PasteESMAData(paquete, "DATOS", "C17", MPSection[2 * index]);
                PasteESMAData(paquete, "DATOS", "C21", MPSection[2 * index + 1]);
                PasteESMAData(paquete, "DATOS", "C18", GPSection[2 * index]);
                PasteESMAData(paquete, "DATOS", "C22", GPSection[2 * index + 1]);

                // Ponemos las cargas
                PasteESMAData(paquete, "DATOS", "C28", data[15]);
                PasteESMAData(paquete, "DATOS", "C30", data[16]);
                PasteESMAData(paquete, "DATOS", "C31", data[17]);
                PasteESMAData(paquete, "DATOS", "C33", data[18]);
                PasteESMAData(paquete, "DATOS", "C36", data[19]);
                PasteESMAData(paquete, "DATOS", "C37", data[20]);

                // Guardar los cambios
                paquete.Save();
            }
        }


        public object[] GetESMAData(string ESMARoute, bool TrackerType)
        {
            // Abrimos la ESMA
            FileInfo archivoExcel = new FileInfo(ESMARoute);

            // Inicializamos el output
            object[] data;

            using (ExcelPackage paquete = new ExcelPackage(archivoExcel))
            {
                // Datos generales
                string ProjectName = ObtainESMAData(paquete, "Datos de entrada cálculo", "C5")?.ToString() ?? string.Empty;
                string Reference = ObtainESMAData(paquete, "Datos de entrada cálculo", "C6")?.ToString() ?? string.Empty;
                string localization = ObtainESMAData(paquete, "Datos de entrada cálculo", "G6")?.ToString() ?? string.Empty;
                string country = ObtainESMAData(paquete, "Datos de entrada cálculo", "G5")?.ToString() ?? string.Empty;
                string regulation = ObtainESMAData(paquete, "Datos de entrada cálculo", "I5")?.ToString() ?? string.Empty;
                string language = ObtainESMAData(paquete, "Datos de entrada cálculo", "K6")?.ToString() ?? string.Empty;

                // Variables dependientes del tipo de tracker
                double str = 0.0, length = 0.0, width = 0.0, thickness = 0.0, inclination = 0.0, AxisHeight = 0.0, SelfWeight = 0.0, WindPressure = 0.0, NumberStr = 0.0, weight = 0.0;
                string panel = string.Empty;

                // Variables dependientes de la normativa
                double WindSpeed = 0.0, SnowPressure = 0.0;
                string SeismicZone = string.Empty;
                string Ag = string.Empty;


                if (TrackerType == true) // Tracker tipo 1 ESMA
                {
                    str = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "C30") ?? 0.0), 3);
                    panel = ObtainESMAData(paquete, "Datos de entrada cálculo", "H30")?.ToString() ?? string.Empty;
                    length = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "J30") ?? 0.0), 3);
                    width = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "K30") ?? 0.0), 3);
                    thickness = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "L30") ?? 0.0), 3);
                    weight = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "E33") ?? 0.0), 3);
                    inclination = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "D30") ?? 0.0), 3);
                    AxisHeight = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "E30") ?? 0.0), 3);
                    NumberStr = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "B30") ?? 0.0), 3);

                    SelfWeight = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas", "P8") ?? 0.0), 3);
                    WindPressure = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas", "P10") ?? 0.0), 3);
                }
                else // Tracker tipo 2 ESMA
                {
                    str = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "C53") ?? 0.0), 3);
                    panel = ObtainESMAData(paquete, "Datos de entrada cálculo", "H53")?.ToString() ?? string.Empty;
                    length = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "J53") ?? 0.0), 3);
                    width = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "K53") ?? 0.0), 3);
                    thickness = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "L53") ?? 0.0), 3);
                    weight = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "E56") ?? 0.0), 3);
                    inclination = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "D53") ?? 0.0), 3);
                    AxisHeight = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "E53") ?? 0.0), 3);
                    NumberStr = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Datos de entrada cálculo", "B53") ?? 0.0), 3);

                    SelfWeight = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas", "P35") ?? 0.0), 3);
                    WindPressure = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas", "P37") ?? 0.0), 3);
                }

                // Cargas dependiendo normativa
                switch (regulation)
                {
                    case "EU":
                        WindSpeed = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas EU", "B8") ?? 0.0), 3);
                        SnowPressure = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas EU", "G18") ?? 0.0), 3);
                        SeismicZone = ObtainESMAData(paquete, "Cargas estáticas EU", "E14")?.ToString() ?? string.Empty;
                        Ag = ObtainESMAData(paquete, "Cargas estáticas EU", "F14")?.ToString() ?? string.Empty;
                        break;

                    case "ASCE7-05":
                        WindSpeed = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas ASCE7-05", "B8") ?? 0.0), 3);
                        SnowPressure = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas ASCE7-05", "G19") ?? 0.0), 3);
                        SeismicZone = ObtainESMAData(paquete, "Cargas estáticas ASCE7-05", "E15")?.ToString() ?? string.Empty;
                        Ag = ObtainESMAData(paquete, "Cargas estáticas ASCE7-05", "F15")?.ToString() ?? string.Empty;
                        break;

                    case "ASCE7-16":
                        WindSpeed = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas ASCE7-16", "B8") ?? 0.0), 3);
                        SnowPressure = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas ASCE7-16", "G19") ?? 0.0), 3);
                        SeismicZone = ObtainESMAData(paquete, "Cargas estáticas ASCE7-16", "E15")?.ToString() ?? string.Empty;
                        Ag = ObtainESMAData(paquete, "Cargas estáticas ASCE7-16", "F15")?.ToString() ?? string.Empty;
                        break;

                    case "NTC-2018":
                        WindSpeed = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas NTC-2018", "C8") ?? 0.0), 3);
                        SnowPressure = Math.Round(Convert.ToDouble(ObtainESMAData(paquete, "Cargas estáticas NTC-2018", "G23") ?? 0.0), 3);
                        SeismicZone = ObtainESMAData(paquete, "Cargas estáticas NTC-2018", "E19")?.ToString() ?? string.Empty;
                        Ag = ObtainESMAData(paquete, "Cargas estáticas NTC-2018", "F19")?.ToString() ?? string.Empty;
                        break;

                    default:
                        var ventana = new Incidencias();
                        ventana.ConfigurarIncidencia("Normativa no encontrada", TipoIncidencia.Error);
                        ventana.ShowDialog();
                        break;
                }
                data = new object[] { ProjectName, Reference, localization, country, str, panel, length, width, thickness, weight, inclination, AxisHeight,
                            regulation, language, NumberStr, SelfWeight, WindSpeed, WindPressure, SnowPressure, SeismicZone, Ag };
            }

            return data;
        }


        public object ObtainESMAData(ExcelPackage paquete, string Sheet, string Cell)
        {
            ExcelWorksheet hoja = paquete.Workbook.Worksheets[Sheet];
            return hoja.Cells[Cell].Value;
        }


        public void PasteESMAData(ExcelPackage paquete, string Sheet, string Cell, object value)
        {
            ExcelWorksheet hoja = paquete.Workbook.Worksheets[Sheet];

            // Asignar el valor a la celda
            hoja.Cells[Cell].Value = value;
        }

    }

    public class FileMethod
    {
        string initialFileDirectory = "";
        string initialFolderDirectory = "";


        public string SearchFile(string initialDirectoryGiven = null, string filter = null)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo",
                Filter = string.IsNullOrEmpty(filter) ? "Todos los archivos (*.*)|*.*" : filter,
                InitialDirectory = string.IsNullOrEmpty(initialDirectoryGiven) ? (string.IsNullOrEmpty(initialFileDirectory) ?
                                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : initialFileDirectory) : initialDirectoryGiven
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Filtro para que no se guarde como directorio inicial cuando se coja el archivo excel POT de SSE_Proyectos
                initialFileDirectory = !openFileDialog.FileName.Contains("SSE") ? Path.GetDirectoryName(openFileDialog.FileName) : initialFileDirectory;
                return openFileDialog.FileName;
            }

            return string.Empty;
        }


        public string SearchSAPFile(string initialDirectoryGiven = null)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo",
                Filter = "Archivos SDB (*.sdb)|*.sdb",
                InitialDirectory = string.IsNullOrEmpty(initialDirectoryGiven) ? (string.IsNullOrEmpty(initialFileDirectory) ?
                                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : initialFileDirectory) : initialDirectoryGiven
            };

            if (openFileDialog.ShowDialog() == true)
            {
                initialFileDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                return openFileDialog.FileName;
            }

            return string.Empty;
        }


        public string SearchFolder(string initialDirectoryGiven = null)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog
            {
                Title = "Seleccionar archivo",
                InitialDirectory = string.IsNullOrEmpty(initialDirectoryGiven) ? (string.IsNullOrEmpty(initialFolderDirectory) ?
                                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : initialFolderDirectory) : initialDirectoryGiven
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                initialFolderDirectory = openFolderDialog.FolderName;
                return openFolderDialog.FolderName;
            }
            return string.Empty;
        }


        public void StoreFileRoutes(ref string[] FileRouteList, int index, string tracker)
        {
            var ventana = new Incidencias();
            ventana.ConfigurarIncidencia($"Selecciona el archivo de posicion de defensa {tracker}", TipoIncidencia.Pregunta);
            ventana.ShowDialog();
            FileRouteList[index] = SearchSAPFile();

            ventana.ConfigurarIncidencia($"Selecciona el archivo de posicion intermedia {tracker}", TipoIncidencia.Pregunta);
            ventana.ShowDialog();
            FileRouteList[index + 1] = SearchSAPFile();

            ventana.ConfigurarIncidencia($"Selecciona el archivo de posicion de funcionamiento {tracker}", TipoIncidencia.Pregunta);
            ventana.ShowDialog();
            FileRouteList[index + 2] = SearchSAPFile();
        }


        public string[] CopyPOTExcel(CheckBox[] checkBoxes, TextBox POTSaveRoute, TextBox OriginalRoute)
        {
            string[] Routes = new string[4];
            string baseName = Path.GetFileNameWithoutExtension(OriginalRoute.Text);
            string[] FileName = { "_2str.xlsm", "_1.5str.xlsm", "_1str.xlsm", "_0.5str.xlsm" };

            for (int i = 0; i < checkBoxes.Length; i++)
            {
                if (checkBoxes[i].IsChecked == true)
                {
                    string SaveRoute = Path.Combine(POTSaveRoute.Text, baseName + FileName[i]);
                    File.Copy(OriginalRoute.Text, SaveRoute, true);
                    Routes[i] = SaveRoute;
                }
            }

            return Routes;
        }

    }

    public class SAPMethod
    {
        //cOAPI mySapObject; //aplicación SAP2000
        //protected cSapModel mySapModel; //fichero de SAP dentro del programa
        string ProgramPath = @"C:\Program Files\Computers and Structures\SAP2000 25\SAP2000.exe";//Asignamos la ruta de la aplicación sap2000 para ejecutarlo



        public List<string> SearchSAPFiles(string SAPFolderRoute)
        {
            List<string> SAPFilesRoute = new List<string>();

            foreach (string file in Directory.GetFiles(SAPFolderRoute, "*.sdb", SearchOption.TopDirectoryOnly))
            {
                SAPFilesRoute.Add(file);
            }
            return SAPFilesRoute;
        }


        public cOAPI OpenSAPObject()
        {
            cHelper myHelper = new Helper();
            cOAPI mySapObject = null;

            myHelper = (cHelper)Activator.CreateInstance(Type.GetTypeFromProgID("SAP2000v1.Helper", true));
            mySapObject = myHelper.CreateObject(ProgramPath);
            mySapObject.ApplicationStart(eUnits.N_mm_C);

            return mySapObject;
        }


        //public cSapModel OpenSAPModel(cOAPI SapObject)
        //{
        //    mySapModel = SapObject.SapModel;
        //    mySapModel.InitializeNewModel();

        //    return mySapModel;
        //}


        //public (cOAPI, cSapModel) TrySAPConnection()
        //{
        //    try
        //    {
        //        mySapObject = OpenSAPObject();
        //        mySapModel = OpenSAPModel(mySapObject);
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show("Imposible conexión, intentar de nuevo");
        //        return (null, null);
        //    }

        //    return (mySapObject, mySapModel);
        //}


        public void LoadModels(cSapModel SapModel, string SAPFileRoute)
        {
            SapModel.File.OpenFile(SAPFileRoute);
        }


        private void RunModel(cSapModel SapModel)
        {
            if (SapModel.GetModelIsLocked() == false)
            {
                SapModel.Analyze.RunAnalysis();
            }
        }


        private void SelectHypotesis(cSapModel SapModel, string Combo, bool Deselect)
        {
            if (Deselect == true)
            {
                SapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();
            }

            SapModel.Results.Setup.SetComboSelectedForOutput(Combo);
        }


        public string[] GetNodesTracker(cSapModel SapModel)
        {
            int ret = 0;

            string MPGroup = "01 Pilares Centrales";
            string GPGroup = "02 Pilares Generales";

            int NumberItems = 0;
            int[] ObjectType = new int[1];
            string[] ObjectNameG = new string[1];
            string[] ObjectNameM = new string[1];

            ret = SapModel.GroupDef.GetAssignments(MPGroup, ref NumberItems, ref ObjectType, ref ObjectNameM);
            ret = SapModel.GroupDef.GetAssignments(GPGroup, ref NumberItems, ref ObjectType, ref ObjectNameG);

            string[] piles = new string[ObjectNameM.Length + ObjectNameG.Length];
            ObjectNameM.CopyTo(piles, 0);
            ObjectNameG.CopyTo(piles, ObjectNameM.Length);

            string[] nodes = new string[piles.Length];
            for (int i = 0; i < piles.Length; i++)
            {
                string Base = "";
                string Cabeza = "";
                ret = SapModel.FrameObj.GetPoints(piles[i], ref Base, ref Cabeza);
                nodes[i] = Base;
            }
            return nodes;
        }


        public object[,] ObtainReactionsOfNodes(cSapModel SapModel, string[] nodes)
        {
            int ret = 0;

            int NumberResults = 1;
            string[] obj = new string[1];
            string[] elm = new string[1];
            string[] LoadCase = new string[1];
            string[] StepType = new string[1];
            double[] StepNum = new double[1];
            double[] F1 = new double[1];
            double[] F2 = new double[1];
            double[] F3 = new double[1];
            double[] M1 = new double[1];
            double[] M2 = new double[1];
            double[] M3 = new double[1];

            List<object[]> ReactionsTable = new List<object[]>();

            // Obtenemos las reacciones en los apoyos
            foreach (string node in nodes)
            {
                ret = SapModel.Results.JointReact(node, eItemTypeElm.Element, ref NumberResults, ref obj, ref elm, ref LoadCase, ref StepType, ref StepNum, ref F1, ref F2, ref F3, ref M1, ref M2, ref M3);

                for (int i = 0; i < NumberResults; i++)
                {
                    object[] fila = { node, LoadCase[i], "Combination", StepType[i], F1[i], F2[i], F3[i], M1[i], M2[i], M3[i] };

                    ReactionsTable.Add(fila);
                }
            }

            // Convertimos los resultados de formato array unidimensional a bidimensional (tabla)
            object[,] OutputTable = new object[ReactionsTable.Count, 10];

            for (int i = 0; i < ReactionsTable.Count; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    OutputTable[i, j] = ReactionsTable[i][j];
                }
            }

            return OutputTable;
        }


        public object[,] ObtainReactionsInSAP(cSapModel SapModel)  // Funcion que ejecuta el proceso anterior automaticamente
        {
            SapModel.SetPresentUnits(eUnits.kN_m_C);

            RunModel(SapModel);
            SelectHypotesis(SapModel, "ULS", true);
            string[] nodes = GetNodesTracker(SapModel);
            object[,] reactions = ObtainReactionsOfNodes(SapModel, nodes);

            return reactions;
        }


        public string GetFrameSection(cSapModel SapModel, bool MP)
        {
            string Name = MP ? "Column_0" : "Column_1";
            string PropName = "";
            string SAuto = "";

            SapModel.FrameObj.GetSection(Name, ref PropName, ref SAuto);

            return PropName;
        }


        public object[,] FilterPileReactions(object[,] OutputTable, bool isMotorPile)
        {
            List<int> SelectedPileIndices = new List<int>();

            for (int i = 0; i < OutputTable.GetLength(0); i++)
            {
                bool isMatch = OutputTable[i, 0].ToString() == "mpi";
                if (isMotorPile ? isMatch : !isMatch)
                {
                    SelectedPileIndices.Add(i);
                }
            }

            object[,] FilteredReactions = new object[SelectedPileIndices.Count, 10];
            int index = 0;

            foreach (int i in SelectedPileIndices)
            {
                for (int j = 0; j < 10; j++)
                {
                    FilteredReactions[index, j] = OutputTable[i, j];
                }
                index++;
            }

            return FilteredReactions;
        }


        public void RenameSections(ref string[] PileSection, int index)
        {
            PileSection[index] = PileSection[index].Substring(0, PileSection[index].IndexOf('/')).Trim();

            // Quitamos el guion si es una C
            PileSection[index] = PileSection[index].Replace("-", "");

            // Si son IPE o IPEA ponemos el formato de la Excel POT con espacios
            if (PileSection[index].StartsWith("IPEA"))
            {
                string numbers = PileSection[index].Substring(4);
                PileSection[index] = "IPE A " + numbers;
            }
            else if (PileSection[index].StartsWith("IPE"))
            {
                string numbers = PileSection[index].Substring(3);
                PileSection[index] = "IPE " + numbers;
            }

            // Ponemos el decimal en las C de espesores redondos para que lo reconozca el excel POT
            if (PileSection[index].StartsWith("C"))
            {
                string[] partes = PileSection[index].Split('x');
                string ultimaParte = partes[partes.Length - 1];

                if (!ultimaParte.Contains(","))
                {
                    partes[partes.Length - 1] = ultimaParte + ",0";
                    PileSection[index] = string.Join("x", partes);
                }
            }

        }


        public void CloseModels(cOAPI SAPObject, cSapModel SapModel)
        {
            SAPObject.ApplicationExit(true);
            SAPObject = null;
            SapModel = null;

            GC.Collect(); // Forzar recolección de basura para limpiar instancias
            GC.WaitForPendingFinalizers();
        }

    }

}
