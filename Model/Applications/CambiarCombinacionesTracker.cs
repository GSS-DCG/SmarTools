using SmarTools.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SmarTools.Model.Repository;
using ClosedXML.Excel;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using ModernUI.View;
using SmarTools.APPS;
using OfficeOpenXml;
using SAP2000v1;

namespace SmarTools.Model.Applications
{
    class CambiarCombinacionesTracker
    {
        public static string ruta = @"Z:\300SmarTools\04 Combinaciones\Coeficientes_" + MainView.Globales._revisionCoeficientes + ".xlsx";

        public static List<Combination> Combinations = new List<Combination>();

        public static void GenerarCombinaciones (CambiarCombinacionesTrackerAPP vista)
        {
            var loadingWindow = new Status();

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                //Obtenemos los casos de carga
                #region
                Dictionary<string, string> CasosSeleccionados = new Dictionary<string, string>();

                if (vista.Aplicar_Dead.IsChecked == true)
                    CasosSeleccionados["DEAD"] = "DEAD";

                if (vista.Aplicar_Dead.IsChecked == true)
                    CasosSeleccionados["PP Paneles"] = "DEAD";

                if (vista.Aplicar_Presion.IsChecked == true)
                    CasosSeleccionados["W1_Press"] = "WIND";

                if (vista.Aplicar_Succion.IsChecked == true)
                    CasosSeleccionados["W2_Suct"] = "WIND";

                if (vista.Aplicar_Lateral_90.IsChecked == true)
                    CasosSeleccionados["W3_90º"] = "WIND";

                if (vista.Aplicar_Lateral_270.IsChecked == true)
                    CasosSeleccionados["W4_270º"] = "WIND";

                if (vista.Aplicar_Nieve.IsChecked == true)
                    CasosSeleccionados["Snow"] = "SNOW";

                if (vista.Aplicar_NieveAccidental.IsChecked == true)
                    CasosSeleccionados["Accidental_Snow"] = "SNOW";

                if (vista.Aplicar_SismoX.IsChecked == true)
                    CasosSeleccionados["Ex"] = "QUAKE";

                if (vista.Aplicar_SismoY.IsChecked == true)
                    CasosSeleccionados["Ey"] = "QUAKE";
                #endregion

                //Obtenemos la normativa
                var normativa = (vista.Normativa.SelectedItem as ComboBoxItem)?.Content?.ToString();
                //List<string> coef = Coeficientes(vista, normativa).Select(x => x.Item2.Text).ToList();

                //Limpiamos la lista
                vista.Combinaciones_Carga.Items.Clear();

                //Separamos las cargas: peso propio, carga muerta, viento, nieve, nieve accidental
                #region
                var cargasDead = CasosSeleccionados
                    .Where(c => c.Value == "DEAD" && !c.Key.StartsWith("CM"))
                    .ToList();

                var cargasWind = CasosSeleccionados
                    .Where(c => c.Value == "WIND")
                    .ToList();

                var cargasSnow = CasosSeleccionados
                    .Where(c => c.Value == "SNOW" && !c.Key.StartsWith("Accidental"))
                    .ToList();

                var cargasAccidentalSnow = CasosSeleccionados
                    .Where(c => c.Value == "SNOW" && c.Key.StartsWith("Accidental"))
                    .ToList();

                var cargasQuake = CasosSeleccionados
                    .Where(c => c.Value == "QUAKE")
                    .ToList();

                string combinacion = "";
                #endregion

                int cont = 1;

                // Eurocódigo e Italia NTC-2018
                if (normativa == "Eurocódigo" || normativa == "NTC-2018")
                {
                    #region ESTADOS LÍMITES ÚLTIMOS
                    //Caso 1a: Permanentes. Situación Permanente Favorable
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                    }
                    Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    string comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                    cont++;
                    #endregion
                    //Caso 1b: Permanentes. Situación Permanente Desfavorable
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                    }
                    Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                    cont++;
                    #endregion
                    //Caso 2a: Permanentes + viento. Situación Permanente Favorable
                    #region
                    foreach (var wind in cargasWind)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                        }
                        Combination.Hipotesis.Add(wind.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                    }
                    #endregion
                    //Caso 2b: Permanentes + viento. Situación Permanente Desfavorable
                    #region
                    foreach (var wind in cargasWind)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                        }
                        Combination.Hipotesis.Add(wind.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                    }
                    #endregion
                    //Caso 3a: Permanentes + Nieve. Situación Permanente Favorable
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    //Caso 3b: Permanentes + Nieve. Situación Permanente Desfavorable
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    //Caso 4: Permanentes + Viento + Nieve (altitud más-menos de 1000m)
                    if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Menos1000_Check.IsChecked == true) //Altitud de nieve menor o igual a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H<=1000 metros. Psi0
                    {
                        //Caso 4a: Permanentes + Viento + Nieve (Altitud menos de 1000 m). Situación Persistente Favorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Menos1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                cont++;
                            }
                        }
                        #endregion

                        //Caso 4b: Permanentes + Viento + Nieve (Altitud menos de 1000 m). Situación Persistente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Menos1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    else if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Mas1000_Check.IsChecked == true)//Altitud de nieve mayor a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H>1000 metros. Psi0
                    {
                        //Caso 4a: Permanentes + Viento + Nieve (Altitud más de 1000 m). Situación Persistente Favorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Mas1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                cont++;
                            }
                        }
                        #endregion
                        //Caso 4b: Permanentes + Viento + Nieve (Altitud más de 1000 m). Situación Persistente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Mas1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    //Caso 5a: Permanentes + Nieve + Viento. Situación Permanente Favorable
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            //Viento con Psi0
                            Combination.Hipotesis.Add(viento.Key);
                            string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Viento.Text)).ToString("F2");
                            Combination.Mayoracion.Add(double.Parse(coeficiente));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    //Caso 5b: Permanentes + Nieve + Viento. Situación Permanente Desavorable
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            //Viento con Psi0
                            Combination.Hipotesis.Add(viento.Key);
                            string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Viento.Text)).ToString("F2");
                            Combination.Mayoracion.Add(double.Parse(coeficiente));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    //Sismo
                    if (vista.Aplicar_SismoX.IsChecked == true || vista.Aplicar_SismoY.IsChecked == true)
                    {
                        bool primerSismo = true;
                        //Caso 6a: Peso propio + Sismo (+Ex, +Ey). Accidentales
                        #region
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        //Caso 6b: Peso propio + Sismo (-Ex, +Ey). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        //Caso 6c: Peso propio + Sismo (+Ex, -Ey). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        //Caso 6d: Peso propio + Sismo (-Ex, -Ey). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        //Caso 6e: Peso propio + Sismo (+Ey, +Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        //Caso 6f: Peso propio + Sismo (-Ey,+Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        //Caso 6g: Peso propio + Sismo (+Ey, -Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        //Caso 6h: Peso propio + Sismo (-Ey, -Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                    }

                    //Caso 7: Nieve Accidental
                    #region
                    if (vista.Aplicar_NieveAccidental.IsChecked == true)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var nieveAcc in cargasAccidentalSnow)
                        {
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                            }
                            Combination.Hipotesis.Add(nieveAcc.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                            foreach (var viento in cargasWind)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                                }
                                // Viento
                                Combination.Hipotesis.Add(viento.Key);
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Psi1_Viento.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                // Nieve Accidental
                                Combination.Hipotesis.Add(nieveAcc.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));

                                //Creamos combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                cont++;
                            }
                        }
                    }

                    #endregion
                    #endregion

                    #region ESTADOS LÍMITES DE SERVICIO
                    cont = 1;
                    // Caso 1: Permanentes. Situación Permanente Desfavorable
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                    }
                    Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                    cont++;

                    #endregion
                    // Caso 2: Permanentes + Viento. Situación Permanente Desfavorable
                    #region
                    foreach (var viento in cargasWind)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                        }
                        // Viento 
                        Combination.Hipotesis.Add(viento.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                        Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                        vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                        cont++;
                    }

                    #endregion
                    // Caso 3a: Permanentes + Nieve. Situación Permanente Desfavorable
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Menos1000_Check.IsChecked == true) //Altitud de nieve menor o igual a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H<=1000 metros. Psi0
                    {
                        // Caso 4: Permanentes + Viento + Nieve (Altitud Menor de 1000). Situación Permanente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Desfavorable_SLS.Text) * double.Parse(vista.Psi0_Menos1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Mas1000_Check.IsChecked == true) //Altitud de nieve mayor a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H>1000 metros. Psi0
                    {
                        // Caso 4: Permanentes + Viento + Nieve (Altitud mayor de 1000). Situación Permanente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Desfavorable_SLS.Text) * double.Parse(vista.Psi0_Mas1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    // Caso 5: Permanentes + Nieve + Viento. Situación Permanente Desfavorable
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                            //Viento con Psi0
                            Combination.Hipotesis.Add(viento.Key);
                            string coeficiente = (double.Parse(vista.Variable_Desfavorable_SLS.Text) * double.Parse(vista.Psi0_Viento.Text)).ToString("F2");
                            Combination.Mayoracion.Add(double.Parse(coeficiente));

                            //Creamos la combinación
                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion

                    #endregion
                }

                // ASCE 7-05 y ASCE 7-16
                if (normativa == "ASCE7-05" || normativa == "ASCE7-16")
                {
                    #region ESTADOS LÍMITES ÚLTIMOS
                    //Caso 1: D
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Gamma1.Text));
                    }
                    Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    string comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                    cont++;
                    #endregion
                    // Caso 2: D + S
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma2.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma3.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    
                    }
                    #endregion
                    // Caso 3: D + W + S
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma4.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma6.Text));

                            //Viento
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma5.Text));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    // Caso 4: D + W + S
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma7.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma9.Text));

                            //Viento
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma8.Text));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    // Caso 5: D + S + E
                    if (vista.Aplicar_SismoX.IsChecked == true || vista.Aplicar_SismoY.IsChecked == true)
                    {
                        bool primerSismo = true;
                        // Caso 5a: +Ex +Ey
                        #region
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                        // Caso 5b: +Ex -Ey
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                        // Caso 5c: -Ex +Ey
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                        // Caso 5d: -Ex -Ey
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                        // Caso 5e: +Ey +Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                        // Caso 5f: +Ey -Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                        // Caso 5g: -Ey +Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                        // Caso 5h: -Ey -Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                        #endregion
                    }
                    // Caso 6: D + W
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma13.Text));
                            }
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma14.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion
                    // Caso 7: D + E
                    #region
                    if (vista.Aplicar_SismoX.IsChecked == true || vista.Aplicar_SismoY.IsChecked == true)
                    {
                        bool primerSismo = true;
                        // Caso 7a: +Ex +Ey
                        #region
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        // Caso 7b: +Ex -Ey
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        // Caso 7c: -Ex +Ey
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        // Caso 7d: -Ex -Ey
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        // Caso 7e: +Ey +Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        // Caso 7f: +Ey -Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        // Caso 7g: -Ey +Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion
                        // Caso 7h: -Ey -Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        cont++;
                        #endregion

                    }
                    #endregion
                    #endregion

                    #region ESTADOS LÍMITES DE SERVICIO
                    cont = 1;
                    // Caso 1: D
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Gamma17.Text));
                    }
                    Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                    cont++;
                    #endregion

                    // Caso 2: D + S
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma18.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma19.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 3: D + S
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma20.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma21.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 4: D + W
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma22.Text));
                            }
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma23.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 5: D + W + S
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma24.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma26.Text));

                            //Viento
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma25.Text));

                            //Creamos la combinación
                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 6: D + W
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma27.Text));
                            }
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma28.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            cont++;
                        }
                    }
                    #endregion

                    #endregion
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

        public static void AplicarCombinaciones (CambiarCombinacionesTrackerAPP vista)
        {
            var loadingWindow = new Status();
            try
            {
                Herramientas.AbrirArchivoSAP2000();
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                cHelper cHelper = MainView.Globales._myHelper;
                cOAPI mySapObject = MainView.Globales._mySapObject;
                cSapModel mySapModel = MainView.Globales._mySapModel;

                //Obtenemos la normativa
                var normativa = (vista.Normativa.SelectedItem as ComboBoxItem)?.Content?.ToString();

                //Limpiamos la lista
                vista.Combinaciones_Carga.Items.Clear();

                //Limpiamos los Load Patterns, Load Cases y Combinations del modelo
                int NumberNames = 0;
                string[] Names = new string[0];

                mySapModel.RespCombo.GetNameList(ref NumberNames, ref Names);
                foreach (var name in Names)
                {
                    mySapModel.RespCombo.Delete(name);
                }

                mySapModel.LoadCases.GetNameList_1(ref NumberNames, ref Names);
                foreach (var name in Names)
                {
                    if (name != "MODAL" || name != "Ex" || name != "Ey")
                    {
                        mySapModel.LoadCases.Delete(name);
                    }
                }

                mySapModel.LoadPatterns.GetNameList(ref NumberNames, ref Names);
                foreach (var name in Names)
                {
                    if (name != "MODAL" || name != "Ex" || name != "Ey")
                    {
                        mySapModel.LoadPatterns.Delete(name);
                    }
                }

                bool contieneEx = Names.Contains("Ex");
                bool contieneEy = Names.Contains("Ey");

                //Obtenemos los casos de carga
                #region
                Dictionary<string, string> CasosSeleccionados = new Dictionary<string, string>();

                if (vista.Aplicar_Dead.IsChecked == true)
                    CasosSeleccionados["DEAD"] = "DEAD";

                if (vista.Aplicar_Dead.IsChecked == true)
                    CasosSeleccionados["PP Paneles"] = "DEAD";

                if (vista.Aplicar_Presion.IsChecked == true)
                    CasosSeleccionados["W1_Press"] = "WIND";

                if (vista.Aplicar_Succion.IsChecked == true)
                    CasosSeleccionados["W2_Suct"] = "WIND";

                if (vista.Aplicar_Lateral_90.IsChecked == true)
                    CasosSeleccionados["W3_90º"] = "WIND";

                if (vista.Aplicar_Lateral_270.IsChecked == true)
                    CasosSeleccionados["W4_270º"] = "WIND";

                if (vista.Aplicar_Nieve.IsChecked == true)
                    CasosSeleccionados["Snow"] = "SNOW";

                if (vista.Aplicar_NieveAccidental.IsChecked == true)
                    CasosSeleccionados["Accidental_Snow"] = "SNOW";

                if (vista.Aplicar_SismoX.IsChecked == true && Names.Contains("Ex"))
                    CasosSeleccionados["Ex"] = "QUAKE";

                if (vista.Aplicar_SismoY.IsChecked == true && Names.Contains("Ey"))
                    CasosSeleccionados["Ey"] = "QUAKE";
                #endregion

                //Creamos Load Patterns y Load Cases
                Sap2000CreateLoadPattern(CasosSeleccionados.Keys.ToList(), CasosSeleccionados.Values.ToList());
                Sap2000CreateLoadCases(CasosSeleccionados.Keys.ToList(), CasosSeleccionados.Values.ToList());

                //Separamos las cargas: peso propio, carga muerta, viento, nieve, nieve accidental
                #region
                var cargasDead = CasosSeleccionados
                    .Where(c => c.Value == "DEAD" && !c.Key.StartsWith("CM"))
                    .ToList();

                var cargasWind = CasosSeleccionados
                    .Where(c => c.Value == "WIND")
                    .ToList();

                var cargasSnow = CasosSeleccionados
                    .Where(c => c.Value == "SNOW" && !c.Key.StartsWith("Accidental"))
                    .ToList();

                var cargasAccidentalSnow = CasosSeleccionados
                    .Where(c => c.Value == "SNOW" && c.Key.StartsWith("Accidental"))
                    .ToList();

                var cargasQuake = CasosSeleccionados
                    .Where(c => c.Value == "QUAKE")
                    .ToList();

                string combinacion = "";
                #endregion

                int cont = 1;

                // Eurocódigo e Italia NTC-2018
                if (normativa == "Eurocódigo" || normativa == "NTC-2018")
                {
                    #region ESTADOS LÍMITES ÚLTIMOS
                    //Caso 1a: Permanentes. Situación Permanente Favorable
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                    }
                    Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    string comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                    Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                    cont++;
                    #endregion
                    //Caso 1b: Permanentes. Situación Permanente Desfavorable
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                    }
                    Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                    Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                    cont++;
                    #endregion
                    //Caso 2a: Permanentes + viento. Situación Permanente Favorable
                    #region
                    foreach (var wind in cargasWind)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                        }
                        Combination.Hipotesis.Add(wind.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                    }
                    #endregion
                    //Caso 2b: Permanentes + viento. Situación Permanente Desfavorable
                    #region
                    foreach (var wind in cargasWind)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                        }
                        Combination.Hipotesis.Add(wind.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                    }
                    #endregion
                    //Caso 3a: Permanentes + Nieve. Situación Permanente Favorable
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    //Caso 3b: Permanentes + Nieve. Situación Permanente Desfavorable
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    //Caso 4: Permanentes + Viento + Nieve (altitud más-menos de 1000m)
                    if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Menos1000_Check.IsChecked == true) //Altitud de nieve menor o igual a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H<=1000 metros. Psi0
                    {
                        //Caso 4a: Permanentes + Viento + Nieve (Altitud menos de 1000 m). Situación Persistente Favorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Menos1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                                cont++;
                            }
                        }
                        #endregion

                        //Caso 4b: Permanentes + Viento + Nieve (Altitud menos de 1000 m). Situación Persistente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Menos1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    else if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Mas1000_Check.IsChecked == true)//Altitud de nieve mayor a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H>1000 metros. Psi0
                    {
                        //Caso 4a: Permanentes + Viento + Nieve (Altitud más de 1000 m). Situación Persistente Favorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Mas1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                                cont++;
                            }
                        }
                        #endregion
                        //Caso 4b: Permanentes + Viento + Nieve (Altitud más de 1000 m). Situación Persistente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Mas1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    //Caso 5a: Permanentes + Nieve + Viento. Situación Permanente Favorable
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Favorable.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            //Viento con Psi0
                            Combination.Hipotesis.Add(viento.Key);
                            string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Viento.Text)).ToString("F2");
                            Combination.Mayoracion.Add(double.Parse(coeficiente));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    //Caso 5b: Permanentes + Nieve + Viento. Situación Permanente Desavorable
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Persistente_Desfavorable.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Persistente_Desfavorable.Text));

                            //Viento con Psi0
                            Combination.Hipotesis.Add(viento.Key);
                            string coeficiente = (double.Parse(vista.Variable_Persistente_Desfavorable.Text) * double.Parse(vista.Psi0_Viento.Text)).ToString("F2");
                            Combination.Mayoracion.Add(double.Parse(coeficiente));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    //Sismo
                    if (cargasQuake.Count != 0) //vista.Aplicar_SismoX.IsChecked == true || vista.Aplicar_SismoY.IsChecked == true
                    {
                        bool primerSismo = true;
                        //Caso 6a: Peso propio + Sismo (+Ex, +Ey). Accidentales
                        #region
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        //Caso 6b: Peso propio + Sismo (-Ex, +Ey). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        //Caso 6c: Peso propio + Sismo (+Ex, -Ey). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        //Caso 6d: Peso propio + Sismo (-Ex, -Ey). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        //Caso 6e: Peso propio + Sismo (+Ey, +Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        //Caso 6f: Peso propio + Sismo (-Ey,+Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        //Caso 6g: Peso propio + Sismo (+Ey, -Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        //Caso 6h: Peso propio + Sismo (-Ey, -Ex). Accidentales
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                        }
                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        })); vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                    }

                    //Caso 7: Nieve Accidental
                    #region
                    if (vista.Aplicar_NieveAccidental.IsChecked == true)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var nieveAcc in cargasAccidentalSnow)
                        {
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                            }
                            Combination.Hipotesis.Add(nieveAcc.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                            foreach (var viento in cargasWind)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Accidental_Desfavorable.Text));
                                }
                                // Viento
                                Combination.Hipotesis.Add(viento.Key);
                                string coeficiente = (double.Parse(vista.Accidental_Accidental_Desfavorable.Text) * double.Parse(vista.Psi1_Viento.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                // Nieve Accidental
                                Combination.Hipotesis.Add(nieveAcc.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Accidental_Accidental_Desfavorable.Text));

                                //Creamos combinación
                                Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                                Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                                cont++;
                            }
                        }
                    }

                    #endregion

                    #endregion

                    #region ESTADOS LÍMITES DE SERVICIO
                    cont = 1;
                    // Caso 1: Permanentes. Situación Permanente Desfavorable
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                    }
                    Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                    Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                    cont++;
                    #endregion
                    // Caso 2: Permanentes + Viento. Situación Permanente Desfavorable
                    #region
                    foreach (var viento in cargasWind)
                    {
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();

                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                        }
                        // Viento 
                        Combination.Hipotesis.Add(viento.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                        Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                        vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                    }
                    #endregion
                    // Caso 3a: Permanentes + Nieve. Situación Permanente Desfavorable
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Menos1000_Check.IsChecked == true) //Altitud de nieve menor o igual a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H<=1000 metros. Psi0
                    {
                        // Caso 4: Permanentes + Viento + Nieve (Altitud Menor de 1000). Situación Permanente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Desfavorable_SLS.Text) * double.Parse(vista.Psi0_Menos1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                                Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    if (vista.Aplicar_Nieve.IsChecked == true && vista.Nieve_Mas1000_Check.IsChecked == true) //Altitud de nieve mayor a 1000m Psi0; //Coeficiente de Simultaneidad. Nieve. Edificios emplazados en altitud H>1000 metros. Psi0
                    {
                        // Caso 4: Permanentes + Viento + Nieve (Altitud mayor de 1000). Situación Permanente Desfavorable
                        #region
                        foreach (var viento in cargasWind)
                        {
                            foreach (var nieve in cargasSnow)
                            {
                                Combination.Hipotesis.Clear();
                                Combination.Mayoracion.Clear();

                                foreach (var carga in cargasDead)
                                {
                                    Combination.Hipotesis.Add(carga.Key);
                                    Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                                }
                                //Viento
                                Combination.Hipotesis.Add(viento.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                                //Nieve con Psi0
                                Combination.Hipotesis.Add(nieve.Key);
                                string coeficiente = (double.Parse(vista.Variable_Desfavorable_SLS.Text) * double.Parse(vista.Psi0_Mas1000.Text)).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));

                                //Creamos la combinación
                                Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                                comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                                vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                                Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                                cont++;
                            }
                        }
                        #endregion
                    }
                    // Caso 5: Permanentes + Nieve + Viento. Situación Permanente Desfavorable
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Permanente_Desfavorable_SLS.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Variable_Desfavorable_SLS.Text));

                            //Viento con Psi0
                            Combination.Hipotesis.Add(viento.Key);
                            string coeficiente = (double.Parse(vista.Variable_Desfavorable_SLS.Text) * double.Parse(vista.Psi0_Viento.Text)).ToString("F2");
                            Combination.Mayoracion.Add(double.Parse(coeficiente));

                            //Creamos la combinación
                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion

                    #endregion
                }

                // ASCE 7-05
                if (normativa == "ASCE7-05" || normativa == "ASCE7-16")
                {
                    #region ESTADOS LÍMITES ÚLTIMOS
                    //Caso 1: D
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Gamma1.Text));
                    }
                    Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    string comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                    Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                    cont++;
                    #endregion
                    // Caso 2: D + S
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma2.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma3.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    // Caso 3: D + W + S
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma4.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma6.Text));

                            //Viento
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma5.Text));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    // Caso 4: D + W + S
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma7.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma9.Text));

                            //Viento
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma8.Text));

                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    // Caso 5: D + S + E
                    if (cargasQuake.Count != 0) //vista.Aplicar_SismoX.IsChecked == true || vista.Aplicar_SismoY.IsChecked == true
                    {
                        bool primerSismo = true;
                        // Caso 5a: +Ex +Ey
                        #region
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                        // Caso 5b: +Ex -Ey
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                        // Caso 5c: -Ex +Ey
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                        // Caso 5d: -Ex -Ey
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake)
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                        // Caso 5e: +Ey +Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                        // Caso 5f: +Ey -Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                        // Caso 5g: -Ey +Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                        // Caso 5h: -Ey -Ex
                        #region
                        primerSismo = true;
                        foreach (var nieve in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();
                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma10.Text));
                            }
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma11.Text));
                            foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                            {
                                Combination.Hipotesis.Add(sismo.Key);
                                if (primerSismo)
                                {
                                    Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma12.Text));
                                    primerSismo = false;
                                }
                                else
                                {
                                    string coeficiente = (-1 * double.Parse(vista.Gamma12.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                    Combination.Mayoracion.Add(double.Parse(coeficiente));
                                }
                            }
                            //Creamos la combinación
                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                            {
                                string signo = coef >= 0 ? "+" : "";
                                return $"{signo}{coef}{hip}";
                            }));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                        #endregion
                    }
                    // Caso 6: D + W
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma13.Text));
                            }
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma14.Text));

                            Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion
                    // Caso 7: D + E
                    #region
                    if (vista.Aplicar_SismoX.IsChecked == true || vista.Aplicar_SismoY.IsChecked == true)
                    {
                        bool primerSismo = true;
                        // Caso 7a: +Ex +Ey
                        #region
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        // Caso 7b: +Ex -Ey
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }
                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        // Caso 7c: -Ex +Ey
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        // Caso 7d: -Ex -Ey
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake)
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        // Caso 7e: +Ey +Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        // Caso 7f: +Ey -Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        // Caso 7g: -Ey +Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion
                        // Caso 7h: -Ey -Ex
                        #region
                        primerSismo = true;
                        Combination.Hipotesis.Clear();
                        Combination.Mayoracion.Clear();
                        foreach (var carga in cargasDead)
                        {
                            Combination.Hipotesis.Add(carga.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma15.Text));
                        }

                        foreach (var sismo in cargasQuake.AsEnumerable().Reverse())
                        {
                            Combination.Hipotesis.Add(sismo.Key);
                            if (primerSismo)
                            {
                                Combination.Mayoracion.Add(-1 * double.Parse(vista.Gamma16.Text));
                                primerSismo = false;
                            }
                            else
                            {
                                string coeficiente = (-1 * double.Parse(vista.Gamma16.Text) * double.Parse(vista.Porcentaje_sismo.Text) / 100).ToString("F2");
                                Combination.Mayoracion.Add(double.Parse(coeficiente));
                            }
                        }
                        //Creamos la combinación
                        Combinations.Add(new Combination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                        comb = string.Join("", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) =>
                        {
                            string signo = coef >= 0 ? "+" : "";
                            return $"{signo}{coef}{hip}";
                        }));
                        vista.Combinaciones_Carga.Items.Add("ULS" + cont.ToString() + ": " + comb);
                        Sap2000CreateCombination("ULS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                        cont++;
                        #endregion

                    }
                    #endregion
                    #endregion

                    #region ESTADOS LÍMITES DE SERVICIO
                    cont = 1;
                    // Caso 1: D
                    #region
                    Combination.Hipotesis.Clear();
                    Combination.Mayoracion.Clear();

                    foreach (var carga in cargasDead)
                    {
                        Combination.Hipotesis.Add(carga.Key);
                        Combination.Mayoracion.Add(double.Parse(vista.Gamma17.Text));
                    }
                    Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                    comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                    vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                    Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                    cont++;
                    #endregion

                    // Caso 2: D + S
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma18.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma19.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 3: D + S
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var snow in cargasSnow)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma20.Text));
                            }
                            Combination.Hipotesis.Add(snow.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma21.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 4: D + W
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma22.Text));
                            }
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma23.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 5: D + W + S
                    #region
                    foreach (var nieve in cargasSnow)
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma24.Text));
                            }
                            //Nieve
                            Combination.Hipotesis.Add(nieve.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma26.Text));

                            //Viento
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma25.Text));

                            //Creamos la combinación
                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion

                    // Caso 6: D + W
                    #region
                    if (vista.Aplicar_Nieve.IsChecked == true)//Si tenemos carga de nieve
                    {
                        foreach (var viento in cargasWind)
                        {
                            Combination.Hipotesis.Clear();
                            Combination.Mayoracion.Clear();

                            foreach (var carga in cargasDead)
                            {
                                Combination.Hipotesis.Add(carga.Key);
                                Combination.Mayoracion.Add(double.Parse(vista.Gamma27.Text));
                            }
                            Combination.Hipotesis.Add(viento.Key);
                            Combination.Mayoracion.Add(double.Parse(vista.Gamma28.Text));

                            Combinations.Add(new Combination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion));
                            comb = string.Join("+", Combination.Hipotesis.Zip(Combination.Mayoracion, (hip, coef) => $"{coef}{hip}"));
                            vista.Combinaciones_Carga.Items.Add("SLS" + cont.ToString() + ": " + comb);
                            Sap2000CreateCombination("SLS" + cont.ToString(), Combination.Hipotesis, Combination.Mayoracion);
                            cont++;
                        }
                    }
                    #endregion

                    #endregion
                }

                //Creamos la envolvente de ELU
                Sap2000CreateEnvelopeCombination();

                //Asignamos las combinaciones a las comprobaciones de acero
                Sap2000AssingDesignSteelCombos();

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

        public static List<(double,TextBox)> Coeficientes (CambiarCombinacionesTrackerAPP vista, string normativa)
        {
            using (ExcelPackage package = new ExcelPackage(ruta))
            {
                //Eurocódigo
                var Eurocodigo = new List<(double valor, TextBox caja)>
                {
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","B2"),vista.Permanente_Persistente_Favorable),
                    (ExcelFunctions.LeerCelda(ruta,"Eurocódigo","C2"),vista.Permanente_Desfavorable_SLS),
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
                    (ExcelFunctions.LeerCelda(ruta,"NTC-2018","C2"),vista.Permanente_Desfavorable_SLS),
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
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-05","B28"),vista.Gamma28),
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
                    (ExcelFunctions.LeerCelda(ruta,"ASCE7-16","B28"),vista.Gamma28),
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
            cHelper cHelper = MainView.Globales._myHelper;
            cOAPI mySapObject = MainView.Globales._mySapObject;
            cSapModel mySapModel = MainView.Globales._mySapModel;

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
            cHelper cHelper = MainView.Globales._myHelper;
            cOAPI mySapObject = MainView.Globales._mySapObject;
            cSapModel mySapModel = MainView.Globales._mySapModel;

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
            cHelper cHelper = MainView.Globales._myHelper;
            cOAPI mySapObject = MainView.Globales._mySapObject;
            cSapModel mySapModel = MainView.Globales._mySapModel;

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
            cHelper cHelper = MainView.Globales._myHelper;
            cOAPI mySapObject = MainView.Globales._mySapObject;
            cSapModel mySapModel = MainView.Globales._mySapModel;

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

        public static void Sap2000AssingDesignSteelCombos()
        {
            cHelper cHelper = MainView.Globales._myHelper;
            cOAPI mySapObject = MainView.Globales._mySapObject;
            cSapModel mySapModel = MainView.Globales._mySapModel;

            int num = 0;
            string[] array = new string[150];
            int ret = mySapModel.RespCombo.GetNameList(ref num, ref array);
            for (int i = 0; i < num; i++)
            {
                if (array[i].Substring(0, 3) == "ULS" && array[i].Length > 3)
                {
                    ret = mySapModel.DesignSteel.SetComboStrength(array[i], true);
                    ret = mySapModel.DesignColdFormed.SetComboStrength(array[i], true);
                }
                else if (array[i].Substring(0, 3) == "SLS" && array[i].Length > 3)
                {
                    ret = mySapModel.DesignSteel.SetComboDeflection(array[i], true);
                    ret = mySapModel.DesignColdFormed.SetComboDeflection(array[i], true);
                }
            }
        }
    }
}
