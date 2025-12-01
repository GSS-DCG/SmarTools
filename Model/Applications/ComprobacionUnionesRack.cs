using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomDocumentInformationPanel;
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
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static SmarTools.Model.Applications.ItaliaNTC2018;

namespace SmarTools.Model.Applications
{
    public class Resultados
    {
        public string UNION { get; set; }
        public string ELEMENTO { get; set; }
        public string ESPESOR { get; set; }
        public string MATERIAL { get; set; }
        public string AXIL { get; set; }
        public string Vz { get; set; }
        public string RESULTANTE { get; set; }
        public string MAXADM { get; set; }
        public string CHECK { get; set; }
    }

    public class ResultEjion
    {
        public string CASO { get; set; }
        public string VzCorrea { get; set; }
        public string Ejion { get; set; }
        public string Espesor { get; set; }
        public string Material { get; set; }
        public string MaxAdm { get; set; }
        public string RATIO { get; set; }
    }

    public class ResultPlacas
    {
        public string UNION { get; set; }
        public string ELEMENTO { get; set; }
        public string R { get; set; }
        public string MaxAdm { get; set; }
        public string Espesor { get; set; }
        public string Material { get; set; }
        public string CODIGO { get; set; }
        public string RATIO { get; set; }
    }

    class ComprobacionUnionesRack
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static string ruta = @"Z:\300SmarTools\03 Uniones\Uniones RackR3_" + MainView.Globales._revisionUnionesRack + ".xlsx";

