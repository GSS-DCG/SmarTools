using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SmarTools.Model.Repository;
using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using System.IO;
using ClosedXML;
using ClosedXML.Excel;
using OfficeOpenXml;
using SmarTools.View;


namespace SmarTools.Model.Applications
{
    internal class NumeroPilares
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static string rutaUniones = @"Z:\300SmarTools\03 Uniones\Uniones 1VR5_"+MainView.Globales._revisionUniones1V+".xlsx";

        public static void NumeroPilares1V(NumeroPilaresAPP vista)
        {
            var loadingWindow = new Status();

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                //Esfuerzos unión BS
                var uniones = CargarDesdeExcel(rutaUniones);
                double[] esfuerzos_BS = uniones["BS"];
                double Mt = esfuerzos_BS[5];
                double N = esfuerzos_BS[0];
                double V = esfuerzos_BS[1];
                string rutaArchivo = vista.RutaESMA.Text;

                using (ExcelPackage package = new ExcelPackage(rutaArchivo))
                {
                    //Obtenemos los datos
                    double parEstaticoExp = LeerCelda(rutaArchivo, "Cálculo Motor", "O22");
                    double parEstaticoRes = LeerCelda(rutaArchivo, "Cálculo Motor", "O23");
                    double longitudNorte = LeerCelda(rutaArchivo, "Datos de entrada cálculo", "D20");
                    double longitudSur = LeerCelda(rutaArchivo, "Datos de entrada cálculo", "H20");
                    double ang_Exp = LeerCelda(rutaArchivo, "Datos de entrada cálculo", "E37");
                    double ang_Res = LeerCelda(rutaArchivo, "Datos de entrada cálculo", "E38");
                    double Npaneles_Exp = Math.Max(LeerCelda(rutaArchivo, "Datos de entrada cálculo", "K37"), LeerCelda(rutaArchivo, "Datos de entrada cálculo", "L37"));
                    double Npaneles_Res = Math.Max(LeerCelda(rutaArchivo, "Datos de entrada cálculo", "K38"), LeerCelda(rutaArchivo, "Datos de entrada cálculo", "L38"));
                    double Apanel = LeerCelda(rutaArchivo, "Cargas", "T10");
                    double Ppanel = LeerCelda(rutaArchivo, "Cargas", "P8");
                    double Pnieve_Exp = Math.Abs(LeerCelda(rutaArchivo, "Cargas", "O16"));
                    double Pnieve_Res = Math.Abs(LeerCelda(rutaArchivo, "Cargas", "O18"));
                    double Psup_Exp = Math.Abs(LeerCelda(rutaArchivo, "Cargas", "K9"));
                    double Psup_Res = Math.Abs(LeerCelda(rutaArchivo, "Cargas", "L18"));
                    double Pinf_Exp = Math.Abs(LeerCelda(rutaArchivo, "Cargas", "J9"));
                    double Pinf_Res = Math.Abs(LeerCelda(rutaArchivo, "Cargas", "K18"));
                    double Mayoracion_pesopropio = LeerCelda(rutaArchivo, "Cálculo Motor", "L22");
                    double Mayoracion_viento = LeerCelda(rutaArchivo, "Cálculo Motor", "J22");
                    double Mayoracion_nieve = LeerCelda(rutaArchivo, "Cálculo Motor", "N22");

                    //Cálculos
                    double longSemitracker = Math.Max(longitudNorte, longitudSur);
                    double valor = (longSemitracker - 1500) / 9000;
                    double npilares_vano;

                    if(valor-Math.Floor(valor)<0.1)
                    {
                        npilares_vano = Math.Floor(valor) * 2 + 1;
                    }
                    else
                    {
                        npilares_vano = Math.Ceiling(valor) * 2 + 1;
                    }

                    double npilares_torsor_Exp = Math.Ceiling(parEstaticoExp / Mt);
                    double npilares_torsor_Res = Math.Ceiling(parEstaticoRes / Mt);

                    double axil_Exp = Npaneles_Exp * Apanel * ((Mayoracion_pesopropio * Ppanel) + (Mayoracion_nieve * Pnieve_Exp) + Mayoracion_viento * Math.Cos(ang_Exp * Math.PI / 180) * (Psup_Exp / 2 + Pinf_Exp / 2));
                    double axil_Res = Npaneles_Res * Apanel * ((Mayoracion_pesopropio * Ppanel) + (Mayoracion_nieve * Pnieve_Res) + Mayoracion_viento * Math.Cos(ang_Res * Math.PI / 180) * (Psup_Res / 2 + Pinf_Res / 2));
                    double npilares_axil_Exp = Math.Ceiling(axil_Exp / N) * 2 + 1;
                    double npilares_axil_Res = Math.Ceiling(axil_Res / N) * 2 + 1;

                    double cortante_Exp = Npaneles_Exp * Apanel * (Mayoracion_viento * Math.Sin(ang_Exp * Math.PI / 180) * (Psup_Exp / 2 + Pinf_Exp / 2));
                    double cortante_Res = Npaneles_Res * Apanel * (Mayoracion_viento * Math.Sin(ang_Res * Math.PI / 180) * (Psup_Res / 2 + Pinf_Res / 2));
                    double npilares_cortante_Exp = Math.Ceiling(cortante_Exp / V) * 2 + 1;
                    double npilares_cortante_Res = Math.Ceiling(cortante_Res / V) * 2 + 1;

                    double npilares_Exp = Math.Max(Math.Max(npilares_vano, npilares_torsor_Exp), Math.Max(npilares_axil_Exp, npilares_cortante_Exp));
                    double npilares_Res = Math.Max(Math.Max(npilares_vano, npilares_torsor_Res), Math.Max(npilares_axil_Res, npilares_cortante_Res));

                    //Pasamos los resultados a la parte gráfica
                    vista.numPilaresExp.Text = npilares_Exp.ToString();
                    vista.numPilaresRes.Text = npilares_Res.ToString();

                    var limitaciones_Exp = new List<(double valor, string descripcion)>
                {
                    (npilares_vano,"Vano máximo" ),
                    (npilares_torsor_Exp, "Par torsor"),
                    (npilares_cortante_Exp,"Cortante máximo" ),
                    (npilares_axil_Exp,"Axil máximo" )
                };

                    var limitaciones_Res = new List<(double valor, string descripcion)>
                {
                    (npilares_vano,"Vano máximo" ),
                    (npilares_torsor_Res, "Par torsor"),
                    (npilares_cortante_Res,"Cortante máximo" ),
                    (npilares_axil_Res,"Axil máximo" )
                };

                    var limitacion_Exp = limitaciones_Exp.FirstOrDefault(x => x.valor == npilares_Exp).descripcion;
                    var limitacion_Res = limitaciones_Res.FirstOrDefault(x => x.valor == npilares_Res).descripcion;

                    vista.limitacionExp.Text = limitacion_Exp;
                    vista.limitacionRes.Text = limitacion_Res;

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

        public static Dictionary<string, double[]> CargarDesdeExcel(string rutaArchivo)
        {
            var datos = new Dictionary<string, double[]>();

            using (var workbook = new XLWorkbook(rutaArchivo))
            {
                var hoja = workbook.Worksheet(1); // Primera hoja
                var filas = hoja.RangeUsed().RowsUsed();

                foreach (var fila in filas.Skip(1)) // Saltar encabezado
                {
                    string nombre = fila.Cell(1).GetString();
                    double[] valores = new double[6];

                    for (int i = 0; i < 6; i++)
                    {
                        valores[i] = fila.Cell(i + 2).GetDouble();
                    }

                    datos[nombre] = valores;
                }
            }

            return datos;

        }
    }
}
