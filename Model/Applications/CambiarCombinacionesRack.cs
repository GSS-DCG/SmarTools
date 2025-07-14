using DocumentFormat.OpenXml.Drawing;
using ModernUI.View;
using OfficeOpenXml;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.Model.Repository;
using SmarTools.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SmarTools.Model.Applications
{
    internal class CambiarCombinacionesRack
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static string ruta = @"Z:\300Logos\04 Combinaciones\Coeficientes.xlsx";

        public static void GenerarCombinaciones(CambiarCombinacionesRackAPP vista)
        {
            var loadingWindow = new Status();
            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                //Obtenemos los casos de carga
                List<string> LoadPattern = new List<string>()
                {
                    "DEAD",
                    "PP Paneles",
                    "W1_Press",
                    "W2_Suct",
                    "W3_90º",
                    "W4_270º",
                    "Snow",
                    "Accidental_Snow",
                    "Ex",
                    "Ey",
                };
                List<string> TypeLoad = new List<string>()
                { 
                    "DEAD",
                    "DEAD",
                    "WIND",
                    "WIND",
                    "WIND",
                    "WIND",
                    "SNOW",
                    "SNOW",
                    "QUAKE",
                    "QUAKE",
                };
                List <bool> aplicarCasos = new List<bool>()
                {
                    vista.Aplicar_Dead.IsChecked==true,
                    vista.Aplicar_PPaneles.IsChecked==true,
                    vista.Aplicar_Presion.IsChecked==true,
                    vista.Aplicar_Succion.IsChecked==true,
                    vista.Aplicar_Lateral_90.IsChecked==true,
                    vista.Aplicar_Lateral_270.IsChecked==true,
                    vista.Aplicar_Nieve.IsChecked==true,
                    vista.Aplicar_NieveAccidental.IsChecked==true,
                    vista.Aplicar_SismoX.IsChecked==true,
                    vista.Aplicar_SismoY.IsChecked==true,
                };
                Dictionary<string, string> CasosSeleccionados = new Dictionary<string, string>();
                for (int i = 0;i<LoadPattern.Count;i++)
                {
                    if (aplicarCasos[i])
                    {
                        CasosSeleccionados[LoadPattern[i]] = TypeLoad[i];
                    }
                }

                //Obtenemos los coeficientes
                var normativa = (vista.Normativa.SelectedItem as ComboBoxItem)?.Content?.ToString();
                List<string> coef=Coeficientes(vista,normativa).Select(x=>x.Item2.Text).ToList();

                //Creamos Load Patterns y Load Cases
                Sap2000CreateLoadPattern(CasosSeleccionados.Keys.ToList(), CasosSeleccionados.Values.ToList());
                Sap2000CreateLoadCases(CasosSeleccionados.Keys.ToList(), CasosSeleccionados.Values.ToList());


                /// La combinacion de nieve accidental tiene que llamarse "Accidental_Snow"
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

        public static void AplicarCombinaciones(CambiarCombinacionesRackAPP vista)
        {

        }

        public static List<(double,TextBox)> Coeficientes(CambiarCombinacionesRackAPP vista, string normativa)
        {
            using (ExcelPackage package = new ExcelPackage(ruta))
            {
                //Eurocódigo
                var Eurocodigo = new List<(double valor, TextBox caja)>
                {
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B2"),vista.Permanente_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C2"),vista.Permanente_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","D2"),vista.Permanente_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","E2"),vista.Permanente_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B3"),vista.Permanente_NoCte_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C3"),vista.Permanente_NoCte_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","D3"),vista.Permanente_NoCte_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","E3"),vista.Permanente_NoCte_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B4"),vista.Variable_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C4"),vista.Variable_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","D4"),vista.Variable_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","E4"),vista.Variable_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B5"),vista.Accidental_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C5"),vista.Accidental_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","D5"),vista.Accidental_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","E5"),vista.Accidental_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B7"),vista.Psi0_Mas1000),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C7"),vista.Psi1_Mas1000),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","D7"),vista.Psi2_Mas1000),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B8"),vista.Psi0_Menos1000),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C8"),vista.Psi1_Menos1000),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","D8"),vista.Psi2_Menos1000),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B9"),vista.Psi0_Viento),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C9"),vista.Psi1_Viento),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","D9"),vista.Psi2_Viento),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B11"),vista.Permanente_Favorable_SLS),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C11"),vista.Permanente_Desfavorable_SLS),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B12"),vista.Permanente_NoCte_Favorable_SLS),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C12"),vista.Permanente_NoCte_Desfavorable_SLS),

                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B13"),vista.Variable_Favorable_SLS),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C13"),vista.Variable_Desfavorable_SLS),
                };

                //NTC-2018
                var NTC2018 = new List<(double valor, TextBox caja)>
                {
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B2"),vista.Permanente_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C2"),vista.Permanente_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","D2"),vista.Permanente_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","E2"),vista.Permanente_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B3"),vista.Permanente_NoCte_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C3"),vista.Permanente_NoCte_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","D3"),vista.Permanente_NoCte_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","E3"),vista.Permanente_NoCte_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B4"),vista.Variable_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C4"),vista.Variable_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","D4"),vista.Variable_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","E4"),vista.Variable_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B5"),vista.Accidental_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C5"),vista.Accidental_Persistente_Desfavorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","D5"),vista.Accidental_Accidental_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","E5"),vista.Accidental_Accidental_Desfavorable),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B7"),vista.Psi0_Mas1000),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C7"),vista.Psi1_Mas1000),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","D7"),vista.Psi2_Mas1000),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B8"),vista.Psi0_Menos1000),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C8"),vista.Psi1_Menos1000),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","D8"),vista.Psi2_Menos1000),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B9"),vista.Psi0_Viento),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C9"),vista.Psi1_Viento),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","D9"),vista.Psi2_Viento),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B11"),vista.Permanente_Favorable_SLS),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C11"),vista.Permanente_Desfavorable_SLS),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B12"),vista.Permanente_NoCte_Favorable_SLS),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C12"),vista.Permanente_NoCte_Desfavorable_SLS),

                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","B13"),vista.Variable_Favorable_SLS),
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C13"),vista.Variable_Desfavorable_SLS),
                };

                //ASCE7-05
                var ASCE7_05 = new List<(double valor, TextBox caja)>
                {
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B1"),vista.Gamma1),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B2"),vista.Gamma2),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B3"),vista.Gamma3),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B4"),vista.Gamma4),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B5"),vista.Gamma5),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B6"),vista.Gamma6),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B7"),vista.Gamma7),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B8"),vista.Gamma8),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B9"),vista.Gamma9),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B10"),vista.Gamma10),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B11"),vista.Gamma11),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B12"),vista.Gamma12),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B13"),vista.Gamma13),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B14"),vista.Gamma14),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B15"),vista.Gamma15),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B16"),vista.Gamma16),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B17"),vista.Gamma17),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B18"),vista.Gamma18),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B19"),vista.Gamma19),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B20"),vista.Gamma20),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B21"),vista.Gamma21),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B22"),vista.Gamma22),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B23"),vista.Gamma23),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B24"),vista.Gamma24),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B25"),vista.Gamma25),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B26"),vista.Gamma26),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B27"),vista.Gamma27),
                };

                //ASCE7-05
                var ASCE7_16 = new List<(double valor, TextBox caja)>
                {
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B1"),vista.Gamma1),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B2"),vista.Gamma2),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B3"),vista.Gamma3),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B4"),vista.Gamma4),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B5"),vista.Gamma5),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B6"),vista.Gamma6),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B7"),vista.Gamma7),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B8"),vista.Gamma8),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B9"),vista.Gamma9),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B10"),vista.Gamma10),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B11"),vista.Gamma11),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B12"),vista.Gamma12),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B13"),vista.Gamma13),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B14"),vista.Gamma14),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B15"),vista.Gamma15),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B16"),vista.Gamma16),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B17"),vista.Gamma17),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B18"),vista.Gamma18),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B19"),vista.Gamma19),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B20"),vista.Gamma20),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B21"),vista.Gamma21),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B22"),vista.Gamma22),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B23"),vista.Gamma23),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B24"),vista.Gamma24),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B25"),vista.Gamma25),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B26"),vista.Gamma26),
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B27"),vista.Gamma27),
                };

                //Selección según la normativa
                switch (normativa)
                {
                    case "Eurocódigo":
                        return Eurocodigo;
                    case "NTC-2018":
                        return NTC2018;
                    case "ASCE7-05":
                        return ASCE7_05;
                    case "ASCE7-16":
                        return ASCE7_16;
                    default:
                        return null;
                }
            }
        }

        //Crear Patrones de Carga
        public static void Sap2000CreateLoadPattern(List<string> LoadPattern, List<string> TypeLoad)
        {
            int ret = 0;
            //Creamos los patrones de carga solo si coinciden el indice de las dos listas
            eLoadPatternType LoadPatternType = eLoadPatternType.Dead; //Tipo de patron de carga que queremos crear
            if (LoadPattern.Count() == TypeLoad.Count())
            {
                for (int i = 0; i < LoadPattern.Count(); i++)
                {
                    if (LoadPattern[i] != "DEAD") //Si no es el caso DEAD que siempre está por defecto
                    {
                        switch (TypeLoad[i])
                        {
                            case "DEAD":
                                LoadPatternType = eLoadPatternType.Dead;
                                break;
                            case "LIVE":
                                LoadPatternType = eLoadPatternType.Live;
                                break;
                            case "WIND":
                                LoadPatternType = eLoadPatternType.Wind;
                                break;
                            case "SNOW":
                                LoadPatternType = eLoadPatternType.Snow;
                                break;
                            case "QUAKE":
                                LoadPatternType = eLoadPatternType.Quake;
                                break;
                            default:
                                break;
                        }
                        ret = mySapModel.LoadPatterns.Add(LoadPattern[i], LoadPatternType);
                    }
                }
            }
        }

        //Crear Casos de Carga
        public static void Sap2000CreateLoadCases(List<string> LoadPattern, List<string> TypeLoad, string SpectreFunction = "", double ScaleFactor = 0, bool listarCombinaciones = false /*Variable que solo se emplea si queremos listar las combinaciones*/)
        {
            int ret = 0;
            bool casoSismicoEx = true; //fijamos que el primer casos sísmico es Ex
                                       //Creamos un caso de carga por cada patron de carga
            if (LoadPattern.Count() == TypeLoad.Count())
            {
                for (int i = 0; i < LoadPattern.Count(); i++)
                {
                    if (TypeLoad[i] != "QUAKE" || listarCombinaciones) //Si no es un caso sísmico o no queremos listar las combinaciones
                    {
                        ret = mySapModel.LoadCases.StaticLinear.SetCase(LoadPattern[i]);
                        string[] LoadType = { "load" };
                        string[] LoadName = { LoadPattern[i] };
                        double[] SF = { 1d };
                        ret = mySapModel.LoadCases.StaticLinear.SetLoads(LoadPattern[i], 1, ref LoadType, ref LoadName, ref SF);
                    }
                    else //Para los casos sísmicos Ex y Ey
                    {
                        //Creamos el caso sísmico. Espéctro de respuesta
                        ret = mySapModel.LoadCases.ResponseSpectrum.SetCase(LoadPattern[i]);
                        const int NumberLoads = 1; //Ex o Ey
                        if (casoSismicoEx)
                        {
                            string[] LoadName = new string[1] { "U1" };
                            string[] Func = new string[1] { SpectreFunction };
                            double[] SF = new double[1] { ScaleFactor };
                            string[] CSys = new string[1] { "GLOBAL" };
                            double[] Ang = new double[1] { 0 };
                            ret = mySapModel.LoadCases.ResponseSpectrum.SetLoads(LoadPattern[i], NumberLoads, ref LoadName, ref Func, ref SF, ref CSys, ref Ang);
                            casoSismicoEx = false; //Indicamos que ya se creo el caso Ex                            
                        }
                        else
                        {
                            string[] LoadName = new string[1] { "U2" };
                            string[] Func = new string[1] { SpectreFunction };
                            double[] SF = new double[1] { ScaleFactor };
                            string[] CSys = new string[1] { "GLOBAL" };
                            double[] Ang = new double[1] { 0 };
                            ret = mySapModel.LoadCases.ResponseSpectrum.SetLoads(LoadPattern[i], NumberLoads, ref LoadName, ref Func, ref SF, ref CSys, ref Ang);
                        }
                    }
                }

            }
        }

        //Generar Envolvente de Combinaciones
        public static void Sap2000CreateEnvelopeCombination(bool ULS = true, bool SLS = true)
        {
            int ret = 0;

            if (ULS)
            {
                //Creamos envolvente de combinaciones de ULS
                ret = mySapModel.RespCombo.Add("ULS", 1); //1 = Envelope

                //Obtenemos la lista de combinaciones de carga
                int NumberNames = 0;
                string[] MyName = new string[150];

                //Obtenemos el numero y los patrones de carga
                ret = mySapModel.RespCombo.GetNameList(ref NumberNames, ref MyName);

                //Añadimos las combinaciones ULS a la envolvente
                eCNameType LoadType = eCNameType.LoadCombo;
                for (int i = 0; i < NumberNames; i++)
                {
                    if (MyName[i].Substring(0, 3) == "ULS") //Si es una combinación ULS 
                    {
                        ret = mySapModel.RespCombo.SetCaseList("ULS", ref LoadType, MyName[i], 1.00);
                    }
                }
            }
            if (SLS)
            {
                //Creamos envolvente de combinaciones de ULS
                ret = mySapModel.RespCombo.Add("SLS", 1); //1 = Envelope

                //Obtenemos la lista de combinaciones de carga
                int NumberNames = 0;
                string[] MyName = new string[150];

                //Obtenemos el numero y los patrones de carga
                ret = mySapModel.RespCombo.GetNameList(ref NumberNames, ref MyName);

                //Añadimos las combinaciones SLS a la envolvente
                eCNameType LoadType = eCNameType.LoadCombo;
                for (int i = 0; i < NumberNames; i++)
                {
                    if (MyName[i].Substring(0, 3) == "SLS")
                    {
                        ret = mySapModel.RespCombo.SetCaseList("SLS", ref LoadType, MyName[i], 1.00);
                    }
                }
            }
        }

        //Generador de Combinacion
        public static void Sap2000CreateCombination(string ComboName, List<String> LoadCases, List<double> ScaleFactor)
        {
            int ret = 0;
            //Creamos la combinación de carga
            ret = mySapModel.RespCombo.Add(ComboName, 0); //Combinación Lineal

            //Añadimos los casos de carga y mayoración de los mismos
            eCNameType LoadType = eCNameType.LoadCase;
            for (int i = 0; i < LoadCases.Count(); i++)
            {
                ret = mySapModel.RespCombo.SetCaseList(ComboName, ref LoadType, LoadCases[i], ScaleFactor[i]);
            }
        }
    }
}