        public static void ComprobarUniones(ComprobacionUnionesRackAPP vista)
        {
            var loadingWindow = new Status();

            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();
                SAP.AnalysisSubclass.RunModel(mySapModel);

                //Limpiar tablas
                vista.TablaUniones.ItemsSource = null;
                vista.TablaEjion.ItemsSource = null;

                mySapModel.SetPresentUnits(eUnits.kN_m_C);

                //Crear listas compartidas
                List<Resultados> resultados = new List<Resultados>();
                List<ResultEjion> resultadosEjion = new List<ResultEjion>();
                List<ResultPlacas> resultadosPlacas = new List<ResultPlacas>();

                //Datos uniones
                Dictionary<string, double[]> uniones_perfiles = CargarUniones("Perfiles");
                Dictionary<string, double[]> uniones_ejiones = CargarUniones("Ejiones");
                Dictionary<string, double[]> uniones_placas = CargarUniones("Placas");
                string[] ejiones = ObtenerEncabezadosArray(ruta, 2);

                //Comprobar uniones
                UnionVigaCorrea(vista, resultados,resultadosEjion,uniones_perfiles,uniones_ejiones,ejiones);
                UnionVigaPilar(vista, resultados,uniones_perfiles);
                UnionPilarDiagonal(vista, resultados,resultadosPlacas, uniones_perfiles,uniones_placas);
                UnionVigaDiagonal(vista, resultados,resultadosPlacas, uniones_perfiles, uniones_placas);

                //Asignar todos los resultados a las tablas
                vista.TablaUniones.ItemsSource = resultados;
                vista.TablaEjion.ItemsSource = resultadosEjion;
                vista.TablaPlacas.ItemsSource = resultadosPlacas;
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

        public static void UnionVigaCorrea(ComprobacionUnionesRackAPP vista, List<Resultados> resultados, List<ResultEjion> resultadosEjion, Dictionary<string, double[]> uniones_perfiles, Dictionary<string, double[]> uniones_ejiones,string[] ejiones)
        {
            double R=RVigaCorrea(vista, out double Nmax, out double Vymax);
            if (R == 0) return;
            
            mySapModel.SelectObj.ClearSelection();

            int NumberItems = 0;
            int[] objectType = new int[1];
            string[] objectName = new string[1];
            string PropName = "";
            string SAuto = "";

            //Datos Correa
            mySapModel.FrameObj.SetSelected("03 Correas",true,eItemType.Group);
            mySapModel.SelectObj.GetSelected(ref NumberItems,ref objectType,ref objectName);
            mySapModel.FrameObj.GetSection(objectName[0], ref PropName, ref SAuto);

            double espesor_correa = SAP.DesignSubclass.ObtenerEspesor(PropName);
            string material_correa = SAP.DesignSubclass.ObtenerMaterial(PropName);
            int altura = SAP.DesignSubclass.ObtenerAlturaC(PropName);
            string ejion = "E80";

            if (altura == 80 || altura == 90) ejion = "E80";
            else if (altura == 100 || altura == 110) ejion = "E100";
            else if (altura == 120 || altura == 125 || altura == 135) ejion = "E120";
            else if (altura == 150 || altura == 175 || altura == 195 || altura == 200) ejion = "E150";

            string comb_correa = espesor_correa.ToString("F1")+"_"+ material_correa;
            double Apr = 0;

            //Datos viga
            mySapModel.SelectObj.ClearSelection();
            mySapModel.FrameObj.GetSection("Beam_1", ref PropName, ref SAuto);

            double espesor_viga=SAP.DesignSubclass.ObtenerEspesor(PropName);
            string material_viga = SAP.DesignSubclass.ObtenerMaterial(PropName);
            string comb_viga=espesor_viga.ToString("F1") +"_"+ material_viga;

            double[] esf_correa = uniones_perfiles[comb_correa];
            double[] esf_viga = uniones_perfiles[comb_viga];
            double[] ejiones_presion = uniones_ejiones[ejion+"_P"];
            double[] ejiones_succion = uniones_ejiones[ejion + "_S"];

            //Esfuerzos máximos del modelo: Presión para correa y Presión-Succión para Ejión
            double presion = PresSuccCorrea(vista)[0];
            double succion = PresSuccCorrea(vista)[1];

            //Buscamos el ejión que cumpla
            string ejionSeleccionado = SeleccionarEjion(ejiones,ejiones_presion,ejiones_succion,presion,succion, out double Padm, out double Sadm);
            string espesorEjion = ejionSeleccionado.Split('_')[0];
            double.TryParse(espesorEjion,out double EE);
            EE = EE * 10;
            string materialEjion = ejionSeleccionado.Split('_')[1];
            string MM = "";
            switch(materialEjion)
            {
                case "S355JR":
                    MM = "S3";
                    break;

                case "S350GD":
                    MM = "M3";
                    break;

                case "S420MC":
                    MM = "S4";
                    break;

                case "S420GD":
                    MM = "M4";
                    break;

                case "S460MC":
                    MM = "S5";
                    break;

                case "S450GD":
                    MM = "M5";
                    break;
            }
            string codigoEjion = "";
            switch(ejion)
            {
                case "E80":
                    codigoEjion = "FXESB"+EE+"080"+MM+"AAA00";
                    break;

                case "E100":
                    codigoEjion = "FXESB"+EE+"100"+MM+"AAA00";
                    break;

                case "E120":
                    codigoEjion = "FXESB"+EE+"120"+MM+"AAA00";
                    break;

                case "E150":
                    codigoEjion = "FXESB"+EE+"150"+MM+"AAA00";
                    break;
            }
            vista.CodigoEjion.Content = codigoEjion;

            //Esfuerzo máximo admisible de unión Viga-correa:Correa. CV_C, posición 7
            double Vmax_correa = esf_correa[7];
            double ratio=presion/Vmax_correa*100;
            string check = (ratio > 100) ? "No Cumple" : "Cumple";

            //Esfuerzo máximo admisible de unión Viga-correa: Viga. CV_V, posición 8
            double Vmax_viga=esf_viga[8];
            double ratio2=Math.Sqrt(Math.Pow(Vymax,2)+Math.Pow(Nmax,2))/Vmax_viga*100;
            string check2 = (ratio > 100) ? "No Cumple" : "Cumple";

            //Añadir resultados a la tabla
            resultados.Add(new Resultados {UNION="Unión Viga-Correa", ELEMENTO="Correa", ESPESOR = espesor_correa.ToString("F1"), MATERIAL= material_correa, AXIL="-", Vz=presion.ToString("F3"), RESULTANTE=presion.ToString("F3"),MAXADM=Vmax_correa.ToString("F3"),CHECK=check });
            resultados.Add(new Resultados { UNION = "Unión Viga-Correa", ELEMENTO = "Viga", ESPESOR = espesor_viga.ToString("F1"), MATERIAL = material_viga, AXIL = Nmax.ToString("F3"), Vz = Vymax.ToString("F3"), RESULTANTE = Math.Sqrt(Math.Pow(Vymax, 2) + Math.Pow(Nmax, 2)).ToString("F3"), MAXADM = Vmax_viga.ToString("F3"), CHECK = check2 });
            resultadosEjion.Add(new ResultEjion {CASO = "Presión", VzCorrea = presion.ToString("F3"), Ejion = ejion, Espesor = espesorEjion, Material = materialEjion, MaxAdm = Padm.ToString("F3"), RATIO = (presion/Padm*100).ToString("F1")+"%" });
            resultadosEjion.Add(new ResultEjion { CASO = "Succión", VzCorrea = succion.ToString("F3"), Ejion = ejion, Espesor = espesorEjion, Material = materialEjion, MaxAdm = Sadm.ToString("F3"), RATIO = (succion / Sadm * 100).ToString("F1") + "%" });
        }

        public static void UnionVigaPilar(ComprobacionUnionesRackAPP vista, List<Resultados> resultados, Dictionary<string, double[]> uniones_perfiles)
        {
            //Lista de vigas y propiedades
            string[] vigas = SAP.ElementFinderSubclass.FixedSubclass.ListaVigas(mySapModel);
            string PropName = "";
            string SAuto = "";
            mySapModel.FrameObj.GetSection(vigas[0], ref PropName, ref SAuto);
            double espesor_viga = SAP.DesignSubclass.ObtenerEspesor(PropName);
            string material_viga = SAP.DesignSubclass.ObtenerMaterial(PropName);
            string comb_viga = espesor_viga.ToString("F1") + "_" + material_viga;

            //Esfuerzos máximos admisibles vigas
            double[] esf_vigas = uniones_perfiles[comb_viga];
            double Radm_viga = esf_vigas[0];

            
            if (vista.Monoposte.IsChecked == true)
            {
                //Lista de pilares y propiedades
                string[] pilares = SAP.ElementFinderSubclass.FixedSubclass.ListaPilares(mySapModel);
                
                mySapModel.FrameObj.GetSection(pilares[0],ref PropName,ref SAuto);
                double espesor_pilar = SAP.DesignSubclass.ObtenerEspesor(PropName);
                string material_pilar = SAP.DesignSubclass.ObtenerMaterial(PropName);
                string comb_pilar = espesor_pilar.ToString("F1") + "_" + material_pilar;

                //Esfuerzos máximos admisibles pilares
                double[] esf_pilares = uniones_perfiles[comb_pilar];
                double Radm_pilar= esf_pilares[1];

                //Esfuerzos en el modelo
                RPilares(pilares, out double[] esfuerzosULS, out double[] esfuerzosSLS);
                double R = Math.Sqrt(Math.Pow(esfuerzosULS[0],2) + Math.Pow(esfuerzosULS[1],2));

                double Apr_viga = R / Radm_viga * 100;
                double Apr_pilar = R / Radm_pilar * 100;

                string check_viga = (Apr_viga < 100) ? "Cumple" : "No Cumple";
                string check_pilar = (Apr_pilar < 100) ? "Cumple" : "No Cumple";

                //Añadimos los resultados a la tabla
                resultados.Add(new Resultados { UNION = "Unión Viga-Pilar", ELEMENTO = "Viga", ESPESOR = espesor_viga.ToString("F1"), MATERIAL = material_viga, AXIL = esfuerzosULS[0].ToString("F3"), Vz = esfuerzosULS[1].ToString("F3"), RESULTANTE = R.ToString("F3"), MAXADM = Radm_viga.ToString("F3"), CHECK = check_viga });
                resultados.Add(new Resultados { UNION = "Unión Viga-Pilar", ELEMENTO = "Pilar", ESPESOR = espesor_pilar.ToString("F1"), MATERIAL = material_pilar, AXIL = esfuerzosULS[0].ToString("F3"), Vz = esfuerzosULS[1].ToString("F3"), RESULTANTE = R.ToString("F3"), MAXADM = Radm_pilar.ToString("F3"), CHECK = check_pilar });
            }
            else if (vista.Biposte.IsChecked == true)
            {
                //Lista de pilares y propiedades
                string[] pilaresDelanteros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
                string[] pilaresTraseros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);

                    //Delantero
                mySapModel.FrameObj.GetSection(pilaresDelanteros[0], ref PropName, ref SAuto);
                double espesor_pilarDel = SAP.DesignSubclass.ObtenerEspesor(PropName);
                string material_pilarDel = SAP.DesignSubclass.ObtenerMaterial(PropName);
                string comb_pilarDel = espesor_pilarDel.ToString("F1") + "_" + material_pilarDel;

                    //Trasero
                mySapModel.FrameObj.GetSection(pilaresTraseros[0], ref PropName, ref SAuto);
                double espesor_pilarTras = SAP.DesignSubclass.ObtenerEspesor(PropName);
                string material_pilarTras = SAP.DesignSubclass.ObtenerMaterial(PropName);
                string comb_pilarTras = espesor_pilarTras.ToString("F1") + "_" + material_pilarTras;

                //Esfuerzos máximos admisibles pilares
                double[] esf_pilaresDel = uniones_perfiles[comb_pilarDel];
                double[] esf_pilarTras = uniones_perfiles[comb_pilarTras];
                double Radm_pilarDel = esf_pilaresDel[1];
                double Radm_pilarTras = esf_pilarTras[1];

                //Esfuerzos en el modelo
                RPilares(pilaresDelanteros, out double[] esfuerzosDelanterosULS, out double[] esfuerzosDelanterosSLS);
                RPilares(pilaresTraseros, out double[] esfuerzosTraserosULS, out double[] esfuerzosTraserosSLS);
                double Rdel = Math.Sqrt(Math.Pow(esfuerzosDelanterosULS[0], 2) + Math.Pow(esfuerzosDelanterosULS[1],2));
                double Rtras = Math.Sqrt(Math.Pow(esfuerzosTraserosULS[0], 2) + Math.Pow(esfuerzosTraserosULS[1], 2));

                double Apr_viga =Math.Max(Rdel, Rtras)/Radm_viga*100;
                double Apr_pilarDel = Rdel / Radm_pilarDel * 100;
                double Apr_pilarTras = Rtras / Radm_pilarTras * 100;

                string check_viga = (Apr_viga < 100) ? "Cumple" : "No Cumple";
                string check_pilarDel = (Apr_pilarDel < 100) ? "Cumple" : "No Cumple";
                string check_pilarTras = (Apr_pilarTras < 100) ? "Cumple" : "No Cumple";

                //Para los esfuerzos de la viga, buscamos la mayor R entre el pilar delantero y el trasero
                double[] esfuerzos_viga = (Rdel > Rtras) ? esfuerzosDelanterosULS : esfuerzosTraserosULS;

                //Añadimos los resultados a la tabla
                resultados.Add(new Resultados { UNION = "Unión Viga-Pilar", ELEMENTO = "Viga", ESPESOR = espesor_viga.ToString("F1"), MATERIAL = material_viga, AXIL = esfuerzos_viga[0].ToString("F3"), Vz = esfuerzos_viga[1].ToString("F3"), RESULTANTE = Math.Max(Rdel,Rtras).ToString("F3"), MAXADM = Radm_viga.ToString("F3"), CHECK = check_viga });
                resultados.Add(new Resultados { UNION = "Unión Viga-Pilar", ELEMENTO = "Pilar Delantero", ESPESOR = espesor_pilarDel.ToString("F1"), MATERIAL = material_pilarDel, AXIL = esfuerzosDelanterosULS[0].ToString("F3"), Vz = esfuerzosDelanterosULS[1].ToString("F3"), RESULTANTE = Rdel.ToString("F3"), MAXADM = Radm_pilarDel.ToString("F3"), CHECK = check_pilarDel });
                resultados.Add(new Resultados { UNION = "Unión Viga-Pilar", ELEMENTO = "Pilar Trasero", ESPESOR = espesor_pilarTras.ToString("F1"), MATERIAL = material_pilarTras, AXIL = esfuerzosTraserosULS[0].ToString("F3"), Vz = esfuerzosTraserosULS[1].ToString("F3"), RESULTANTE = Rtras.ToString("F3"), MAXADM = Radm_pilarTras.ToString("F3"), CHECK = check_pilarTras });
            }
        }

