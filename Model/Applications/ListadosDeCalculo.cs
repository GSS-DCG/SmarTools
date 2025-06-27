using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using SmarTools.Model.Repository;
using ModernUI.View;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.View;
using static OfficeOpenXml.ExcelErrorValue;
using System.IO;

namespace SmarTools.Model.Applications
{
    internal class ListadosDeCalculo
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        //Variables absolutas
        public static string rutaPlantillaWord = @"Z:\16 Documentacion\99 Pruebas Extraccion Memoria\Plantilla Advanced report.docx";

        public static void ListadosDeCalculo1V (ListadosDeCalculo1VAPP vista)
        {
            var loadingWindow = new Status();

            MessageBox.Show("Antes de ejecutar, no olvide cerrar todas las ventanas de excel abiertas", "Aviso", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                string rutaSAP = vista.RutaSAP.Text;
                string rutaWord = vista.RutaWord.Text;
                bool sismo = false;

                if (vista.Sismo.IsChecked == true)
                {
                    sismo = true;
                }

                if (Directory.Exists(rutaSAP))
                {
                    string[] modelos = Directory.GetFiles(rutaSAP, "*.sdb");

                    foreach (string modelo in modelos)
                    {
                        ObtenerListados(modelo, rutaWord, sismo, vista);
                    }
                }
                MessageBox.Show("Proceso terminado", "Finalizado", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
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

        public static void ListadosDeCalculo2V(ListadosDeCalculo2VAPP vista)
        {
            var loadingWindow = new Status();

            MessageBox.Show("Antes de ejecutar, no olvide cerrar todas las ventanas de excel abiertas", "Aviso", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                string rutaSAP = vista.RutaSAP.Text;
                string rutaWord = vista.RutaWord.Text;
                bool sismo = false;

                if (vista.Sismo.IsChecked == true)
                {
                    sismo = true;
                }

                if (Directory.Exists(rutaSAP))
                {
                    string[] modelos = Directory.GetFiles(rutaSAP, "*.sdb");

                    foreach (string modelo in modelos)
                    {
                        ObtenerListados2V(modelo, rutaWord, sismo, vista);
                    }
                }
                MessageBox.Show("Proceso terminado", "Finalizado", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
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

        public static void ObtenerListados(string modelo, string rutaWord, bool sismo, ListadosDeCalculo1VAPP vista)
        {
            //Abrimos el modelo SAP2000
            //cOAPI mySapObject = SAP.FileManagerSubclass.OpenSAPObjectHidden();
            cOAPI mySapObject = SAP.FileManagerSubclass.OpenSAPObject();
            cSapModel mySapModel = SAP.FileManagerSubclass.OpenSAPModel(mySapObject);
            SAP.FileManagerSubclass.LoadModels(mySapModel, modelo);

            //Calculamos el modelo, analizamos y cambiamos las unidades
            mySapModel.Analyze.RunAnalysis();
            mySapModel.DesignColdFormed.StartDesign();
            mySapModel.DesignSteel.StartDesign();
            mySapModel.SetPresentUnits(eUnits.kN_m_C);

            //Obtener el listado de las tablas
            string[] tablas = ListadoTablas(vista, mySapModel);

            //Creamos el documento Word a partir de la plantilla
            string[] partes = modelo.Split("\\");
            string nombre = partes.Last().Split(".")[0];
            string nuevoDocumento = WordFunctions.CreateDocument(nombre, rutaWord, rutaPlantillaWord);
            Microsoft.Office.Interop.Word.Document doc = WordFunctions.OpenWord(nuevoDocumento);

            //Apartado 1: CALCULATION MODEL
            WordFunctions.AddText("CALCULATION MODEL", doc, true);

            string[,] tabla = SAP.TablaJointCoordinates(mySapModel);
            InsertarTabla(doc, tabla, "Table 1: Joint Coordinates");

            tabla = SAP.TablaConnectivityFrame(mySapModel);
            InsertarTabla(doc, tabla, "Table 2: Connectivity-Frame");

            tabla = SAP.TablaMaterialColdFormed(mySapModel);
            InsertarTabla(doc, tabla, "Table 3: Material Properties - Cold Formed Data");

            tabla = SAP.TablaMaterialSteel(mySapModel);
            InsertarTabla(doc, tabla, "Table 4: Material Properties - Steel Data");

            tabla = SAP.TablaSectionProperties(mySapModel);
            InsertarTabla(doc, tabla, "Table 5: Frame Section Properties - General");

            tabla = SAP.TablaSectionAssignments(mySapModel);
            InsertarTabla(doc, tabla, "Table 6: Frame Section Assignments");

            //Apartado 2:REACTIONS
            WordFunctions.AddText("REACTIONS", doc, true);

            tabla = SAP.TablaJointReactions(mySapModel);
            InsertarTabla(doc, tabla, "Table 7: Joint Reactions");

            //Apartado 3:RESULTS
            WordFunctions.AddText("RESULTS", doc, true);

            tabla = SAP.TablaElementForces(mySapModel);
            InsertarTabla(doc, tabla, "Table 8:Element Forces - Frames");

            WordFunctions.AddText("P: Axial Force", doc);
            WordFunctions.AddText("V2: Shear Force in strong axis (2)", doc);
            WordFunctions.AddText("V3: Shear Force in weak axis (3)", doc);
            WordFunctions.AddText("T: Torsional force", doc);
            WordFunctions.AddText("M2: Bending moment on axis 2", doc);
            WordFunctions.AddText("M3: Bending moment on axis 3", doc);
            WordFunctions.AddPageBreak(doc);

            tabla = SAP.TablaColdFormedDesign1(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 9:Cold Formed Design 1 - Summary Data");

            tabla = SAP.TablaColdFormedDesign2(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 10:Cold Formed Design 2 - Axial Details");

            tabla = SAP.TablaColdFormedDesign3a(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 11:Cold Formed Design 3a - Flexure Details Y-Y");

            tabla = SAP.TablaColdFormedDesign3b(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 12:Cold Formed Design 3b - Flexure Details Z-Z");

            tabla = SAP.TablaColdFormedDesign4(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 13:Cold Formed Design 4 - Shear Details");

            tabla = SAP.TablaSteelDesign1(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 14: Steel Design 1 - Summary Data");

            tabla = SAP.TablaSteelDesign2(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 15: Steel Design 2 - NMM Details");

            //Apartado 4:SEISMIC
            if (sismo == true)
            {
                WordFunctions.AddText("SEISMIC", doc, true);

                tabla = SAP.TablaResponseSpectrum1(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 16.1: Response Spectrum Modal Information");

                tabla = SAP.TablaResponseSpectrum2(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 16.2: Response Spectrum Modal Information");

                tabla = SAP.TablaModalParticipatingRatios1(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 17.1: Modal Participating Mass Ratios");

                tabla = SAP.TablaModalParticipatingRatios2(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 17.2: Modal Participating Mass Ratios");

                tabla = SAP.TablaModalLoadRatios(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 18: Modal Load Participation Ratios");
            }

            //Cerrar documento word y el modelo de sap
            WordFunctions.CloseWord(doc);
            mySapObject.ApplicationExit(false);
        }

        public static void ObtenerListados2V(string modelo, string rutaWord, bool sismo, ListadosDeCalculo2VAPP vista)
        {
            //Abrimos el modelo SAP2000
            //cOAPI mySapObject = SAP.FileManagerSubclass.OpenSAPObjectHidden();
            cOAPI mySapObject = SAP.FileManagerSubclass.OpenSAPObject();
            cSapModel mySapModel = SAP.FileManagerSubclass.OpenSAPModel(mySapObject);
            SAP.FileManagerSubclass.LoadModels(mySapModel, modelo);

            //Calculamos el modelo, analizamos y cambiamos las unidades
            mySapModel.Analyze.RunAnalysis();
            mySapModel.DesignColdFormed.StartDesign();
            mySapModel.DesignSteel.StartDesign();
            mySapModel.SetPresentUnits(eUnits.kN_m_C);

            //Obtener el listado de las tablas
            string[] tablas = ListadoTablas2V(vista, mySapModel);

            //Creamos el documento Word a partir de la plantilla
            string[] partes = modelo.Split("\\");
            string nombre = partes.Last().Split(".")[0];
            string nuevoDocumento = WordFunctions.CreateDocument(nombre, rutaWord, rutaPlantillaWord);
            Microsoft.Office.Interop.Word.Document doc = WordFunctions.OpenWord(nuevoDocumento);

            //Apartado 1: CALCULATION MODEL
            WordFunctions.AddText("CALCULATION MODEL", doc, true);

            string[,] tabla = SAP.TablaJointCoordinates(mySapModel);
            InsertarTabla(doc, tabla, "Table 1: Joint Coordinates");

            tabla = SAP.TablaConnectivityFrame(mySapModel);
            InsertarTabla(doc, tabla, "Table 2: Connectivity-Frame");

            tabla = SAP.TablaMaterialColdFormed(mySapModel);
            InsertarTabla(doc, tabla, "Table 3: Material Properties - Cold Formed Data");

            tabla = SAP.TablaMaterialSteel(mySapModel);
            InsertarTabla(doc, tabla, "Table 4: Material Properties - Steel Data");

            tabla = SAP.TablaSectionProperties(mySapModel);
            InsertarTabla(doc, tabla, "Table 5: Frame Section Properties - General");

            tabla = SAP.TablaSectionAssignments(mySapModel);
            InsertarTabla(doc, tabla, "Table 6: Frame Section Assignments");

            //Apartado 2:REACTIONS
            WordFunctions.AddText("REACTIONS", doc, true);

            tabla = SAP.TablaJointReactions(mySapModel);
            InsertarTabla(doc, tabla, "Table 7: Joint Reactions");

            //Apartado 3:RESULTS
            WordFunctions.AddText("RESULTS", doc, true);

            tabla = SAP.TablaElementForces(mySapModel);
            InsertarTabla(doc, tabla, "Table 8:Element Forces - Frames");

            WordFunctions.AddText("P: Axial Force", doc);
            WordFunctions.AddText("V2: Shear Force in strong axis (2)", doc);
            WordFunctions.AddText("V3: Shear Force in weak axis (3)", doc);
            WordFunctions.AddText("T: Torsional force", doc);
            WordFunctions.AddText("M2: Bending moment on axis 2", doc);
            WordFunctions.AddText("M3: Bending moment on axis 3", doc);
            WordFunctions.AddPageBreak(doc);

            tabla = SAP.TablaColdFormedDesign1(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 9:Cold Formed Design 1 - Summary Data");

            tabla = SAP.TablaColdFormedDesign2(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 10:Cold Formed Design 2 - Axial Details");

            tabla = SAP.TablaColdFormedDesign3a(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 11:Cold Formed Design 3a - Flexure Details Y-Y");

            tabla = SAP.TablaColdFormedDesign3b(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 12:Cold Formed Design 3b - Flexure Details Z-Z");

            tabla = SAP.TablaColdFormedDesign4(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 13:Cold Formed Design 4 - Shear Details");

            tabla = SAP.TablaSteelDesign1(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 14: Steel Design 1 - Summary Data");

            tabla = SAP.TablaSteelDesign2(mySapModel, tablas);
            InsertarTabla(doc, tabla, "Table 15: Steel Design 2 - NMM Details");

            //Apartado 4:SEISMIC
            if (sismo == true)
            {
                WordFunctions.AddText("SEISMIC", doc, true);

                tabla = SAP.TablaResponseSpectrum1(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 16.1: Response Spectrum Modal Information");

                tabla = SAP.TablaResponseSpectrum2(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 16.2: Response Spectrum Modal Information");

                tabla = SAP.TablaModalParticipatingRatios1(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 17.1: Modal Participating Mass Ratios");

                tabla = SAP.TablaModalParticipatingRatios2(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 17.2: Modal Participating Mass Ratios");

                tabla = SAP.TablaModalLoadRatios(mySapModel, tablas);
                InsertarTabla(doc, tabla, "Table 18: Modal Load Participation Ratios");
            }

            //Cerrar documento word y el modelo de sap
            WordFunctions.CloseWord(doc);
            mySapObject.ApplicationExit(false);
        }

        public static string[] ListadoTablas(ListadosDeCalculo1VAPP vista, cSapModel mySapModel)
        {
            int NumberTables = 0;
            string[] TableKey = new string[500];
            string[] TableName = new string[500];
            int[] importType = new int[500];
            bool[] isEmpty = new bool[500];
            string[] tablas = new string[18];
            
            int ret = mySapModel.DatabaseTables.GetAllTables(ref NumberTables, ref TableKey, ref TableName, ref importType, ref isEmpty);
            ret = mySapModel.DatabaseTables.GetAvailableTables(ref NumberTables, ref TableKey, ref TableName, ref importType);

            for (int i = 0; i < NumberTables; i++)
            {
                if (TableName[i].Contains("Joint Coordinates"))
                {
                    tablas[0] = TableName[i];
                }
                if (TableName[i].Contains("Connectivity") && TableName[i].Contains("-") && TableName[i].Contains("Frame"))
                {
                    tablas[1] = TableName[i];
                }
                if (TableName[i].Contains("Material Properties") && TableName[i].Contains("03d") && TableName[i].Contains("Cold Formed Data"))
                {
                    tablas[2] = TableName[i];
                }
                if (TableName[i].Contains("Material Properties") && TableName[i].Contains("03a") && TableName[i].Contains("Steel Data"))
                {
                    tablas[3] = TableName[i];
                }
                if (TableName[i].Contains("Frame Section Properties 01") && TableName[i].Contains("-") && TableName[i].Contains("General"))
                {
                    tablas[4] = TableName[i];
                }
                if (TableName[i].Contains("Frame Section Assignments"))
                {
                    tablas[5] = TableName[i];
                }
                if (TableName[i].Contains("Joint Reactions"))
                {
                    tablas[6] = TableName[i];
                }
                if (TableName[i].Contains("Element Forces") && TableName[i].Contains("-") && TableName[i].Contains("Frames"))
                {
                    tablas[7] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 1") && TableName[i].Contains("-") && TableName[i].Contains("Summary Data"))
                {
                    tablas[8] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 2") && TableName[i].Contains("-") && TableName[i].Contains("Axial Details"))
                {
                    tablas[9] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 3a") && TableName[i].Contains("-") && TableName[i].Contains("Flexure Details"))
                {
                    tablas[10] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 3b") && TableName[i].Contains("-") && TableName[i].Contains("Flexure Details"))
                {
                    tablas[11] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 4") && TableName[i].Contains("-") && TableName[i].Contains("Shear Details"))
                {
                    tablas[12] = TableName[i];
                }
                if (TableName[i].Contains("Steel Design 1") && TableName[i].Contains("-") && TableName[i].Contains("Summary Data"))
                {
                    tablas[13] = TableName[i];
                }
                if (TableName[i].Contains("Steel Design 2") && TableName[i].Contains("-") && TableName[i].Contains("Details"))
                {
                    tablas[14] = TableName[i];
                }
                if (TableName[i] == "Response Spectrum Modal Information")
                {
                    tablas[15] = TableName[i];
                }
                if (TableName[i] == "Modal Participating Mass Ratios")
                {
                    tablas[16] = TableName[i];
                }
                if (TableName[i] == "Modal Load Participation Ratios")
                {
                    tablas[17] = TableName[i];
                }
            }

            return tablas;
        }

        public static string[] ListadoTablas2V(ListadosDeCalculo2VAPP vista, cSapModel mySapModel)
        {
            int NumberTables = 0;
            string[] TableKey = new string[500];
            string[] TableName = new string[500];
            int[] importType = new int[500];
            bool[] isEmpty = new bool[500];
            string[] tablas = new string[18];

            int ret = mySapModel.DatabaseTables.GetAllTables(ref NumberTables, ref TableKey, ref TableName, ref importType, ref isEmpty);
            ret = mySapModel.DatabaseTables.GetAvailableTables(ref NumberTables, ref TableKey, ref TableName, ref importType);

            for (int i = 0; i < NumberTables; i++)
            {
                if (TableName[i].Contains("Joint Coordinates"))
                {
                    tablas[0] = TableName[i];
                }
                if (TableName[i].Contains("Connectivity") && TableName[i].Contains("-") && TableName[i].Contains("Frame"))
                {
                    tablas[1] = TableName[i];
                }
                if (TableName[i].Contains("Material Properties") && TableName[i].Contains("03d") && TableName[i].Contains("Cold Formed Data"))
                {
                    tablas[2] = TableName[i];
                }
                if (TableName[i].Contains("Material Properties") && TableName[i].Contains("03a") && TableName[i].Contains("Steel Data"))
                {
                    tablas[3] = TableName[i];
                }
                if (TableName[i].Contains("Frame Section Properties 01") && TableName[i].Contains("-") && TableName[i].Contains("General"))
                {
                    tablas[4] = TableName[i];
                }
                if (TableName[i].Contains("Frame Section Assignments"))
                {
                    tablas[5] = TableName[i];
                }
                if (TableName[i].Contains("Joint Reactions"))
                {
                    tablas[6] = TableName[i];
                }
                if (TableName[i].Contains("Element Forces") && TableName[i].Contains("-") && TableName[i].Contains("Frames"))
                {
                    tablas[7] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 1") && TableName[i].Contains("-") && TableName[i].Contains("Summary Data"))
                {
                    tablas[8] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 2") && TableName[i].Contains("-") && TableName[i].Contains("Axial Details"))
                {
                    tablas[9] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 3a") && TableName[i].Contains("-") && TableName[i].Contains("Flexure Details"))
                {
                    tablas[10] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 3b") && TableName[i].Contains("-") && TableName[i].Contains("Flexure Details"))
                {
                    tablas[11] = TableName[i];
                }
                if (TableName[i].Contains("Cold Formed Design 4") && TableName[i].Contains("-") && TableName[i].Contains("Shear Details"))
                {
                    tablas[12] = TableName[i];
                }
                if (TableName[i].Contains("Steel Design 1") && TableName[i].Contains("-") && TableName[i].Contains("Summary Data"))
                {
                    tablas[13] = TableName[i];
                }
                if (TableName[i].Contains("Steel Design 2") && TableName[i].Contains("-") && TableName[i].Contains("Details"))
                {
                    tablas[14] = TableName[i];
                }
                if (TableName[i] == "Response Spectrum Modal Information")
                {
                    tablas[15] = TableName[i];
                }
                if (TableName[i] == "Modal Participating Mass Ratios")
                {
                    tablas[16] = TableName[i];
                }
                if (TableName[i] == "Modal Load Participation Ratios")
                {
                    tablas[17] = TableName[i];
                }
            }

            return tablas;
        }

        public static void InsertarTabla(Microsoft.Office.Interop.Word.Document doc, string[,] tabla, string titulo)
        {
            WordFunctions.AddText(titulo, doc);
            Microsoft.Office.Interop.Excel.Workbook libro = ExcelFunctions.PasteTableInExcel(tabla);
            FormatoCopiarExcelYPegaWord(libro, doc);
        }

        public static void FormatoCopiarExcelYPegaWord(Microsoft.Office.Interop.Excel.Workbook libro, Microsoft.Office.Interop.Word.Document doc)
        {
            ExcelFunctions.FormatSubclass.ApplyFont(8, "Neo Tech Std", libro, true, true);
            ExcelFunctions.FormatSubclass.ApplyColorToRow(1, System.Drawing.Color.Black, System.Drawing.Color.LightBlue, libro);
            ExcelFunctions.FormatSubclass.ApplyFontToRow(9, "Neo Tech Std", 1, libro, true, true);
            ExcelFunctions.CopyExcel(libro);
            WordFunctions.Paste(doc);
            WordFunctions.AutoFitTableWidth(doc);
            WordFunctions.FormatHeaderRow(doc, 1);
            WordFunctions.AddPageBreak(doc);
            ExcelFunctions.CloseExcel();
        }
    }
}