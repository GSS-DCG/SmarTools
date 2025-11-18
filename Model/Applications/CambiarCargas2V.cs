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
using DocumentFormat.OpenXml.Math;
using System.Windows.Documents;
//using System.Windows.Forms;

namespace SmarTools.Model.Applications
{
    internal class CambiarCargas2V
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static int Ancho = 0;
        public static int Altura = 0;

        public static void CargarDatos(CambiarCargas2VAPP vista)
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
                    vista.Presion_Pico,
                    vista.Friccion,
                    vista.Presion,
                    vista.Succion,
                    vista.G,
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
                                vista.Carga_Nieve.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Nieve_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Carga_NieveAccidental.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "NieveAcc_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Presion_Sup.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Psup_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Presion_Inf.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Pinf_Expuesto_Defensa_Tr1").ToString("F3");
                                vista.Presion_Pico.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "Presion_pico_Defensa_Tr1").ToString("F3");
                                vista.Succion_Sup.Text = "N/A";
                                vista.Succion_Inf.Text = "N/A";
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Expuesto_Defensa_Tr1").ToString("F3");
                                }
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
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Expuesto_Func_Tr1").ToString("F3");
                                }
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
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Resguardo_Defensa_Tr1").ToString("F3");
                                }
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
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Resguardo_Func_Tr1").ToString("F3");
                                }
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
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Expuesto_Defensa_Tr2").ToString("F3");
                                }
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
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Expuesto_Func_Tr2").ToString("F3");
                                }
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
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Resguardo_Defensa_Tr2").ToString("F3");
                                }
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
                                if (vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                                {
                                    vista.G.Text = ExcelFunctions.LeerCeldaPorNombre(rutaArchivo, "G_Resguardo_Func_Tr2").ToString("F3");
                                }
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

        public static void AsignarCargas(CambiarCargas2VAPP vista)
        {
            Herramientas.AbrirArchivoSAP2000();
            var loadingWindow = new Status();
            mySapModel.SetPresentUnits(eUnits.kN_m_C);
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();
                SAP.AnalysisSubclass.UnlockModel(mySapModel);

                if (vista.PesoPropio_Check.IsChecked == true && double.TryParse(vista.PesoPropio_Panel.Text, out var PP_Panel))
                    CargaGravitacionalPaneles(PP_Panel, "PP PANELES");

                if (vista.PesoCable_Check.IsChecked == true && double.TryParse(vista.PesoPropio_Cable.Text, out var PP_Cable))
                    PesoPropioCable(PP_Cable);

                if (vista.CargaNieve_Check.IsChecked == true && double.TryParse(vista.Carga_Nieve.Text, out var Nieve))
                    CargaGravitacionalPaneles(Nieve, "Snow");

                if (vista.CargaNieveAccidental_Check.IsChecked == true && double.TryParse(vista.Carga_NieveAccidental.Text, out var NieveAccidental))
                    CargaGravitacionalPaneles(NieveAccidental, "Accidental_Snow");

                if (vista.PresionSup_Check.IsChecked == true && double.TryParse(vista.Presion_Sup.Text, out var PresionSup))
                    CargaPresionPanelSuperior(PresionSup, "W1_Pos_Cfmin", true);

                if (vista.PresionInf_Check.IsChecked == true && double.TryParse(vista.Presion_Inf.Text, out var PresionInf))
                    CargaPresionPanelInferior(PresionInf, "W1_Pos_Cfmin", true);

                if (vista.SuccionSup_Check.IsChecked == true && double.TryParse(vista.Succion_Sup.Text, out var SuccionSup))
                    CargaPresionPanelSuperior(SuccionSup, "W1_Neg_Cfmin", true);

                if (vista.SuccionInf_Check.IsChecked == true && double.TryParse(vista.Succion_Inf.Text, out var SuccionInf))
                    CargaPresionPanelInferior(SuccionInf, "W1_Neg_Cfmin", true);

                if (vista.VientoLateral_Check.IsChecked==true)
                {
                    if(vista.ASCE7_05.IsChecked == true || vista.ASCE7_16.IsChecked == true)
                    {
                        if(double.TryParse(vista.Presion_Pico.Text, out var PresionPico)&& 
                            double.TryParse(vista.Presion.Text, out var Presion)&& 
                            double.TryParse(vista.Succion.Text, out var Succion)&& 
                            double.TryParse(vista.G.Text, out var G))
                        {
                            LimpiarCargaLateral();
                            double carga = PresionPico * G * 1.95 / 1000;
                            CargaLateralElementos(carga);
                            CargaPresionPanelSuperior(Presion, "W1_Pos_Cfmin", false);
                            CargaPresionPanelInferior(Presion, "W1_Pos_Cfmin", false);
                            CargaPresionPanelSuperior(Succion, "W1_Neg_Cfmin", false);
                            CargaPresionPanelInferior(Succion, "W1_Neg_Cfmin", false);
                        }
                        else
                        {
                            MessageBox.Show("Debe introducir la presión pico, presión, succión y el valor de G para el cálculo de la carga de viento lateral", "Aviso", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if(vista.Eurocodigo.IsChecked == true || vista.NTC2018.IsChecked == true)
                    {
                        if(double.TryParse(vista.Presion_Pico.Text, out var PresionPico)&& double.TryParse(vista.Friccion.Text, out var Friccion))
                        {
                            LimpiarCargaLateral();
                            double carga = PresionPico * 1.8 / 1000;
                            CargaLateralElementos(carga);
                            CargaPaneles(Friccion);
                        }
                        else
                        {
                            MessageBox.Show("Debe introducir tanto la presión pico como la fuerza de fricción para el cálculo de la carga de viento lateral","Aviso",MessageBoxButton.OK,MessageBoxImage.Error);
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
            string[] secundarias = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel, false).Concat(SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, false)).ToArray();
            string[] nudos = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, secundarias, 2);
            double[] carga = { 0, 0, -PP_Cable, 0, 0, 0 };
            //Asignamos las cargas
            for (int i = 0; i < nudos.Length; i++)
            {
                int ret = mySapModel.PointObj.SetLoadForce(nudos[i], "PP PANELES", ref carga,true,"Global",eItemType.Objects);
            }
        }

        public static void CargaPresionPanelSuperior(double carga, string hipotesis, bool reemplazar)
        {
            //Buscamos la altura del tracker para poder seleccionar solo los paneles superiores
            double x = 0, y = 0, z = 0;
            mySapModel.PointElm.GetCoordCartesian("mps", ref x, ref y, ref z);

            //Para buscar un punto por encima de H, le sumamos 1mm
            double H = z + 0.001;

            //Seleccionamos los paneles superiores
            mySapModel.SelectObj.ClearSelection();
            int ret = mySapModel.SelectObj.CoordinateRange(0, 10, -50, 50, H, 10, false, "Global", true, false, false, true, false, false);

            //Sustituimos la carga
            int NumberItems = 0;
            int[] ObjecType = new int[1];
            string[] ObjectName = new string[1];
            ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjecType, ref ObjectName);
            for (int i = 0; i < NumberItems; i++)
            {
                ret = mySapModel.AreaObj.SetLoadUniformToFrame(ObjectName[i], hipotesis, carga, 3, 1, reemplazar);
            }
            mySapModel.SelectObj.ClearSelection();
        }

        public static void CargaPresionPanelInferior(double carga, string hipotesis, bool reemplazar)
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
                ret = mySapModel.AreaObj.SetLoadUniformToFrame(ObjectName[i], hipotesis, carga, 3, 1, reemplazar);
            }
            mySapModel.SelectObj.ClearSelection();
        }

        public static void CargaLateralElementos(double carga)
        {
            int ret = 0;
            double Fw = 0;

            //Seleccionamos los pilares extremos
            string[] pilares_norte = SAP.ElementFinderSubclass.TrackerSubclass.NorthPiles(mySapModel);
            string[] pilares_sur = SAP.ElementFinderSubclass.TrackerSubclass.SouthPiles(mySapModel);
            string[] pilares = { pilares_norte.Last(), pilares_sur.Last() };

            //Obtenemos el tipo de perfil y sección transversal
            string[] pilar = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, pilares[0]);

            //Seleccionamos las secundarias extremas
            string[] secundarias_norte_sup = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel);
            string[] secundarias_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, false);
            string[] secundarias_sur_sup = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel);
            string[] secundarias_sur_inf = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel, false);
            string[] secundarias = { secundarias_norte_sup.Last(), secundarias_norte_inf.Last(), secundarias_sur_sup.Last(), secundarias_sur_inf.Last() };

            //Obtenemos la altura
            string[] secundaria = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, secundarias[0]);
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            //Obtenemos los refuerzos extremos
            string[] refuerzos_norte_sup=SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel);
            string[] refuerzos_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel,false);
            string[] refuerzos_sur_sup = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryReinforcedBeams(mySapModel);
            string[] refuerzos_sur_inf = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryReinforcedBeams(mySapModel,false);
            string[] refuerzos = {refuerzos_norte_sup.Last(), refuerzos_norte_inf.Last(), refuerzos_sur_sup.Last(), refuerzos_sur_inf.Last() };

            //Asignamos las cargas a los elementos
            if (pilar[1] == "Conformado")
            {
                Match ancho = Regex.Match(pilar[0], @"C-(\d{2,3})x");
                Ancho = int.Parse(ancho.Groups[1].Value);
                Fw = carga * Ancho;
            }
            else if (pilar[1] == "Laminado")
            {
                string FileName = "", MatProp = "", Notes = "", GUID = "";
                double T3 = 0, T2 = 0, Tf = 0, Tw = 0, T2b = 0, Tfb = 0, FilletRadius = 0;
                int Color = 0;
                ret=mySapModel.PropFrame.GetISection_1(pilar[0], ref FileName, ref MatProp, ref T3, ref T2, ref Tf, ref Tw, ref T2b, ref Tfb, ref FilletRadius, ref Color, ref Notes, ref GUID);
                Fw = carga * T3 * 1000;
            }

            ret = mySapModel.LoadPatterns.Add("W3_90º", eLoadPatternType.Wind, 0, true);
            ret = mySapModel.LoadPatterns.Add("W4_270º", eLoadPatternType.Wind, 0, true);

            for (int i = 0; i < pilares.Length; i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(pilares[i], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(pilares[i], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }

            if (Altura == 0)
            {
                Match altura = Regex.Match(secundaria[0], @"OH-(\d{2,3})x");
                Altura = int.Parse(altura.Groups[1].Value);
            }

            Fw = carga * Altura;

            for (int i = 0; i < secundarias.Length; i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(secundarias[i], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(secundarias[i], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
                mySapModel.FrameObj.SetLoadDistributed(refuerzos[i], "W3_90º", 1, 5, 0, 1, Fw, Fw);
                mySapModel.FrameObj.SetLoadDistributed(refuerzos[i], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw));
            }
        }

        public static void CargaPaneles(double carga)
        {
            //Seleccionamos las secundarias y los refuerzos
            string[] secundarias_norte_sup = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel);
            string[] secundarias_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, false);
            string[] secundarias_sur_sup = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel);
            string[] secundarias_sur_inf = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel, false);
            string[] secundarias = secundarias_norte_sup
                .Concat(secundarias_norte_inf)
                .Concat(secundarias_sur_sup)
                .Concat(secundarias_sur_inf)
                .ToArray();

            string[] refuerzos_norte_sup = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel);
            string[] refuerzos_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel, false);
            string[] refuerzos_sur_sup = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryReinforcedBeams(mySapModel);
            string[] refuerzos_sur_inf = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryReinforcedBeams(mySapModel, false);
            string[] refuerzos = refuerzos_norte_sup
                .Concat(refuerzos_norte_inf)
                .Concat (refuerzos_sur_sup)
                .Concat(refuerzos_sur_inf)
                .ToArray ();

            //Número de secundarias y ancho del módulo
            int n_sb = secundarias_norte_sup.Length + secundarias_sur_sup.Length;
            double ancho_modulo = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, "nps2", "nps3");

            //Obtenemos la altura si no se ha obtenido ya con el código anterior
            if (Altura == 0)
            {
                string[] secundaria = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, secundarias[0]);
                SAP.AnalysisSubclass.UnlockModel(mySapModel);
                Match altura = Regex.Match(secundaria[0], @"OH-(\d{2,3})x");
                Altura = int.Parse(altura.Groups[1].Value);
            }

            //Calculamos carga y asignamos
            double Fw = (carga / n_sb) / ancho_modulo;

            int ret = mySapModel.LoadPatterns.Add("W3_90º", eLoadPatternType.Wind, 0, true);
            ret = mySapModel.LoadPatterns.Add("W4_270º", eLoadPatternType.Wind, 0, true);

            for (int i = 0; i < secundarias.Length; i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(secundarias[i], "W3_90º", 1, 5, 0, 1, Fw, Fw, "Global", true, false, eItemType.Objects);
                mySapModel.FrameObj.SetLoadDistributed(secundarias[i], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw), "Global", true, false, eItemType.Objects);
                mySapModel.FrameObj.SetLoadDistributed(refuerzos[i], "W3_90º", 1, 5, 0, 1, Fw, Fw, "Global", true, false, eItemType.Objects);
                mySapModel.FrameObj.SetLoadDistributed(refuerzos[i], "W4_270º", 1, 5, 0, 1, (-Fw), (-Fw), "Global", true, false, eItemType.Objects);
            }
        }

        public static void LimpiarCargaLateral()
        {
            //Seleccionamos los pilares extremos
            string[] pilares_norte = SAP.ElementFinderSubclass.TrackerSubclass.NorthPiles(mySapModel);
            string[] pilares_sur = SAP.ElementFinderSubclass.TrackerSubclass.SouthPiles(mySapModel);
            string[] pilares = { pilares_norte.Last(), pilares_sur.Last() };

            //Seleccionamos las secundarias
            string[] secundarias_norte_sup = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel);
            string[] secundarias_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, false);
            string[] secundarias_sur_sup = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel);
            string[] secundarias_sur_inf = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel, false);
            string[] secundarias = secundarias_norte_sup
                .Concat(secundarias_norte_inf)
                .Concat(secundarias_sur_sup)
                .Concat(secundarias_sur_inf)
                .ToArray();

            //Obtenemos los refuerzos
            string[] refuerzos_norte_sup = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel);
            string[] refuerzos_norte_inf = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryReinforcedBeams(mySapModel, false);
            string[] refuerzos_sur_sup = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryReinforcedBeams(mySapModel);
            string[] refuerzos_sur_inf = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryReinforcedBeams(mySapModel, false);
            string[] refuerzos = refuerzos_norte_sup
                .Concat(refuerzos_norte_inf)
                .Concat(refuerzos_sur_sup)
                .Concat(refuerzos_sur_inf)
                .ToArray();

            //Borramos la carga en todos los elementos
            for (int i = 0; i < secundarias.Length; i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(secundarias[i], "W3_90º", 1, 5, 0, 1, 0, 0);
                mySapModel.FrameObj.SetLoadDistributed(secundarias[i], "W4_270º", 1, 5, 0, 1, 0, 0);
                mySapModel.FrameObj.SetLoadDistributed(refuerzos[i], "W3_90º", 1, 5, 0, 1, 0, 0);
                mySapModel.FrameObj.SetLoadDistributed(refuerzos[i], "W4_270º", 1, 5, 0, 1, 0, 0);
            }

            for (int i = 0; i < pilares.Length; i++)
            {
                mySapModel.FrameObj.SetLoadDistributed(pilares[i], "W3_90º", 1, 5, 0, 1, 0, 0);
                mySapModel.FrameObj.SetLoadDistributed(pilares[i], "W4_270º", 1, 5, 0, 1, 0, 0);
            }
        }
    }
}
