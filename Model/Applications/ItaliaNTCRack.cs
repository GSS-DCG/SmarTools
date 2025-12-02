using DocumentFormat.OpenXml.Spreadsheet;
using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.Model.Repository;
using SmarTools.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SmarTools.Model.Applications.ItaliaNTCRack;
using System.Windows;

namespace SmarTools.Model.Applications
{
    class ItaliaNTCRack
    {
        public static string ruta_base_steel = $@"Z:\300SmarTools\05 Documentos\SteelSlendernessCheck.rtf";
        public static string ruta_base_cold = $@"Z:\300SmarTools\05 Documentos\ColdFormedSlendernessCheck.rtf";
        public static string ruta_guardado_steel = $@"{Directory.GetCurrentDirectory()}\Documentos\SteelSlendernessCheckMod.rtf";
        public static string ruta_guardado_cold = $@"{Directory.GetCurrentDirectory()}\Documentos\ColdFormedSlendernessCheckMod.rtf";

        public static cHelper my_help = MainView.Globales._myHelper;
        public static cOAPI my_Sap_object = MainView.Globales._mySapObject;
        public static cSapModel my_Sap_Model = MainView.Globales._mySapModel;
        public static int Ancho = 0;
        public static int Altura = 0;

        public static void ComprobarNTC(ItaliaNTCRackAPP vista)
        {
            var loadingWindow = new Status();
            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();
                Herramientas.AbrirArchivoSAP2000();

                int ret = 0;

                string[][] DataSteel = new string[2000][];
                int n_steel = 0;
                string[][] DataCold = new string[2000][];
                int n_cold = 0;

                bool cumplen_todos_los_perfiles = true;
                my_Sap_Model.SetPresentUnits(eUnits.kN_m_C);
                SAP.AnalysisSubclass.UnlockModel(my_Sap_Model);
                SAP2000.CambiarNormativaSteel(my_help, my_Sap_object, my_Sap_Model);
                SAP2000.CambiarTablasPandeoColdFormed(my_Sap_Model, "Column");
                SAP2000.CambiarTablasPandeoColdFormed(my_Sap_Model, "Beam");
                SAP2000.CambiarTablasPandeoColdFormed(my_Sap_Model, "Purlin");

                ret = my_Sap_Model.SelectObj.Group("04 Diagonales");

                int NumberItemsDiag = 0;
                int[] ObjectType = { };
                string[] ObjectName = { };
                ret = my_Sap_Model.SelectObj.GetSelected(ref NumberItemsDiag, ref ObjectType, ref ObjectName);

                if (NumberItemsDiag != 0)
                {
                    SAP2000.CambiarTablasPandeoColdFormed(my_Sap_Model, "Diag");
                }
                SAP2000.CambiarCoeficientePandeoFija(my_Sap_Model,vista.MONOPOSTE.IsChecked.Value);
                SAP2000.CalcularMordelo(my_help, my_Sap_object, my_Sap_Model);

                my_Sap_Model.DesignSteel.StartDesign();

                int NumberItemsSteel = 0;
                int numeromatrix = 300;
                string[] FrameNameSteel = new string[numeromatrix];
                double[] RatioSteel = new double[numeromatrix];
                int[] RatioTypeSteel = new int[numeromatrix];
                double[] LocationSteel = new double[numeromatrix];
                string[] ComboNameSteel = new string[numeromatrix];
                string[] ErrorSummarySteel = new string[numeromatrix];
                string[] WarningSummarySteel = new string[numeromatrix];

                my_Sap_Model.SelectObj.All();
                my_Sap_Model.DesignSteel.GetSummaryResults("All", ref NumberItemsSteel, ref FrameNameSteel, ref RatioSteel, ref RatioTypeSteel, ref LocationSteel, ref ComboNameSteel, ref ErrorSummarySteel, ref WarningSummarySteel, eItemType.SelectedObjects);


                my_Sap_Model.DesignColdFormed.StartDesign();

                int NumberItemsCold = 0;
                string[] FrameName = new string[numeromatrix];
                double[] Ratio = new double[numeromatrix];
                int[] RatioType = new int[numeromatrix];
                double[] Location = new double[numeromatrix];
                string[] ComboName = new string[numeromatrix];
                string[] ErrorSummary = new string[numeromatrix];
                string[] WarningSummary = new string[numeromatrix];

                my_Sap_Model.SelectObj.All();
                my_Sap_Model.DesignColdFormed.GetSummaryResults("All", ref NumberItemsCold, ref FrameName, ref Ratio, ref RatioType, ref Location, ref ComboName, ref ErrorSummary, ref WarningSummary, eItemType.SelectedObjects);

                List<Pilar> itemsPilar = new List<Pilar>();
                List<Viga> itemsViga = new List<Viga>();
                List<Correa> itemsCorrea = new List<Correa>();
                List<Diagonal> itemsDiagonal = new List<Diagonal>();

                for (int n = 0; n < NumberItemsSteel; n++)
                {
                    string subcadena = "";
                    try
                    {
                        int indiceCaracter = FrameNameSteel[n].IndexOf("_");
                        subcadena = FrameNameSteel[n].Substring(0, indiceCaracter);
                    }
                    catch
                    {
                        subcadena = FrameNameSteel[n];
                    }

                    string estado = "No Ok";

                    if (FrameNameSteel[n].Contains("Column"))
                    {
                        string PropName = "";
                        string sAuto = "";

                        my_Sap_Model.FrameObj.GetSection(FrameNameSteel[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);
                        string[] limitacionEsbeltez = COMPROBACIONES.Limitacion_Esbletez(
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2,
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2,
                        R22,
                        R33
                        );

                        if (RatioSteel[n] < 1 & limitacionEsbeltez[3] == "true")
                        {
                            estado = "OK";
                        }

                        itemsPilar.Add(new Pilar()
                        {
                            NOMBRE_PILAR = FrameNameSteel[n],
                            SECCION_PILAR = PropName,
                            MATERIAL_PILAR = "Steel",
                            RATIO_PILAR = Math.Round(RatioSteel[n], 2).ToString(),
                            ESBELTEZ_PILAR = limitacionEsbeltez[2],
                            ESTADO_PILAR = estado
                        });

                        DataSteel[n_steel] =
                            [FrameNameSteel[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        limitacionEsbeltez[2]
                            ];

                        n_steel++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }
                    }
                    else if (FrameNameSteel[n].Contains("Purlin"))
                    {
                        string PropName = "";
                        string sAuto = "";
                        my_Sap_Model.FrameObj.GetSection(FrameNameSteel[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);
                        string[] limitacionEsbeltez = COMPROBACIONES.Limitacion_Esbletez(
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2,
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2,
                        R22,
                        R33
                        );

                        if (RatioSteel[n] < 1 & limitacionEsbeltez[3] == "true")
                        {
                            estado = "OK";
                        }

                        itemsCorrea.Add(new Correa()
                        {
                            NOMBRE_CORREA = FrameNameSteel[n],
                            SECCION_CORREA = PropName,
                            MATERIAL_CORREA = "Steel",
                            RATIO_CORREA = Math.Round(RatioSteel[n], 2).ToString(),
                            ESBELTEZ_CORREA = limitacionEsbeltez[2],
                            ESTADO_CORREA = estado
                        });

                        DataSteel[n_steel] =
                            [FrameNameSteel[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n])) * 2).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        limitacionEsbeltez[2]
                            ];

                        n_steel++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }
                    }
                    else if (FrameNameSteel[n].Contains("Diag"))
                    {
                        string PropName = "";
                        string sAuto = "";
                        my_Sap_Model.FrameObj.GetSection(FrameNameSteel[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);
                        string[] limitacionEsbeltez = COMPROBACIONES.Limitacion_Esbletez(
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n])) * 2,
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n])) * 2,
                        R22,
                        R33
                        );

                        if (RatioSteel[n] < 1 & limitacionEsbeltez[3] == "true")
                        {
                            estado = "OK";
                        }

                        itemsDiagonal.Add(new Diagonal()
                        {
                            NOMBRE_DIAGONAL = FrameNameSteel[n],
                            SECCION_DIAGONAL = PropName,
                            MATERIAL_DIAGONAL = "Steel",
                            RATIO_DIAGONAL = Math.Round(RatioSteel[n], 2).ToString(),
                            ESBELTEZ_DIAGONAL = limitacionEsbeltez[2],
                            ESTADO_DIAGONAL = estado
                        });

                        DataSteel[n_steel] =
                            [FrameNameSteel[n],
                        PropName,
                        ( (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n])) * 2).ToString(),
                        ( (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n])) * 2).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        limitacionEsbeltez[2]
                            ];

                        n_steel++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }
                    }
                    else if (FrameNameSteel[n].Contains("Beam"))
                    {
                        string PropName = "";
                        string sAuto = "";
                        my_Sap_Model.FrameObj.GetSection(FrameNameSteel[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);


                        if (RatioSteel[n] < 1)
                        {
                            estado = "OK";
                        }

                        itemsViga.Add(new Viga()
                        {
                            NOMBRE_VIGA = FrameNameSteel[n],
                            SECCION_VIGA = PropName,
                            MATERIAL_VIGA = "Steel",
                            RATIO_VIGA = Math.Round(RatioSteel[n], 2).ToString(),
                            ESBELTEZ_VIGA = "N.A",
                            ESTADO_VIGA = estado
                        });

                        DataSteel[n_steel] =
                            [FrameNameSteel[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]))).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]))).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        "N.A"
                            ];

                        n_steel++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }

                    }

                }

                for (int n = 0; n < NumberItemsCold; n++)
                {
                    string subcadena = "";
                    try
                    {
                        int indiceCaracter = FrameName[n].IndexOf("_");
                        subcadena = FrameName[n].Substring(0, indiceCaracter);
                    }
                    catch
                    {
                        subcadena = FrameName[n];
                    }

                    string estado = "No Ok";

                    if (Ratio[n] < 1)
                    {
                        estado = "OK";
                    }

                    if (FrameName[n].Contains("Column"))
                    {
                        string PropName = "";
                        string sAuto = "";
                        my_Sap_Model.FrameObj.GetSection(FrameName[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);
                        string[] limitacionEsbeltez = COMPROBACIONES.Limitacion_Esbletez(
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2,
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2,
                        R22,
                        R33
                        );

                        if (Ratio[n] < 1 & limitacionEsbeltez[3] == "true")
                        {
                            estado = "OK";
                        }

                        itemsPilar.Add(new Pilar()
                        {
                            NOMBRE_PILAR = FrameName[n],
                            SECCION_PILAR = PropName,
                            MATERIAL_PILAR = "Cold Formed",
                            RATIO_PILAR = Math.Round(Ratio[n], 2).ToString(),
                            ESBELTEZ_PILAR = limitacionEsbeltez[2],
                            ESTADO_PILAR = estado
                        });

                        DataCold[n_cold] =
                            [FrameName[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        limitacionEsbeltez[2]
                            ];

                        n_cold++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }
                    }
                    else if (FrameName[n].Contains("Purlin"))
                    {
                        string PropName = "";
                        string sAuto = "";
                        my_Sap_Model.FrameObj.GetSection(FrameName[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);
                        string[] limitacionEsbeltez = COMPROBACIONES.Limitacion_Esbletez(
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2,
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2,
                        R22,
                        R33
                        );

                        if (Ratio[n] < 1 & limitacionEsbeltez[3] == "true")
                        {
                            estado = "OK";
                        }

                        itemsCorrea.Add(new Correa()
                        {
                            NOMBRE_CORREA = FrameName[n],
                            SECCION_CORREA = PropName,
                            MATERIAL_CORREA = "Cold Formed",
                            RATIO_CORREA = Math.Round(Ratio[n], 2).ToString(),
                            ESBELTEZ_CORREA = limitacionEsbeltez[2],
                            ESTADO_CORREA = estado
                        });

                        DataCold[n_cold] =
                            [FrameName[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n])) * 2).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        limitacionEsbeltez[2]
                            ];

                        n_cold++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }
                    }
                    else if (FrameName[n].Contains("Diag"))
                    {
                        string PropName = "";
                        string sAuto = "";
                        my_Sap_Model.FrameObj.GetSection(FrameName[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);
                        string[] limitacionEsbeltez = COMPROBACIONES.Limitacion_Esbletez(
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n])) * 2,
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n])) * 2,
                        R22,
                        R33
                        );

                        if (Ratio[n] < 1 & limitacionEsbeltez[3] == "true")
                        {
                            estado = "OK";
                        }

                        itemsDiagonal.Add(new Diagonal()
                        {
                            NOMBRE_DIAGONAL = FrameName[n],
                            SECCION_DIAGONAL = PropName,
                            MATERIAL_DIAGONAL = "Cold Formed",
                            RATIO_DIAGONAL = Math.Round(Ratio[n], 2).ToString(),
                            ESBELTEZ_DIAGONAL = limitacionEsbeltez[2],
                            ESTADO_DIAGONAL = estado
                        });

                        DataCold[n_cold] =
                            [FrameName[n],
                        PropName,
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n]) * 2).ToString(),
                        (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) + HERRAMIENTAS_AUXILIARES.LongitudRefuerzo(my_Sap_Model, FrameName[n]) * 2).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        limitacionEsbeltez[2]
                            ];

                        n_cold++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }
                    }
                    else if (FrameName[n].Contains("Beam") | FrameName[n].Contains("Beam"))
                    {
                        string PropName = "";
                        string sAuto = "";
                        my_Sap_Model.FrameObj.GetSection(FrameName[n], ref PropName, ref sAuto);

                        double Area = 0;
                        double As2 = 0;
                        double As3 = 0;
                        double Torsion = 0;
                        double I22 = 0;
                        double I33 = 0;
                        double S22 = 0;
                        double S33 = 0;
                        double Z22 = 0;
                        double Z33 = 0;
                        double R22 = 0;
                        double R33 = 0;
                        my_Sap_Model.PropFrame.GetSectProps(PropName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33);


                        if (Ratio[n] < 1)
                        {
                            estado = "OK";
                        }

                        itemsViga.Add(new Viga()
                        {
                            NOMBRE_VIGA = FrameName[n],
                            SECCION_VIGA = PropName,
                            MATERIAL_VIGA = "Cold Formed",
                            RATIO_VIGA = Math.Round(Ratio[n], 2).ToString(),
                            ESBELTEZ_VIGA = "N.A",
                            ESTADO_VIGA = estado
                        });

                        DataCold[n_cold] =
                            [FrameName[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]))).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]))).ToString(),
                        Math.Round(R22,4).ToString(),
                        Math.Round(R33,4).ToString(),
                        "N.A"
                            ];
                        n_cold++;

                        if (estado != "OK")
                        {
                            cumplen_todos_los_perfiles = false;
                        }
                    }
                }

                my_Sap_Model.SelectObj.ClearSelection();

                vista.PILARES.ItemsSource = itemsPilar;

                vista.CORREAS.ItemsSource = itemsCorrea;

                vista.VIGAS.ItemsSource = itemsViga;

                vista.DIAGONAL.ItemsSource = itemsDiagonal;

                if (cumplen_todos_los_perfiles)
                {
                    MessageBox.Show("OK:\n La solucion actual cumple resistentemente", "ITA_NTC_2018 Result", MessageBoxButton.OK, MessageBoxImage.Information);

                    //HERRAMIENTAS_AUXILIARES.ExportarTablas(ruta_base_cold, ruta_guardado_cold, DataCold);
                    //HERRAMIENTAS_AUXILIARES.ExportarTablas(ruta_base_steel, ruta_guardado_steel, DataSteel);
                }
                else
                {
                    MessageBox.Show("Fallo:\n La solucion actual no cumple resistentemente", "ITA_NTC_2018 Result", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    var ventana = new Incidencias();
                    ventana.ConfigurarIncidencia("Se ha producido un error", TipoIncidencia.Error);
                    ventana.ShowDialog();
                }
            }
        }

        public class Pilar
        {
            public string NOMBRE_PILAR { get; set; }

            public string SECCION_PILAR { get; set; }

            public string MATERIAL_PILAR { get; set; }

            public string RATIO_PILAR { get; set; }

            public string ESBELTEZ_PILAR { get; set; }

            public string ESTADO_PILAR { get; set; }
        }

        public class Viga
        {
            public string NOMBRE_VIGA { get; set; }

            public string SECCION_VIGA { get; set; }

            public string MATERIAL_VIGA { get; set; }

            public string RATIO_VIGA { get; set; }

            public string ESBELTEZ_VIGA { get; set; }

            public string ESTADO_VIGA { get; set; }
        }

        public class Diagonal
        {
            public string NOMBRE_DIAGONAL { get; set; }

            public string SECCION_DIAGONAL { get; set; }

            public string MATERIAL_DIAGONAL { get; set; }

            public string RATIO_DIAGONAL { get; set; }

            public string ESBELTEZ_DIAGONAL { get; set; }

            public string ESTADO_DIAGONAL { get; set; }
        }

        public class Correa
        {
            public string NOMBRE_CORREA { get; set; }

            public string SECCION_CORREA { get; set; }

            public string MATERIAL_CORREA { get; set; }

            public string RATIO_CORREA { get; set; }

            public string ESBELTEZ_CORREA { get; set; }

            public string ESTADO_CORREA { get; set; }
        }
    }
}
