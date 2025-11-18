using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SmarTools.Model.Repository;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using SmarTools.View;
using FontAwesome.Sharp;
using System.Windows;
using System.Globalization;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;

namespace SmarTools.Model.Applications
{

    /// <summary>
    /// Cachea propiedades de sección leídas de las Database Tables de SAP2000.
    /// Usa "Frame Section Properties 01 - General" (A, S, I, Material, etc.)
    /// y calcula kg/m ≈ ρ·A si hay densidad disponible. Si no, usa A como proxy.
    /// </summary>
    internal static class SectionCatalog
    {
        private static readonly Dictionary<string, SectionData> _byName = new(StringComparer.OrdinalIgnoreCase);
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        private static bool _loaded;

        public static IReadOnlyDictionary<string, SectionData> All => _byName;

        private sealed class TablePayload
        {
            public string TableKey;
            public string[] FieldKeys;
            public string[] TableData;
            public int NumberRecords;
        }

        private static double ParseDouble(string s)
        {

            if (string.IsNullOrWhiteSpace(s)) return 0;

            // 1) Intento con Invariant (punto decimal)
            if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v;

            // 2) Intento con cultura actual (por si la decimal es coma)
            if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.CurrentCulture, out v))
                return v;

            // 3) Normalización simple: cambia coma→punto y reintenta
            var sDot = s.Replace(',', '.');
            if (double.TryParse(sDot, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out v))
                return v;

            // 4) Normalización inversa: punto→coma y reintenta con cultura actual
            var sComma = s.Replace('.', ',');
            if (double.TryParse(sComma, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.CurrentCulture, out v))
                return v;

