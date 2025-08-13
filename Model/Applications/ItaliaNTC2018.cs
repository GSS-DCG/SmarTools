using Microsoft.VisualBasic;
using Microsoft.Win32;
using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.Model.Repository;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SmarTools.APPS.ItaliaNTC1VAPP;

namespace SmarTools.Model.Applications
{
    public class ItaliaNTC2018
    {
        public static string ruta_base_steel = $@"Z:\300Logos\05 Documentos\SteelSlendernessCheck.rtf";
        public static string ruta_base_cold = $@"Z:\300Logos\05 Documentos\ColdFormedSlendernessCheck.rtf";
        public static string ruta_guardado_steel = $@"{Directory.GetCurrentDirectory()}\Documentos\SteelSlendernessCheckMod.rtf";
        public static string ruta_guardado_cold = $@"{Directory.GetCurrentDirectory()}\Documentos\ColdFormedSlendernessCheckMod.rtf";

        public static cHelper my_help = MainView.Globales._myHelper;
        public static cOAPI my_Sap_object = MainView.Globales._mySapObject;
        public static cSapModel my_Sap_Model = MainView.Globales._mySapModel;
        public static int Ancho = 0;
        public static int Altura = 0;

        public static void ComprobarNTC(TextBox CABEZA_MOTOR, TextBox CABEZA_GENERAL, ListView PILAR, ListView PILAR_MOTOR, ListView VIGA_PRINCIPAL, ListView VIGA_SECUNDARIA)
        {
            int ret = 0;
            bool estrategia = false;
            double dist_pm = 0;
            double dist_pg = 0;

            string[][] DataSteel = new string[500][];
            int n_steel = 0;
            string[][] DataCold = new string[500][];
            int n_cold = 0;

            bool cumplen_todos_los_perfiles = true;

            if (CABEZA_MOTOR.Text != "")
            {
                dist_pm = double.Parse(CABEZA_MOTOR.Text) / 1000;
            }

            if (CABEZA_MOTOR.Text != "")
            {
                dist_pg = double.Parse(CABEZA_GENERAL.Text) / 1000;
            }
            Herramientas.AbrirArchivoSAP2000();
            my_Sap_Model.SetPresentUnits(eUnits.kN_m_C);
            SAP.AnalysisSubclass.UnlockModel(my_Sap_Model);
            SAP2000.CambiarNormativaSteel(my_help, my_Sap_object, my_Sap_Model);
            SAP2000.CambiarTablasPandeoColdFormed(my_Sap_Model, "Column");
            SAP2000.CambiarTablasPandeoSteel(my_Sap_Model, "Column");
            SAP2000.CambiarCoeficientePandeo(my_Sap_Model);
            SAP2000.CalcularMordelo(my_help, my_Sap_object, my_Sap_Model);

            my_Sap_Model.DesignSteel.StartDesign();

            int NumberItemsSteel = 0;
            string[] FrameNameSteel = new string[500];
            double[] RatioSteel = new double[500];
            int[] RatioTypeSteel = new int[500];
            double[] LocationSteel = new double[500];
            string[] ComboNameSteel = new string[500];
            string[] ErrorSummarySteel = new string[500];
            string[] WarningSummarySteel = new string[500];

            my_Sap_Model.SelectObj.All();
            my_Sap_Model.DesignSteel.GetSummaryResults("All", ref NumberItemsSteel, ref FrameNameSteel, ref RatioSteel, ref RatioTypeSteel, ref LocationSteel, ref ComboNameSteel, ref ErrorSummarySteel, ref WarningSummarySteel, eItemType.SelectedObjects);


            my_Sap_Model.DesignColdFormed.StartDesign();

            int NumberItemsCold = 0;
            string[] FrameName = new string[500];
            double[] Ratio = new double[500];
            int[] RatioType = new int[500];
            double[] Location = new double[500];
            string[] ComboName = new string[500];
            string[] ErrorSummary = new string[500];
            string[] WarningSummary = new string[500];

            my_Sap_Model.SelectObj.All();
            my_Sap_Model.DesignColdFormed.GetSummaryResults("All", ref NumberItemsCold, ref FrameName, ref Ratio, ref RatioType, ref Location, ref ComboName, ref ErrorSummary, ref WarningSummary, eItemType.SelectedObjects);

            List<PilarMotor> itemsMotor = new List<PilarMotor>();
            List<Pilar> items = new List<Pilar>();
            List<Viga> itemsViga = new List<Viga>();
            List<Secundaria> itemsSecundaria = new List<Secundaria>();

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

                if (FrameNameSteel[n] == "Column_0")
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
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pm) * 2,
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pm) * 2,
                    R22,
                    R33
                    );

                    if (RatioSteel[n] < 1 & limitacionEsbeltez[3] == "true")
                    {
                        estado = "OK";
                    }

                    itemsMotor.Add(new PilarMotor()
                    {
                        NOMBRE_PILAR_MOTOR = FrameNameSteel[n],
                        SECCION_PILAR_MOTOR = PropName,
                        MATERIAL_PILAR_MOTOR = "Steel",
                        RATIO_PILAR_MOTOR = Math.Round(RatioSteel[n], 2).ToString(),
                        ESBELTEZ_PILAR_MOTOR = limitacionEsbeltez[2],
                        ESTADO_PILAR_MOTOR = estado
                    });

                    DataSteel[n_steel] =
                        [FrameNameSteel[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pm) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pm) * 2).ToString(),
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
                else if (subcadena == "Column")
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
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pg) * 2,
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pg) * 2,
                    R22,
                    R33
                    );

                    if (RatioSteel[n] < 1 & limitacionEsbeltez[3] == "true")
                    {
                        estado = "OK";
                    }

                    items.Add(new Pilar()
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
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pm) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameNameSteel[n]) - dist_pm) * 2).ToString(),
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
                else if (FrameNameSteel[n].Contains("SB"))
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

                    itemsSecundaria.Add(new Secundaria()
                    {
                        NOMBRE_SECUNDARIA = FrameNameSteel[n],
                        SECCION_SECUNDARIA = PropName,
                        MATERIAL_SECUNDARIA = "Steel",
                        RATIO_SECUNDARIA = Math.Round(RatioSteel[n], 2).ToString(),
                        ESBELTEZ_SECUNDARIA = limitacionEsbeltez[2],
                        ESTADO_SECUNDARIA = estado
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
                else if (FrameNameSteel[n].Contains("B") | FrameNameSteel[n].Contains("B-"))
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

                if (FrameName[n] == "Column_0")
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
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pm) * 2,
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pm) * 2,
                    R22,
                    R33
                    );

                    if (Ratio[n] < 1 & limitacionEsbeltez[3] == "true")
                    {
                        estado = "OK";
                    }

                    itemsMotor.Add(new PilarMotor()
                    {
                        NOMBRE_PILAR_MOTOR = FrameName[n],
                        SECCION_PILAR_MOTOR = PropName,
                        MATERIAL_PILAR_MOTOR = "Cold Formed",
                        RATIO_PILAR_MOTOR = Math.Round(Ratio[n], 2).ToString(),
                        ESBELTEZ_PILAR_MOTOR = limitacionEsbeltez[2],
                        ESTADO_PILAR_MOTOR = estado
                    });

                    DataCold[n_cold] =
                        [FrameName[n],
                        PropName,
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pm) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pm) * 2).ToString(),
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
                else if (subcadena == "Column")
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
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pg) * 2,
                    (HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pg) * 2,
                    R22,
                    R33
                    );

                    if (Ratio[n] < 1 & limitacionEsbeltez[3] == "true")
                    {
                        estado = "OK";
                    }

                    items.Add(new Pilar()
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
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pm) * 2).ToString(),
                        ((HERRAMIENTAS_AUXILIARES.LongitudSegmento(my_Sap_Model, FrameName[n]) - dist_pm) * 2).ToString(),
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
                else if (FrameName[n].Contains("SB"))
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

                    itemsSecundaria.Add(new Secundaria()
                    {
                        NOMBRE_SECUNDARIA = FrameName[n],
                        SECCION_SECUNDARIA = PropName,
                        MATERIAL_SECUNDARIA = "Cold Formed",
                        RATIO_SECUNDARIA = Math.Round(Ratio[n], 2).ToString(),
                        ESBELTEZ_SECUNDARIA = limitacionEsbeltez[2],
                        ESTADO_SECUNDARIA = estado
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
                else if (FrameName[n].Contains("B") | FrameName[n].Contains("B-"))
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


            PILAR_MOTOR.ItemsSource = itemsMotor;

            PILAR.ItemsSource = items;

            VIGA_PRINCIPAL.ItemsSource = itemsViga;

            VIGA_SECUNDARIA.ItemsSource = itemsSecundaria;

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

        public class PilarMotor
        {
            public string NOMBRE_PILAR_MOTOR { get; set; }

            public string SECCION_PILAR_MOTOR { get; set; }

            public string MATERIAL_PILAR_MOTOR { get; set; }

            public string RATIO_PILAR_MOTOR { get; set; }

            public string ESBELTEZ_PILAR_MOTOR { get; set; }

            public string ESTADO_PILAR_MOTOR { get; set; }
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

        public class Secundaria
        {
            public string NOMBRE_SECUNDARIA { get; set; }

            public string SECCION_SECUNDARIA { get; set; }

            public string MATERIAL_SECUNDARIA { get; set; }

            public string RATIO_SECUNDARIA { get; set; }

            public string ESBELTEZ_SECUNDARIA { get; set; }

            public string ESTADO_SECUNDARIA { get; set; }
        }
    }

    public class SAP2000
    {
        public static int AsignarCargas
            (
            cHelper myHelper,
            cOAPI mySapObject,
            cSapModel mySapModel,
            bool estrategia,
            string F_Nieve,
            string M_Nieve,
            string F_SUP_PRESION,
            string F_INF_PRESION,
            string F_SUP_SUCCION,
            string F_INF_SUCCION,
            string ProgramPath
            )
        {
            int ret = 0;

            if (mySapModel != null)
            {
                if (mySapModel.GetModelIsLocked() == true)
                {
                    ret = mySapModel.SetModelIsLocked(false);
                }

                ret = mySapModel.SetPresentUnits(eUnits.kN_m_C);


                Console.WriteLine("Estrategia Funcionamiento");

                if (F_Nieve != "")
                {
                    /* 
                    * Añadimos la carga generada por la Nieve 
                    */
                    ret = mySapModel.SelectObj.ClearSelection();
                    ret = mySapModel.SelectObj.CoordinateRange(-10, 10, -100, 100, 0, 10, false, "Global", true, false, false, true, false, false);
                    double nieve;
                    double.TryParse(F_Nieve, out nieve);
                    ret = mySapModel.AreaObj.SetLoadUniformToFrame("Paneles", "Snow", nieve, 11, 1, true, "Global", eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (M_Nieve != "")
                {
                    /* 
                    * Añadimos el momento generada por la Nieve 
                    */
                    ret = mySapModel.SelectObj.ClearSelection();
                    int n_joints = 0;
                    double[] momento_nieve = { 0, 0, 0, 0, -double.Parse(M_Nieve), 0 };
                    double[] momento_nieve_ext = { 0, 0, 0, 0, -double.Parse(M_Nieve) / 2, 0 };
                    while (ret == 0)
                    {
                        n_joints++;
                        if (n_joints == 1)
                        {
                            ret = mySapModel.PointObj.SetLoadForce($"vp-{n_joints}", "Snow", ref momento_nieve_ext, true, "Global");
                            ret = mySapModel.PointObj.SetLoadForce($"vp{n_joints}", "Snow", ref momento_nieve_ext, true, "Global");
                        }
                        else
                        {
                            ret = mySapModel.PointObj.SetLoadForce($"vp-{n_joints}", "Snow", ref momento_nieve, true, "Global");
                            ret = mySapModel.PointObj.SetLoadForce($"vp{n_joints}", "Snow", ref momento_nieve, true, "Global");
                        }
                    }

                    ret = mySapModel.PointObj.SetLoadForce($"vp-{n_joints - 1}", "Snow", ref momento_nieve_ext, true, "Global");
                    ret = mySapModel.PointObj.SetLoadForce($"vp{n_joints - 1}", "Snow", ref momento_nieve_ext, true, "Global");

                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (F_SUP_PRESION != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento a presion en el paño superior
                    */
                    ret = mySapModel.SelectObj.ClearSelection();
                    ret = mySapModel.SelectObj.CoordinateRange(0.1, 10, -100, 100, 0, 10, false, "Global", true, false, false, true, false, false);
                    double presionSup;
                    double.TryParse(F_SUP_PRESION, out presionSup);
                    ret = mySapModel.AreaObj.SetLoadUniformToFrame("Paneles", "W1_Pos_Cfmin", presionSup, 3, 1, true, "Local", eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (F_INF_PRESION != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento a presion en el paño inferior
                    */
                    ret = mySapModel.SelectObj.ClearSelection();
                    ret = mySapModel.SelectObj.CoordinateRange(-10, -0.01, -100, 100, 0, 10, false, "Global", true, false, false, true, false, false);
                    double presionInf;
                    double.TryParse(F_INF_PRESION, out presionInf);
                    ret = mySapModel.AreaObj.SetLoadUniformToFrame("Paneles", "W1_Pos_Cfmin", presionInf, 3, 1, true, "Local", eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (estrategia == true)
                {
                    if (F_SUP_SUCCION != "")
                    {
                        /* 
                        * Añadimos la carga generada por el viento a succion en el paño superior
                        */
                        ret = mySapModel.SelectObj.ClearSelection();
                        ret = mySapModel.SelectObj.CoordinateRange(0.1, 10, -100, 100, 0, 10, false, "Global", true, false, false, true, false, false);
                        double succionSup;
                        double.TryParse(F_SUP_SUCCION, out succionSup);
                        ret = mySapModel.AreaObj.SetLoadUniformToFrame("Paneles", "W1_Neg_Cfmin", succionSup, 3, 1, true, "Local", eItemType.SelectedObjects);
                        ret = mySapModel.SelectObj.ClearSelection();
                    }
                }
                else
                {
                    ret = mySapModel.SelectObj.ClearSelection();
                    ret = mySapModel.SelectObj.CoordinateRange(0.1, 10, -100, 100, 0, 10, false, "Global", true, false, false, true, false, false);
                    ret = mySapModel.AreaObj.DeleteLoadUniformToFrame("Paneles", "W1_Neg_Cfmin", eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (estrategia == true)
                {
                    if (F_INF_SUCCION != "")
                    {
                        /* 
                        * Añadimos la carga generada por el viento a succion en el paño inferior
                        */
                        ret = mySapModel.SelectObj.ClearSelection();
                        ret = mySapModel.SelectObj.CoordinateRange(-10, -0.01, -100, 100, 0, 10, false, "Global", true, false, false, true, false, false);
                        double succionInf;
                        double.TryParse(F_INF_SUCCION, out succionInf);
                        ret = mySapModel.AreaObj.SetLoadUniformToFrame("Paneles", "W1_Neg_Cfmin", succionInf, 3, 1, true, "Local", eItemType.SelectedObjects);
                        ret = mySapModel.SelectObj.ClearSelection();

                    }
                }
                else
                {
                    ret = mySapModel.SelectObj.ClearSelection();
                    ret = mySapModel.SelectObj.CoordinateRange(-10, -0.01, -100, 100, 0, 10, false, "Global", true, false, false, true, false, false);
                    ret = mySapModel.AreaObj.DeleteLoadUniformToFrame("Paneles", "W1_Neg_Cfmin", eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                return ret;
            }
            else
            {
                Console.WriteLine("No se ha podido establecer conexion con SAP2000");
                ret = 1;
                return ret;
            }
        }

        public static int AsignarCargasFija
            (
            cHelper myHelper,
            cOAPI mySapObject,
            cSapModel mySapModel,
            bool estrategia,
            string F_Nieve,
            string F_SUP_PRESION,
            string F_INF_PRESION,
            string F_SUP_SUCCION,
            string F_INF_SUCCION,
            string PILAR_DELANTERO,
            string PILAR_TRASERO,
            string VIGA,
            string DIAGONAL,
            string PANEL,
            string CORREA,
            bool MONOPOSTE_BIPOSTE,
            string ProgramPath
            )
        {
            int ret = 0;

            if (mySapModel != null)
            {
                if (mySapModel.GetModelIsLocked() == true)
                {
                    ret = mySapModel.SetModelIsLocked(false);
                }

                ret = mySapModel.SetPresentUnits(eUnits.kN_m_C);

                double[] coord_inf = new double[3];
                double[] coord_sup = new double[3];

                ret = mySapModel.SelectObj.ClearSelection();
                ret = mySapModel.SelectObj.Group("06 Paneles");

                int NumberItems = 0;
                int[] ObjectType = { };
                string[] ObjectName = { };
                ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);

                int NumberPoints = 0;
                string[] Point = { };
                ret = mySapModel.AreaObj.GetPoints(ObjectName[0], ref NumberPoints, ref Point);

                double[] coord_point_x_0 = new double[NumberPoints];
                double[] coord_point_y_0 = new double[NumberPoints];
                double[] coord_point_z_0 = new double[NumberPoints];

                for (int n = 0; n < NumberPoints; n++)
                {
                    double X = 0;
                    double Y = 0;
                    double Z = 0;

                    ret = mySapModel.PointObj.GetCoordCartesian(Point[n], ref X, ref Y, ref Z);

                    coord_point_x_0[n] = X;
                    coord_point_y_0[n] = Y;
                    coord_point_z_0[n] = Z;
                }

                coord_inf = [coord_point_x_0[0], coord_point_y_0[0], coord_point_z_0[0]];

                ret = mySapModel.AreaObj.GetPoints(ObjectName[NumberItems - 1], ref NumberPoints, ref Point);

                double[] coord_point_x = new double[NumberPoints];
                double[] coord_point_y = new double[NumberPoints];
                double[] coord_point_z = new double[NumberPoints];

                for (int n = 0; n < NumberPoints; n++)
                {
                    double X = 0;
                    double Y = 0;
                    double Z = 0;

                    ret = mySapModel.PointObj.GetCoordCartesian(Point[n], ref X, ref Y, ref Z);

                    coord_point_x[n] = X;
                    coord_point_y[n] = Y;
                    coord_point_z[n] = Z;
                }

                coord_sup = [coord_point_x[2], coord_point_y[2], coord_point_z[2]];

                /* Obtenemos el punto medio*/

                double[] coord_media = new double[3];

                coord_media = [HERRAMIENTAS_AUXILIARES.puntomedio(coord_inf[0], coord_sup[0]), 0, HERRAMIENTAS_AUXILIARES.puntomedio(coord_inf[2], coord_sup[2])];

                double lateral_paneles = Math.Sqrt(Math.Pow(coord_point_z_0[1] - coord_point_z_0[0], 2) + Math.Pow(coord_point_x_0[1] - coord_point_x_0[0], 2));
                double largo_paneles = Math.Abs(coord_point_y_0[0]) + Math.Abs(coord_point_y_0[3]);
                double area = lateral_paneles * largo_paneles;

                double tot_area = NumberItems * area;

                if (F_Nieve != "")
                {
                    /* 
                    * Añadimos la carga generada por la Nieve 
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    double nieve;
                    double.TryParse(F_Nieve, out nieve);

                    nieve = (nieve * tot_area) / (NumberItems * 2 * largo_paneles);

                    ret = mySapModel.FrameObj.SetLoadDistributed("03 Correas", "Snow", 1, 10, 0, 1, nieve, nieve, "Global", ItemType: eItemType.Group);
                    ret = mySapModel.SelectObj.ClearSelection();

                }

                if (F_SUP_PRESION != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento a presion en el paño superior
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.SelectObj.CoordinateRange(
                        coord_media[0],
                        coord_sup[0],
                        coord_inf[1],
                        coord_sup[1],
                        -10,
                        10,
                        false,
                        "Global",
                        true,
                        false,
                        true,
                        false,
                        false,
                        false);

                    ret = mySapModel.SelectObj.Group("01 Pilares", true);
                    ret = mySapModel.SelectObj.Group("02 Vigas", true);
                    ret = mySapModel.SelectObj.Group("04 Diagonales", true);
                    ret = mySapModel.SelectObj.Group("05 Arriostramiento Correas", true);
                    ret = mySapModel.SelectObj.Group("06 Paneles", true);


                    double presionSup;
                    double.TryParse(F_SUP_PRESION, out presionSup);

                    presionSup = (presionSup * tot_area) / (NumberItems * 2 * largo_paneles);

                    ret = mySapModel.FrameObj.SetLoadDistributed("03 Correas", "W1_Press", 1, 2, 0, 1, presionSup, presionSup, "Local", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (F_INF_PRESION != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento a presion en el paño inferior
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.SelectObj.CoordinateRange(
                        coord_inf[0],
                        coord_media[0],
                        coord_inf[1],
                        coord_sup[1],
                        -10,
                        10,
                        false,
                        "Global",
                        true,
                        false,
                        true,
                        false,
                        false,
                        false);

                    ret = mySapModel.SelectObj.Group("01 Pilares", true);
                    ret = mySapModel.SelectObj.Group("02 Vigas", true);
                    ret = mySapModel.SelectObj.Group("04 Diagonales", true);
                    ret = mySapModel.SelectObj.Group("05 Arriostramiento Correas", true);
                    ret = mySapModel.SelectObj.Group("06 Paneles", true);


                    double presionInf;
                    double.TryParse(F_INF_PRESION, out presionInf);

                    presionInf = (presionInf * tot_area) / (NumberItems * 2 * largo_paneles);

                    ret = mySapModel.FrameObj.SetLoadDistributed("03 Correas", "W1_Press", 1, 2, 0, 1, presionInf, presionInf, "Local", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (F_SUP_SUCCION != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento a Succion en el paño superior
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.SelectObj.CoordinateRange(
                        coord_media[0],
                        coord_sup[0],
                        coord_inf[1],
                        coord_sup[1],
                        -10,
                        10,
                        false,
                        "Global",
                        true,
                        false,
                        true,
                        false,
                        false,
                        false);

                    ret = mySapModel.SelectObj.Group("01 Pilares", true);
                    ret = mySapModel.SelectObj.Group("02 Vigas", true);
                    ret = mySapModel.SelectObj.Group("04 Diagonales", true);
                    ret = mySapModel.SelectObj.Group("05 Arriostramiento Correas", true);
                    ret = mySapModel.SelectObj.Group("06 Paneles", true);


                    double succSup;
                    double.TryParse(F_SUP_SUCCION, out succSup);

                    succSup = (succSup * tot_area) / (NumberItems * 2 * largo_paneles);

                    ret = mySapModel.FrameObj.SetLoadDistributed("03 Correas", "W2_Suct", 1, 2, 0, 1, succSup, succSup, "Local", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (F_INF_SUCCION != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento a Succion en el paño inferior
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.SelectObj.CoordinateRange(
                        coord_inf[0],
                        coord_media[0],
                        coord_inf[1],
                        coord_sup[1],
                        -10,
                        10,
                        false,
                        "Global",
                        true,
                        false,
                        true,
                        false,
                        false,
                        false);

                    ret = mySapModel.SelectObj.Group("01 Pilares", true);
                    ret = mySapModel.SelectObj.Group("02 Vigas", true);
                    ret = mySapModel.SelectObj.Group("04 Diagonales", true);
                    ret = mySapModel.SelectObj.Group("05 Arriostramiento Correas", true);
                    ret = mySapModel.SelectObj.Group("06 Paneles", true);


                    double succInf;
                    double.TryParse(F_INF_SUCCION, out succInf);

                    succInf = (succInf * tot_area) / (NumberItems * 2 * largo_paneles);

                    ret = mySapModel.FrameObj.SetLoadDistributed("03 Correas", "W2_Suct", 1, 2, 0, 1, succInf, succInf, "Local", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (PILAR_DELANTERO != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento lateral en el los pilares delanteros de los extremos.
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    if (MONOPOSTE_BIPOSTE == false)
                    {
                        ret = mySapModel.SelectObj.CoordinateRange(
                        coord_inf[0],
                        coord_media[0],
                        coord_inf[1],
                        coord_sup[1],
                        -10,
                        10,
                        false,
                        "Global",
                        true,
                        false,
                        true,
                        false,
                        false,
                        false);

                        ret = mySapModel.SelectObj.Group("02 Vigas", true);
                        ret = mySapModel.SelectObj.Group("03 Correas", true);
                        ret = mySapModel.SelectObj.Group("04 Diagonales", true);
                        ret = mySapModel.SelectObj.Group("05 Arriostramiento Correas", true);
                        ret = mySapModel.SelectObj.Group("06 Paneles", true);

                    }
                    else if (MONOPOSTE_BIPOSTE == true)
                    {
                        ret = mySapModel.SelectObj.Group("01 Pilares");
                    }

                    ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.FrameObj.SetSelected(ObjectName[0], true, eItemType.Objects);
                    ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberItems - 1], true, eItemType.Objects);

                    double pilarDel;
                    double.TryParse(PILAR_DELANTERO, out pilarDel);

                    ret = mySapModel.FrameObj.SetLoadDistributed("01 Pilares", "W3_90º", 1, 5, 0, 1, pilarDel, pilarDel, "Global", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.FrameObj.SetLoadDistributed("01 Pilares", "W4_270º", 1, 5, 0, 1, -pilarDel, -pilarDel, "Global", ItemType: eItemType.SelectedObjects);

                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (PILAR_TRASERO != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento lateral en el los pilares traseros de los extremos.
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    if (MONOPOSTE_BIPOSTE == false)
                    {
                        ret = mySapModel.SelectObj.CoordinateRange(
                        coord_media[0],
                        coord_sup[0],
                        coord_inf[1],
                        coord_sup[1],
                        -10,
                        10,
                        false,
                        "Global",
                        true,
                        false,
                        true,
                        false,
                        false,
                        false);

                        ret = mySapModel.SelectObj.Group("02 Vigas", true);
                        ret = mySapModel.SelectObj.Group("03 Correas", true);
                        ret = mySapModel.SelectObj.Group("04 Diagonales", true);
                        ret = mySapModel.SelectObj.Group("05 Arriostramiento Correas", true);
                        ret = mySapModel.SelectObj.Group("06 Paneles", true);
                    }

                    else if (MONOPOSTE_BIPOSTE == true)
                    {
                        ret = mySapModel.SelectObj.Group("01 Pilares");
                    }

                    ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.FrameObj.SetSelected(ObjectName[0], true, eItemType.Objects);
                    ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberItems - 1], true, eItemType.Objects);

                    double pilarTras;
                    double.TryParse(PILAR_TRASERO, out pilarTras);

                    ret = mySapModel.FrameObj.SetLoadDistributed("01 Pilares", "W3_90º", 1, 5, 0, 1, pilarTras, pilarTras, "Global", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.FrameObj.SetLoadDistributed("01 Pilares", "W4_270º", 1, 5, 0, 1, -pilarTras, -pilarTras, "Global", ItemType: eItemType.SelectedObjects);

                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (VIGA != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento lateral en el las vigas de los extremos.
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.SelectObj.Group("02 Vigas");

                    ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.FrameObj.SetSelected(ObjectName[0], true, eItemType.Objects);
                    ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberItems - 1], true, eItemType.Objects);

                    double viga;
                    double.TryParse(VIGA, out viga);

                    ret = mySapModel.FrameObj.SetLoadDistributed("02 Vigas", "W3_90º", 1, 5, 0, 1, viga, viga, "Global", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.FrameObj.SetLoadDistributed("02 Vigas", "W4_270º", 1, 5, 0, 1, -viga, -viga, "Global", ItemType: eItemType.SelectedObjects);

                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (DIAGONAL != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento lateral en las diagonales de los extremos.
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.SelectObj.Group("04 Diagonales");

                    ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);

                    ret = mySapModel.SelectObj.ClearSelection();
                    int NumberViga = 0;
                    int[] ObjectViga = { };
                    string[] ObjectNameViga = { };
                    ret = mySapModel.SelectObj.Group("02 Vigas");

                    ret = mySapModel.SelectObj.GetSelected(ref NumberViga, ref ObjectViga, ref ObjectNameViga);
                    ret = mySapModel.SelectObj.ClearSelection();

                    if (NumberViga == NumberItems)
                    {
                        ret = mySapModel.FrameObj.SetSelected(ObjectName[0], true, eItemType.Objects);
                        ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberItems - 1], true, eItemType.Objects);
                    }
                    else
                    {
                        ret = mySapModel.FrameObj.SetSelected(ObjectName[0], true, eItemType.Objects);
                        ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberViga - 1], true, eItemType.Objects);
                        ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberViga], true, eItemType.Objects);
                        ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberItems - 1], true, eItemType.Objects);
                    }

                    double diagonal;
                    double.TryParse(DIAGONAL, out diagonal);

                    ret = mySapModel.FrameObj.SetLoadDistributed("04 Diagonales", "W3_90º", 1, 5, 0, 1, diagonal, diagonal, "Global", ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.FrameObj.SetLoadDistributed("04 Diagonales", "W4_270º", 1, 5, 0, 1, -diagonal, -diagonal, "Global", ItemType: eItemType.SelectedObjects);

                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (PANEL != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento lateral en el las vigas de los extremos.
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.SelectObj.Group("02 Vigas");

                    ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);
                    ret = mySapModel.SelectObj.ClearSelection();

                    ret = mySapModel.FrameObj.SetSelected(ObjectName[0], true, eItemType.Objects);
                    ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberItems - 1], true, eItemType.Objects);

                    double panel;
                    double.TryParse(PANEL, out panel);

                    ret = mySapModel.FrameObj.SetLoadDistributed("02 Vigas", "W3_90º", 1, 5, 0, 1, panel, panel, "Global", Replace: false, ItemType: eItemType.SelectedObjects);
                    ret = mySapModel.FrameObj.SetLoadDistributed("02 Vigas", "W4_270º", 1, 5, 0, 1, -panel, -panel, "Global", Replace: false, ItemType: eItemType.SelectedObjects);

                    ret = mySapModel.SelectObj.ClearSelection();
                }

                if (CORREA != "")
                {
                    /* 
                    * Añadimos la carga generada por el viento lateral en el las vigas de los extremos.
                    */
                    ret = mySapModel.SelectObj.ClearSelection();

                    double correa;
                    double.TryParse(CORREA, out correa);

                    ret = mySapModel.FrameObj.SetLoadDistributed("03 Correas", "W3_90º", 1, 5, 0, 1, correa, correa, "Global", Replace: true, ItemType: eItemType.Group);
                    ret = mySapModel.FrameObj.SetLoadDistributed("03 Correas", "W4_270º", 1, 5, 0, 1, -correa, -correa, "Global", Replace: true, ItemType: eItemType.Group);

                    ret = mySapModel.SelectObj.ClearSelection();
                }

                return ret;
            }
            else
            {
                Console.WriteLine("No se ha podido establecer conexion con SAP2000");
                ret = 1;
                return ret;
            }
        }

        public static int CombinacionesCarga(
            cSapModel mySapModel,
            bool estrategia,
            bool sismo,
            bool nieve,
            bool fija,
            int h_s
            )
        {
            int ret = 0;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// Creacion ULS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            List<string> LoadCases;
            string[][] ScaleFactor;
            string[][] ScaleFactorFija;

            if (mySapModel != null)
            {
                if (mySapModel.GetModelIsLocked() == true)
                {
                    ret = mySapModel.SetModelIsLocked(false);
                }

                ret = mySapModel.SetPresentUnits(eUnits.kN_m_C);


                switch (estrategia)
                {
                    case false: //Se calcula en defensa
                        switch (nieve)
                        {
                            case true: //Con Nieve
                                LoadCases = ["DEAD", "PP PANELES", "W1_Pos_Cfmin", "Snow"];
                                //ELU
                                ScaleFactor = new string[10][];

                                ScaleFactor[0] = ["1", "1", "0", "0"];
                                ScaleFactor[1] = ["1,3", "1,3", "0", "0"];
                                ScaleFactor[2] = ["1", "1", "1,5", "0"];
                                ScaleFactor[3] = ["1,3", "1,3", "1,5", "0"];
                                ScaleFactor[4] = ["1", "1", "0", "1,5"];
                                ScaleFactor[5] = ["1,3", "1,3", "0", "1,5"];
                                ScaleFactor[6] = ["1", "1", "0,9", "1,5"];
                                ScaleFactor[7] = ["1,3", "1,3", "0,9", "1,5"];
                                ScaleFactor[8] = ["1", "1", "1,5", "0,75"];
                                ScaleFactor[9] = ["1,3", "1,3", "1,5", "0,75"];
                                break;

                            case false: //Sin Nieve
                                LoadCases = ["DEAD", "PP PANELES", "W1_Pos_Cfmin"];

                                ScaleFactor = new string[4][];

                                ScaleFactor[0] = ["1", "1", "0"];
                                ScaleFactor[1] = ["1,3", "1,3", "0"];
                                ScaleFactor[2] = ["1", "1", "1,5"];
                                ScaleFactor[3] = ["1,3", "1,3", "1,5"];
                                break;
                        }
                        break;
                    case true:  //Se calcula en funcionamiento
                        switch (nieve)
                        {
                            case true: //Con Nieve
                                LoadCases = ["DEAD", "PP PANELES", "W1_Pos_Cfmin", "W1_Neg_Cfmin", "Snow"];

                                ScaleFactor = new string[16][];

                                ScaleFactor[0] = ["1", "1", "0", "0", "0"];
                                ScaleFactor[1] = ["1,3", "1,3", "0", "0", "0"];
                                ScaleFactor[2] = ["1", "1", "1,5", "0", "0"];
                                ScaleFactor[3] = ["1,3", "1,3", "1,5", "0", "0"];
                                ScaleFactor[4] = ["1", "1", "0", "1,5", "0"];
                                ScaleFactor[5] = ["1,3", "1,3", "0", "1,5", "0"];
                                ScaleFactor[6] = ["1", "1", "0", "0", "1,5"];
                                ScaleFactor[7] = ["1,3", "1,3", "0", "0", "1,5"];
                                ScaleFactor[8] = ["1", "1", "0,9", "0", "1,5"];
                                ScaleFactor[9] = ["1,3", "1,3", "0,9", "0", "1,5"];
                                ScaleFactor[10] = ["1", "1", "0", "0,9", "1,5"];
                                ScaleFactor[11] = ["1,3", "1,3", "0", "0,9", "1,5"];
                                ScaleFactor[12] = ["1", "1", "1,5", "0", "0,75"];
                                ScaleFactor[13] = ["1,3", "1,3", "1,5", "0", "0,75"];
                                ScaleFactor[14] = ["1", "1", "0", "1,5", "0,75"];
                                ScaleFactor[15] = ["1,3", "1,3", "0", "1,5", "0,75"];
                                break;

                            case false: //Sin nieve
                                LoadCases = ["DEAD", "PP PANELES", "W1_Pos_Cfmin", "W1_Neg_Cfmin"];

                                ScaleFactor = new string[6][];

                                ScaleFactor[0] = ["1", "1", "0", "0"];
                                ScaleFactor[1] = ["1,3", "1,3", "0", "0"];
                                ScaleFactor[2] = ["1", "1", "1,5", "0"];
                                ScaleFactor[3] = ["1,3", "1,3", "1,5", "0"];
                                ScaleFactor[4] = ["1", "1", "0", "1,5"];
                                ScaleFactor[5] = ["1,3", "1,3", "0", "1,5"];
                                break;
                        }
                        break;
                }

                int n_cm = 6;

                if (fija == true)
                {
                    switch (sismo)
                    {
                        case false:
                            switch (nieve)
                            {
                                case true:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 3:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 4:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 5:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 6:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 7:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 8:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                    }
                                    break;

                                case false:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 3:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 4:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 5:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 6:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 7:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 8:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                    }
                                    break;
                            }
                            break;

                        case true:
                            switch (nieve)
                            {
                                case true:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 3:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 4:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 5:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 6:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 7:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 8:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                    }
                                    break;

                                case false:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 3:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 4:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 5:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 6:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 7:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 8:
                                            LoadCases = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }

                }

                if (fija == false)
                {
                    //Creamos las ULS
                    List<string> LoadCasesF = new List<string>(LoadCases);
                    for (int i = 1; ScaleFactor.Length >= i; i++)
                    {
                        List<string> ScaleFactorF = new List<string>(ScaleFactor[i - 1]);
                        Sap2000CreateCombination(mySapModel, $"ULS{i}", LoadCasesF, ScaleFactorF);
                    }
                }
                else if (fija == true)
                {
                    ScaleFactorFija = HERRAMIENTAS_AUXILIARES.combinacionesfijaULS(n_cm, true, true, true, true, nieve, h_s, sismo);

                    //Creamos las ULS
                    List<string> LoadCasesF = new List<string>(LoadCases);
                    for (int i = 1; ScaleFactorFija.Length >= i; i++)
                    {
                        List<string> ScaleFactorFijaF = new List<string>(ScaleFactorFija[i - 1]);
                        Sap2000CreateCombination(mySapModel, $"ULS{i}", LoadCasesF, ScaleFactorFijaF);
                    }
                }

                if (fija == false)
                {
                    //Creamos las ULS Sismo
                    List<string> LoadCasesSismo = ["Ex", "Ey"];
                    string[][] ScaleFactorSismo = new string[8][];

                    ScaleFactorSismo[0] = ["-0,3", "-1"];
                    ScaleFactorSismo[1] = ["0,3", "-1"];
                    ScaleFactorSismo[2] = ["-1", "-0,3"];
                    ScaleFactorSismo[3] = ["-1", "0,3"];
                    ScaleFactorSismo[4] = ["0,3", "1"];
                    ScaleFactorSismo[5] = ["-0,3", "1"];
                    ScaleFactorSismo[6] = ["1", "0,3"];
                    ScaleFactorSismo[7] = ["1", "-0,3"];

                    List<string> LoadCasesFSismo = new List<string>(LoadCasesSismo);
                    if (sismo == true)
                    {
                        for (int i = ScaleFactor.Length; (ScaleFactor.Length + 7) >= i; i++)
                        {
                            List<string> ScaleFactorFSismo = new List<string>(ScaleFactorSismo[i - ScaleFactor.Length]);
                            Sap2000CreateCombination(mySapModel, $"ULS{i + 1}", LoadCasesFSismo, ScaleFactorFSismo);
                        }
                    }
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /// Creacion SLS
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                List<string> LoadCasesSLS;
                string[][] ScaleFactorSLS;
                string[][] ScaleFactorSLSFija;

                switch (estrategia)
                {
                    case false: //Se calcula en defensa
                        switch (nieve)
                        {
                            case true: //Con Nieve
                                LoadCasesSLS = ["DEAD", "PP PANELES", "W1_Pos_Cfmin", "Snow"];
                                //ELU
                                ScaleFactorSLS = new string[5][];

                                ScaleFactorSLS[0] = ["1", "1", "0", "0"];
                                ScaleFactorSLS[1] = ["1", "1", "1", "0"];
                                ScaleFactorSLS[2] = ["1", "1", "0", "1"];
                                ScaleFactorSLS[3] = ["1", "1", "1", "0,5"];
                                ScaleFactorSLS[4] = ["1", "1", "0,6", "1"];
                                break;

                            case false: //Sin Nieve
                                LoadCasesSLS = ["DEAD", "PP PANELES", "W1_Pos_Cfmin"];

                                ScaleFactorSLS = new string[2][];

                                ScaleFactorSLS[0] = ["1", "1", "0"];
                                ScaleFactorSLS[1] = ["1", "1", "1"];
                                break;
                        }
                        break;
                    case true:  //Se calcula en funcionamiento
                        switch (nieve)
                        {
                            case true: //Con Nieve
                                LoadCasesSLS = ["DEAD", "PP PANELES", "W1_Pos_Cfmin", "W1_Neg_Cfmin", "Snow"];

                                ScaleFactorSLS = new string[8][];

                                ScaleFactorSLS[0] = ["1", "1", "0", "0", "0"];
                                ScaleFactorSLS[1] = ["1", "1", "1", "0", "0"];
                                ScaleFactorSLS[2] = ["1", "1", "0", "1", "0"];
                                ScaleFactorSLS[3] = ["1", "1", "0", "0", "1"];
                                ScaleFactorSLS[4] = ["1", "1", "1", "0", "0,5"];
                                ScaleFactorSLS[5] = ["1", "1", "0", "1", "0,5"];
                                ScaleFactorSLS[6] = ["1", "1", "0,6", "0", "1"];
                                ScaleFactorSLS[7] = ["1", "1", "0", "0,6", "1"];
                                break;

                            case false: //Sin nieve
                                LoadCasesSLS = ["DEAD", "PP PANELES", "W1_Pos_Cfmin", "W1_Neg_Cfmin"];

                                ScaleFactorSLS = new string[3][];

                                ScaleFactorSLS[0] = ["1", "1", "0", "0"];
                                ScaleFactorSLS[1] = ["1", "1", "1", "0"];
                                ScaleFactorSLS[2] = ["1", "1", "0", "1"];
                                break;
                        }
                        break;
                }

                if (fija == true)
                {
                    switch (sismo)
                    {
                        case false:
                            switch (nieve)
                            {
                                case true:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 3:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 4:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 5:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 6:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 7:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                        case 8:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow"];
                                            break;
                                    }
                                    break;

                                case false:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 3:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 4:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 5:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 6:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 7:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                        case 8:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º"];
                                            break;
                                    }
                                    break;
                            }
                            break;

                        case true:
                            switch (nieve)
                            {
                                case true:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 3:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 4:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 5:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 6:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 7:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                        case 8:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Snow", "Ex", "Ey"];
                                            break;
                                    }
                                    break;

                                case false:
                                    switch (n_cm)
                                    {
                                        case 2:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 3:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 4:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 5:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 6:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 7:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                        case 8:
                                            LoadCasesSLS = ["DEAD", "PP PANELES", "CM1", "CM2", "CM3", "CM4", "CM5", "CM6", "CM7", "CM8", "W1_Press", "W2_Suct", "W3_90º", "W4_270º", "Ex", "Ey"];
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }

                }


                if (fija == false)
                {
                    //Creamos las SLS
                    List<string> LoadCasesSLSF = new List<string>(LoadCasesSLS);
                    for (int i = 1; ScaleFactorSLS.Length >= i; i++)
                    {
                        List<string> ScaleFactorSLSF = new List<string>(ScaleFactorSLS[i - 1]);
                        Sap2000CreateCombination(mySapModel, $"SLS{i}", LoadCasesSLSF, ScaleFactorSLSF);
                    }
                }
                else if (fija == true)
                {
                    ScaleFactorSLSFija = HERRAMIENTAS_AUXILIARES.combinacionesfijaSLS(n_cm, true, true, true, true, nieve, h_s, sismo);

                    //Creamos las SLS
                    List<string> LoadCasesSLSF = new List<string>(LoadCasesSLS);
                    for (int i = 1; ScaleFactorSLSFija.Length >= i; i++)
                    {
                        List<string> ScaleFactorSLSF = new List<string>(ScaleFactorSLSFija[i - 1]);
                        Sap2000CreateCombination(mySapModel, $"SLS{i}", LoadCasesSLSF, ScaleFactorSLSF);
                    }
                }

                if (fija == false)
                {
                    //Creamos las SLS Sismo
                    List<string> LoadCasesSismoSLS = ["Ex", "Ey"];
                    string[][] ScaleFactorSismoSLS = new string[4][];

                    ScaleFactorSismoSLS[0] = ["-0,3", "-1"];
                    ScaleFactorSismoSLS[1] = ["0,3", "-1"];
                    ScaleFactorSismoSLS[2] = ["-1", "-0,3"];
                    ScaleFactorSismoSLS[3] = ["-1", "0,3"];

                    List<string> LoadCasesFSismoSLS = new List<string>(LoadCasesSismoSLS);
                    if (sismo == true)
                    {
                        for (int i = ScaleFactorSLS.Length; (ScaleFactorSLS.Length + 3) >= i; i++)
                        {
                            List<string> ScaleFactorFSismoSLS = new List<string>(ScaleFactorSismoSLS[i - ScaleFactorSLS.Length]);
                            Sap2000CreateCombination(mySapModel, $"SLS{i - ScaleFactorSLS.Length + 1}", LoadCasesFSismoSLS, ScaleFactorFSismoSLS);
                        }
                    }
                }

                Sap2000CreateEnvelopeCombination(mySapModel);

                return ret;
            }
            else
            {
                Console.WriteLine("No se ha podido establecer conexion con SAP2000");
                ret = 1;
                return ret;
            }

            void Sap2000CreateCombination(cSapModel mySapModel, string ComboName, List<string> LoadCases, List<string> ScaleFactor)
            {
                int ret = 0;
                ret = mySapModel.RespCombo.Add(ComboName, 0);
                eCNameType CNameType = eCNameType.LoadCase;
                for (int i = 0; i < LoadCases.Count(); i++)
                {
                    if (double.Parse(ScaleFactor[i]) != 0)
                    {
                        ret = mySapModel.RespCombo.SetCaseList(ComboName, ref CNameType, LoadCases[i], double.Parse(ScaleFactor[i]));
                    }
                }
            }

            void Sap2000CreateEnvelopeCombination(cSapModel mySapModel, bool ULS = true, bool SLS = true)
            {
                int ret = 0;

                if (ULS)
                {
                    ret = mySapModel.RespCombo.Add("ULS", 1);
                    int NumberNames = 0;
                    string[] MyName = new string[150];
                    ret = mySapModel.RespCombo.GetNameList(ref NumberNames, ref MyName);
                    eCNameType CNameType = eCNameType.LoadCombo;
                    for (int i = 0; i < NumberNames; i++)
                    {
                        if (MyName[i].Substring(0, 3) == "ULS")
                        {
                            ret = mySapModel.RespCombo.SetCaseList("ULS", ref CNameType, MyName[i], 1.0);
                        }
                    }
                }
                if (!SLS)
                {
                    return;
                }
                ret = mySapModel.RespCombo.Add("SLS", 1);
                int NumberNames2 = 0;
                string[] MyName2 = new string[150];
                ret = mySapModel.RespCombo.GetNameList(ref NumberNames2, ref MyName2);
                eCNameType CNameType2 = eCNameType.LoadCombo;
                for (int j = 0; j < NumberNames2; j++)
                {
                    if (MyName2[j].Substring(0, 3) == "SLS")
                    {
                        ret = mySapModel.RespCombo.SetCaseList("SLS", ref CNameType2, MyName2[j], 1.0);
                    }
                }
            }
        }

        public static void Sap2000AssingDesignSteelCombos(cSapModel mySapModel)
        {
            int ret = 0;
            int NumberNames = 0;
            string[] MyName = new string[150];
            ret = mySapModel.RespCombo.GetNameList(ref NumberNames, ref MyName);
            for (int i = 0; i < NumberNames; i++)
            {
                if (MyName[i].Substring(0, 3) == "ULS" && MyName[i].Length > 3)
                {
                    ret = mySapModel.DesignSteel.SetComboStrength(MyName[i], Selected: true);
                    ret = mySapModel.DesignColdFormed.SetComboStrength(MyName[i], Selected: true);
                }
                else if (MyName[i].Substring(0, 3) == "SLS" && MyName[i].Length > 3)
                {
                    ret = mySapModel.DesignSteel.SetComboDeflection(MyName[i], Selected: true);
                    ret = mySapModel.DesignColdFormed.SetComboDeflection(MyName[i], Selected: true);
                }
            }
        }

        public static int CalcularMordelo
            (
            cHelper myHelper,
            cOAPI mySapObject,
            cSapModel mySapModel
            )
        {
            int ret = 0;

            if (mySapModel.GetModelIsLocked() == true)
            {
                MessageBoxResult result = MessageBox.Show("El modelo SAP ya está calculado.\n ¿Volver a Calcular el modelo?", null, MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Si el usuario selecciona "Yes", ejecutar el análisis
                    Console.WriteLine("El usuario seleccionó 'Yes'. Volviendo a calcular el modelo...");
                    ret = mySapModel.SetModelIsLocked(false);
                    ret = mySapModel.Analyze.RunAnalysis();
                }
                else if (result == MessageBoxResult.No)
                {
                    // Si el usuario selecciona "No", no hacer nada o realizar otra acción
                    Console.WriteLine("El usuario seleccionó 'No'. No se recalculará el modelo.");
                }
            }
            else
            {
                ret = mySapModel.Analyze.RunAnalysis();
            }

            return ret;
        }

        public static int CambiarNormativaSteel
            (
            cHelper myHelper,
            cOAPI mySapObject,
            cSapModel mySapModel
            )
        {
            int ret = 0;

            if (mySapModel.GetModelIsLocked() == true)
            {
                ret = mySapModel.SetModelIsLocked(false);
            }

            ret = mySapModel.DesignSteel.SetCode("Italian NTC 2018");

            /* Si queremos definir algún overwrite en la normativa italiana utilizamos:
            *
            *  ret = mySapModel.DesignSteel.Italian_NTC_2018.SetPreference(int IDItem, double value);
            *  
            *  IDItem list:
            *  3 = Method Used for Buckling in P-M-M
            *  4 = Framing type
            *  5 = GammaM0
            *  6 = GammeM1
            *  7 = GammaM2
            *  8 = Consider deflection
            *  9 = DL deflection limit, L/Value
            *  10 = SDL + LL deflection limit, L/Value
            *  11 = LL deflection limit, L/Value
            *  12 = Total deflection limit, L/Value
            *  13 = Total camber limit, L/Value
            *  14 = Pattern live load factor
            *  15 = Demand/capacity ratio limit
            *  16 = Multi-Response Case Design
            *  17 = Behavior Factor, q0
            *  18 = System Overstrength Factor, W
            *  19 = Consider P-Delta
            *  20 = Consider Torsion
            *  21 = Ignore Seismic Code
            *  22 = Ignore Special Seismic Load
            *  23 = Is Doubler Plate Plug-Welded
            * 
            *  Value reference:
            *  3 = K factor method
            *       1 or "Method A"
            *       2 or "Method B" - Default
            *       3 or "Both"
            *  4 = Framing type
            *       1 or "DCH-MRF"
            *       2 or "DCL-MRF"
            *       3 or "DCH-CBF"
            *       4 or "DCL-CBF"
            *       5 or "DCH-EBF"
            *       6 or "DCL-EBF"
            *       7 or "InvPendulum"
            *       8 or "Non Dissipative" - (Default)
            *  5 = GammaM0
            *       Default = 1.05, Value > 0
            *  6 = GammaM1
            *       Default = 1.05, Value > 0
            *  7 = GammaM2
            *       Default = 1.25, Value > 0
            *  8 = Consider deflection
            *       1 or "No" - Default
            *       2 or "Yes
            *  9 = DL deflection limit, L/Value
            *       Default = 0, Value > 0
            *  10 = SDL + LL deflection limit, L/Value
            *       Default = 0, Value > 0
            *  11 = LL deflection limit, L/Value
            *       Default = 300, Value > 0 
            *  12 = Total deflection limit, L/Value
            *       Default = 0, Value > 0 
            *  13 = Total camber limit, L/Value
            *       Default = 250, Value > 0
            *  14 = Pattern live load factor
            *       Default = 0, Value >= 0 
            *  15 = Demand/capacity ratio limit
            *       Default = 1.0, Value > 0
            *  16 = Multi-response case design
            *       1 or "Envelopes" - (Default)
            *       2 or "Step-by-step"
            *       3 or "Last step"
            *       4 or "Envelopes -- All"
            *       5 or "Step-by-step -- All" 
            *  17 = Behavior Factor, q0
            *       Default = 1, Value > 0 
            *  18 = System Overstrength Factor, W
            *       Default = 1.0, Value > 0 
            *  19 = Consider P-Delta
            *       1 or "No" - Default
            *       2 or "Yes" 
            *  20 = Consider Torsion
            *       1 or "No" - Default
            *       2 or "Yes" 
            *  21 = Ignore Seismic Code
            *       1 or "No" - Default
            *       2 or "Yes" 
            *  22 = Ignore Special Seismic Load
            *       1 or "No" - Default
            *       2 or "Yes" 
            *  23 = Is Doubler Plate Plug-Welded
            *       1 or "No" - Default
            *       2 or "Yes"
            */

            return ret;
        }

        public static int CambiarCoeficientePandeo(cSapModel mySapModel)
        {
            int ret = 0;

            ret = mySapModel.SelectObj.Group("01 Pilares Centrales");
            ret = mySapModel.SelectObj.Group("02 Pilares Generales");

            ret = mySapModel.DesignSteel.Italian_NTC_2018.SetOverwrite("", 45, "", 1, eItemType.SelectedObjects);
            ret = mySapModel.DesignSteel.Italian_NTC_2018.SetOverwrite("", 24, "", 1.6, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            ret = mySapModel.SelectObj.Group("01 Pilares Centrales");
            ret = mySapModel.SelectObj.Group("02 Pilares Generales");
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 10, 1, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 13, 1.6, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            ret = mySapModel.SelectObj.Group("05 Vigas Secundarias");

            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 10, 1, eItemType.SelectedObjects);
            //ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 34, 1, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 5, 2, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 6, 2, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            ret = mySapModel.SelectObj.Group("05 Vigas Secundarias");
            ret = mySapModel.DesignSteel.Italian_NTC_2018.SetOverwrite("", 45, "", 1, eItemType.SelectedObjects);
            ret = mySapModel.DesignSteel.Italian_NTC_2018.SetOverwrite("", 18, "", 2, eItemType.SelectedObjects);
            ret = mySapModel.DesignSteel.Italian_NTC_2018.SetOverwrite("", 19, "", 2, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            return ret;
        }

        public static int CambiarCoeficientePandeoFija(cSapModel mySapModel, bool monoposte)
        {
            int ret = 0;
            /*
             * Sustituimos los coeficientes de pandeo
             * 1.-PILARES:
             *  * K_ltb = 1
             *  * C_1 = 1.6
             */
            ret = mySapModel.SelectObj.Group("01 Pilares");

            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 10, 1, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 13, 1.6, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            /*
             * 2.-VIGAS:
             *  * C_1 = 1
             */

            double ratio_pandeo = PorcentajePandeoVigas(LongitudEntreElementos(mySapModel, monoposte), HERRAMIENTAS_AUXILIARES.LongitudSegmento(mySapModel, "Beam_1"));

            ret = mySapModel.SelectObj.Group("02 Vigas");
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 5, ratio_pandeo, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 6, ratio_pandeo, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 13, 1, eItemType.SelectedObjects);

            ret = mySapModel.SelectObj.ClearSelection();

            /*
             * 3.-CORREAS:
             *  * L_y = 1
             *  * L_z = 1
             *  * C_1 = 1.23
             */
            ret = mySapModel.SelectObj.Group("03 Correas");

            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 5, 1, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 6, 1, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 13, 1.23, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            /*
             * 4.- EXTREMOS CORREAS:
             *  * L_y = 2
             *  * L_z = 2
             */
            ret = SeleccionarExtremosDeCorreas(mySapModel);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 5, 2, eItemType.SelectedObjects);
            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 6, 2, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            /*
             * 5.-DIAGONALES:
             *  * C_1 = 1
             */
            ret = mySapModel.SelectObj.Group("04 Diagonales");

            ret = mySapModel.DesignColdFormed.EuroCold06.SetOverwrite("", 13, 1, eItemType.SelectedObjects);
            ret = mySapModel.SelectObj.ClearSelection();

            return ret;
        }

        public static int CambiarTablasPandeoColdFormed(cSapModel mySapModel, string prefijo)
        {
            int ret = 0;

            ret = mySapModel.DatabaseTables.CancelTableEditing();

            //Sacamos el listado de tablas disponibles
            int nTables = 0;
            string[] ListaKeyTablas = new string[500];
            string[] ListaTablas = new string[500];
            int[] import = new int[500];

            ret = mySapModel.DatabaseTables.GetAvailableTables(ref nTables, ref ListaKeyTablas, ref ListaTablas, ref import);

            //Obtenemos la versión de SAP2000 que se está ejecutando
            string versionSAP = "";
            double nVersionSAP = 0;
            ret = mySapModel.GetVersion(ref versionSAP, ref nVersionSAP);

            //Cargamos la base de datos de la tabla a editar
            int version = 0;
            string[] keyFieldsInclude = new string[500];
            int numberRecords = 0;
            string[] TablaData = new string[500];

            int posC1C2C3Option = 0;

            string caracter = "_";
            string subcadena;
            int indiceCaracter;

            /***
             * ColdFormed
             ***/
            version = 0;
            keyFieldsInclude = new string[500];
            numberRecords = 0;
            TablaData = new string[500];

            ret = mySapModel.DatabaseTables.GetTableForEditingArray("Overwrites - Cold Formed Design - Eurocode 3 1-3 2006", "ALL", ref version, ref keyFieldsInclude, ref numberRecords, ref TablaData);

            for (int i = 0; i < keyFieldsInclude.Count(); i++)
            {
                if (keyFieldsInclude[i] == "C1C2C3Option")
                {
                    posC1C2C3Option = i;
                    break;
                }
            }

            if (nVersionSAP >= 25) //Si la versión de SAP2000 es mayor o igual a v25 sustituimos "Method 1 - EC3" por "Method 1 - ENV 1993-1-1:1992"
            {
                for (int i = 0; i < TablaData.Count(); i += keyFieldsInclude.Count())
                {
                    if (TablaData[i + posC1C2C3Option] == "Method 1 - EC3") //Si la versión de SAP2000 es mayor o igual a v25 tenemos que adaptar el nombre
                    {
                        TablaData[i + posC1C2C3Option] = "Method 1 - ENV 1993-1-1:1992";
                    }
                }
            }

            for (int i = 0; i < TablaData.Count(); i += keyFieldsInclude.Count())
            {
                try
                {
                    indiceCaracter = TablaData[i].IndexOf(caracter);
                    subcadena = TablaData[i].Substring(0, indiceCaracter);
                }
                catch
                {
                    subcadena = TablaData[i];

                }

                if (subcadena == prefijo)
                {
                    string BendingC1C2C3 = "Method 3 - User Defined";
                    if (BendingC1C2C3 == "Method 1 - EC3" && nVersionSAP >= 25) //Si la versión de SAP2000 es mayor o igual a v25 tenemos que adaptar el nombre
                    {
                        BendingC1C2C3 = "Method 1 - ENV 1993-1-1:1992";
                    }

                    TablaData[i + posC1C2C3Option] = BendingC1C2C3;
                }
            }

            //if (keyFieldsInclude[posC1C2C3Option] == "C1C2C3Option")
            //{
            //    keyFieldsInclude[posC1C2C3Option] = "PsiC2C3Option";
            //}

            ret = mySapModel.DatabaseTables.SetTableForEditingArray("Overwrites - Cold Formed Design - Eurocode 3 1-3 2006", ref version, ref keyFieldsInclude, numberRecords, ref TablaData);

            int nErrores = 0;
            int nMsgError = 0;
            int nWarnMsgs = 0;
            int NInfoMsgs = 0;
            string ImportLog = "";

            ret = mySapModel.DatabaseTables.ApplyEditedTables(true, ref nErrores, ref nMsgError, ref nWarnMsgs, ref NInfoMsgs, ref ImportLog);

            ret = mySapModel.DatabaseTables.CancelTableEditing();

            return ret;
        }

        public static int CambiarTablasPandeoSteel(cSapModel mySapModel, string prefijo)
        {
            int ret = 0;

            ret = mySapModel.DatabaseTables.CancelTableEditing();

            //Sacamos el listado de tablas disponibles
            int nTables = 0;
            string[] ListaKeyTablas = new string[500];
            string[] ListaTablas = new string[500];
            int[] import = new int[500];

            ret = mySapModel.DatabaseTables.GetAvailableTables(ref nTables, ref ListaKeyTablas, ref ListaTablas, ref import);

            //Obtenemos la versión de SAP2000 que se está ejecutando
            string versionSAP = "";
            double nVersionSAP = 0;
            ret = mySapModel.GetVersion(ref versionSAP, ref nVersionSAP);

            //Cargamos la base de datos de la tabla a editar
            int version = 0;
            string[] keyFieldsInclude = new string[500];
            int numberRecords = 0;
            string[] TablaData = new string[500];

            int posC1C2C3Option = 0;

            string caracter = "_";
            string subcadena;
            int indiceCaracter;

            /***
             * ColdFormed
             ***/
            version = 0;
            keyFieldsInclude = new string[500];
            numberRecords = 0;
            TablaData = new string[500];

            ret = mySapModel.DatabaseTables.GetTableForEditingArray("Overwrites - Steel Design - Italian NTC 2018", "ALL", ref version, ref keyFieldsInclude, ref numberRecords, ref TablaData);

            for (int i = 0; i < keyFieldsInclude.Count(); i++)
            {
                if (keyFieldsInclude[i] == "C1C2C3Option")
                {
                    posC1C2C3Option = i;
                    break;
                }
            }

            if (nVersionSAP >= 25) //Si la versión de SAP2000 es mayor o igual a v25 sustituimos "Method 1 - EC3" por "Method 1 - ENV 1993-1-1:1992"
            {
                for (int i = 0; i < TablaData.Count(); i += keyFieldsInclude.Count())
                {
                    if (TablaData[i + posC1C2C3Option] == "Method 1 - EC3") //Si la versión de SAP2000 es mayor o igual a v25 tenemos que adaptar el nombre
                    {
                        TablaData[i + posC1C2C3Option] = "Method 1 - ENV 1993-1-1:1992";
                    }
                }
            }

            for (int i = 0; i < TablaData.Count(); i += keyFieldsInclude.Count())
            {
                try
                {
                    indiceCaracter = TablaData[i].IndexOf(caracter);
                    subcadena = TablaData[i].Substring(0, indiceCaracter);
                }
                catch
                {
                    subcadena = TablaData[i];

                }

                if (subcadena == prefijo)
                {
                    string BendingC1C2C3 = "Method 3 - User Defined";
                    if (BendingC1C2C3 == "Method 1 - EC3" && nVersionSAP >= 25) //Si la versión de SAP2000 es mayor o igual a v25 tenemos que adaptar el nombre
                    {
                        BendingC1C2C3 = "Method 1 - ENV 1993-1-1:1992";
                    }

                    TablaData[i + posC1C2C3Option] = BendingC1C2C3;
                }
            }

            if (keyFieldsInclude[posC1C2C3Option] == "C1C2C3Option")
            {
                keyFieldsInclude[posC1C2C3Option] = "PsiC2C3Option";
            }

            ret = mySapModel.DatabaseTables.SetTableForEditingArray("Overwrites - Steel Design - Italian NTC 2018", ref version, ref keyFieldsInclude, numberRecords, ref TablaData);

            int nErrores = 0;
            int nMsgError = 0;
            int nWarnMsgs = 0;
            int NInfoMsgs = 0;
            string ImportLog = "";

            ret = mySapModel.DatabaseTables.ApplyEditedTables(true, ref nErrores, ref nMsgError, ref nWarnMsgs, ref NInfoMsgs, ref ImportLog);

            ret = mySapModel.DatabaseTables.CancelTableEditing();

            return ret;
        }

        public static int SeleccionarExtremosDeCorreas(cSapModel mySapModel)
        {
            int ret = 0;

            ret = mySapModel.SetModelIsLocked(false);
            ret = mySapModel.SelectObj.ClearSelection();

            ret = mySapModel.SelectObj.Group("03 Correas");

            int NumberItems = 0;
            int[] ObjectType = { };
            string[] ObjectName = { };

            ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);

            double[] coord_inf = new double[3];
            double[] coord_sup = new double[3];

            ret = mySapModel.SelectObj.ClearSelection();
            ret = mySapModel.SelectObj.Group("06 Paneles");

            int NumberItemsPanel = 0;
            int[] ObjectTypePanel = { };
            string[] ObjectNamePanel = { };
            ret = mySapModel.SelectObj.GetSelected(ref NumberItemsPanel, ref ObjectTypePanel, ref ObjectNamePanel);

            int NumberPoints = 0;
            string[] Point = { };
            ret = mySapModel.AreaObj.GetPoints(ObjectNamePanel[0], ref NumberPoints, ref Point);

            double[] coord_point_x_0 = new double[NumberPoints];
            double[] coord_point_y_0 = new double[NumberPoints];
            double[] coord_point_z_0 = new double[NumberPoints];

            for (int n = 0; n < NumberPoints; n++)
            {
                double X = 0;
                double Y = 0;
                double Z = 0;

                ret = mySapModel.PointObj.GetCoordCartesian(Point[n], ref X, ref Y, ref Z);

                coord_point_x_0[n] = X;
                coord_point_y_0[n] = Y;
                coord_point_z_0[n] = Z;
            }

            coord_inf = [coord_point_x_0[0], coord_point_y_0[0], coord_point_z_0[0]];

            ret = mySapModel.AreaObj.GetPoints(ObjectNamePanel[NumberItemsPanel - 1], ref NumberPoints, ref Point);

            double[] coord_point_x = new double[NumberPoints];
            double[] coord_point_y = new double[NumberPoints];
            double[] coord_point_z = new double[NumberPoints];

            for (int n = 0; n < NumberPoints; n++)
            {
                double X = 0;
                double Y = 0;
                double Z = 0;

                ret = mySapModel.PointObj.GetCoordCartesian(Point[n], ref X, ref Y, ref Z);

                coord_point_x[n] = X;
                coord_point_y[n] = Y;
                coord_point_z[n] = Z;
            }

            coord_sup = [coord_point_x[2], coord_point_y[2], coord_point_z[2]];

            ret = mySapModel.SelectObj.ClearSelection();
            ret = mySapModel.SelectObj.CoordinateRange(coord_inf[0] - 1, coord_sup[0] + 1, coord_inf[1] - 1, coord_inf[1] + 1, coord_inf[2] - 1, coord_sup[2] + 1, false, "Global", true, false, true, false, true, false);

            int NumberItemsCorrea = 0;
            int[] ObjectTypeCorrea = { };
            string[] ObjectNameCorrea = { };

            ret = mySapModel.SelectObj.GetSelected(ref NumberItemsCorrea, ref ObjectTypeCorrea, ref ObjectNameCorrea);

            int n_Segmentos_Correa = NumberItems / NumberItemsCorrea;

            ret = mySapModel.SelectObj.ClearSelection();

            for (int i = 0; i < NumberItems; i = i + n_Segmentos_Correa)
            {
                if (i == 0)
                {
                    ret = mySapModel.FrameObj.SetSelected(ObjectName[i], true, 0);
                }
                else
                {
                    ret = mySapModel.FrameObj.SetSelected(ObjectName[i], true, 0);
                    ret = mySapModel.FrameObj.SetSelected(ObjectName[i - 1], true, 0);
                }
            }

            ret = mySapModel.FrameObj.SetSelected(ObjectName[NumberItems - 1], true, 0);

            return ret;
        }

        public static double LongitudEntreElementos(cSapModel mySapModel, bool monoposte)
        {
            int ret = 0;

            double longitud_entre_elementos = 0;
            ret = mySapModel.SetModelIsLocked(false);
            ret = mySapModel.SelectObj.ClearSelection();

            ret = mySapModel.SelectObj.Group("02 Vigas");

            int NumberItems = 0;
            int[] ObjectType = { };
            string[] ObjectName = { };
            ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);

            string point1 = "";
            string point2 = "";
            ret = mySapModel.FrameObj.GetPoints(ObjectName[0], ref point1, ref point2);

            ret = mySapModel.SelectObj.ClearSelection();

            double[] coord_1 = new double[3];
            double[] coord_2 = new double[3];
            ret = mySapModel.PointObj.GetCoordCartesian(point1, ref coord_1[0], ref coord_1[1], ref coord_1[2]);
            ret = mySapModel.PointObj.GetCoordCartesian(point2, ref coord_2[0], ref coord_2[1], ref coord_2[2]);

            ret = mySapModel.SelectObj.CoordinateRange(coord_1[0], coord_2[0], coord_1[1], coord_2[1], coord_1[2], coord_2[2], false, "Global", false, true, false, false, false, false);

            ret = mySapModel.SelectObj.GetSelected(ref NumberItems, ref ObjectType, ref ObjectName);

            ret = mySapModel.SelectObj.ClearSelection();

            string[] puntos = new string[20];
            //Eliminamos los puntos que pertenecen a las correas

            int n = 0;
            foreach (string punto in ObjectName)
            {
                if (punto == point1 || punto == point2) { }

                else if (punto.Contains("nCc_") || punto.Contains("nCv_")) { }

                else
                {
                    puntos[n] = punto;
                    ret = mySapModel.PointObj.SetSelected(punto, true, 0);
                    n++;
                }
            }

            if (monoposte) // Si es monoposte
            {
                if (puntos[0] != null && puntos[1] != null && puntos[2] == null)
                {
                    longitud_entre_elementos = HERRAMIENTAS_AUXILIARES.LongitudEntrePuntos(mySapModel, puntos[0], puntos[1]);
                }
                else if (puntos[0] != null && puntos[1] != null && puntos[2] != null)
                {
                    string point_diag_1 = "";
                    string point_diag_2 = "";

                    foreach (string punto in puntos)
                    {
                        if (punto != null)
                        {
                            if (punto.Contains("nPs") == false)
                            {
                                if (point_diag_1 == "")
                                {
                                    point_diag_1 = punto;
                                }
                                else
                                {
                                    point_diag_2 = punto;
                                }
                            }
                        }
                    }

                    longitud_entre_elementos = HERRAMIENTAS_AUXILIARES.LongitudEntrePuntos(mySapModel, point_diag_1, point_diag_2);
                }
            }

            else //Si es biposte
            {
                if (puntos[0].Contains("nPs") && puntos[1].Contains("nPs") && puntos[2] == null)
                {
                    longitud_entre_elementos = HERRAMIENTAS_AUXILIARES.LongitudEntrePuntos(mySapModel, puntos[0], puntos[1]);
                }

                else
                {
                    string point_pil_1 = "";
                    string point_pil_2 = "";

                    foreach (string punto in puntos)
                    {
                        if (punto != null)
                        {
                            if (punto.Contains("nPs"))
                            {
                                if (point_pil_1 == "")
                                {
                                    point_pil_1 = punto;
                                }
                                else
                                {
                                    point_pil_2 = punto;
                                }
                            }
                        }
                    }

                    longitud_entre_elementos = HERRAMIENTAS_AUXILIARES.LongitudEntrePuntos(mySapModel, point_pil_1, point_pil_2);
                }
            }

            return longitud_entre_elementos;
        }

        public static double PorcentajePandeoVigas(double longitudPandeo, double longitudViga)
        {
            double porcentaje_pandeo = longitudPandeo / longitudViga;

            return porcentaje_pandeo;
        }
    }

    public class COMPROBACIONES
    {
        public static string[] Limitacion_Esbletez(double l_0y, double l_0z, double i_y, double i_z)
        {
            /*
            ret[0]: lambda_y = l0/i (Esbletez eje y)
            ret[1]: lambda_z = l0/i (Esbeltez eje z)
            ret[2]: max (lambda_y, lambda_z)
            ret[3]: Comprobacion esbelte max 
            */

            string[] ret = new string[4];

            ret[0] = (l_0y / i_y).ToString();
            ret[1] = (l_0z / i_z).ToString();
            ret[2] = Math.Round(Math.Max(double.Parse(ret[0]), double.Parse(ret[1])), 2).ToString();

            if (Math.Max(double.Parse(ret[0]), double.Parse(ret[1])) <= 200)
            {

                ret[3] = "true";
            }
            else
            {
                ret[3] = "false";
            }
            return ret;
        }

    }

    public class HERRAMIENTAS_AUXILIARES
    {
        public static string BuscarArchivo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Seleccionar archivo";
            openFileDialog.Filter = "Todos los archivos (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public static void Sap2000DeleteAllCaseAndCombinations(cSapModel mySapModel)
        {
            int ret = 0;
            int NumberNames = 0;
            string[] MyName = new string[50];
            ret = mySapModel.RespCombo.GetNameList(ref NumberNames, ref MyName);
            for (int num = NumberNames - 1; num >= 0; num--)
            {
                ret = mySapModel.RespCombo.Delete(MyName[num]);
            }
        }

        public static double LongitudSegmento(cSapModel sapModel, string elementName)
        {
            double x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0;
            string point1 = "";
            string point2 = "";

            // Obtener las coordenadas de los nodos del elemento
            sapModel.FrameObj.GetPoints(elementName, ref point1, ref point2);
            sapModel.PointObj.GetCoordCartesian(point1, ref x1, ref y1, ref z1);
            sapModel.PointObj.GetCoordCartesian(point2, ref x2, ref y2, ref z2);

            // Calcular la longitud del elemento
            double length = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2));

            return Math.Round(length, 2);
        }

        public static double LongitudRefuerzo(cSapModel sapModel, string elementName)
        {
            double x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0;
            string point1 = "";
            string point2 = "";

            elementName = elementName.Replace("_", "r_");

            // Obtener las coordenadas de los nodos del elemento
            sapModel.FrameObj.GetPoints(elementName, ref point1, ref point2);
            sapModel.PointObj.GetCoordCartesian(point1, ref x1, ref y1, ref z1);
            sapModel.PointObj.GetCoordCartesian(point2, ref x2, ref y2, ref z2);

            // Calcular la longitud del elemento
            double length = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2));

            return Math.Round(length, 2);
        }

        public static void ExportarTablas(string ruta_tabla_base, string ruta_guardado, string[][] dataArray)
        {
            // Cargar el archivo .rtf
            string rtfFilePath = ruta_tabla_base;
            FlowDocument flowDocument = new FlowDocument();
            TextRange textRange;
            using (FileStream fileStream = new FileStream(rtfFilePath, FileMode.Open))
            {
                textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                textRange.Load(fileStream, DataFormats.Rtf);
            }

            // Buscar la tabla existente
            Table existingTable = null;
            foreach (Block block in flowDocument.Blocks)
            {
                if (block is Table table)
                {
                    existingTable = table;
                    break;
                }
            }

            if (existingTable == null)
            {
                MessageBox.Show("No se encontró ninguna tabla en el archivo .rtf.");
                return;
            }

            // Añadir nuevas filas y rellenar columnas
            foreach (string[] data in dataArray)
            {
                if (data != null)
                {
                    TableRow row = new TableRow();
                    existingTable.RowGroups[0].Rows.Add(row);

                    for (int i = 0; i < existingTable.Columns.Count; i++)
                    {
                        Run run = new Run(data[i])
                        {
                            FontFamily = new FontFamily("Neo Tech Std"),
                            FontSize = 11
                        };
                        Paragraph paragraph = new Paragraph(run)
                        {
                            TextAlignment = TextAlignment.Center
                        };
                        TableCell cell = new TableCell(paragraph);
                        row.Cells.Add(cell);
                    }
                }
            }

            // Guardar el archivo .rtf modificado
            using (FileStream fileStream = new FileStream(ruta_guardado, FileMode.Create))
            {
                textRange.Save(fileStream, DataFormats.Rtf);
            }
        }

        public static string[] AñadirColumnasListView(ListView mylist, int n_cm, bool W1_Press, bool W2_Suc, bool W3_90, bool W4_270, bool nieve, bool sismo)
        {
            GridView gridView = mylist.View as GridView;

            gridView.Columns.Clear();

            int n_sismo = 0;
            if (sismo == true)
            {
                n_sismo = 2;
            }

            string[] columnas_creadas = new string[1 + n_cm + booltonumero(W1_Press) + booltonumero(W2_Suc) + booltonumero(W3_90) + booltonumero(W4_270) + booltonumero(nieve) + n_sismo];
            int n_columnas_creadas = 0;

            if (gridView != null)
            {
                // Creamos una columna para el peso propio
                GridViewColumn columna1 = new GridViewColumn();
                columna1.Header = "PP";
                columna1.DisplayMemberBinding = new System.Windows.Data.Binding("PESO_PROPIO");
                columna1.Width = 50;
                gridView.Columns.Add(columna1);

                columnas_creadas[n_columnas_creadas] = "PESO_PROPIO";
                n_columnas_creadas++;

                for (int i = 1; i <= n_cm; i++)
                {
                    GridViewColumn columna = new GridViewColumn();
                    columna.Header = $"CM {i}";
                    columna.DisplayMemberBinding = new System.Windows.Data.Binding($"CM_{i}");
                    columna.Width = 50;
                    gridView.Columns.Add(columna);

                    columnas_creadas[n_columnas_creadas] = $"CM_{i}";
                    n_columnas_creadas++;
                }
                // Crear y añadir W1_PRESS
                if (W1_Press)
                {
                    GridViewColumn columna2 = new GridViewColumn();
                    columna2.Header = "W1 PRESS";
                    columna2.DisplayMemberBinding = new System.Windows.Data.Binding("W1");
                    columna2.Width = 100;
                    gridView.Columns.Add(columna2);

                    columnas_creadas[n_columnas_creadas] = $"W1";
                    n_columnas_creadas++;
                }

                // Crear y añadir W2_SUC
                if (W2_Suc)
                {
                    GridViewColumn columna3 = new GridViewColumn();
                    columna3.Header = "W2 SUCCION";
                    columna3.DisplayMemberBinding = new System.Windows.Data.Binding("W2");
                    columna3.Width = 100;
                    gridView.Columns.Add(columna3);

                    columnas_creadas[n_columnas_creadas] = $"W2";
                    n_columnas_creadas++;
                }

                if (W3_90)
                {
                    GridViewColumn columna4 = new GridViewColumn();
                    columna4.Header = "W3 90º";
                    columna4.DisplayMemberBinding = new System.Windows.Data.Binding("W3");
                    columna4.Width = 100;
                    gridView.Columns.Add(columna4);

                    columnas_creadas[n_columnas_creadas] = $"W3";
                    n_columnas_creadas++;
                }

                if (W4_270)
                {
                    GridViewColumn columna5 = new GridViewColumn();
                    columna5.Header = "W4 270º";
                    columna5.DisplayMemberBinding = new System.Windows.Data.Binding("W4");
                    columna5.Width = 100;
                    gridView.Columns.Add(columna5);

                    columnas_creadas[n_columnas_creadas] = $"W4";
                    n_columnas_creadas++;
                }

                if (nieve)
                {
                    GridViewColumn columna6 = new GridViewColumn();
                    columna6.Header = "S1";
                    columna6.DisplayMemberBinding = new System.Windows.Data.Binding("S1");
                    columna6.Width = 50;
                    gridView.Columns.Add(columna6);

                    columnas_creadas[n_columnas_creadas] = $"S1";
                    n_columnas_creadas++;
                }

                if (sismo)
                {
                    GridViewColumn columna7 = new GridViewColumn();
                    columna7.Header = "SX";
                    columna7.DisplayMemberBinding = new System.Windows.Data.Binding("SX");
                    columna7.Width = 50;
                    gridView.Columns.Add(columna7);

                    columnas_creadas[n_columnas_creadas] = $"SX";
                    n_columnas_creadas++;

                    GridViewColumn columna8 = new GridViewColumn();
                    columna8.Header = "SY";
                    columna8.DisplayMemberBinding = new System.Windows.Data.Binding("SY");
                    columna8.Width = 50;
                    gridView.Columns.Add(columna8);

                    columnas_creadas[n_columnas_creadas] = $"SY";
                    n_columnas_creadas++;
                }
            }
            Console.WriteLine("Variables Columnas Creadas");
            foreach (string s in columnas_creadas)
            {
                Console.WriteLine($"    -> {s}");
            }

            return columnas_creadas;
        }

        public static int booltonumero(bool condicional)
        {
            if (condicional) { return 1; }
            else { return 0; }
        }

        public static string[][] combinacionesfijaULS(int n_CM, bool W1_Press, bool W2_Suc, bool W3_90, bool W4_270, bool S1, int h_s, bool check_sismo)
        {
            /* 
            -> PP = {1 ; 1.3};
            -> cm = {1.3};
            -> W1; W2; W3; W4  = {1.5 ; 0.9};
            -> S1 = {1.5};
            */

            double[] Coef_pp = { 1, 1.3 };

            double[] Coef_cm = { 1.3 };

            if (n_CM == 8 && check_sismo)
            {
                Coef_cm = [1, 1.3];
            }

            double[] Coef_W = { 1.5, 0.9 };

            double phi = 0;
            switch (h_s)
            {
                case < 1000:
                    phi = 0.5;
                    break;
                case > 1000:
                    phi = 0.7;
                    break;
            }
            double[] Coef_S = { 1.5, 1.5 * phi };

            bool[] wx = [W1_Press, W2_Suc, W3_90, W4_270];
            bool[] sx = [S1];

            int n_wx = 0;
            int n_sx = 0;

            foreach (bool viento in wx) { if (viento) { n_wx++; } }
            foreach (bool nieve in sx) { if (nieve) { n_sx++; } }

            //int numero_combinaciones = (Coef_pp.Count() * (1 + n_wx + n_sx)) + (Coef_pp.Count() * n_CM * Coef_cm.Count() * n_wx * Coef_W.Count() * Coef_S.Count());
            int numero_combinaciones = 0;

            switch (check_sismo)
            {
                case false:
                    switch (S1) //Sin sismo
                    {
                        case false: //Sin nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 30;
                                    break;
                                case 3:
                                    numero_combinaciones = 40;
                                    break;
                                case 4:
                                    numero_combinaciones = 50;
                                    break;
                                case 5:
                                    numero_combinaciones = 60;
                                    break;
                                case 6:
                                    numero_combinaciones = 70;
                                    break;
                                case 7:
                                    numero_combinaciones = 80;
                                    break;
                                case 8:
                                    numero_combinaciones = 90;
                                    break;
                            }
                            break;

                        case true: //Con nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 84;
                                    break;
                                case 3:
                                    numero_combinaciones = 112;
                                    break;
                                case 4:
                                    numero_combinaciones = 140;
                                    break;
                                case 5:
                                    numero_combinaciones = 168;
                                    break;
                                case 6:
                                    numero_combinaciones = 196;
                                    break;
                                case 7:
                                    numero_combinaciones = 224;
                                    break;
                                case 8:
                                    numero_combinaciones = 252;
                                    break;
                            }
                            break;
                    }
                    break;

                case true:
                    switch (S1) //Con Sismo
                    {
                        case false: //Sin nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 54;
                                    break;
                                case 3:
                                    numero_combinaciones = 72;
                                    break;
                                case 4:
                                    numero_combinaciones = 90;
                                    break;
                                case 5:
                                    numero_combinaciones = 108;
                                    break;
                                case 6:
                                    numero_combinaciones = 126;
                                    break;
                                case 7:
                                    numero_combinaciones = 144;
                                    break;
                                case 8:
                                    numero_combinaciones = 162;
                                    break;
                            }
                            break;

                        case true: //Con nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 108;
                                    break;
                                case 3:
                                    numero_combinaciones = 144;
                                    break;
                                case 4:
                                    numero_combinaciones = 180;
                                    break;
                                case 5:
                                    numero_combinaciones = 216;
                                    break;
                                case 6:
                                    numero_combinaciones = 252;
                                    break;
                                case 7:
                                    numero_combinaciones = 288;
                                    break;
                                case 8:
                                    numero_combinaciones = 548;
                                    break;
                            }
                            break;
                    }
                    break;
            }

            int numero_factores = 2 + n_CM + n_wx + n_sx;

            if (check_sismo)
            {
                numero_factores = numero_factores + 2;
            }
            string[][] ScaleFactor = new string[numero_combinaciones][];

            int pos_cb = 1;

            for (int y = 0; y < numero_combinaciones; y = y + 2)
            {
                ScaleFactor[y] = new string[numero_factores];
                ScaleFactor[y + 1] = new string[numero_factores];

                for (int x = 0; x <= n_CM; x++)
                {
                    if (x == 0)
                    {
                        ScaleFactor[y][0] = Coef_pp[0].ToString();
                        ScaleFactor[y + 1][0] = Coef_pp[1].ToString();
                        ScaleFactor[y][1] = Coef_pp[0].ToString();
                        ScaleFactor[y + 1][1] = Coef_pp[1].ToString();

                    }
                    else if (pos_cb == x && y > 1 && pos_cb != 0)
                    {
                        ScaleFactor[y][pos_cb + 1] = Coef_cm[0].ToString();
                        ScaleFactor[y + 1][pos_cb + 1] = Coef_cm[0].ToString();

                        if (pos_cb == n_CM)
                        {
                            pos_cb = -1;
                        }
                    }
                }

                if (y > 1)
                {
                    pos_cb++;
                }
            }

            int pos_w = 0;
            int index_w = 0;

            for (int y = 0; y < numero_combinaciones; y = y + 2)
            {
                for (int x = n_CM + 1; x <= n_CM + n_wx; x++)
                {
                    if (pos_w == 0 && ScaleFactor[y][n_CM + 1] != null)
                    {
                        pos_w++;
                        break;
                    }
                    else if (pos_w != 0 && ScaleFactor[y][n_CM + 1] != null && index_w < Coef_W.Count())
                    {
                        if (pos_w + 1 <= n_wx)
                        {
                            ScaleFactor[y][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                            ScaleFactor[y + 1][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                            pos_w++;
                            break;
                        }
                        else
                        {
                            ScaleFactor[y][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                            ScaleFactor[y + 1][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                            pos_w = 0;
                            if (index_w + 1 == Coef_W.Count())
                            {
                                index_w = 0;
                                pos_w = 1;
                            }
                            else
                            {
                                index_w++;
                            }

                            break;
                        }

                    }
                    else if (pos_w <= n_wx && pos_w != 0 && ScaleFactor[y][n_CM + 1] == null && index_w < Coef_W.Count())
                    {
                        ScaleFactor[y][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                        ScaleFactor[y + 1][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                    }

                }
            }

            int index_s = 0;
            if (n_sx != 0)
            {
                for (int y = 0; y < numero_combinaciones; y = y + 2)
                {
                    for (int x = n_CM + n_wx + 1; x <= n_CM + n_wx + n_sx; x++)
                    {
                        if (index_s == 0 && ScaleFactor[y][n_CM + 1] != null && ScaleFactor[y][n_CM + n_wx + 1] != null)
                        {
                            index_s++;
                            break;
                        }
                        else if (index_s > 0 && index_s < Coef_S.Count() && ScaleFactor[y][n_CM + 1] != null && ScaleFactor[y][n_CM + n_wx + 1] != null)
                        {
                            ScaleFactor[y][n_CM + n_wx + 1 + 1] = Coef_S[index_s - 1].ToString();
                            ScaleFactor[y + 1][n_CM + n_wx + 1 + 1] = Coef_S[index_s - 1].ToString();
                            index_s++;
                            break;
                        }
                        else if (index_s > 0 && index_s - 1 < Coef_S.Count())
                        {
                            ScaleFactor[y][n_CM + n_wx + 1 + 1] = Coef_S[index_s - 1].ToString();
                            ScaleFactor[y + 1][n_CM + n_wx + 1 + 1] = Coef_S[index_s - 1].ToString();
                        }

                    }
                }
            }

            double[] Coef_sismo_x = { -0.3, 0.3, -1, 1 };
            double[] Coef_sismo_y = { -0.3, 0.3, -1, 1 };

            int numero_combinaciones_sismo = 8 * (n_CM + 1);

            int numeromatriz_coef_x = 0;
            int numeromatriz_coef_y = 0;

            //Combinaciones sismo, se añaden de abajo arriba 
            int cm = n_CM;
            if (check_sismo)
            {
                for (int i = 0; i < numero_combinaciones_sismo; i++)
                {
                    ScaleFactor[numero_combinaciones - i - 1][0] = "1";

                    numeromatriz_coef_y = 0;

                    if (Math.Abs(Coef_sismo_x[numeromatriz_coef_x]) - Math.Abs(Coef_sismo_y[numeromatriz_coef_y]) == 0)
                    {
                        while (Math.Abs(Coef_sismo_x[numeromatriz_coef_x]) - Math.Abs(Coef_sismo_y[numeromatriz_coef_y]) == 0)
                        {
                            numeromatriz_coef_y = numeromatriz_coef_y + 1;
                        }

                    }

                    for (int cm_n = 1; cm_n <= n_CM; cm_n++)
                    {
                        if (cm == cm_n)
                        {
                            ScaleFactor[numero_combinaciones - i - 1][cm_n + 1] = "1";

                        }
                        else
                        {
                            ScaleFactor[numero_combinaciones - i - 1][cm_n + 1] = "";
                        }
                    }

                    for (int w_n = 1; w_n <= n_wx; w_n++)
                    {
                        ScaleFactor[numero_combinaciones - i - 1][n_CM + w_n + 1] = "";
                    }

                    ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 1] = "";


                    ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 1 + 1] = Coef_sismo_x[numeromatriz_coef_x].ToString();
                    ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1] = Coef_sismo_y[numeromatriz_coef_y].ToString();

                    if (cm == 0)
                    {
                        cm = n_CM + 1;
                        numeromatriz_coef_x = numeromatriz_coef_x + 1;
                        if (numeromatriz_coef_x == Coef_sismo_x.Count())
                        {
                            numeromatriz_coef_x = 0;
                        }
                    }

                    cm = cm - 1;
                }

                for (int i = 0; i < (numero_combinaciones_sismo / 2); i++)
                {
                    double num = Convert.ToDouble(ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1]);
                    ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1] = (-num).ToString();
                }
            }

            ConvertEmptyToZero(ScaleFactor);

            return ScaleFactor;
        }

        public static string[][] combinacionesfijaSLS(int n_CM, bool W1_Press, bool W2_Suc, bool W3_90, bool W4_270, bool S1, int h_s, bool check_sismo)
        {
            /* 
            -> PP = {1 ; 1.3};
            -> cm = {1.3};
            -> W1; W2; W3; W4  = {1.5 ; 0.9};
            -> S1 = {1.5};
            */

            double[] Coef_pp = { 1 };

            double[] Coef_cm = { 1 };

            if (n_CM == 8 && check_sismo)
            {
                Coef_cm = [1];
            }

            double[] Coef_W = { 1, 0.6 };

            double phi = 0;
            switch (h_s)
            {
                case < 1000:
                    phi = 0.5;
                    break;
                case > 1000:
                    phi = 0.7;
                    break;
            }
            double[] Coef_S = { 1, 1 * phi };

            bool[] wx = [W1_Press, W2_Suc, W3_90, W4_270];
            bool[] sx = [S1];

            int n_wx = 0;
            int n_sx = 0;

            foreach (bool viento in wx) { if (viento) { n_wx++; } }
            foreach (bool nieve in sx) { if (nieve) { n_sx++; } }

            //int numero_combinaciones = (Coef_pp.Count() * (1 + n_wx + n_sx)) + (Coef_pp.Count() * n_CM * Coef_cm.Count() * n_wx * Coef_W.Count() * Coef_S.Count());
            int numero_combinaciones = 0;

            switch (check_sismo)
            {
                case false:
                    switch (S1) //Sin sismo
                    {
                        case false: //Sin nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 15;
                                    break;
                                case 3:
                                    numero_combinaciones = 20;
                                    break;
                                case 4:
                                    numero_combinaciones = 25;
                                    break;
                                case 5:
                                    numero_combinaciones = 30;
                                    break;
                                case 6:
                                    numero_combinaciones = 35;
                                    break;
                                case 7:
                                    numero_combinaciones = 40;
                                    break;
                                case 8:
                                    numero_combinaciones = 45;
                                    break;
                            }
                            break;

                        case true: //Con nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 42;
                                    break;
                                case 3:
                                    numero_combinaciones = 56;
                                    break;
                                case 4:
                                    numero_combinaciones = 70;
                                    break;
                                case 5:
                                    numero_combinaciones = 84;
                                    break;
                                case 6:
                                    numero_combinaciones = 98;
                                    break;
                                case 7:
                                    numero_combinaciones = 112;
                                    break;
                                case 8:
                                    numero_combinaciones = 126;
                                    break;
                            }
                            break;
                    }
                    break;

                case true:
                    switch (S1) //Con Sismo
                    {
                        case false: //Sin nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 27;
                                    break;
                                case 3:
                                    numero_combinaciones = 36;
                                    break;
                                case 4:
                                    numero_combinaciones = 45;
                                    break;
                                case 5:
                                    numero_combinaciones = 54;
                                    break;
                                case 6:
                                    numero_combinaciones = 63;
                                    break;
                                case 7:
                                    numero_combinaciones = 72;
                                    break;
                                case 8:
                                    numero_combinaciones = 81;
                                    break;
                            }
                            break;

                        case true: //Con nieve
                            switch (n_CM)
                            {
                                case 2:
                                    numero_combinaciones = 54;
                                    break;
                                case 3:
                                    numero_combinaciones = 72;
                                    break;
                                case 4:
                                    numero_combinaciones = 90;
                                    break;
                                case 5:
                                    numero_combinaciones = 108;
                                    break;
                                case 6:
                                    numero_combinaciones = 126;
                                    break;
                                case 7:
                                    numero_combinaciones = 144;
                                    break;
                                case 8:
                                    numero_combinaciones = 162;
                                    break;
                            }
                            break;
                    }
                    break;
            }

            int numero_factores = 2 + n_CM + n_wx + n_sx;

            if (check_sismo)
            {
                numero_factores = numero_factores + 2;
            }
            string[][] ScaleFactor = new string[numero_combinaciones][];

            int pos_cm = 0;

            for (int y = 0; y < numero_combinaciones; y = y + 1)
            {
                ScaleFactor[y] = new string[numero_factores];

                ScaleFactor[y][0] = Coef_pp[0].ToString();
                ScaleFactor[y][1] = Coef_pp[0].ToString();

                if (y > 0)
                {
                    for (int x = 2; x <= n_CM + 1; x++)
                    {
                        if (x - 2 == pos_cm)
                        {
                            ScaleFactor[y][x] = Coef_cm[0].ToString();
                        }
                    }
                    if (pos_cm == n_CM)
                    {
                        pos_cm = -1;
                    }
                    pos_cm++;

                }
            }

            int pos_w = 0;
            int index_w = 0;

            for (int y = 0; y < numero_combinaciones; y = y + 1)
            {
                for (int x = n_CM + 1; x <= n_CM + n_wx; x++)
                {
                    if (pos_w == 0 && ScaleFactor[y][n_CM + 1] != null)
                    {
                        pos_w++;
                        break;
                    }
                    else if (pos_w != 0 && ScaleFactor[y][n_CM + 1] != null && index_w < Coef_W.Count())
                    {
                        if (pos_w + 1 <= n_wx)
                        {
                            ScaleFactor[y][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                            pos_w++;
                            break;
                        }
                        else
                        {
                            ScaleFactor[y][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                            pos_w = 0;
                            if (index_w + 1 == Coef_W.Count())
                            {
                                index_w = 0;
                                pos_w = 1;
                            }
                            else
                            {
                                index_w++;
                            }

                            break;
                        }

                    }
                    else if (pos_w <= n_wx && pos_w != 0 && ScaleFactor[y][n_CM + 1] == null && index_w < Coef_W.Count())
                    {
                        ScaleFactor[y][pos_w + n_CM + 1] = Coef_W[index_w].ToString();
                    }

                }
            }

            int index_s = 0;
            if (n_sx != 0)
            {
                for (int y = 0; y < numero_combinaciones; y = y + 1)
                {
                    for (int x = n_CM + n_wx + 1; x <= n_CM + n_wx + n_sx; x++)
                    {
                        if (index_s == 0 && ScaleFactor[y][n_CM + 1] != null && ScaleFactor[y][n_CM + n_wx + 1] != null)
                        {
                            index_s++;
                            break;
                        }
                        else if (index_s > 0 && index_s < Coef_S.Count() && ScaleFactor[y][n_CM + 1] != null && ScaleFactor[y][n_CM + n_wx + 1] != null)
                        {
                            ScaleFactor[y][n_CM + n_wx + 1 + 1] = Coef_S[index_s - 1].ToString();
                            index_s++;
                            break;
                        }
                        else if (index_s > 0 && index_s - 1 < Coef_S.Count())
                        {
                            ScaleFactor[y][n_CM + n_wx + 1 + 1] = Coef_S[index_s - 1].ToString();
                        }

                    }
                }
            }

            double[] Coef_sismo_x = { -0.1 };
            double[] Coef_sismo_y = { -0.1 };

            int numero_combinaciones_sismo = 8 * (n_CM + 1);

            int numeromatriz_coef_x = 0;
            int numeromatriz_coef_y = 0;

            //Combinaciones sismo, se añaden de abajo arriba 
            int cm = n_CM;
            if (check_sismo)
            {
                for (int i = 0; i < numero_combinaciones_sismo; i++)
                {
                    numeromatriz_coef_y = 0;

                    if (Coef_sismo_x.Length == 1 || Coef_sismo_y.Length == 1)
                    {

                        for (int w_n = 1; w_n <= n_wx; w_n++)
                        {
                            ScaleFactor[numero_combinaciones - i - 1][n_CM + w_n + 1] = "";
                        }

                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 1] = "";

                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 1 + 1] = Coef_sismo_x[0].ToString();
                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1] = Coef_sismo_y[0].ToString();
                    }
                    else
                    {
                        if (Math.Abs(Coef_sismo_x[numeromatriz_coef_x]) - Math.Abs(Coef_sismo_y[numeromatriz_coef_y]) == 0)
                        {

                            while (Math.Abs(Coef_sismo_x[numeromatriz_coef_x]) - Math.Abs(Coef_sismo_y[numeromatriz_coef_y]) == 0)
                            {
                                numeromatriz_coef_y = numeromatriz_coef_y + 1;
                            }

                        }

                        for (int cm_n = 1; cm_n <= n_CM; cm_n++)
                        {
                            if (cm == cm_n)
                            {
                                ScaleFactor[numero_combinaciones - i - 1][cm_n + 1] = "1";

                            }
                            else
                            {
                                ScaleFactor[numero_combinaciones - i - 1][cm_n + 1] = "";
                            }
                        }

                        for (int w_n = 1; w_n <= n_wx; w_n++)
                        {
                            ScaleFactor[numero_combinaciones - i - 1][n_CM + w_n + 1] = "";
                        }

                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 1] = "";


                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 1 + 1] = Coef_sismo_x[numeromatriz_coef_x].ToString();
                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1] = Coef_sismo_y[numeromatriz_coef_y].ToString();

                        if (cm == 0)
                        {
                            cm = n_CM + 1;
                            numeromatriz_coef_x = numeromatriz_coef_x + 1;
                            if (numeromatriz_coef_x == Coef_sismo_x.Count())
                            {
                                numeromatriz_coef_x = 0;
                            }
                        }

                        cm = cm - 1;
                    }
                }

                if (Coef_sismo_x.Length == 1 || Coef_sismo_y.Length == 1)
                {
                    for (int i = 0; i < (numero_combinaciones_sismo / 2); i++)
                    {

                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2] = "";
                    }

                    for (int i = numero_combinaciones_sismo / 2; i < numero_combinaciones_sismo; i++)
                    {

                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1] = "";
                    }
                }
                else
                {
                    for (int i = 0; i < (numero_combinaciones_sismo / 2); i++)
                    {
                        double num = Convert.ToDouble(ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1]);
                        ScaleFactor[numero_combinaciones - i - 1][n_CM + n_wx + 1 + 2 + 1] = (-num).ToString();
                    }
                }
            }

            ConvertEmptyToZero(ScaleFactor);

            return ScaleFactor;
        }

        static void ConvertEmptyToZero(string[][] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array[i].Length; j++)
                {
                    if (string.IsNullOrEmpty(array[i][j]))
                    {
                        array[i][j] = "0";
                    }
                }
            }
        }

        public static double puntomedio(double primer_punto, double segundo_punto)
        {
            double punto_medio;

            double min = Math.Min(primer_punto, segundo_punto);
            double max = Math.Max(primer_punto, segundo_punto);

            punto_medio = min + (Math.Abs(max) + Math.Abs(min)) / 2;

            return punto_medio;
        }

        public static double LongitudEntrePuntos(cSapModel mySapModel, string point1, string point2)
        {
            int ret = 0;

            double[] coord_1 = new double[3];
            double[] coord_2 = new double[3];

            ret = mySapModel.PointObj.GetCoordCartesian(point1, ref coord_1[0], ref coord_1[1], ref coord_1[2]);
            ret = mySapModel.PointObj.GetCoordCartesian(point2, ref coord_2[0], ref coord_2[1], ref coord_2[2]);

            double Longitud = Math.Sqrt(Math.Pow(coord_1[0] - coord_2[0], 2) + Math.Pow(coord_1[1] - coord_2[1], 2) + Math.Pow(coord_1[2] - coord_2[2], 2));

            return Longitud;
        }

    }
}

