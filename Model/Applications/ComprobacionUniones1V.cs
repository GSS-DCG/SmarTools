using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using OfficeOpenXml;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace SmarTools.Model.Applications
{
    internal class ComprobacionUniones1V
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        public static void ComprobarUniones1V(ComprobacionUnionesAPP vista)
        {
            var ruta = @"Z:\300Logos\03 Uniones\Uniones 1VR5.xlsx";
            var uniones = CargarDesdeExcel(ruta);

            //Datos de las uniones
            double[] BS = uniones["BS"];
            double[] MS = uniones["MS"];
            double[] BC = uniones["BC"];
            double[] SB_13 = uniones["SB_1,3"];
            double[] SB_15 = uniones["SB_1,5"];
            double[] SB_16 = uniones["SB_1,6"];
            double[] SB_18 = uniones["SB_1,8"];
            double[] SB_2 = uniones["SB_2"];

            

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

        public static void UnionBS(ComprobacionUnionesAPP vista)
        {

        }
    }
}
