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
    internal class CambiarCargas1V
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        public static void CargarDatos(CambiarCargas1VAPP vista)
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
                    vista.Presion_Pico,
                    vista.Presion,
                    vista.Succion,
                };
                foreach (var textbox in textboxs)
                {
                    textbox.Text = string.Empty;
                }

                using (ExcelPackage package = new ExcelPackage(rutaArchivo))
                {
                    //Obtenemos los datos y los pegamos directamente en la ventana
                    //Tracker tipo 1
                    if (vista.TrackerTipo_1.IsChecked == true)
                    {
                        vista.PesoPropio_Panel.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Peso_Propio_Panel_Tr1").ToString("F3");
                        vista.PesoPropio_Cable.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Carga_Cable_Tr1").ToString("F3");

                        if (vista.Eurocodigo.IsChecked == true)
                        {
                            vista.Friccion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Friccion_EU").ToString("F3");
                        }
                        else if (vista.NTC2018.IsChecked == true)
                        {
                            vista.Friccion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Friccion_NTC2018").ToString("F3");
                        }
                        else if (vista.ASCE7_05.IsChecked == true)
                        {
                            vista.Presion.Text=ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_ASCE7_05").ToString("F3");
                            vista.Succion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Succion_ASCE7_05").ToString("F3");
                        }
                        else if (vista.ASCE7_16.IsChecked == true)
                        {
                            vista.Presion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_ASCE7_16").ToString("F3");
                            vista.Succion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Succion_ASCE7_16").ToString("F3");
                        }
                        if (vista.Expuesto.IsChecked == true)
                        {
                            if (vista.Defensa.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Defensa_Tr1").ToString("F3");
                                vista.Succion_Sup.Text = "N/A";
                                vista.Succion_Inf.Text = "N/A";
                            }
                            else if (vista.Funcionamiento.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Expuesto_Func_Tr1").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Expuesto_Func_Tr1").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Expuesto_Func_Tr1").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Expuesto_Func_Tr1").ToString("F3");
                                vista.Succion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Ssup_Expuesto_Func_Tr1").ToString("F3");
                                vista.Succion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Sinf_Expuesto_Func_Tr1").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Func_Tr1").ToString("F3");
                            }
                        }
                        else if (vista.Resguardo.IsChecked == true)
                        {
                            if (vista.Defensa.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Resguardo_Defensa_Tr1").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Resguardo_Defensa_Tr1").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Resguardo_Defensa_Tr1").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Resguardo_Defensa_Tr1").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Defensa_Tr1").ToString("F3");
                                vista.Succion_Sup.Text = "N/A";
                                vista.Succion_Inf.Text = "N/A";
                            }
                            else if (vista.Funcionamiento.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Resguardo_Func_Tr1").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Resguardo_Func_Tr1").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Resguardo_Func_Tr1").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Resguardo_Func_Tr1").ToString("F3");
                                vista.Succion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Ssup_Resguardo_Func_Tr1").ToString("F3");
                                vista.Succion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Sinf_Resguardo_Func_Tr1").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Func_Tr1").ToString("F3");
                            }
                        }
                    }

                    //Tracker tipo 2
                    else if (vista.TrackerTipo_2.IsChecked == true)
                    {
                        vista.PesoPropio_Panel.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Peso_Propio_Panel_Tr2").ToString("F3");
                        vista.PesoPropio_Cable.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Carga_Cable_Tr2").ToString("F3");

                        if (vista.Eurocodigo.IsChecked == true)
                        {
                            vista.Friccion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Friccion_EU").ToString("F3");
                        }
                        else if (vista.NTC2018.IsChecked == true)
                        {
                            vista.Friccion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Friccion_NTC2018").ToString("F3");
                        }
                        else if (vista.ASCE7_05.IsChecked == true)
                        {
                            vista.Presion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_ASCE7_05").ToString("F3");
                            vista.Succion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Succion_ASCE7_05").ToString("F3");
                        }
                        else if (vista.ASCE7_16.IsChecked == true)
                        {
                            vista.Presion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_ASCE7_16").ToString("F3");
                            vista.Succion.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Succion_ASCE7_16").ToString("F3");
                        }
                        if (vista.Expuesto.IsChecked == true)
                        {
                            if (vista.Defensa.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Expuesto_Defensa_Tr2").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Expuesto_Defensa_Tr2").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Expuesto_Defensa_Tr2").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Expuesto_Defensa_Tr2").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Defensa_Tr2").ToString("F3");
                                vista.Succion_Sup.Text = "N/A";
                                vista.Succion_Inf.Text = "N/A";
                            }
                            else if (vista.Funcionamiento.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Expuesto_Func_Tr2").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Expuesto_Func_Tr2").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Expuesto_Func_Tr2").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Expuesto_Func_Tr2").ToString("F3");
                                vista.Succion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Ssup_Expuesto_Func_Tr2").ToString("F3");
                                vista.Succion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Sinf_Expuesto_Func_Tr2").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Func_Tr2").ToString("F3");
                            }
                        }
                        else if (vista.Resguardo.IsChecked == true)
                        {
                            if (vista.Defensa.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Resguardo_Defensa_Tr2").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Resguardo_Defensa_Tr2").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Resguardo_Defensa_Tr2").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Resguardo_Defensa_Tr2").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Defensa_Tr2").ToString("F3");
                                vista.Succion_Sup.Text = "N/A";
                                vista.Succion_Inf.Text = "N/A";
                            }
                            else if (vista.Funcionamiento.IsChecked == true)
                            {
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Resguardo_Func_Tr2").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Resguardo_Func_Tr2").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Resguardo_Func_Tr2").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Resguardo_Func_Tr2").ToString("F3");
                                vista.Succion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Ssup_Resguardo_Func_Tr2").ToString("F3");
                                vista.Succion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Sinf_Resguardo_Func_Tr2").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Func_Tr2").ToString("F3");
                            }
                        }
                    }
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

        public static void AsignarCargas(CambiarCargas1VAPP vista)
        {
            Herramientas.AbrirArchivoSAP2000();
            var loadingWindow = new Status();
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                var textboxs = new TextBox[]
                {
                    //vista.PesoPropio_Panel,
                    //vista.PesoPropio_Cable,
                    //vista.Carga_Nieve,
                    //vista.Carga_NieveAccidental,
                    //vista.Presion_Sup,
                    //vista.Presion_Inf,
                    //vista.Succion_Sup,
                    //vista.Succion_Inf,
                    vista.Presion_Pico,
                    vista.Friccion,
                    vista.Presion,
                    vista.Succion,
                };
                
                if(vista.PesoPropio_Check.IsChecked == true && double.TryParse(vista.PesoPropio_Panel.Text, out var PP_Panel))
                    CargaGravitacionalPaneles(PP_Panel,"PP Paneles");

                if(vista.PesoCable_Check.IsChecked==true && double.TryParse(vista.PesoPropio_Cable.Text,out var PP_Cable))
                    PesoPropioCable(PP_Cable);

                if (vista.CargaNieve_Check.IsChecked == true && double.TryParse(vista.Carga_Nieve.Text, out var Nieve))
                    CargaGravitacionalPaneles(Nieve, "Snow");

                if (vista.CargaNieveAccidental_Check.IsChecked == true && double.TryParse(vista.Carga_NieveAccidental.Text, out var NieveAccidental))
                    CargaGravitacionalPaneles(NieveAccidental, "Accidental_Snow");

                if (vista.PresionSup_Check.IsChecked == true && double.TryParse(vista.Presion_Sup.Text, out var PresionSup))
                    CargaPresionPanelSuperior(PresionSup, "W1_Pos_Cfmin");

                if (vista.PresionInf_Check.IsChecked == true && double.TryParse(vista.Presion_Inf.Text, out var PresionInf))
                    CargaPresionPanelInferior(PresionInf, "W1_Pos_Cfmin");

                if (vista.SuccionSup_Check.IsChecked == true && double.TryParse(vista.Succion_Sup.Text, out var SuccionSup))
                    CargaPresionPanelSuperior(SuccionSup, "W1_Neg_Cfmin");

                if (vista.SuccionInf_Check.IsChecked == true && double.TryParse(vista.Succion_Inf.Text, out var SuccionInf))
                    CargaPresionPanelInferior(SuccionInf, "W1_Neg_Cfmin");

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

        public static void CargaGravitacionalPaneles(double carga, string hipotesis)
        {
            //Seleccionamos todos los paneles y sustituimos la carga actual por la nueva en la hipótesis elegida
            mySapModel.SelectObj.ClearSelection();
            int ret = mySapModel.AreaObj.SetSelected("06 Paneles", true, eItemType.Group);

            int NumberItems = 0;
            int[] ObjecType = new int[1];
            string[] ObjectName = new string[1];
            ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjecType, ref ObjectName);
            for (int i = 0; i < NumberItems; i++)
            {
                ret = mySapModel.AreaObj.SetLoadUniformToFrame(ObjectName[i], hipotesis, carga, 10, 1);
            }
            mySapModel.SelectObj.ClearSelection();
        }

        public static void PesoPropioCable(double PP_Cable)
        {
            //Obtenemos los nombres de los puntos inferiores de las secundarias
            string[] secundarias=SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel,false).Concat(SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, false)).ToArray();
            string[] nudos = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, secundarias, 2);
            double[] carga = { 0, 0, -PP_Cable, 0, 0, 0 };
            //Asignamos las cargas
            for (int i=0; i < nudos.Length; i++)
            {
                int ret = mySapModel.PointObj.SetLoadForce(nudos[i], "PP Paneles",ref carga);
            }
        }

        public static void CargaPresionPanelSuperior(double carga,string hipotesis)
        {
            //Buscamos la altura del tracker para poder seleccionar solo los paneles superiores
            double x = 0, y = 0, z = 0;
            mySapModel.PointElm.GetCoordCartesian("mps",ref x,ref y,ref z);

            //Para buscar un punto por encima de H, le sumamos 1mm
            double H = z + 0.001;

            //Seleccionamos los paneles superiores
            mySapModel.SelectObj.ClearSelection();
            int ret = mySapModel.SelectObj.CoordinateRange(0,10,-50,50,H,10,false,"Global",true,false,false,true,false,false);

            //Sustituimos la carga
            int NumberItems = 0;
            int[] ObjecType = new int[1];
            string[] ObjectName = new string[1];
            ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjecType, ref ObjectName);
            for (int i = 0; i < NumberItems; i++)
            {
                ret = mySapModel.AreaObj.SetLoadUniformToFrame(ObjectName[i], hipotesis, carga, 3, 1);
            }
            mySapModel.SelectObj.ClearSelection();
        }

        public static void CargaPresionPanelInferior(double carga, string hipotesis)
        {
            //Buscamos la altura del tracker para poder seleccionar solo los paneles inferiores
            double x = 0, y = 0, z = 0;
            mySapModel.PointElm.GetCoordCartesian("mps", ref x, ref y, ref z);

            //Para buscar un punto por debajo de H, le restamos 1mm
            double H = z - 0.001;

            //Seleccionamos los paneles inferiores
            mySapModel.SelectObj.ClearSelection();
            int ret = mySapModel.SelectObj.CoordinateRange(0, -10, -50, 50, H, -10, false, "Global", true, false, false, true, false, false);

            //Sustituimos la carga
            int NumberItems = 0;
            int[] ObjecType = new int[1];
            string[] ObjectName = new string[1];
            ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjecType, ref ObjectName);
            for (int i = 0; i < NumberItems; i++)
            {
                ret = mySapModel.AreaObj.SetLoadUniformToFrame(ObjectName[i], hipotesis, carga, 3, 1);
            }
            mySapModel.SelectObj.ClearSelection();
        }
    }
}