        public static void UnionPilarDiagonal(ComprobacionUnionesRackAPP vista, List<Resultados> resultados, List<ResultPlacas> resultadosPlacas, Dictionary<string, double[]> uniones_perfiles, Dictionary<string, double[]> uniones_placas)
        {
            //Esfuerzos en diagonal
            RDiagonal(vista, out double[] RDiagdel, out double[] RDiagTras);
            double RDdel =Math.Sqrt(Math.Pow(RDiagdel[0],2) + Math.Pow(RDiagdel[1],2));
            
            if (vista.Monoposte.IsChecked == true)
            {
                //Perfiles y propiedades
                string[] pilares = SAP.ElementFinderSubclass.FixedSubclass.ListaPilares(mySapModel);
                double espesor_pilar = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, pilares));
                string material_pilar = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName(mySapModel, pilares));
                (string[] diagDel, string[] diagTras) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                double espesor_diagDel = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, diagDel));
                string material_diagDel = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName(mySapModel,diagDel));

                string comb_pilar = espesor_pilar.ToString("F1") + "_" + material_pilar;
                string comb_diagDel = espesor_diagDel.ToString("F1") + "_" + material_diagDel;

                //Esfuerzos máximos admisibles
                double[] esf_pilar = uniones_perfiles[comb_pilar];
                double Radm_pilar = esf_pilar[2];
                double[] esf_diagDel = uniones_perfiles[comb_diagDel];
                double Radm_diagDel = esf_diagDel[4];

                //Aprovechamientos 
                double Apr_pilar = RDdel / Radm_pilar * 100;
                double Apr_diagDel = RDdel / Radm_diagDel * 100;
                string check_pilar = (Apr_pilar < 100) ? "Cumple" : "No Cumple";
                string check_diagDel = (Apr_diagDel < 100) ? "Cumple" : "No Cumple";

                if (vista.DosDiagonal.IsChecked == true)
                {
                    double RDTras = Math.Sqrt(Math.Pow(RDiagTras[0], 2) + Math.Pow(RDiagTras[1], 2));

                    //Perfiles y propiedades
                    double espesor_diagTras = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, diagTras));
                    string material_diagTras = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName (mySapModel, diagTras));
                    string comb_diagTras = espesor_diagTras.ToString("F1") + "_" + material_diagTras;

                    //Esfuerzos máximos admisibles
                    double[] esf_diagTras = uniones_perfiles[comb_diagTras];
                    double Radm_diagTras = esf_diagTras[4];
                    Radm_pilar = esf_pilar[3];

                    //Esfuerzos en pilar
                    RPilarArbol(out double Rpilar, out double P, out double V2);

                    //Placa
                    string placaValida = "";
                    string RPpilar = "0";
                    string RPdiagonal = "0";
                    string ratiopilar = "0";
                    string ratiodiagDel = "0";
                    string ratiodiagTras = "0";
                    
                    foreach (var fila in uniones_placas)
                    {
                        string placa = fila.Key;
                        double MaxDiagonal = fila.Value[1];
                        double MaxPilar = fila.Value[0];
                        if(MaxPilar>Rpilar&&MaxDiagonal>Math.Max(RDdel,RDTras))
                        {
                            placaValida = placa;
                            RPpilar = MaxPilar.ToString("F2");
                            RPdiagonal = MaxDiagonal.ToString("F2");
                            ratiopilar = (Rpilar / MaxPilar * 100).ToString("F1") + "%";
                            ratiodiagDel = (RDdel / MaxDiagonal * 100).ToString("F1") + "%";
                            ratiodiagTras = (RDTras / MaxDiagonal * 100).ToString("F1") + "%";
                            break;
                        }
                    }
                    EspesorYMaterialPlaca(placaValida, out string espesor_placa, out string material_placa);

                    //Aprovechamientos
                    Apr_pilar = Rpilar / Radm_pilar * 100;
                    double Apr_diagTras= RDTras/Radm_diagTras * 100;
                    check_pilar = (Apr_pilar < 100) ? "Cumple" : "No Cumple";
                    string check_diagTras = (Apr_diagTras < 100) ? "Cumple" : "No Cumple";
                    resultados.Add(new Resultados { UNION = "Unión Pilar-Diagonal", ELEMENTO = "Pilar", ESPESOR = espesor_pilar.ToString("F1"), MATERIAL = material_pilar, AXIL = P.ToString("F3"), Vz = V2.ToString("F3"), RESULTANTE = Rpilar.ToString("F3"), MAXADM = Radm_pilar.ToString("F3"), CHECK = check_pilar });
                    resultados.Add(new Resultados { UNION = "Unión Pilar-Diagonal", ELEMENTO = "Diagonal Delantera", ESPESOR = espesor_diagDel.ToString("F1"), MATERIAL = material_diagDel, AXIL = RDiagdel[0].ToString("F3"), Vz = RDiagdel[1].ToString("F3"), RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_diagDel.ToString("F3"), CHECK = check_diagDel });
                    resultados.Add(new Resultados { UNION = "Unión Pilar-Diagonal", ELEMENTO = "Diagonal Trasera", ESPESOR = espesor_diagTras.ToString("F1"), MATERIAL = material_diagTras, AXIL = RDiagTras[0].ToString("F3"), Vz = RDiagTras[1].ToString("F3"), RESULTANTE = RDTras.ToString("F3"), MAXADM = Radm_diagTras.ToString("F3"), CHECK = check_diagTras });
                    resultadosPlacas.Add(new ResultPlacas { UNION = "Placa Pilar-Diagonal", ELEMENTO = "Pilar", Espesor = espesor_placa, Material = material_placa, CODIGO = "-", R = Rpilar.ToString("F3"), MaxAdm = RPpilar, RATIO = ratiopilar});
                    resultadosPlacas.Add(new ResultPlacas { UNION = "Placa Pilar-Diagonal", ELEMENTO = "Diagonal Delantera", Espesor = espesor_placa, Material = material_placa, CODIGO = "-", R = RDdel.ToString("F3"), MaxAdm = RPdiagonal, RATIO = ratiodiagDel });
                    resultadosPlacas.Add(new ResultPlacas { UNION = "Placa Pilar-Diagonal", ELEMENTO = "Diagonal Trasera", Espesor = espesor_placa, Material = material_placa, CODIGO = "-", R = RDTras.ToString("F3"), MaxAdm = RPdiagonal, RATIO = ratiodiagTras });

                }
                else if(vista.UnaDiagonal.IsChecked == true)
                {
                    resultados.Add(new Resultados { UNION = "Unión Pilar-Diagonal", ELEMENTO = "Diagonal", ESPESOR = espesor_diagDel.ToString("F1"), MATERIAL = material_diagDel, AXIL = RDiagdel[0].ToString("F3"), Vz = RDiagdel[1].ToString("F3"), RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_diagDel.ToString("F3"), CHECK = check_diagDel });
                    resultados.Add(new Resultados { UNION = "Unión Pilar-Diagonal", ELEMENTO = "Pilar", ESPESOR = espesor_pilar.ToString("F1"), MATERIAL = material_pilar, AXIL = "-", Vz = "-", RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_pilar.ToString("F3"), CHECK = check_pilar });
                }
            }
            else if (vista.Biposte.IsChecked == true)
            {
                if (vista.UnaDiagonal.IsChecked == true)
                {
                    //Perfiles y propiedades
                    (string[] diagDel, string[] diagTras) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                    double espesor_diagDel = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, diagDel));
                    string material_diagDel = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName(mySapModel, diagDel));
                    string[] pilaresTras = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);
                    double espesor_pilarTras = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, pilaresTras));
                    string material_pilarTras = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName(mySapModel, pilaresTras));
                    string comb_diagDel = espesor_diagDel.ToString("F1") + "_" + material_diagDel;
                    string comb_pilarTras = espesor_pilarTras.ToString("F1") + "_" + material_pilarTras;

                    //Esfuerzos máximos admisibles
                    double[] esf_diagDel = uniones_perfiles[comb_diagDel];
                    double Radm_diagDel = esf_diagDel[4];
                    double[] esf_pilarTras = uniones_perfiles[comb_pilarTras];
                    double Radm_pilarTras = esf_pilarTras[2];

                    //Aprovechamientos
                    double Apr_pilarTras = RDdel / Radm_pilarTras * 100;
                    double Apr_diagDel = RDdel / Radm_diagDel * 100;

                    string check_pilarTras = (Apr_pilarTras < 100) ? "Cumple" : "No Cumple";
                    string check_diagDel = (Apr_diagDel < 100) ? "Cumple" : "No Cumple";

                    //Añadimos resultados a la tabla
                    resultados.Add(new Resultados { UNION = "Unión Pilar-Diagonal", ELEMENTO = "Diagonal", ESPESOR = espesor_diagDel.ToString("F1"), MATERIAL = material_diagDel, AXIL = RDiagdel[0].ToString("F3"), Vz = RDiagdel[1].ToString("F3"), RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_diagDel.ToString("F3"), CHECK = check_diagDel });
                    resultados.Add(new Resultados { UNION = "Unión Pilar-Diagonal", ELEMENTO = "Pilar", ESPESOR = espesor_pilarTras.ToString("F1"), MATERIAL = material_pilarTras, AXIL = "-", Vz = "-", RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_pilarTras.ToString("F3"), CHECK = check_pilarTras });
                }
            }
        }

        public static void UnionVigaDiagonal(ComprobacionUnionesRackAPP vista, List<Resultados> resultados, List<ResultPlacas> resultadosPlacas, Dictionary<string, double[]> uniones_perfiles, Dictionary<string, double[]> uniones_placas)
        {
            if (vista.SinDiagonal.IsChecked == false)
            {
                //Lista de elementos y propiedades
                string[] vigas = SAP.ElementFinderSubclass.FixedSubclass.ListaVigas(mySapModel);
                (string[] diagDel, string[] diagTras) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                double espesor_viga = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, vigas));
                string material_viga = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName(mySapModel, vigas));
                double espesor_diagDel = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, diagDel));
                string material_diagDel = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName(mySapModel, diagDel));

                string comb_viga = espesor_viga.ToString("F1") + "_" + material_viga;
                string comb_diagDel = espesor_diagDel.ToString("F1") + "_" + material_diagDel;

                //Esfuerzos máximos admisibles
                double[] esf_vigas = uniones_perfiles[comb_viga];
                double[] esf_diagDel = uniones_perfiles[comb_diagDel];
                double Radm_viga = esf_vigas[5];
                double Radm_diagDel = esf_diagDel[6];

                //Esfuerzos del modelo
                RDiagonal(vista, out double[] RDiagdel, out double[] RDiagTras);
                double RDdel = Math.Sqrt(Math.Pow(RDiagdel[0], 2) + Math.Pow(RDiagdel[1], 2));

                double Apr_viga = RDdel / Radm_viga * 100;
                string check_viga = (Apr_viga < 100) ? "Cumple" : "No Cumple";
                double Apr_diagDel = RDdel / Radm_diagDel * 100;
                string check_diagDel = (Apr_diagDel < 100) ? "Cumple" : "No Cumple";

                //Placa viga-diagonal
                string placa_Ddel="";
                string RPviga="0";
                string RPDiag = "0";
                string RatioDiag = "0";
                string RatioViga = "0";
                foreach (var fila in uniones_placas)
                {
                    string placa = fila.Key;
                    double Rdiagonal = fila.Value[3];
                    double Rviga = fila.Value[2];
                    if(Rdiagonal>RDdel&&Rviga>RDdel)
                    {
                        placa_Ddel = placa;
                        RPDiag = Rdiagonal.ToString("F2");
                        RPviga = Rviga.ToString("F2");
                        RatioDiag = (RDdel / Rdiagonal * 100).ToString("F1");
                        RatioViga = (RDdel / Rviga * 100).ToString("F1");
                        break;
                    }
                }
                EspesorYMaterialPlaca(placa_Ddel, out string espesor_placa, out string material_placa);

                if (vista.UnaDiagonal.IsChecked == true)
                {
                    //Añadimos los resultados a la tabla
                    resultados.Add(new Resultados { UNION = "Unión Viga-Diagonal", ELEMENTO = "Viga", ESPESOR = espesor_viga.ToString("F1"), MATERIAL = material_viga, AXIL = "-", Vz = "-", RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_viga.ToString("F3"), CHECK = check_viga });
                    resultados.Add(new Resultados { UNION = "Unión Viga-Diagonal", ELEMENTO = "Diagonal", ESPESOR = espesor_diagDel.ToString("F1"), MATERIAL = material_diagDel, AXIL = RDiagdel[0].ToString("F3"), Vz = RDiagdel[1].ToString("F3"), RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_diagDel.ToString("F3"), CHECK = check_diagDel });
                    resultadosPlacas.Add(new ResultPlacas {UNION="Placa Viga-Diagonal", ELEMENTO="Diagonal",Espesor=espesor_placa,Material=material_placa,CODIGO="-",R=RDdel.ToString("F3"),MaxAdm=RPDiag,RATIO=RatioDiag + "%" });
                    resultadosPlacas.Add(new ResultPlacas { UNION = "Placa Viga-Diagonal", ELEMENTO = "Viga", Espesor = espesor_placa, Material = material_placa, CODIGO = "-", R = RDdel.ToString("F3"), MaxAdm = RPviga, RATIO = RatioViga + "%" });
                }
                else if (vista.DosDiagonal.IsChecked==true)
                {
                    //Lista de elementos y propiedades
                    double espesor_diagTras = SAP.DesignSubclass.ObtenerEspesor(SAP.DesignSubclass.GetPropName(mySapModel, diagTras));
                    string material_diagTras = SAP.DesignSubclass.ObtenerMaterial(SAP.DesignSubclass.GetPropName(mySapModel, diagTras));
                    string comb_diagTras = espesor_diagTras.ToString("F1") + "_" + material_diagTras;

                    //Esfuerzos máximos admisibles
                    double[] esf_diagTras = uniones_perfiles[comb_diagTras];
                    double Radm_diagTras = esf_diagDel[6];

                    //Esfuerzos del modelo
                    double RDTras = Math.Sqrt(Math.Pow(RDiagTras[0], 2) + Math.Pow(RDiagTras[1], 2));
                    double Rviga = Math.Max(RDdel,RDTras);
                    double Apr_diagTras = RDdel / Radm_diagTras * 100;
                    Apr_viga = Rviga / Radm_viga * 100;
                    string check_diagTras = (Apr_diagTras < 100) ? "Cumple" : "No Cumple";
                    check_viga = (Apr_viga < 100) ? "Cumple" : "No Cumple";
                    string RatioDiagDel = "0";
                    string RatioDiagTras = "0";

                    foreach (var fila in uniones_placas)
                    {
                        string placa = fila.Key;
                        double Rdiagonal = fila.Value[3];
                        double RViga = fila.Value[2];
                        if (Rdiagonal > Rviga && RViga > Rviga)
                        {
                            placa_Ddel = placa;
                            RPDiag = Rdiagonal.ToString("F2");
                            RPviga = RViga.ToString("F2");
                            RatioDiagDel = (RDdel / Rdiagonal * 100).ToString("F1");
                            RatioDiagTras = (RDTras / Rdiagonal * 100).ToString("F1");
                            RatioViga = (Rviga / RViga * 100).ToString("F1");
                            break;
                        }
                    }
                    EspesorYMaterialPlaca(placa_Ddel, out espesor_placa, out material_placa);

                    //Añadimos los resultados a la tabla
                    resultados.Add(new Resultados { UNION = "Unión Viga-Diagonal", ELEMENTO = "Viga", ESPESOR = espesor_viga.ToString("F1"), MATERIAL = material_viga, AXIL = "-", Vz = "-", RESULTANTE = Rviga.ToString("F3"), MAXADM = Radm_viga.ToString("F3"), CHECK = check_viga });
                    resultados.Add(new Resultados { UNION = "Unión Viga-Diagonal", ELEMENTO = "Diagonal Delantera", ESPESOR = espesor_diagDel.ToString("F1"), MATERIAL = material_diagDel, AXIL = RDiagdel[0].ToString("F3"), Vz = RDiagdel[1].ToString("F3"), RESULTANTE = RDdel.ToString("F3"), MAXADM = Radm_diagDel.ToString("F3"), CHECK = check_diagDel });
                    resultados.Add(new Resultados { UNION = "Unión Viga-Diagonal", ELEMENTO = "Diagonal Trasera", ESPESOR = espesor_diagTras.ToString("F1"), MATERIAL = material_diagTras, AXIL = RDiagTras[0].ToString("F3"), Vz = RDiagTras[1].ToString("F3"), RESULTANTE = RDTras.ToString("F3"), MAXADM = Radm_diagTras.ToString("F3"), CHECK = check_diagTras });
                    resultadosPlacas.Add(new ResultPlacas { UNION = "Placa Viga-Diagonal", ELEMENTO = "Diagonal Delantera", Espesor = espesor_placa, Material = material_placa, CODIGO = "-", R = RDdel.ToString("F3"), MaxAdm = RPDiag, RATIO = RatioDiagDel + "%" });
                    resultadosPlacas.Add(new ResultPlacas { UNION = "Placa Viga-Diagonal", ELEMENTO = "Diagonal Trasera", Espesor = espesor_placa, Material = material_placa, CODIGO = "-", R = RDTras.ToString("F3"), MaxAdm = RPDiag, RATIO = RatioDiagTras + "%" });
                    resultadosPlacas.Add(new ResultPlacas { UNION = "Placa Viga-Diagonal", ELEMENTO = "Viga", Espesor = espesor_placa, Material = material_placa, CODIGO = "-", R = Rviga.ToString("F3"), MaxAdm = RPviga, RATIO = RatioViga + "%" });
                }
            }
        }

        public static void RDiagonal(ComprobacionUnionesRackAPP vista, out double[] RDel, out double[] RTras)
        {
            RDel = new double [] { 0, 0, 0, 0, 0, 0 };
            RTras = new double[] { 0, 0, 0, 0, 0, 0 };

            if(vista.SinDiagonal.IsChecked==false)
            {
                (string[] diagDel, string[] diagTras) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);

                RDel = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel, diagDel, "ULS");
                RTras = null;

                if (vista.DosDiagonal.IsChecked == true)
                {
                    RTras = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel, diagTras, "ULS");
                }
            }
        }

        public static void RPilarArbol(out double R, out double saltoP, out double saltoV2)
        {
            //Pilares y diagonales
            string[] pilares = SAP.ElementFinderSubclass.FixedSubclass.ListaPilares(mySapModel);
            (string[] diagDel, string[] diagTras) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);

            //Necesitamos conocer el punto del pilar de unión con la diagonal
            SAP.DesignSubclass.GetFrameEndCoords(mySapModel, pilares[0], out double xi, out double yi, out double zi, out double xj, out double yj, out double zj);
            double ZiPilar = zi;
            SAP.DesignSubclass.GetFrameEndCoords(mySapModel, diagDel[0], out xi, out yi, out zi, out xj, out yj, out zj);
            double ZiDiag = zi;
            double station = ZiDiag - ZiPilar;

            Dictionary<string, (double saltoAxil, double saltoV2, double resultante)> esfuerzos = new();

            //Esfuerzos en los pilares

            foreach (string pilar in pilares)
            {
                SAP.AnalysisSubclass.FrameForces(mySapModel, "ULS", pilar,
                    out double[] P, out double[] V2, out double[] V3,
                    out double[] T, out double[] M2, out double[] M3, out double[] ObjSta, out string[] StepType);

                // Filtrar filas que coinciden con station
                double tolerancia = 0.001;
                var indices = ObjSta.Select((val, idx) => new { val, idx })
                                    .Where(x => Math.Abs(x.val-station)<tolerancia)
                                    .Select(x => x.idx)
                                    .ToList();

                if (indices.Count > 0)
                {
                    // Filtrar por StepType
                    var indicesMax = indices.Where(i => StepType[i].Equals("Max", StringComparison.OrdinalIgnoreCase)).ToList();
                    var indicesMin = indices.Where(i => StepType[i].Equals("Min", StringComparison.OrdinalIgnoreCase)).ToList();

                    // Extraer valores según StepType
                    var valoresPMax = indicesMax.Select(i => P[i]).ToList();
                    var valoresPMin = indicesMin.Select(i => P[i]).ToList();
                    var valoresV2Max = indicesMax.Select(i => V2[i]).ToList();
                    var valoresV2Min = indicesMin.Select(i => V2[i]).ToList();

                    // Calcular saltos entre los dos valores (si hay 2)
                    double saltoPMax = valoresPMax.Count >= 2 ? Math.Abs(valoresPMax[0] - valoresPMax[1]) : 0;
                    double saltoPMin = valoresPMin.Count >= 2 ? Math.Abs(valoresPMin[0] - valoresPMin[1]) : 0;
                    double saltoV2Max = valoresV2Max.Count >= 2 ? Math.Abs(valoresV2Max[0] - valoresV2Max[1]) : 0;
                    double saltoV2Min = valoresV2Min.Count >= 2 ? Math.Abs(valoresV2Min[0] - valoresV2Min[1]) : 0;

                    // Tomar el mayor salto para P y V2
                    saltoP = Math.Max(saltoPMax, saltoPMin);
                    saltoV2 = Math.Max(saltoV2Max, saltoV2Min);

                    // Resultante
                    double resultante = Math.Sqrt(Math.Pow(saltoP, 2) + Math.Pow(saltoV2, 2));

                    esfuerzos[pilar] = (saltoP, saltoV2, resultante);
                }
            }
            // Ahora puedes buscar el pilar con mayor resultante
            var mayor = esfuerzos.OrderByDescending(e => e.Value.resultante).First();
            saltoP=mayor.Value.saltoAxil;
            saltoV2 = mayor.Value.saltoV2;
            R = mayor.Value.resultante;
        }

        public static void RPilares(string[] pilares, out double[] esfuerzosULS, out double[] esfuerzosSLS)
        {
            double z = SAP.AnalysisSubclass.GetFrameLength3D(mySapModel, pilares[0]);

            esfuerzosULS = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel, pilares,"ULS");
            esfuerzosSLS = SAP.AnalysisSubclass.ObtenerMaximosEsfuerzos(mySapModel, pilares, "SLS");
        }

        public static double RVigaCorrea(ComprobacionUnionesRackAPP vista, out double Nmax, out double Vymax)
        {
            Nmax = 0.0;
            Vymax = 0.0;

            //Validaciones básicas del input
            if(string.IsNullOrWhiteSpace(vista.NumCorreas.Text)||!int.TryParse(vista.NumCorreas.Text,out int nCorr)||nCorr<1)
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Intoduce un número de correas válido", TipoIncidencia.Advertencia);
                ventana.ShowDialog();
                return 0.0;
            }

            //Seleccionar el grupo y obtener los objetos seleccionados
            int ret = 0;
            ret |= mySapModel.SelectObj.ClearSelection();
            ret |= mySapModel.FrameObj.SetSelected("03 Correas", true, eItemType.Group);

            int numberItems = 0;
            int[] objectType=new int[1];
            string[] objectName = new string[1];
            ret |= mySapModel.SelectObj.GetSelected(ref numberItems,ref objectType, ref objectName);

            if(ret!=0||numberItems==0||objectName==null||objectName.Length==0)
                return 0.0;

            //Filtrar solo los frameObj que sean Purlins y agrupar por índice Purlin_N
            var regex=new Regex(@"Purlin_(\d+)",RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            //Filtra por nombre y agrupa
            var groups = new Dictionary<int, List<string>>();
            foreach(var name in objectName)
            {
                if(string.IsNullOrWhiteSpace(name)) continue;
                var m = regex.Match(name);
                if(!m.Success) continue;
                if (!int.TryParse(m.Groups[1].Value, out int idx)) continue;
                if(!groups.TryGetValue(idx,out var list))
                {
                    list= new List<string>();
                    groups[idx]=list;
                }
                list.Add(name);
            }

            if(groups.Count==0) return 0.0;

            //Ordenar cada grupo (correa) por coordenada Y (o por Y mínima de extremos)
            foreach(var key in groups.Keys.ToList())
            {
                var ordered = groups[key]
                    .Select(frameName=>
                    {
                        SAP.DesignSubclass.GetFrameEndCoords(mySapModel,frameName, out double xi, out double yi, out double zi, out double xj, out double yj, out double zj);
                        double ySort = Math.Min(yi, yj);
                        return new { Name = frameName, Y = ySort };
                    })
                    .OrderBy(p=>p.Y)
                    .Select(p=>p.Name)
                    .ToList();
                groups[key] = ordered;
            }

            //Recorrer cada correa y sumar Vz en las uniones adyacentes: max global
            double rMax = 0.0;

            foreach (var kv in groups)
            {
                var frames = kv.Value;
                if (frames.Count < 2) continue;//sin uniones internas

                for (int i = 1; i < frames.Count; i++)
                {
                    string prev = frames[i - 1];
                    string next = frames[i];

                    // Estación final del elemento previo = longitud real (3D)
                    double Lprev =SAP.AnalysisSubclass.GetFrameLength3D(mySapModel, prev);

                    // Vz en extremo j del previo
                    double[] esfPrev = SAP.AnalysisSubclass.ObtenerEsfuerzosUnaBarraULS(mySapModel, prev, Lprev);
                    double vzPrev = Math.Abs(SAP.DesignSubclass.GetVz(esfPrev)); // encapsulo acceso a índice
                    double Nprev = Math.Abs(SAP.DesignSubclass.GetEsfuerzo(esfPrev, 1));
                    double Vyprev = Math.Abs(SAP.DesignSubclass.GetEsfuerzo(esfPrev, 2));

                    // Vz en extremo i del siguiente (estación 0)
                    double[] esfNext = SAP.AnalysisSubclass.ObtenerEsfuerzosUnaBarraULS(mySapModel, next, 0.0);
                    double vzNext = Math.Abs(SAP.DesignSubclass.GetVz(esfNext));
                    double NNext = Math.Abs(SAP.DesignSubclass.GetEsfuerzo(esfNext, 0));
                    double VyNext = Math.Abs(SAP.DesignSubclass.GetEsfuerzo(esfNext, 2));

                    double r = vzPrev + vzNext;
                    double rn = Nprev + NNext;
                    double rvy = Vyprev + VyNext;

                    if (r > rMax) rMax = r;
                    if (rn>Nmax) Nmax = rn;
                    if (rvy>Vymax) Vymax = rvy;
                }
            }

            return rMax;
        }

        public static double[] PresSuccCorrea(ComprobacionUnionesRackAPP vista)
        {
            //Validaciones básicas del input
            if (string.IsNullOrWhiteSpace(vista.NumCorreas.Text) || !int.TryParse(vista.NumCorreas.Text, out int nCorr) || nCorr < 1)
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Intoduce un número de correas válido", TipoIncidencia.Advertencia);
                ventana.ShowDialog();
                return null;
            }

            //Seleccionar el grupo y obtener los objetos seleccionados
            int ret = 0;
            ret |= mySapModel.SelectObj.ClearSelection();
            ret |= mySapModel.FrameObj.SetSelected("03 Correas", true, eItemType.Group);

            int numberItems = 0;
            int[] objectType = new int[1];
            string[] objectName = new string[1];
            ret |= mySapModel.SelectObj.GetSelected(ref numberItems, ref objectType, ref objectName);

            if (ret != 0 || numberItems == 0 || objectName == null || objectName.Length == 0)
                return null;

            //Filtrar solo los frameObj que sean Purlins y agrupar por índice Purlin_N
            var regex = new Regex(@"Purlin_(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            //Filtra por nombre y agrupa
            var groups = new Dictionary<int, List<string>>();
            foreach (var name in objectName)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                var m = regex.Match(name);
                if (!m.Success) continue;
                if (!int.TryParse(m.Groups[1].Value, out int idx)) continue;
                if (!groups.TryGetValue(idx, out var list))
                {
                    list = new List<string>();
                    groups[idx] = list;
                }
                list.Add(name);
            }

            if (groups.Count == 0) return null;

            //Ordenar cada grupo (correa) por coordenada Y (o por Y mínima de extremos)
            foreach (var key in groups.Keys.ToList())
            {
                var ordered = groups[key]
                    .Select(frameName =>
                    {
                        SAP.DesignSubclass.GetFrameEndCoords(mySapModel, frameName, out double xi, out double yi, out double zi, out double xj, out double yj, out double zj);
                        double ySort = Math.Min(yi, yj);
                        return new { Name = frameName, Y = ySort };
                    })
                    .OrderBy(p => p.Y)
                    .Select(p => p.Name)
                    .ToList();
                groups[key] = ordered;
            }

            //Recorrer cada correa y sumar Vz en las uniones adyacentes: max global
            mySapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();
            mySapModel.Results.Setup.SetComboSelectedForOutput("ULS");

            double rPresionMaxGlobal = 0.0;
            double rSuccionMaxGlobal = 0.0;

            // 'groups' = Dictionary<string, List<string>> con las barras ORDENADAS por nudo
            foreach (var kv in groups)
            {
                var frames = kv.Value;
                if (frames == null || frames.Count < 2) continue; // sin nudos internos

                for (int i = 1; i < frames.Count; i++)
                {
                    string prev = frames[i - 1]; // n-1 (derecha)
                    string next = frames[i];     // n   (izquierda)

                    // Estación j del previo = Lprev
                    double Lprev = SAP.AnalysisSubclass.GetFrameLength3D(mySapModel, prev);

                    // 1) Vz en n[0] (next, estación 0.0): obtener (Max, Min) algebraicos
                    var (vzNextMax, vzNextMin) = ObtenerVzMaxMinULS_EnEstacion(next, 0.0);

                    // 2) Vz en n-1[i] (prev, estación Lprev): obtener (Max, Min) algebraicos
                    var (vzPrevMax, vzPrevMin) = ObtenerVzMaxMinULS_EnEstacion(prev, Lprev);

                    // 3) Concomitancias:
                    //   Presión: n[0]_max + (n-1)[i]_min  -> valores absolutos tras emparejar
                    //   Succión: n[0]_min + (n-1)[i]_max  -> valores absolutos tras emparejar
                    double rPresion = Math.Abs(vzNextMax) + Math.Abs(vzPrevMin);
                    double rSuccion = Math.Abs(vzNextMin) + Math.Abs(vzPrevMax);

                    if (rPresion > rPresionMaxGlobal) rPresionMaxGlobal = rPresion;
                    if (rSuccion > rSuccionMaxGlobal) rSuccionMaxGlobal = rSuccion;
                }
            }

            return new double[] {rPresionMaxGlobal,rSuccionMaxGlobal};

        }

        public static Dictionary<string, double[]> CargarUniones(string tipo)
        {
            var datos = new Dictionary<string, double[]>();

            int i = 1;
            if (tipo != null) 
            { 
                switch(tipo)
                {
                    case "Perfiles":
                        i = 1;
                        break;

                    case "Ejiones":
                        i = 2;
                        break;

                    case "Placas":
                        i=3;
                        break;

                    default:
                        break;
                }
            }

            using (var workbook = new XLWorkbook(ruta))
            {
                var hoja = workbook.Worksheet(i);
                var filas = hoja.RangeUsed().RowsUsed();
                var columnas = hoja.RangeUsed().ColumnsUsed();

                foreach(var fila in filas.Skip(1))//saltar encabezado
                {
                    string nombre = fila.Cell(1).GetString();
                    double[] valores = new double[columnas.Count()];

                    for(int j=0;j<columnas.Count()-1;j++)
                    {
                        valores[j] = fila.Cell(j+2).GetDouble();
                    }
                    datos[nombre] = valores;
                }
            }

            return datos;
        }

        public static string[] ObtenerEncabezadosArray(string ruta, int numeroHoja = 2)
        {
            using (var workbook = new XLWorkbook(ruta))
            {
                var hoja = workbook.Worksheet(numeroHoja);
                var rango = hoja.RangeUsed();
                if (rango == null) return Array.Empty<string>();

                var filaCabecera = rango.FirstRowUsed();
                var columnas = rango.ColumnsUsed();
                if (!columnas.Any()) return Array.Empty<string>();

                return columnas
                    .Skip(1)
                    .Select(col => filaCabecera.Cell(col.ColumnNumber()).GetString().Trim())
                    .ToArray();
            }
        }

        public static string SeleccionarEjion(string[] encabezados, double[] presion, double[] succion, double Pmax, double Smax, out double Padm, out double Sadm)
        {
            string piezaSeleccionada = null;
            double espesorMinimo = double.MaxValue;
            Padm = 0;
            Sadm=0;

            for (int i = 0; i < encabezados.Length; i++)
            {
                if (presion[i]>=Pmax&&succion[i]>=Smax)
                {
                    //Extraer espesor del encabezado(antes del "_")
                    string[] partes = encabezados[i].Split('_');
                    if(partes.Length>0)
                    {
                        if (double.TryParse(partes[0],NumberStyles.Any,CultureInfo.InvariantCulture, out double espesor))
                        { 
                            if(espesor<espesorMinimo)
                            {
                                espesorMinimo = espesor;
                                piezaSeleccionada = encabezados[i];
                                Padm = presion[i];
                                Sadm = succion[i];
                            }        
                        }
                    }
                }
            }

            return piezaSeleccionada ?? "No hay pieza que cumpla los requisitos";
        }

        public static (double vzMax, double vzMin) ObtenerVzMaxMinULS_EnEstacion(string barra, double station)
        {
            // No cambiamos combo aquí. Haz fuera: Setup.SetComboSelectedForOutput("ULS")
            mySapModel.SetPresentUnits(eUnits.kN_m_C);

            int NumberResults = 0;
            string[] Obj = new string[0];
            double[] ObjSta = new double[0];
            string[] Elm = new string[0];
            double[] ElmSta = new double[0];
            string[] LoadCase = new string[0];
            string[] StepType = new string[0];
            double[] StepNum = new double[0];
            double[] P = new double[0];
            double[] V2 = new double[0];
            double[] V3 = new double[0];
            double[] T = new double[0];
            double[] M2 = new double[0];
            double[] M3 = new double[0];

            int ret = mySapModel.Results.FrameForce(
                barra, eItemTypeElm.ObjectElm,
                ref NumberResults,
                ref Obj, ref ObjSta, ref Elm, ref ElmSta, ref LoadCase, ref StepType, ref StepNum,
                ref P, ref V2, ref V3, ref T, ref M2, ref M3
            );

            if (ret != 0 || NumberResults <= 0 || ObjSta == null || ObjSta.Length == 0)
                return (0.0, 0.0);

            double L = SAP.AnalysisSubclass.GetFrameLength3D(mySapModel, barra);
            // Tolerancia proporcional a L (funciona bien tanto en barras cortas como largas)
            double tol = Math.Max(1e-6, 1e-4 * Math.Max(1.0, L));

            // Normalizamos StepType para evitar problemas de mayúsculas/minúsculas
            Func<string, string> norm = s => (s ?? "").Trim().ToLowerInvariant();

            bool anyMatch = false;
            double vzMax = double.NegativeInfinity; // algebraico más positivo
            double vzMin = double.PositiveInfinity; // algebraico más negativo

            for (int i = 0; i < NumberResults; i++)
            {
                if (Math.Abs(ObjSta[i] - station) <= tol)
                {
                    anyMatch = true;
                    string st = norm(StepType[i]);

                    // Muchas veces SAP devuelve exactamente "Max" y "Min"
                    bool isMax = st.Contains("max"); // f. de seguridad
                    bool isMin = st.Contains("min");

                    double vz = V2[i]; // tu Vz

                    if (isMax)
                        vzMax = Math.Max(vzMax, vz);
                    if (isMin)
                        vzMin = Math.Min(vzMin, vz);
                }
            }

            // Fallback si no hay match por tolerancia: tomamos el punto "más cercano"
            if (!anyMatch)
            {
                int idx = IndexOfClosest(ObjSta, station);
                if (idx >= 0)
                {
                    // En algunos modelos (o si no es combo envolvente), puede no venir "Max/Min".
                    // En ese caso tomamos algebraicos locales como "aprox."
                    string st = norm(StepType[idx]);
                    double vz = V3[idx];
                    if (st.Contains("max"))
                    {
                        vzMax = vz;
                        // buscamos también el "min" más cercano dentro de la misma estación
                        for (int i = 0; i < NumberResults; i++)
                        {
                            double sta = ObjSta[i];
                            if (Math.Abs(sta - (ObjSta[idx])) <= tol && norm(StepType[i]).Contains("min"))
                                vzMin = Math.Min(vzMin, V3[i]);
                        }
                    }
                    else if (st.Contains("min"))
                    {
                        vzMin = vz;
                        for (int i = 0; i < NumberResults; i++)
                        {
                            double sta =ObjSta[i];
                            if (Math.Abs(sta - (ObjSta[idx])) <= tol && norm(StepType[i]).Contains("max"))
                                vzMax = Math.Max(vzMax, V3[i]);
                        }
                    }
                    else
                    {
                        // Sin etiquetas claras: usamos estadística básica en esa estación
                        // (máximo y mínimo algebraicos entre todos los pasos coincidentes)
                        double staRef = ObjSta[idx];
                        vzMax = double.NegativeInfinity;
                        vzMin = double.PositiveInfinity;
                        for (int i = 0; i < NumberResults; i++)
                        {
                            double sta = ObjSta[i];
                            if (Math.Abs(sta - staRef) <= tol)
                            {
                                vzMax = Math.Max(vzMax, V3[i]);
                                vzMin = Math.Min(vzMin, V3[i]);
                            }
                        }
                    }
                }
            }

            if (double.IsNegativeInfinity(vzMax)) vzMax = 0.0;
            if (double.IsPositiveInfinity(vzMin)) vzMin = 0.0;

            return (vzMax, vzMin);
        }

        public static double ObtenerEsfMaxULS_EnEstacion (string barra, double station, int esfuerzo)
        {
            // No cambiamos combo aquí. Haz fuera: Setup.SetComboSelectedForOutput("ULS")
            mySapModel.SetPresentUnits(eUnits.kN_m_C);

            int NumberResults = 0;
            string[] Obj = new string[0];
            double[] ObjSta = new double[0];
            string[] Elm = new string[0];
            double[] ElmSta = new double[0];
            string[] LoadCase = new string[0];
            string[] StepType = new string[0];
            double[] StepNum = new double[0];
            double[] P = new double[0];
            double[] V2 = new double[0];
            double[] V3 = new double[0];
            double[] T = new double[0];
            double[] M2 = new double[0];
            double[] M3 = new double[0];

            int ret = mySapModel.Results.FrameForce(
                barra, eItemTypeElm.ObjectElm,
                ref NumberResults,
                ref Obj, ref ObjSta, ref Elm, ref ElmSta, ref LoadCase, ref StepType, ref StepNum,
                ref P, ref V2, ref V3, ref T, ref M2, ref M3
            );

            if (ret != 0 || NumberResults <= 0 || ObjSta == null || ObjSta.Length == 0)
                return 0.0;

            double L = SAP.AnalysisSubclass.GetFrameLength3D(mySapModel, barra);
            // Tolerancia proporcional a L (funciona bien tanto en barras cortas como largas)
            double tol = Math.Max(1e-6, 1e-4 * Math.Max(1.0, L));

            // Normalizamos StepType para evitar problemas de mayúsculas/minúsculas
            Func<string, string> norm = s => (s ?? "").Trim().ToLowerInvariant();

            bool anyMatch = false;
            double vzMax = double.NegativeInfinity; // algebraico más positivo
            double vzMin = double.PositiveInfinity; // algebraico más negativo

            for (int i = 0; i < NumberResults; i++)
            {
                if (Math.Abs(ObjSta[i] - station) <= tol)
                {
                    anyMatch = true;
                    string st = norm(StepType[i]);

                    // Muchas veces SAP devuelve exactamente "Max" y "Min"
                    bool isMax = st.Contains("max"); // f. de seguridad
                    bool isMin = st.Contains("min");

                    double vz = 0;

                    switch (esfuerzo)
                    {
                        case 1:
                            vz=P[i];
                            break;
                        case 2:
                            vz = V2[i];
                            break;
                        case 3:
                            vz = V3[i];
                            break;
                        case 4:
                            vz = T[i];
                            break;
                        case 5:
                            vz = M2[i];
                            break;
                        case 6:
                            vz = M3[i];
                            break;
                    }

                    if (isMax)
                        vzMax = Math.Max(vzMax, vz);
                    if (isMin)
                        vzMin = Math.Min(vzMin, vz);
                }
            }

            // Fallback si no hay match por tolerancia: tomamos el punto "más cercano"
            if (!anyMatch)
            {
                int idx = IndexOfClosest(ObjSta, station);
                if (idx >= 0)
                {
                    // En algunos modelos (o si no es combo envolvente), puede no venir "Max/Min".
                    // En ese caso tomamos algebraicos locales como "aprox."
                    string st = norm(StepType[idx]);
                    double vz = V3[idx];
                    if (st.Contains("max"))
                    {
                        vzMax = vz;
                        // buscamos también el "min" más cercano dentro de la misma estación
                        for (int i = 0; i < NumberResults; i++)
                        {
                            double sta = ObjSta[i];
                            if (Math.Abs(sta - (ObjSta[idx])) <= tol && norm(StepType[i]).Contains("min"))
                                vzMin = Math.Min(vzMin, V3[i]);
                        }
                    }
                    else if (st.Contains("min"))
                    {
                        vzMin = vz;
                        for (int i = 0; i < NumberResults; i++)
                        {
                            double sta = ObjSta[i];
                            if (Math.Abs(sta - (ObjSta[idx])) <= tol && norm(StepType[i]).Contains("max"))
                                vzMax = Math.Max(vzMax, V3[i]);
                        }
                    }
                    else
                    {
                        // Sin etiquetas claras: usamos estadística básica en esa estación
                        // (máximo y mínimo algebraicos entre todos los pasos coincidentes)
                        double staRef = ObjSta[idx];
                        vzMax = double.NegativeInfinity;
                        vzMin = double.PositiveInfinity;
                        for (int i = 0; i < NumberResults; i++)
                        {
                            double sta = ObjSta[i];
                            if (Math.Abs(sta - staRef) <= tol)
                            {
                                vzMax = Math.Max(vzMax, V3[i]);
                                vzMin = Math.Min(vzMin, V3[i]);
                            }
                        }
                    }
                }
            }

            if (double.IsNegativeInfinity(vzMax)) vzMax = 0.0;
            if (double.IsPositiveInfinity(vzMin)) vzMin = 0.0;

            return Math.Max(Math.Abs(vzMax),Math.Abs(vzMin));
        }

        public static int IndexOfClosest(double[] arr, double value)
        {
            if (arr == null || arr.Length == 0) return -1;
            double best = double.PositiveInfinity;
            int idx = -1;
            for (int i = 0; i < arr.Length; i++)
            {
                double d = Math.Abs(arr[i] - value);
                if (d < best) { best = d; idx = i; }
            }
            return idx;
        }

        public static void EspesorYMaterialPlaca (string placa, out string espesor, out string material)
        {
            if(string.IsNullOrWhiteSpace(placa))
            {
                espesor = "Error";
                material = "Error";
            }
            else
            {
                var partes = placa.Split('_');
                if (partes.Length != 2)
                {
                    espesor = "Error";
                    material = "Error";
                }
                else
                {
                    espesor = partes[0];
                    material = partes[1];
                }
            }
        }
    }
}
