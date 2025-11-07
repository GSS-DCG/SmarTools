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
    public class Desplazamientos
    {
        public string Deformacion { get; set; }
        public string Flecha { get; set; }
        public string MaxAdm { get; set; }
        public string Check { get; set; }
    }

    class ComprobacionFlechasRack
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        public ObservableCollection<Desplazamientos> ListaDesplazamientos { get; set; }

        public static void ComprobarFlechas(ComprobacionFlechasRackAPP vista)
        {
            var loadingWindow = new Status();

            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();
                mySapModel.SetPresentUnits(eUnits.N_mm_C);
                SAP.AnalysisSubclass.RunModel(mySapModel);
                SAP.AnalysisSubclass.SelectHypotesis(mySapModel, "SLS", false);

                //Limpiar tabla 
                vista.TablaDesplazamientos.ItemsSource = null;

                //Crear lista compartida
                List<Desplazamientos> resultadosCompletos = new List<Desplazamientos>();

                DesplomePilares(vista, loadingWindow,resultadosCompletos);
                FlechaVigas(vista, loadingWindow, resultadosCompletos);
                FlechaCorreas(vista,loadingWindow, resultadosCompletos);

                //Asignar todos los resultados a la tabla
                vista.TablaDesplazamientos.ItemsSource = resultadosCompletos;

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

        public static void DesplomePilares(ComprobacionFlechasRackAPP vista, Status loadingwindow, List<Desplazamientos> resultados)
        {
            if (vista.Monoposte.IsChecked == true)
            {
                //Nº y nombre de pilares y nudos superiores
                string[] pilares = SAP.ElementFinderSubclass.FixedSubclass.ListaPilares(mySapModel);
                string[] nudos = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilares, 2);
                int npilares = pilares.Length;

                //Longitud de pilares
                double z = SAP.AnalysisSubclass.LongitudSegmento(mySapModel, pilares[0]);

                //Desplazamientos de las cabezas de los pilares
                double desplazamiento = 0;
                foreach (var nudo in nudos)
                {
                    double desp = SAP.DesignSubclass.JointDisplacement(mySapModel, nudo);
                    if (desp > desplazamiento)
                        desplazamiento = desp;
                }

                //Flecha admisible y relación R=H/d
                double dadm = z / 100;
                double R = z / desplazamiento;
                string check = "No Cumple";
                if (dadm > desplazamiento)
                    check = "Cumple";
                //Añadir resultado a la lista
                resultados.Add(new Desplazamientos { Deformacion = "Desplome de pilares", Flecha = desplazamiento.ToString("F3") + " (H/" + R.ToString("F0") + ")", MaxAdm = dadm.ToString("F3"), Check = check });

            }
            else if (vista.Biposte.IsChecked == true)
            {
                //Nº y nombre de pilares y nudos superiores
                string[] pilaresDelanteros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
                string[] pilaresTraseros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);
                string[] nudosDelanteros = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilaresDelanteros, 2);
                string[] nudosTraseros = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilaresTraseros, 2);
                int npilares = pilaresDelanteros.Length;

                //Variables para almacenar datos del desplome de pilares
                double ZDel = SAP.AnalysisSubclass.LongitudSegmento(mySapModel, pilaresDelanteros[0]);
                double ZTras = SAP.AnalysisSubclass.LongitudSegmento(mySapModel, pilaresTraseros[0]);

                //Desplazamientos de las cabezas de los pilares
                double desplazamientoDelantero = 0;
                double desplazamientoTrasero = 0;
                foreach (var nudo in nudosDelanteros)
                {
                    double desp = SAP.DesignSubclass.JointDisplacement(mySapModel, nudo);
                    if (desp > desplazamientoDelantero)
                        desplazamientoDelantero = desp;
                }
                foreach (var nudo in nudosTraseros)
                {
                    double desp = SAP.DesignSubclass.JointDisplacement(mySapModel, nudo);
                    if (desp > desplazamientoTrasero)
                        desplazamientoTrasero = desp;
                }

                //Flecha admisible y relación R=H/d
                double dadmDel = ZDel / 100;
                double dadmTras = ZTras / 100;
                double RDel = ZDel / desplazamientoDelantero;
                double RTras = ZTras / desplazamientoTrasero;
                string checkDel = "No Cumple";
                string checkTras = "No Cumple";
                if (dadmDel > desplazamientoDelantero)
                    checkDel = "Cumple";
                if (dadmTras > desplazamientoTrasero)
                    checkTras = "Cumple";

                //Añadir resultados a la lista
                resultados.Add(new Desplazamientos { Deformacion = "Desplome de pilares delanteros", Flecha = desplazamientoDelantero.ToString("F3") + " (H/" + RDel.ToString("F0") + ")", MaxAdm = dadmDel.ToString("F3"), Check = checkDel });
                resultados.Add(new Desplazamientos { Deformacion = "Desplome de pilares traseros", Flecha = desplazamientoTrasero.ToString("F3") + " (H/" + RTras.ToString("F0") + ")", MaxAdm = dadmTras.ToString("F3"), Check = checkTras });

            }
            else
            {
                loadingwindow.Close();
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Selecciona una tipología de mesa (Monoposte/Biposte)", TipoIncidencia.Advertencia);
                ventana.ShowDialog();
            }
        }

        public static void FlechaVigas(ComprobacionFlechasRackAPP vista, Status loadingwindow, List<Desplazamientos> resultados)
        {
            bool[] apoyo = new bool[] { true, true, true, false, false, false };
            bool[] libre = new bool[6];

            if (vista.Monoposte.IsChecked == true)
            {
                //Nº y nombre de pilares y nudos superiores
                string[] pilares = SAP.ElementFinderSubclass.FixedSubclass.ListaPilares(mySapModel);
                string[] nudos = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilares, 2);
                int npilares = pilares.Length;

                //Desbloqueamos el modelo y ponemos apoyos en las cabezas de los pilares
                AsignarRestraint(apoyo, nudos);

                //Diagonales y vigas (elementos y nudos)
                (string[] DiagInf, string[] DiagSup) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                string[] vigas = SAP.ElementFinderSubclass.FixedSubclass.ListaVigas(mySapModel);
                string[] nudosVigaInf = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, vigas, 1);
                string[] nudosVigaSup = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, vigas, 2);
                double[] desplazamiento_vigaInf = new double[vigas.Length];
                double[] desplazamiento_vigaSup = new double[vigas.Length];
                double[] Linf = new double[vigas.Length];
                double[] Lsup = new double[vigas.Length];

                if (vista.UnaDiagonal.IsChecked==true)
                {
                    //Nudos de diagonales. Ponemos apoyo también
                    string[] nudosDiag = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, DiagInf, 2);
                    AsignarRestraint(apoyo, nudosDiag);
                    SAP.AnalysisSubclass.RunModel(mySapModel);

                    //Desplazamiento de nudos
                    
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        desplazamiento_vigaInf[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaInf[i]);
                        desplazamiento_vigaSup[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaSup[i]);
                    }

                    //Longitud de voladizos
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        Linf[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaInf[i], nudosDiag[i]);
                        Lsup[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaSup[i], nudos[i]);
                    }

                    //Reestablecemos el modelo sin apoyos
                    AsignarRestraint(libre, nudos);
                    AsignarRestraint(libre, nudosDiag);
                }
                else if(vista.DosDiagonal.IsChecked==true)
                {
                    //Nudos de diagonales. Ponemos apoyo también
                    string[] nudosDiagInf = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, DiagInf, 2);
                    string[] nudosDiagSup = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, DiagSup, 2);
                    AsignarRestraint(apoyo, nudosDiagInf);
                    AsignarRestraint(apoyo, nudosDiagSup);
                    SAP.AnalysisSubclass.RunModel(mySapModel);

                    //Desplazamiento de nudos
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        desplazamiento_vigaInf[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaInf[i]);
                        desplazamiento_vigaSup[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaSup[i]);
                    }

                    //Longitud de voladizos
                    Linf = new double[vigas.Length];
                    Lsup = new double[vigas.Length];

                    for (int i = 0; i < vigas.Length; i++)
                    {
                        Linf[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaInf[i], nudosDiagInf[i]);
                        Lsup[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaSup[i], nudosDiagSup[i]);
                    }

                    //Reestablecemos el modelo sin apoyos
                    AsignarRestraint(libre, nudos);
                    AsignarRestraint(libre, nudosDiagInf);
                    AsignarRestraint(libre, nudosDiagSup);
                    
                }
                else if (vista.SinDiagonal.IsChecked==true)
                {
                    loadingwindow.Close();
                    var ventana = new Incidencias();
                    ventana.ConfigurarIncidencia("La configuración Mesa Monoposte sin diagonal no existe", TipoIncidencia.Advertencia);
                    ventana.ShowDialog();
                }

                //Flecha admisible y relación R=L/d
                double dadmInf = 2 * Linf.Max() / 300;
                double dadmSup = 2 * Lsup.Max() / 300;

                double RInf = 2* Linf.Max() / desplazamiento_vigaInf.Max();
                double RSup = 2* Lsup.Max() / desplazamiento_vigaSup.Max();

                string checkInf = "No Cumple";
                string checkSup = "No Cumple";

                if (dadmInf > desplazamiento_vigaInf.Max())
                    checkInf = "Cumple";

                if (dadmSup > desplazamiento_vigaSup.Max())
                    checkSup = "Cumple";

                resultados.Add(new Desplazamientos { Deformacion = "Voladizo inferior de viga", Flecha = desplazamiento_vigaInf.Max().ToString("F3") + " (2L/" + RInf.ToString("F0") + ")", MaxAdm = dadmInf.ToString("F3"), Check = checkInf });
                resultados.Add(new Desplazamientos { Deformacion = "Voladizo superior de viga", Flecha = desplazamiento_vigaSup.Max().ToString("F3") + " (2L/" + RSup.ToString("F0") + ")", MaxAdm = dadmSup.ToString("F3"), Check = checkSup });

            }
            else if (vista.Biposte.IsChecked == true)
            {
                //Nº y nombre de pilares y nudos superiores
                string[] pilaresDelanteros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
                string[] pilaresTraseros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);
                string[] nudosDelanteros = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilaresDelanteros, 2);
                string[] nudosTraseros = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilaresTraseros, 2);
                int npilares = pilaresDelanteros.Length;

                //Desbloqueamos el modelo y ponemos apoyos en las cabezas de los pilares
                AsignarRestraint(apoyo, nudosDelanteros);
                AsignarRestraint(apoyo, nudosTraseros);

                //Diagonales y vigas (elementos y nudos)
                string[] vigas = SAP.ElementFinderSubclass.FixedSubclass.ListaVigas(mySapModel);
                string[] nudosVigaInf = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, vigas, 1);
                string[] nudosVigaSup = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, vigas, 2);
                string[] nudosCentroVano = new string[vigas.Length];
                double[] desplazamiento_vigaInf = new double[vigas.Length];
                double[] desplazamiento_vigaSup = new double[vigas.Length];
                double[] desplazamiento_centroVano = new double[vigas.Length];
                double[] Linf = new double[vigas.Length];
                double[] Lsup = new double[vigas.Length];
                double[] Lvano = new double[vigas.Length];
                
                if (vista.SinDiagonal.IsChecked == true)
                {
                    //Nudos centrales de las vigas (para flecha de vano)
                    for (int i=0;i<vigas.Length;i++)
                    {
                        nudosCentroVano[i] = "nCv_" + i;
                    }
                    SAP.AnalysisSubclass.RunModel(mySapModel);

                    //Desplazamiento de nudos
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        desplazamiento_vigaInf[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaInf[i]);
                        desplazamiento_vigaSup[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaSup[i]);
                        desplazamiento_centroVano[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosCentroVano[i]);
                    }

                    //Longitud de los voladizos y vano
                    for (int i = 0;i < vigas.Length;i++)
                    {
                        Linf[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaInf[i], nudosDelanteros[i]);
                        Lsup[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaSup[i], nudosTraseros[i]);
                        Lvano[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosDelanteros[i] , nudosTraseros[i]);
                    }


                    //Reestablecemos el modelo sin apoyos
                    AsignarRestraint(libre, nudosDelanteros);
                    AsignarRestraint(libre, nudosTraseros);
                }
                else if (vista.UnaDiagonal.IsChecked == true)
                {
                    //Nudos centrales de las vigas (para flecha de vano). Caen aproximadamente a la altura de la tercera correa, si el número de correas es 6
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        nudosCentroVano[i] = "nCc_2_" + (i+1);
                    }

                    (string[] DiagInf, string[] DiagSup) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                    //Nudos de diagonales Ponemos apoyo también
                    string[] nudosDiag = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, DiagInf, 2);
                    AsignarRestraint(apoyo, nudosDiag);
                    SAP.AnalysisSubclass.RunModel(mySapModel);

                    //Desplazamiento de nudos
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        desplazamiento_vigaInf[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaInf[i]);
                        desplazamiento_vigaSup[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaSup[i]);
                        desplazamiento_centroVano[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosCentroVano[i]);
                    }

                    //Longitud de voladizos
                    for (int i = 0;i<vigas.Length;i++)
                    {
                        Linf[i]=SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel,nudosVigaInf[i],nudosDelanteros[i]);
                        Lsup[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaSup[i], nudosTraseros[i]);
                        Lvano[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosDelanteros[i], nudosTraseros[i]);
                    }

                    //Reestablecemos el modelo sin apoyos
                    AsignarRestraint(libre, nudosDelanteros);
                    AsignarRestraint(libre, nudosTraseros);
                    AsignarRestraint(libre, nudosDiag);
                }
                else if (vista.DosDiagonal.IsChecked == true)
                {
                    SAP.AnalysisSubclass.RunModel(mySapModel);

                    //Desplazamiento de nudos
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        desplazamiento_vigaInf[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaInf[i]);
                        desplazamiento_vigaSup[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudosVigaSup[i]);
                    }

                    //Longitud de los voladizos y vano
                    for (int i = 0; i < vigas.Length; i++)
                    {
                        Linf[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaInf[i], nudosDelanteros[i]);
                        Lsup[i] = SAP.AnalysisSubclass.LongitudEntrePuntos(mySapModel, nudosVigaSup[i], nudosTraseros[i]);
                    }

                    //Reestablecemos el modelo sin apoyos
                    AsignarRestraint(libre, nudosDelanteros);
                    AsignarRestraint(libre, nudosTraseros);
                }
               
                //Flecha admisible y relación R=L/d
                double dadmInf = 2 * Linf.Max() / 300;
                double dadmSup = 2 * Lsup.Max() / 300;
                double dadmCdv = Lvano.Max() / 200;

                double RInf = 2* Linf.Max() / desplazamiento_vigaInf.Max();
                double RSup = 2* Lsup.Max() / desplazamiento_vigaSup.Max();
                double RCdv = Lvano.Max() / desplazamiento_centroVano.Max();

                string checkInf = "No Cumple";
                string checkSup = "No Cumple";
                string checkCdv = "No Cumple";

                if (dadmInf > desplazamiento_vigaInf.Max())
                    checkInf = "Cumple";

                if (dadmSup > desplazamiento_vigaSup.Max())
                    checkSup = "Cumple";

                if (vista.Biposte.IsChecked == true && vista.DosDiagonal.IsChecked == false && dadmCdv > desplazamiento_centroVano.Max())
                    checkCdv = "Cumple";

                resultados.Add(new Desplazamientos { Deformacion = "Voladizo inferior de viga", Flecha = desplazamiento_vigaInf.Max().ToString("F3") + " (2L/" + RInf.ToString("F0") + ")", MaxAdm = dadmInf.ToString("F3"), Check = checkInf });
                resultados.Add(new Desplazamientos { Deformacion = "Voladizo superior de viga", Flecha = desplazamiento_vigaSup.Max().ToString("F3") + " (2L/" + RSup.ToString("F0") + ")", MaxAdm = dadmSup.ToString("F3"), Check = checkSup });
                if (vista.Biposte.IsChecked == true && vista.DosDiagonal.IsChecked == false)
                    resultados.Add(new Desplazamientos { Deformacion = "Centro de vano en viga", Flecha = desplazamiento_centroVano.Max().ToString("F3") + " (L/" + RCdv.ToString("F0") + ")", MaxAdm = dadmCdv.ToString("F3"), Check = checkCdv });
            }
        }

        public static void FlechaCorreas(ComprobacionFlechasRackAPP vista, Status loadingwindow, List<Desplazamientos> resultados)
        {
            bool[] apoyo = new bool[] { true, true, true, false, false, false };
            bool[] libre = new bool[6];

            if(!int.TryParse(vista.NumCorreas.Text, out int Ncorreas))
            {
                loadingwindow.Close();
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("El número de correas debe ser un valor numérico válido", TipoIncidencia.Error);
                ventana.ShowDialog();
            }
            else
            {
                var correas = SAP.ElementFinderSubclass.FixedSubclass.ObtenerCorreas(mySapModel);
                if (correas.Length == 0)
                {
                    loadingwindow.Close();
                    var ventana = new Incidencias();
                    ventana.ConfigurarIncidencia("No se encontraron objetos en el grupo 03 Correas", TipoIncidencia.Error);
                    ventana.ShowDialog();
                }
                else
                {
                    var Correas = new Dictionary<string, string[]>();

                    for (int i = 1;i<=Ncorreas;i++)
                    {
                        Correas[$"Correa{i}"] = correas.Where(c=>c.Contains($"Purlin_{i}_")).ToArray();
                    }

                    double Lvoladizo = 0;
                    double Lvano = 0;
                    List <string> apoyos = new List<string>();

                    foreach (var correa in Correas)
                    {
                        string extremo1 = correa.Value.First();
                        string extremo2 = correa.Value.Last();
                        string[] vanos = correa.Value.Skip(1).Take(correa.Value.Length-2).ToArray();
                        Lvoladizo = Math.Max( SAP.AnalysisSubclass.LongitudSegmento(mySapModel, extremo1), SAP.AnalysisSubclass.LongitudSegmento(mySapModel, extremo2));
                        apoyos.Add(SAP.ElementFinderSubclass.GetOneFrameJoints(mySapModel, extremo1, 2));
                        foreach (var barra in vanos)
                        {
                            double L = SAP.AnalysisSubclass.LongitudSegmento(mySapModel,barra);
                            if (L > Lvano)
                                Lvano = L;
                            apoyos.Add(SAP.ElementFinderSubclass.GetOneFrameJoints(mySapModel, barra, 2));
                        }
                    }
                    string[] Apoyos = apoyos.ToArray();
                    AsignarRestraint(apoyo, Apoyos);
                    SAP.AnalysisSubclass.RunModel(mySapModel);
                    List<double> desplazamientos_vol = new List<double>();
                    foreach (var correa in Correas)
                    {
                        string extremo1 = correa.Value.First();
                        string extremo2 = correa.Value.Last();
                        //Voladizos
                        string nudo1 = SAP.ElementFinderSubclass.GetOneFrameJoints(mySapModel, extremo1, 1);
                        string nudo2 = SAP.ElementFinderSubclass.GetOneFrameJoints(mySapModel, extremo2, 2);
                        desplazamientos_vol.Add( SAP.DesignSubclass.JointDisplacement(mySapModel, nudo1));
                        desplazamientos_vol.Add(SAP.DesignSubclass.JointDisplacement(mySapModel, nudo2)); 
                    }
                    double[] desplazamientos_voladizo = desplazamientos_vol.ToArray();
                    //Vanos
                    string[] estabilizadores = SAP.ElementFinderSubclass.FixedSubclass.ObtenerEstabilizadores(mySapModel);
                    string[] cdv = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel,estabilizadores,1).Concat(SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, estabilizadores, 2)).ToArray();
                    double[] desplazamientos_cdv = new double[cdv.Length];
                    for (int i = 0; i < cdv.Length; i++)
                        desplazamientos_cdv[i] = SAP.DesignSubclass.JointDisplacement(mySapModel,cdv[i]);

                    AsignarRestraint(libre, Apoyos);

                    //Flecha admisible y relación R=L/D
                    double dadmvoladizo = 2 * Lvoladizo / 300;
                    double dadmvano = Lvano / 200;

                    double Rvoladizo = 2 * Lvoladizo/desplazamientos_voladizo.Max();
                    double Rvano = Lvano/desplazamientos_cdv.Max();

                    string checkvoladizo = "No Cumple";
                    string checkvano = "No Cumple";

                    if (dadmvoladizo > desplazamientos_voladizo.Max())
                        checkvoladizo = "Cumple";
                    if (dadmvano > desplazamientos_cdv.Max())
                        checkvano = "Cumple";

                    resultados.Add(new Desplazamientos {Deformacion ="Voladizo de correas", Flecha = desplazamientos_voladizo.Max().ToString("F3")+" (2L/" + Rvoladizo.ToString("F0") + ")", MaxAdm = dadmvoladizo.ToString("F3"), Check = checkvoladizo });
                    resultados.Add(new Desplazamientos { Deformacion = "Vano de correas", Flecha = desplazamientos_cdv.Max().ToString("F3") + " (2L/" + Rvano.ToString("F0") + ")", MaxAdm = dadmvano.ToString("F3"), Check = checkvano });
                }
            }
        }

        private static void AsignarRestraint(bool[]restraint,string[] nudos)
        {
            SAP.AnalysisSubclass.UnlockModel(mySapModel);
            foreach (string nudo in nudos)
            {
                mySapModel.PointObj.SetRestraint(nudo, ref restraint);
            }
        }
    }
}