            return 0;
        }

        private static bool TryColIndex(Dictionary<string, int> map, out int idx, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (map.TryGetValue(k, out idx)) return true;
            }
            idx = -1;
            return false;
        }

        private static string ResolveFrameSectionGeneralKey()
        {
            int n = 0;
            string[] keys = null, names = null;
            int[] importType = null;

            int ret = mySapModel.DatabaseTables.GetAvailableTables(ref n, ref keys, ref names, ref importType);
            if (ret != 0 || n == 0 || keys == null || names == null) return null;

            // Exacto
            for (int i = 0; i < n; i++)
                if (string.Equals(names[i], "Frame Section Properties 01 - General", StringComparison.OrdinalIgnoreCase))
                    return keys[i];

            // Parcial (por si varía el literal)
            for (int i = 0; i < n; i++)
                if ((names[i]?.IndexOf("Frame Section Properties", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 &&
                    (names[i]?.IndexOf("General", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                    return keys[i];

            return null;
        }

        private static bool TryGetTableForDisplayArray(string tableKey, out TablePayload payload)
        {
            payload = null;

            // a) Firma “completa”
            try
            {
                var db = mySapModel.DatabaseTables;
                string[] fieldKeyList = null;
                string[] fieldsIncluded = null;
                int numberRecords = 0, tableVersion = 0;
                string groupName = "All";
                string[] tableData = null;

                int ret = db.GetTableForDisplayArray(
                    tableKey,
                    ref fieldKeyList,
                    groupName,
                    ref tableVersion,
                    ref fieldsIncluded,
                    ref numberRecords,
                    ref tableData);

                if (ret == 0 && fieldsIncluded != null && fieldsIncluded.Length > 0 && tableData != null)
                {
                    payload = new TablePayload
                    {
                        TableKey = tableKey,
                        FieldKeys = fieldsIncluded,
                        TableData = tableData,
                        NumberRecords = numberRecords
                    };
                    return true;
                }
            }
            catch { /* probar variante */ }

            return false;
        }

        public static void LoadIfNeeded()
        {
            if (_loaded) return;

            //Si algo falla, dejamos que la excepción suba para depurar bien
            string tableKey = ResolveFrameSectionGeneralKey();
            if (string.IsNullOrEmpty(tableKey))
                throw new Exception("No se encontró la tabla 'Frame Section Properties 01 - General'.");

            mySapModel.SetPresentUnits(eUnits.N_mm_C);

            if (!TryGetTableForDisplayArray(tableKey, out var payload))
                throw new Exception($"GetTableForDisplayArray falló para '{tableKey}'.");

            var fieldKeys = payload.FieldKeys;
            var tableData = payload.TableData;
            int cols = fieldKeys.Length;

            if (cols <= 0 || tableData == null || tableData.Length == 0)
                throw new Exception("Tabla sin columnas o sin datos.");

            int rows = payload.NumberRecords;
            if (rows <= 0) rows = tableData.Length / Math.Max(cols, 1);

            System.Diagnostics.Debug.WriteLine($"[SectionCatalog] {tableKey}: cols={cols}, rows={rows}");
            System.Diagnostics.Debug.WriteLine("[SectionCatalog] Columns: " + string.Join(" | ", fieldKeys));

            var colMap = fieldKeys
                .Select((k, i) => (k, i))
                .ToDictionary(x => x.k, x => x.i, StringComparer.OrdinalIgnoreCase);

            // Índices tolerantes
            int iName, iMat, iArea, iS22, iS33, iI22, iI33, it2, it3, itw, itf, it_web, it_flange;

            TryColIndex(colMap, out iName, "Name", "SectionName", "PropName");
            TryColIndex(colMap, out iMat, "MatProp", "Material");
            TryColIndex(colMap, out iArea, "Area");
            TryColIndex(colMap, out iS22, "S22Left", "Syy", "S2");
            TryColIndex(colMap, out iS33, "S33Top", "Szz", "S3");
            TryColIndex(colMap, out iI22, "I22", "Iyy");
            TryColIndex(colMap, out iI33, "I33", "Izz");
            TryColIndex(colMap, out it2, "t2", "Width");
            TryColIndex(colMap, out it3, "t3", "Depth");
            TryColIndex(colMap, out itw, "tw", "t_web");
            TryColIndex(colMap, out itf, "tf", "t_flange");
            TryColIndex(colMap, out it_web, "t2b");
            TryColIndex(colMap, out it_flange, "tfb");

            // Recorre filas
            for (int r = 0; r < rows; r++)
            {
                string Get(int c)
                {
                    if (c < 0) return null;
                    int pos = r * cols + c;
                    return (pos >= 0 && pos < tableData.Length) ? tableData[pos] : null;
                }

                string name = Get(iName);
                if (string.IsNullOrWhiteSpace(name)) continue;

                string mat = Get(iMat).Split('-')[0].Trim();
                bool coldformed = Get(iMat).Contains("-CF");
                double.TryParse(Get(iArea), out double A);//m2
                double.TryParse(Get(iS22), out double S22);//m3
                double.TryParse(Get(iS33), out double S33);//m3
                double.TryParse(Get(iI22), out double I22);//m4
                double.TryParse(Get(iI33), out double I33);//m4
                double.TryParse(Get(it3), out double H);//m
                H = H * 1000;
                double.TryParse(Get(it2), out double B);//m
                B = B * 1000;
                double.TryParse(Get(itw), out double tw);//m
                tw = (tw == 0) ? 1000 : tw * 1000; // mm, si es 0 número alto para quedarnos con el espesor mínimo y que no salga 0
                double.TryParse(Get(itf), out double tf);//m
                tf = (tf==0) ? 1000 : tf * 1000;
                double.TryParse(Get(it_web), out double t_web);//m
                t_web = (t_web == 0) ? 1000 : t_web * 1000;
                double.TryParse(Get(it_flange), out double t_flange);//m
                t_flange = (t_flange == 0) ? 1000 : t_flange * 1000;
                double tMin = Math.Min(Math.Min(tw,tf), Math.Min(t_web,t_flange));

                var sd = new SectionData
                {
                    Name = name.Trim(),
                    Material = mat?.Trim(),
                    ShapeTag = !string.IsNullOrEmpty(name) ? name.Substring(0, 1).ToUpperInvariant() : "",
                    Area = A,
                    S22 = S22,
                    S33 = S33,
                    I22 = I22,
                    I33 = I33,
                    Height = H,
                    Width = B,
                    MinThickness = tMin,
                    KgPerM = (A > 0 ? A*7850 : 0), // proxy
                    IsColdFormed = coldformed
                };

                _byName[sd.Name] = sd;
            }

            _loaded = true;

            mySapModel.SetPresentUnits(eUnits.kN_m_C);
        }

    }
    internal sealed class SectionData
    {
        public string Name { get; set; }
        public string Material { get; set; }
        public string ShapeTag { get; set; } // "C","W","U","L",...
        public double Area { get; set; }
        public double S22 { get; set; }
        public double S33 { get; set; }
        public double I22 { get; set; }
        public double I33 { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double MinThickness { get; set; }
        public double KgPerM { get; set; } 
        public bool IsColdFormed { get; set; }
    }
    internal static class MaterialMatcher
    {

        // Sufijos típicos de conformado/variante en SAP (amplía si usas otros)
        private static readonly string[] VariantSuffixes = new[]
        {
        "-CF", "_CF", " CF",     // Conformado en frío
        };

        /// <summary>
        /// Devuelve la "familia/base" del material (p. ej., "S350GD" a partir de "S350GD-CF").
        /// </summary>
        public static string Normalize(string mat)
        {
            if (string.IsNullOrWhiteSpace(mat)) return string.Empty;
            var s = mat.Trim();

            // Elimina espacios duplicados
            s = Regex.Replace(s, @"\s+", " ");

            // Elimina sufijos conocidos (independiente de mayúsc/minúsc)
            foreach (var suf in VariantSuffixes)
            {
                if (s.EndsWith(suf, StringComparison.OrdinalIgnoreCase))
                {
                    s = s.Substring(0, s.Length - suf.Length).Trim();
                    break; // elimina un sufijo; si hay más, añade otro bucle o quita el break
                }
            }

            return s.ToUpperInvariant();
        }

        /// <summary>
        /// Devuelve true si el material de la sección pertenece a la misma familia
        /// que el seleccionado (acepta variantes como "-CF").
        /// </summary>
        public static bool IsMatch(string selectedBase, string matFromSection)
        {
            var a = Normalize(selectedBase);
            var b = Normalize(matFromSection);
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

    }
    class DimensionamientoRack
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static string ruta = @"Z:\300SmarTools\03 Uniones\Uniones 1VR5_" + MainView.Globales._revisionUniones1V + ".xlsx";

        public static void FiltrarPerfiles (DimensionamientoRackAPP vista)
        {
            //Limpiar listas
            List<ComboBox> listas = new List<ComboBox>
            {
                vista.PilaresDelanteros,
                vista.PilaresTraseros,
                vista.Vigas,
                vista.Correas,
                vista.DiagonalesDelanteras,
                vista.DiagonalesTraseras,
                vista.Estabilizador,
            };
            foreach (var lista in listas)
            {
                lista.ItemsSource = null;
                lista.Items.Clear();
            }

            //Cargar catálogo de secciones (una sola vez)
            SectionCatalog.LoadIfNeeded();

            //Variables de entrada
            string ambiente = vista.Ambiente.Text;
            string material_pilares = vista.Material_Pilares.Text;
            string material_vigas = vista.Material_Vigas.Text;
            string material_correas = vista.Material_Correas.Text;
            string material_diagonales = vista.Material_Diagonales.Text;
            string material_estabilizador = vista.Material_Estabilizadores.Text;

            //Obtener todas las secciones ordenadas por peso (Área)
            var all = SectionCatalog.All.Values
                .OrderBy(a => a.KgPerM)
                .ToList();

            //Helper locales
            static bool IsC(SectionData s) => s.ShapeTag == "C";
            static bool IsW(SectionData s) => s.ShapeTag == "W";
            static bool IsU(SectionData s) => s.ShapeTag == "U";
            static bool IsL(SectionData s) => s.ShapeTag == "L";

            //Pilares: C o W según check, con mínimos de H y t
            {
                IEnumerable<SectionData> pool;
                if (vista.Pilares_conformados.IsChecked == true)
                    pool = all.Where(IsC);
                else if (vista.Pilares_laminados.IsChecked == true)
                    pool = all.Where(IsW);
                else
                    pool = Enumerable.Empty<SectionData>();

                //Reglas mínimas

                string[] pilares = new string[pool.Count()];
                var poolList = pool.ToList();
                int contador = 0;
                for (int i =0;i<poolList.Count;i++)
                {
                    var s = poolList[i];
                    string type=s.ShapeTag;
                    double height = s.Height;
                    double t = s.MinThickness;
                    if(type=="W")
                    {
                        pilares[contador++] = s.Name;
                    }
                    else if(type=="C")
                    {
                        if(height>=90)
                        {
                            if(t>=2.5)
                            {
                                pilares[contador++] = s.Name;
                            }
                        }
                    }
                }

                Array.Resize(ref pilares, contador);
                AgregarPerfilesPorAmbiente(vista.PilaresDelanteros, pilares, material_pilares, ambiente);
                AgregarPerfilesPorAmbiente(vista.PilaresTraseros, pilares, material_pilares, ambiente);
                vista.PilaresDelanteros.SelectedIndex = 0;
                vista.PilaresTraseros.SelectedIndex = 0;
            }

            //Vigas: C con H>=90 y t>=1.5
            {
                var candidatos = new List<string>();
                var allList = all.ToList();

                for (int i = 0; i < allList.Count; i++)
                {
                    var s = allList[i];
                    if (IsC(s) && s.Height >= 90 && s.MinThickness >= 1.5)
                    {
                        candidatos.Add(s.Name);
                    }
                }
                AgregarPerfilesPorAmbiente(vista.Vigas, candidatos.ToArray(), material_vigas, "Normal");
                vista.Vigas.SelectedIndex = 0;
            }

            //Correas: C ambiente Normal
            {
                var candidatos = new List<string>();
                var allList = all.ToList();

                for (int i = 0; i < allList.Count; i++)
                {
                    var s = allList[i];
                    if (IsC(s))
                    {
                        candidatos.Add(s.Name);
                    }
                }
                AgregarPerfilesPorAmbiente(vista.Correas, candidatos.ToArray(), material_correas, "Normal");
                vista.Correas.SelectedIndex = 0;
            }

            //Diagonales: C o U
            {
                var candidatos = new List<string>();
                var allList = all.ToList();

                for (int i = 0; i < allList.Count; i++)
                {
                    var s = allList[i];
                    if (IsC(s) || IsU(s))
                    {
                        candidatos.Add(s.Name);
                    }
                }

                AgregarPerfilesPorAmbiente(vista.DiagonalesDelanteras, candidatos.ToArray(), material_diagonales, "Normal");
                AgregarPerfilesPorAmbiente(vista.DiagonalesTraseras, candidatos.ToArray(), material_diagonales, "Normal");
                vista.DiagonalesDelanteras.SelectedIndex = 0;
                vista.DiagonalesTraseras.SelectedIndex = 0;
            }

            //Estabilizador: L
            {
                var candidatos = new List<string>();
                var allList = all.ToList();

                for (int i = 0; i < allList.Count; i++)
                {
                    var s = allList[i];
                    if (IsL(s))
                    {
                        candidatos.Add(s.Name);
                    }
                }
                AgregarPerfilesPorAmbiente(vista.Estabilizador, candidatos.ToArray(), material_estabilizador, "Normal");
                vista.Estabilizador.SelectedIndex = 0;
            }
        }

        public static void AsignarPerfiles(DimensionamientoRackAPP vista)
        {
            if (mySapModel == null) return;

            SAP.AnalysisSubclass.UnlockModel(mySapModel);
            try
            {
                var logs = new List<string>();

                //Pilares
                int ret1 = 0;
                if (vista.Monoposte.IsChecked == true)
                {
                    string pilares = vista.PilaresDelanteros.Text;
                    ret1 = mySapModel.FrameObj.SetSection("01 Pilares", pilares, eItemType.Group);
                    CheckRet(ret1, logs, "pilares");
                }
                else if (vista.Biposte.IsChecked == true)
                {
                    string pilarDel = vista.PilaresDelanteros.Text;
                    string[] pilaresDel = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);

                    foreach(var p in pilaresDel)
                    {
                        int r = mySapModel.FrameObj.SetSection(p, pilarDel, eItemType.Objects);
                        if (r !=0) ret1 = r;
                    }
                    CheckRet(ret1, logs, "pilares delanteros");

                    string pilarTra = vista.PilaresTraseros.Text;
                    string[] pilaresTra = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
                    int ret1b = 0;
                    foreach (var p in pilaresTra)
                    {
                        int r = mySapModel.FrameObj.SetSection(p, pilarTra, eItemType.Objects);
                        if (r != 0) ret1b = r;
                    }
                    CheckRet(ret1b, logs, "pilares traseros");
                    ret1 = (ret1 != 0 || ret1b != 0) ? 1 : 0;
                }

                //Diagonales
                int ret2 = 0;
                if (vista.SinDiagonal.IsChecked == true)
                {
                    ret2 = 0;
                }
                else
                {
                    (string[] diagonalesD, string[] diagonalesT) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                    if (vista.UnaDiagonal.IsChecked == true)
                    {
                        string diagonal = vista.DiagonalesDelanteras.Text;
                        foreach (var d in diagonalesD)
                        {
                            int r = mySapModel.FrameObj.SetSection(d, diagonal, eItemType.Objects);
                            if (r != 0) ret2 = r;
                        }
                        CheckRet(ret2, logs, "diagonales");
                    }
                    else if (vista.DosDiagonal.IsChecked == true)
                    {
                        int ret2a =0, ret2b = 0;
                        string diagonalD = vista.DiagonalesDelanteras.Text;
                        string diagonalT = vista.DiagonalesTraseras.Text;

                        foreach (var d in diagonalesD)
                        {
                            int r = mySapModel.FrameObj.SetSection(d, diagonalD, eItemType.Objects);
                            if (r != 0) ret2a = r;
                        }
                        foreach (var d in diagonalesT)
                        {
                            int r = mySapModel.FrameObj.SetSection(d, diagonalT, eItemType.Objects);
                            if (r != 0) ret2b = r;
                        }
                        CheckRet(ret2a, logs, "diagonales delanteras");
                        CheckRet(ret2b, logs, "diagonales traseras");
                        ret2 = (ret2a != 0 || ret2b != 0) ? 1 : 0;
                    }
                }

                //Vigas, Correas y Estabilizador
                int ret3 = mySapModel.FrameObj.SetSection("02 Vigas", vista.Vigas.Text, eItemType.Group);
                CheckRet(ret3, logs, "vigas");

                int ret4 = mySapModel.FrameObj.SetSection("03 Correas", vista.Correas.Text, eItemType.Group);
                CheckRet(ret4, logs, "correas");

                int ret5 = mySapModel.FrameObj.SetSection("05 Arriostramiento Correas", vista.Estabilizador.Text, eItemType.Group);
                CheckRet(ret5, logs, "estabilizadores");

                //Mensajes finales
                if (ret1 == 0 && ret2 == 0 && ret3 == 0 && ret4 == 0 && ret5 == 0)
                    vista.Progreso.Items.Add("Perfiles asignados correctamente");
                else
                {
                    foreach (var msg in logs) vista.Progreso.Items.Add(msg);
                }
            }

            finally
            {
                //SAP.AnalysisSubclass.RunModel(mySapModel);
            }

        }

        private static void CheckRet (int ret, List<string> logs, string ctx)
        {
            if (ret != 0) logs.Add($"No se ha podido asignar correctamente: {ctx}");
        }

        public static void Dimensionar(DimensionamientoRackAPP vista)
        {
            // Limpieza de UI
            vista.Progreso.Items.Clear();
            vista.Resultados.ItemsSource = null;
            vista.Resultados.Items.Clear();

            var loadingWindow = new Status();

            if (vista.PilaresDelanteros.Items.Count == 0)
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Debes filtrar los perfiles antes de dimensionar el modelo", TipoIncidencia.Advertencia);
                ventana.ShowDialog();
                return;
            }

            try
            {
                loadingWindow.Show();
                loadingWindow.UpdateLayout();

                mySapModel.SelectObj.ClearSelection();
                mySapModel.SetPresentUnits(eUnits.kN_m_C);

                // ==============
                // 1) Listas de perfiles filtrados (desde combos)
                // ==============
                string[] PilaresDelanteros = vista.PilaresDelanteros.Items.Cast<string>().ToArray();
                string[] PilaresTraseros = vista.PilaresTraseros.Items.Cast<string>().ToArray();
                string[] Vigas = vista.Vigas.Items.Cast<string>().ToArray();
                string[] Correas = vista.Correas.Items.Cast<string>().ToArray();
                string[] DiagDelanteras = vista.DiagonalesDelanteras.Items.Cast<string>().ToArray();
                string[] DiagTraseras = vista.DiagonalesTraseras.Items.Cast<string>().ToArray();
                string[] Estabilizadores = vista.Estabilizador.Items.Cast<string>().ToArray();

                // ==============
                // 2) Configuración de grupos/elementos a dimensionar
                // ==============
                double.TryParse(vista.Ratio_Pilares.Text, out double pilares);
                double.TryParse(vista.Ratio_Vigas.Text, out double vigas);
                double.TryParse(vista.Ratio_Correas.Text, out double correas);
                double.TryParse(vista.Ratio_Diagonales.Text, out double diagonales);
                double.TryParse(vista.Ratio_Estabilizadores.Text, out double estabilizadores);
                double ratioPilares = pilares/100;
                double ratioVigas = vigas/100;
                double ratioCorreas = correas/100;
                double ratioDiagonales = diagonales/100;
                double ratioEstabilizadores = estabilizadores/100;

                var secciones = new Dictionary<string, (string barraControl, string[] listaperfiles, eItemType tipo, double ratiomax)>(StringComparer.OrdinalIgnoreCase);

                if (vista.Monoposte.IsChecked == true)
                {
                    secciones["01 Pilares"] = ("Column_1", PilaresDelanteros, eItemType.Group, ratioPilares);
                }
                else if (vista.Biposte.IsChecked == true)
                {
                    string[] pilaresDel = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
                    string[] pilaresTra = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);
                    SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "07 Pilares Delanteros", pilaresDel);
                    SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "08 Pilares Traseros", pilaresTra);

                    secciones["07 Pilares Delanteros"] = ("Column_d_2", PilaresDelanteros, eItemType.Group, ratioPilares);
                    secciones["08 Pilares Traseros"] = ("Column_i_2", PilaresTraseros, eItemType.Group, ratioPilares);
                }

                secciones["02 Vigas"] = ("Beam_1", Vigas, eItemType.Group, ratioVigas);
                secciones["03 Correas"] = ("Purlin_1_1_CorreaExterior.Inferior", Correas, eItemType.Group, ratioCorreas);

                if (vista.UnaDiagonal.IsChecked == true)
                {
                    secciones["04 Diagonales"] = ("Diag_Dcha_1", DiagDelanteras, eItemType.Group, ratioDiagonales);
                }
                if (vista.DosDiagonal.IsChecked == true)
                {
                    (string[] diagonalesD, string[] diagonalesT) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                    SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "09 Diagonales Delanteras", diagonalesD);
                    SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "10 Diagonales Traseras", diagonalesT);

                    secciones["09 Diagonales Delanteras"] = ("Diag_Dcha_1", DiagDelanteras, eItemType.Group, ratioDiagonales);
                    secciones["10 Diagonales Traseras"] = ("Diag_Izda_1", DiagTraseras, eItemType.Group, ratioDiagonales);
                }

                secciones["05 Arriostramiento Correas"] = ("Arriostr_21", Estabilizadores, eItemType.Group, ratioEstabilizadores);

                // ==============
                // 3) Estructuras de trabajo por grupo
                // ==============
                var ratiomaxByGroup = secciones.ToDictionary(kv => kv.Key, kv => kv.Value.ratiomax, StringComparer.OrdinalIgnoreCase);
                var designTypeByGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // "Laminado" / "Conformado"
                var currentSecByGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var ratiosByGroup = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

                // Inicializa sección y tipo actuales
                SectionCatalog.LoadIfNeeded();
                foreach (var kv in secciones)
                {
                    string grupo = kv.Key;
                    string barraControl = kv.Value.barraControl;
                    string secActual = GetSectionAssigned(barraControl);
                    if(string.IsNullOrWhiteSpace(secActual))
                    {
                        secActual = kv.Value.listaperfiles?.FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(secActual))
                        {
                            mySapModel.FrameObj.SetSection(grupo,secActual,eItemType.Group);
                        }
                    }
                    
                    currentSecByGroup[grupo] = secActual??"";
                    designTypeByGroup[grupo] = GetDesignTypeFromSection(secActual);
                }

                // Helper local: lectura robusta del ratio (con fallback de motor)
                double LeerRatioRobusto(string grupo)
                {
                    double r;
                    if (designTypeByGroup[grupo].Equals("Laminado", StringComparison.OrdinalIgnoreCase))
                    {
                        r = RatioGrupoLaminado(grupo);
                        if (r <= 0) r = RatioGrupoConformado(grupo); // fallback si devuelve 0
                    }
                    else
                    {
                        r = RatioGrupoConformado(grupo);
                        if (r <= 0) r = RatioGrupoLaminado(grupo);   // fallback si devuelve 0
                    }
                    return r;
                }
                
                // ==============
                // 4) Análisis inicial
                // ==============
                SAP.AnalysisSubclass.RunModel(mySapModel);

                // ==============
                // 5) Bucle de dimensionamiento (diseño "en caliente")
                // ==============
                const int maxLoops = 60; // cortafuegos
                for (int loop = 0; loop < maxLoops; loop++)
                {
                    bool anyChangeThisIter = false;
                    ratiosByGroup.Clear();

                    // Siempre lanzo ambos motores para evitar lecturas a 0
                    SAP.AnalysisSubclass.RunModel(mySapModel);
                    mySapModel.DesignColdFormed.StartDesign();
                    mySapModel.DesignSteel.StartDesign();

                    // Leer ratios de TODOS los grupos con fallback de motor
                    foreach (var g in secciones.Keys)
                    {
                        double r = LeerRatioRobusto(g);
                        ratiosByGroup[g] = r;
                    }

                    // ¿Quiénes siguen vivos? (ratio <=0 o ratio > rmax)
                    var alive = new List<string>();
                    foreach (var g in secciones.Keys)
                    {
                        double r = ratiosByGroup.TryGetValue(g, out var rr) ? rr : 0.0;
                        double rm = ratiomaxByGroup[g];

                        if (r <= 0 || r > rm)
                        {
                            // Solo muestro traza si de verdad no cumple (r > rm)
                            if (r > rm)
                                vista.Progreso.Items.Add($"{g}: Perfil {currentSecByGroup[g]} no válido. Ratio={r:F2} (máx {rm:F2})");

                            alive.Add(g);
                        }
                    }

                    vista.Progreso.Items.Add($"[Iter {loop}] Vivos: {(alive.Count > 0 ? string.Join(", ", alive) : "(ninguno)")}");
                    if (alive.Count == 0) break; // convergencia alcanzada

                    // Cambios de sección en BLOQUE solo para vivos
                    foreach (var g in alive)
                    {
                        var lista = secciones[g].listaperfiles ?? Array.Empty<string>();
                        if (lista.Length == 0)
                        {
                            vista.Progreso.Items.Add($"[Aviso] Grupo '{g}' sin lista de candidatos. Amplía filtros.");
                            continue;
                        }

                        string current = currentSecByGroup[g];
                        double r = ratiosByGroup.TryGetValue(g, out var rr) ? rr : 0.0;
                        double rm = ratiomaxByGroup[g];

                        // Si r <= 0 (lectura dudosa), fuerza salto moderado para no quedarte "clavada"
                        string next = PickJumpCandidate(current, lista, (r <= 0 ? 1.3 : r), (r <= 0 ? 1.0 : rm));

                        // Fallback: “siguiente” explícito si el salto no cambia
                        if (string.IsNullOrEmpty(next) || next.Equals(current, StringComparison.OrdinalIgnoreCase))
                        {
                            int i = Array.FindIndex(lista, s => s.Equals(current, StringComparison.OrdinalIgnoreCase));
                            if (i >= 0 && i + 1 < lista.Length) next = lista[i + 1];
                        }

                        if (!string.IsNullOrEmpty(next) && !next.Equals(current, StringComparison.OrdinalIgnoreCase))
                        {
                            SAP.AnalysisSubclass.UnlockModel(mySapModel);
                            int ret = mySapModel.FrameObj.SetSection(g, next, eItemType.Group);
                            if (ret == 0)
                            {
                                currentSecByGroup[g] = next;
                                anyChangeThisIter = true;

                                // Refresca tipo (puede cambiar de Conformado<->Laminado)
                                designTypeByGroup[g] = GetDesignTypeFromSection(next);
                            }
                            else
                            {
                                vista.Progreso.Items.Add($"[Aviso] No se pudo asignar '{next}' a '{g}'. ret={ret}");
                            }
                        }
                        else
                        {
                            vista.Progreso.Items.Add($"[Aviso] Grupo '{g}' sin candidato superior disponible (lista agotada o salto insuficiente).");
                        }
                    }

                    // Si no hubo ningún cambio, rompe para evitar bucle infinito
                    if (!anyChangeThisIter)
                    {
                        vista.Progreso.Items.Add("[Fin anticipado] No hubo cambios de sección en la iteración. Revisa listas de candidatos o filtros.");
                        break;
                    }

                    // Importante: NO llamar a RunAnalysis aquí; solo al final.
                }

                // ==============
                // 6) Confirmación final (reanálisis + diseño) y ratios válidos
                // ==============
                SAP.AnalysisSubclass.RunModel(mySapModel);
                mySapModel.DesignColdFormed.StartDesign();
                mySapModel.DesignSteel.StartDesign();

                ratiosByGroup.Clear();
                foreach (var g in secciones.Keys)
                    ratiosByGroup[g] = LeerRatioRobusto(g);

                // Traza final (diagnóstico)
                vista.Progreso.Items.Add("=== Ratios finales por grupo ===");
                foreach (var kv in ratiosByGroup)
                    vista.Progreso.Items.Add($"  {kv.Key}: {kv.Value:F3}");

                // ==============
                // 7) Resultados (etiquetas bonitas → claves reales), sin duplicados
                // ==============
                vista.Resultados.ItemsSource = null;
                vista.Resultados.Items.Clear();

                var resumen = new Dictionary<string, (string uiToGroup, string[] nombreBarras)>(StringComparer.OrdinalIgnoreCase);

                if (vista.Monoposte.IsChecked == true)
                    resumen["Pilares"] = ("01 Pilares", new[] { "Column_1" });
                else
                {
                    resumen["Pilares Delanteros"] = ("07 Pilares Delanteros", new[] { "Column_d_2" });
                    resumen["Pilares Traseros"] = ("08 Pilares Traseros", new[] { "Column_i_2" });
                }

                resumen["Vigas"] = ("02 Vigas", new[] { "Beam_1" });
                resumen["Correas"] = ("03 Correas", new[] { "Purlin_1_1_CorreaExterior.Inferior" });

                if (vista.UnaDiagonal.IsChecked == true)
                    resumen["Diagonales"] = ("04 Diagonales", new[] { "Diag_Dcha_1" });
                if (vista.DosDiagonal.IsChecked == true)
                {
                    resumen["Diagonales Delanteras"] = ("09 Diagonales Delanteras", new[] { "Diag_Dcha_1" });
                    resumen["Diagonales Traseras"] = ("10 Diagonales Traseras", new[] { "Diag_Izda_1" });
                }

                resumen["Estabilizadores"] = ("05 Arriostramiento Correas", new[] { "Arriostr_21" });

                foreach (var kv in resumen)
                {
                    string etiquetaUI = kv.Key;
                    string claveGrupo = kv.Value.uiToGroup.Trim();

                    if (!ratiosByGroup.TryGetValue(claveGrupo, out double r))
                    {
                        vista.Progreso.Items.Add($"[Resultados] No hay ratio final para '{claveGrupo}'. Claves: {string.Join(", ", ratiosByGroup.Keys)}");
                        continue;
                    }

                    if (r == 0)
                    {
                        vista.Progreso.Items.Add($"[Resultados] Ratio 0.00 para '{claveGrupo}'. (Comprueba motor de diseño para la lectura final).");
                        continue;
                    }

                    if (r < 1.0) // tolerancia: cambia a <=1.02 si te interesa
                    {
                        Resultados(vista, etiquetaUI, kv.Value.nombreBarras, r);
                    }
                    else
                    {
                        vista.Progreso.Items.Add($"[Resultados] '{claveGrupo}' ratio={r:F2} (no cumple < 1.0).");
                    }
                }
            }
            finally
            {
                try { loadingWindow.Close(); }
                catch
                {
                    var ventana = new Incidencias();
                    ventana.ConfigurarIncidencia("Se ha producido un error", TipoIncidencia.Error);
                    ventana.ShowDialog();
                }
            }
        }



        public static void ObtenerMateriales (DimensionamientoRackAPP vista)
        {
            if (mySapModel == null) return;

            // 1) Pedir la lista de materiales de tipo acero al API (tamaño dinámico)
            int numberNames = 0;
            string[] materiales = null;
            int ret = mySapModel.PropMaterial
                .GetNameList(ref numberNames, ref materiales, eMatType.Steel);

            // Valor por defecto en la UI
            vista.Ambiente.SelectedIndex = 0;

            if (ret != 0 || materiales == null || materiales.Length == 0)
                return;

            //Agrupar por "base"
            var bases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach ( var m in materiales )
            {
                var baseName = MaterialMatcher.Normalize(m);
                if(!string.IsNullOrWhiteSpace(baseName))
                    bases.Add(baseName);
            }

            // 3) Reglas de filtrado por familia (fácil de mantener)
            //    Si más adelante quieres parametrizarlo desde la UI, basta con exponer estas listas.
            string[] VigasDiagCorreasEst = { "S350GD", "S420GD", "S450GD", "S355JR", "S420MC", "S460MC" };
            string[] Pilares = { "S355JR", "S420MC", "S460MC" };

            // Helper: filtra por las "etiquetas" (Contains) de la familia
            static IEnumerable<string> FilterByGrades(
                IEnumerable<string> pool,
                IEnumerable<string> grades) =>
                pool.Where(m => grades.Any(g =>
                             m.IndexOf(g, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .OrderBy(m => m, StringComparer.CurrentCultureIgnoreCase);

            var matVigasDiagCorreasEst = FilterByGrades(bases, VigasDiagCorreasEst).ToList();
            var matPilares = FilterByGrades(bases, Pilares).ToList();

            // 4) Limpiar combos y asignar ItemsSource en bloque (más eficiente que Items.Add)
            vista.Material_Pilares.ItemsSource = matPilares;
            vista.Material_Vigas.ItemsSource = matVigasDiagCorreasEst;
            vista.Material_Diagonales.ItemsSource = matVigasDiagCorreasEst;
            vista.Material_Correas.ItemsSource = matVigasDiagCorreasEst;
            vista.Material_Estabilizadores.ItemsSource = matVigasDiagCorreasEst;

            // 5) Selección segura (solo si hay elementos)
            if (vista.Material_Pilares.Items.Count > 0) vista.Material_Pilares.SelectedIndex = 0;
            if (vista.Material_Vigas.Items.Count > 0) vista.Material_Vigas.SelectedIndex = 0;
            if (vista.Material_Diagonales.Items.Count > 0) vista.Material_Diagonales.SelectedIndex = 0;
            if (vista.Material_Correas.Items.Count > 0) vista.Material_Correas.SelectedIndex = 0;
            if (vista.Material_Estabilizadores.Items.Count > 0) vista.Material_Estabilizadores.SelectedIndex = 0;
        }

        public static void AgregarPerfilesPorAmbiente(ComboBox combo, IEnumerable<string> perfiles, string materialDeseado, string ambienteDeseado)
        {
            //Material/Ambiente
            SectionCatalog.LoadIfNeeded();

            var lista = new List<string>();

            foreach(var name in perfiles)
            {
                if (!SectionCatalog.All.TryGetValue(name, out var sec))
                    continue;

                //Filtro de material por familia
                if(!string.IsNullOrWhiteSpace(materialDeseado))
                {
                    if(!MaterialMatcher.IsMatch(materialDeseado,sec.Material))
                        continue;
                }

                //Ambiente
                bool pasaAmbiente=true;

                var partes = name.Split('/').Select(p=>p.Trim()).ToArray();
                if(ambienteDeseado?.Contains("Ligeramente",StringComparison.OrdinalIgnoreCase)==true)
                {
                    //Si el nombre contiene etiqueta "-0.5", dejamos pasar
                    if(!(partes.Length==3 && partes[2].Contains("-0.5")))
                        pasaAmbiente = false;
                }
                else if(ambienteDeseado?.Contains("Altamente",StringComparison.OrdinalIgnoreCase)==true)
                {
                    if(!(partes.Length ==3 && partes[2].Contains("-1")))
                        pasaAmbiente = false;
                }
                else if(ambienteDeseado?.Equals("Normal",StringComparison.OrdinalIgnoreCase) == true)
                {
                    if(!(partes.Length==2 || partes.Length==1))
                        pasaAmbiente = false;
                }
                if(pasaAmbiente)
                    lista.Add(name);
            }

            // Cargamos la UI de una vez (mejor que bucle)
            combo.ItemsSource = lista;
        }

        public static double RatioGrupo(DimensionamientoRackAPP vista, string grupo, string barra, string[] listaperfiles, eItemType tipo, string tiposeccion)
        {
            mySapModel.SelectObj.ClearSelection();

            //Variables
            int numberItems = 0;
            int[] ObjectType = new int[1], ratioType = new int[1];
            string[] ObjectName = new string[1], ComboName = new string[1], ErrorSummary = new string[1], WarningSummary = new string[1], PropName = new string[1];
            double[] Ratio = new double[1], location = new double[1];

            switch (tiposeccion)
            {
                case "Laminado":

                    mySapModel.DesignSteel.StartDesign();

                    mySapModel.SelectObj.Group(grupo);
                    mySapModel.SelectObj.GetSelected(ref numberItems, ref ObjectType, ref ObjectName);
                    mySapModel.DesignSteel.GetSummaryResults(grupo, ref numberItems, ref ObjectName, ref Ratio, ref ratioType, ref location, ref ComboName, ref ErrorSummary, ref WarningSummary, tipo);

                    if (ErrorSummary.Contains("Section is too slender"))
                    {
                        return 1;
                    }
                    else
                    {
                        return Ratio.Max();
                    }

                case "Conformado":

                    mySapModel.DesignColdFormed.StartDesign();
                    mySapModel.SelectObj.Group(grupo);
                    mySapModel.SelectObj.GetSelected(ref numberItems, ref ObjectType, ref ObjectName);
                    mySapModel.DesignColdFormed.GetSummaryResults(grupo, ref numberItems, ref ObjectName, ref Ratio, ref ratioType, ref location, ref ComboName, ref ErrorSummary, ref WarningSummary, tipo);

                    if (ErrorSummary.Contains("Section is too slender"))
                    {
                        return 1;
                    }
                    else
                    {
                        return Ratio.Max();
                    }
            }
            mySapModel.SelectObj.ClearSelection();

            return 0;
        }

        public static double RatioGrupoConformado(string grupo)
        {
            mySapModel.SelectObj.ClearSelection();

            //Variables
            int numberItems = 0;
            int[] ObjectType = new int[1], ratioType = new int[1];
            string[] ObjectName = new string[1], ComboName = new string[1], ErrorSummary = new string[1], WarningSummary = new string[1], PropName = new string[1];
            double[] Ratio = new double[1], location = new double[1];

            mySapModel.SelectObj.Group(grupo);
            mySapModel.SelectObj.GetSelected(ref numberItems, ref ObjectType, ref ObjectName);
            mySapModel.DesignColdFormed.GetSummaryResults(grupo, ref numberItems, ref ObjectName, ref Ratio, ref ratioType, ref location, ref ComboName, ref ErrorSummary, ref WarningSummary, eItemType.Group);
            mySapModel.SelectObj.ClearSelection();

            if (ErrorSummary.Contains("Section is too slender"))
            {
                return 1;
            }
            else
            {
                return Ratio.Max();
            }
        }

        public static double RatioGrupoLaminado(string grupo)
        {
            mySapModel.SelectObj.ClearSelection();

            //Variables
            int numberItems = 0;
            int[] ObjectType = new int[1], ratioType = new int[1];
            string[] ObjectName = new string[1], ComboName = new string[1], ErrorSummary = new string[1], WarningSummary = new string[1], PropName = new string[1];
            double[] Ratio = new double[1], location = new double[1];

            mySapModel.SelectObj.Group(grupo);
            mySapModel.SelectObj.GetSelected(ref numberItems, ref ObjectType, ref ObjectName);
            mySapModel.DesignSteel.GetSummaryResults(grupo, ref numberItems, ref ObjectName, ref Ratio, ref ratioType, ref location, ref ComboName, ref ErrorSummary, ref WarningSummary, eItemType.Group);
            mySapModel.SelectObj.ClearSelection();

            if (ErrorSummary.Contains("Section is too slender"))
            {
                return 1;
            }
            else
            {
                return Ratio.Max();
            }
        }

        public static void CambiarSecciones (string grupo, string[] listaperfiles)
        {
            SAP.AnalysisSubclass.UnlockModel(mySapModel);
            mySapModel.SelectObj.ClearSelection();
            mySapModel.SelectObj.Group(grupo) ;
            SAP.DesignSubclass.ChangeSection(mySapModel, listaperfiles);
        }

        public static void Resultados(DimensionamientoRackAPP vista, string elemento, string[]nombreBarras, double ratio)
        {
            string[] seccion_tipo = SAP.DesignSubclass.ObtenerSeccionYTipo(mySapModel, nombreBarras[0]);
            vista.Resultados.Items.Add(elemento+": "+seccion_tipo[0]+" Ratio: "+ratio.ToString("F2"));
        }

        public static string[] ObtenerSeccionYTipo(cSapModel mySapModel, string barra)
        {
            SAP.AnalysisSubclass.RunModel(mySapModel);

            //Variables necesarias para SAP2000
            int ret = 0;
            int numberItems = 0;
            int[] objectType = new int[1];
            string[] itemName = new string[1];
            string section = "";

            //Variable de salida
            string[] seccion_tipo = new string[2];

            //Interacción con SAP
            mySapModel.SelectObj.ClearSelection();
            ret = mySapModel.FrameObj.SetSelected(barra, true, eItemType.Objects);

            if (ret == 0)
            {
                mySapModel.SelectObj.GetSelected(ref numberItems, ref objectType, ref itemName);
                ret = mySapModel.DesignColdFormed.GetDesignSection(barra, ref section);
                if (section == "")
                {
                    mySapModel.DesignSteel.GetDesignSection(barra, ref section);
                    seccion_tipo = new string[] { section, "Laminado" };
                }
                else
                {
                    seccion_tipo = new string[] { section, "Conformado" };
                }

            }

            return seccion_tipo;
        }

        private static string GetSectionAssigned (string objectName)
        {
            string sec = null;
            string sAuto = null;
            try
            {
                int ret = mySapModel.FrameObj.GetSection(objectName, ref sec,ref sAuto);
                if (ret != 0 || string.IsNullOrWhiteSpace(sec)) return null;
                return sec.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static string GetDesignTypeFromSection(string sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName)) return "Conformado";
            if (SectionCatalog.All.TryGetValue(sectionName.Trim(), out var sd))
                return sd.IsColdFormed ? "Conformado" : "Laminado";

            // Heurística de respaldo si no estuviera en el catálogo
            // (mejor evitarla, pero nos protege de nombres “fuera de catálogo”)
            if (sectionName.IndexOf("-CF", StringComparison.OrdinalIgnoreCase) >= 0 ||
                sectionName.IndexOf("_CF", StringComparison.OrdinalIgnoreCase) >= 0 ||
                sectionName.IndexOf(" CF", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Conformado";

            return "Conformado"; // por defecto, mantén Conformado
        }

        private static string PickJumpCandidate(string current, string[] lista, double ratio, double ratiomax)
        {
            if (lista == null || lista.Length == 0) return current;
            if (!SectionCatalog.All.TryGetValue(current, out var cur)) return NextAfterCurrent(current, lista);

            // ==== AJUSTES DE SUAVIDAD ====
            const double minScale = 1.03;  // salto mínimo (más pequeño)
            const double maxScale = 1.50;  // techo de salto
            const double alpha = 0.70;  // suavizado (0.6–0.8 suele ir bien)
            const int maxStep = 2;     // saltar como máximo +1 posiciones por iteración
            const double maxIncArea = 0.15;// tope absoluto por iteración (+12%)

            // 1) Escala “suave”
            double excess = ratio / Math.Max(ratiomax, 1e-6);
            double scaled = Math.Pow(excess, alpha);
            double scale = Math.Max(minScale, Math.Min(maxScale, scaled));

            // 2) Métrica por Área (como quedaste)
            double curMetric = Math.Max(cur.Area, 1e-9); // evita 0

            // 3) Target “capado” por incremento absoluto de Área
            double targetMetric = curMetric * scale;
            targetMetric = Math.Min(targetMetric, curMetric * (1.0 + maxIncArea));

            // 4) Primera candidata que alcance el target
            int iCur = Array.FindIndex(lista, s => s.Equals(current, StringComparison.OrdinalIgnoreCase));
            int iChosen = -1;
            for (int i = Math.Max(0, iCur + 1); i < lista.Length; i++)
            {
                if (!SectionCatalog.All.TryGetValue(lista[i], out var sd)) continue;
                double metric = Math.Max(sd.Area, 1e-9);
                if (metric >= targetMetric) { iChosen = i; break; }
            }

            // 5) Si no hay ninguna que alcance el target, coge la siguiente
            if (iChosen < 0)
                iChosen = (iCur >= 0 && iCur + 1 < lista.Length) ? iCur + 1 : iCur;

            // 6) Limita el salto por posiciones
            if (iCur >= 0 && iChosen > iCur && (iChosen - iCur) > maxStep)
                iChosen = iCur + maxStep;

            // 7) Devuelve candidata
            return (iChosen >= 0 && iChosen < lista.Length) ? lista[iChosen] : current;

        }

        private static string NextAfterCurrent(string current, string[] lista)
        {
            int i = Array.FindIndex(lista, s => s.Equals(current, StringComparison.OrdinalIgnoreCase));
            if (i < 0) return lista[0];
            return (i + 1 < lista.Length) ? lista[i + 1] : lista[i]; // si ya es la última, reintenta con la misma
        }

    }
}
