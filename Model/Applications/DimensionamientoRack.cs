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

namespace SmarTools.Model.Applications
{
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
                lista.Items.Clear();
            }

            //Ejecutar análisis y obtener perfiles
            string[] Perfiles = SAP.ElementFinderSubclass.GetFrameSectionsInWeightOrder(mySapModel);

            //Variables de entrada
            string ambiente = vista.Ambiente.Text;
            string material_pilares = vista.Material_Pilares.Text;
            string material_vigas = vista.Material_Vigas.Text;
            string material_correas = vista.Material_Correas.Text;
            string material_diagonales = vista.Material_Diagonales.Text;
            string material_estabilizador = vista.Material_Estabilizadores.Text;

            string[] PerfilesC = Perfiles.Where(s => s.StartsWith("C")).ToArray();
            int seccion = 0;
            double espesor = 0;

            //Filtrado
            #region PILARES
            string[] PerfilesCyW = null;
            if (vista.Pilares_conformados.IsChecked == true)
            {
                PerfilesCyW = Perfiles.Where(s => s.StartsWith("C")).ToArray();
            }
            else if (vista.Pilares_laminados.IsChecked == true)
            {
                PerfilesCyW = Perfiles.Where(s => s.StartsWith("W")).ToArray();
            }
            
            string[] Pilares = new string[Perfiles.Length];
            int index = 0;

            foreach (var perfil in PerfilesCyW)
            {
                if(perfil.StartsWith("C"))
                {
                    seccion = SAP.DesignSubclass.ObtenerSeccion(mySapModel, perfil);
                    espesor = SAP.DesignSubclass.ObtenerEspesor(mySapModel, perfil);
                }
            
                if (perfil.StartsWith("W") || (seccion>=90 && espesor>=2.5))
                {
                    Pilares[index++] = perfil;
                }
            }

            string[] pilaresFiltrados = Pilares.Where(s=>s!=null).ToArray();
            AgregarPerfilesPorAmbiente(vista.PilaresDelanteros, pilaresFiltrados, material_pilares, ambiente);
            AgregarPerfilesPorAmbiente(vista.PilaresTraseros,pilaresFiltrados, material_pilares,ambiente);
            vista.PilaresDelanteros.SelectedIndex = 0;
            vista.PilaresTraseros.SelectedIndex = 0;
            #endregion

            #region VIGAS
            ambiente = "Normal";
            index = 0;
            string[] vigas = new string[PerfilesC.Length];

            foreach (var perfil in PerfilesC)
            {
                seccion = SAP.DesignSubclass.ObtenerSeccion(mySapModel, perfil);
                espesor = SAP.DesignSubclass.ObtenerEspesor(mySapModel, perfil);

                if (seccion >= 90 && espesor >= 1.5)
                {
                    vigas[index++] = perfil;
                }

            }

            string[] vigasFiltradas = vigas.Where(s => s != null).ToArray();
            AgregarPerfilesPorAmbiente(vista.Vigas, vigasFiltradas, material_vigas, ambiente);
            vista.Vigas.SelectedIndex = 0;
            #endregion

            #region CORREAS
            AgregarPerfilesPorAmbiente(vista.Correas, PerfilesC, material_correas, ambiente);
            vista.Correas.SelectedIndex = 0;
            #endregion

            #region DIAGONALES
            string[] Diagonales = Perfiles.Where(s => s.StartsWith("C")|| s.StartsWith("U")).ToArray();
            AgregarPerfilesPorAmbiente(vista.DiagonalesDelanteras,Diagonales, material_diagonales, ambiente);
            AgregarPerfilesPorAmbiente(vista.DiagonalesTraseras,Diagonales, material_diagonales,ambiente);
            vista.DiagonalesDelanteras.SelectedIndex = 0;
            vista.DiagonalesTraseras.SelectedIndex = 0;
            #endregion

            #region ESTABILIZADOR
            string[] Estabilizadores = Perfiles.Where(s=>s.StartsWith("L")).ToArray();
            AgregarPerfilesPorAmbiente(vista.Estabilizador, Estabilizadores, material_estabilizador, ambiente);
            vista.Estabilizador.SelectedIndex = 0;
            #endregion
        }

        public static void AsignarPerfiles(DimensionamientoRackAPP vista)
        {
            if (mySapModel != null)
            {
                SAP.AnalysisSubclass.UnlockModel(mySapModel);

                //Pilares
                int ret1 = 1;

                if (vista.Monoposte.IsChecked == true)
                {
                    string Pilares = vista.PilaresDelanteros.Text;
                    ret1 = mySapModel.FrameObj.SetSection("01 Pilares", Pilares, eItemType.Group);
                }
                else if (vista.Biposte.IsChecked == true)
                {
                    string PilarDelantero = vista.PilaresDelanteros.Text;
                    string[] pilaresDelanteros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
                    foreach (var pilar in pilaresDelanteros)
                    {
                        ret1=mySapModel.FrameObj.SetSection(pilar,PilarDelantero,eItemType.Objects);
                    }
                    string PilarTrasero = vista.PilaresTraseros.Text;
                    string[] pilaresTraseros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);
                    foreach (var pilar in pilaresTraseros)
                    {
                        ret1=mySapModel.FrameObj.SetSection(pilar,PilarTrasero,eItemType.Objects);
                    }
                }

                //Diagonales
                int ret2 = 1;
                if (vista.SinDiagonal.IsChecked == true)
                {
                    ret2 = 0;
                }
                else if (vista.UnaDiagonal.IsChecked == true)
                {
                    string Diagonal = vista.DiagonalesDelanteras.Text;
                    (string[] diagonalesD, string[] diagonalesT) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                    foreach(var diagonal in diagonalesD)
                    {
                        ret2=mySapModel.FrameObj.SetSection(diagonal,Diagonal,eItemType.Objects);
                    }
                }
                else if (vista.DosDiagonal.IsChecked == true)
                {
                    string DiagonalD = vista.DiagonalesDelanteras.Text;
                    string DiagonalT = vista.DiagonalesTraseras.Text;
                    (string[] diagonalesD, string[] diagonalesT) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                    foreach (var diagonal in diagonalesD)
                    {
                        ret2 = mySapModel.FrameObj.SetSection(diagonal, DiagonalD, eItemType.Objects);
                    }
                    foreach (var diagonal in diagonalesT)
                    {
                        ret2 = mySapModel.FrameObj.SetSection(diagonal, DiagonalT, eItemType.Objects);
                    }
                }

                //Resto de elementos
                int ret3 = 1;
                ret3 = mySapModel.FrameObj.SetSection("02 Vigas",vista.Vigas.Text,eItemType.Group);
                int ret4 = 1;
                ret4 = mySapModel.FrameObj.SetSection("03 Correas", vista.Correas.Text, eItemType.Group);
                int ret5 = 1;
                ret5 = mySapModel.FrameObj.SetSection("05 Arriostramiento Correas", vista.Estabilizador.Text, eItemType.Group);

                if (ret1==0 && ret2 == 0 && ret3==0 && ret4 == 0 && ret5==0)
                {
                    vista.Progreso.Items.Add("Perfiles asignados correctamente");
                }
                if (ret1==1)
                {
                    vista.Progreso.Items.Add("No se ha podido asignar correctamente los perfiles a los pilares");
                }
                if (ret2 == 1)
                {
                    vista.Progreso.Items.Add("No se ha podido asignar correctamente los perfiles a las diagonales");
                }
                if (ret3 == 1)
                {
                    vista.Progreso.Items.Add("No se ha podido asignar correctamente los perfiles a las vigas");
                }
                if (ret4 == 1)
                {
                    vista.Progreso.Items.Add("No se ha podido asignar correctamente los perfiles a las correas");
                }
                if (ret5 == 1)
                {
                    vista.Progreso.Items.Add("No se ha podido asignar correctamente los perfiles a los estabilizadores");
                }
            }
        }

        public static void Dimensionar(DimensionamientoRackAPP vista)
        {
            //Preparamos el modelo
            vista.Progreso.Items.Clear();
            vista.Resultados.Items.Clear();

            var loadingWindow = new Status();

            if (vista.PilaresDelanteros.Items.Count == 0)
            {
                var ventana = new Incidencias();
                ventana.ConfigurarIncidencia("Debes filtrar los perfiles antes de dimensionar el modelo", TipoIncidencia.Advertencia);
                ventana.ShowDialog();
            }
            else
            {
                try
                {
                    loadingWindow.Show();
                    loadingWindow.UpdateLayout();

                    mySapModel.SelectObj.ClearSelection();

                    //Listas de perfiles filtrados
                    string[] PilaresDelanteros = vista.PilaresDelanteros.Items.Cast<string>().ToArray();
                    string[] PilaresTraseros = vista.PilaresTraseros.Items.Cast<string>().ToArray();
                    string[] Vigas = vista.Vigas.Items.Cast<string>().ToArray();
                    string[] Correas = vista.Correas.Items.Cast<string>().ToArray();
                    string[] DiagonalesDelanteras = vista.DiagonalesDelanteras.Items.Cast<string>().ToArray();
                    string[] DiagonalesTraseras = vista.DiagonalesTraseras.Items.Cast<string>().ToArray();
                    string[] Estabilizadores = vista.Estabilizador.Items.Cast<string>().ToArray();

                    //Configuramos la lista de elementos a dimensionar
                    #region
                    double ratioPilares = 0.9;
                    double ratioVigas = 0.9;
                    double ratioCorreas = 0.9;
                    double ratioDiagonales = 0.9;
                    double ratioEstabilizadores = 0.9;

                    var secciones = new Dictionary<string, (string barraControl, string[] listaperfiles, eItemType tipo, double ratiomax)>();
                    if (vista.Monoposte.IsChecked == true)
                    {
                        secciones["01 Pilares"] = ("Column_1", PilaresDelanteros, eItemType.Group, ratioPilares);
                    }
                    else if (vista.Biposte.IsChecked == true)
                    {
                        string[] pilaresDelanteros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresDelanteros(mySapModel);
                        string[] pilaresTraseros = SAP.ElementFinderSubclass.FixedSubclass.ListaPilaresTraseros(mySapModel);
                        SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "07 Pilares Delanteros", pilaresDelanteros);
                        SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "08 Pilares Traseros", pilaresTraseros);

                        secciones["07 Pilares Delanteros"] = ("Column_d_2", PilaresDelanteros, eItemType.Group, ratioPilares);
                        secciones["08 Pilares Traseros"] = ("Column_i_2", PilaresTraseros, eItemType.Group, ratioPilares);
                    }
                    secciones["02 Vigas"] = ("Beam_1", Vigas, eItemType.Group, ratioVigas);
                    secciones["03 Correas"] = ("Purlin_1_1_CorreaExterior.Inferior", Correas, eItemType.Group, ratioCorreas);

                    if (vista.UnaDiagonal.IsChecked == true)
                    {
                        secciones["04 Diagonales"] = ("Diag_Dcha_1", DiagonalesDelanteras, eItemType.Group, ratioDiagonales);
                    }
                    if (vista.DosDiagonal.IsChecked == true)
                    {
                        (string[] diagonalesDelanteras, string[] diagonalesTraseras) = SAP.ElementFinderSubclass.FixedSubclass.ListaDiagonales(mySapModel);
                        SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "09 Diagonales Delanteras", diagonalesDelanteras);
                        SAP.DesignSubclass.CrearYAgregarAGrupo(mySapModel, "10 Diagonales Traseras", diagonalesTraseras);

                        secciones["09 Diagonales Delanteras"] = ("Diag_Dcha_1", DiagonalesDelanteras, eItemType.Group, ratioDiagonales);
                        secciones["10 Diagonales Traseras"] = ("Diag_Izda_1", DiagonalesTraseras, eItemType.Group, ratioDiagonales);
                    }
                    secciones["05 Arriostramiento Correas"] = ("Arriostr_21", Estabilizadores, eItemType.Group, ratioEstabilizadores);
                    #endregion
                    //Dimensionamiento
                    #region
                    List<double> ratios = new List<double>();

                    bool comprobacion = false;
                    int index = 0;

                    mySapModel.SetPresentUnits(eUnits.kN_m_C);
                    SAP.AnalysisSubclass.RunModel(mySapModel);
                    var secciontipo = new Dictionary<string, (string seccion, string tipo)>();

                    while (comprobacion == false)
                    {
                        ratios.Clear();

                        mySapModel.DesignColdFormed.StartDesign();
                        mySapModel.DesignSteel.StartDesign();
                        foreach (var propiedad in secciones)
                        {
                            string grupo = propiedad.Key;
                            string barraControl = propiedad.Value.barraControl;
                            string[] listaperfiles = propiedad.Value.listaperfiles;
                            eItemType tipo = propiedad.Value.tipo;
                            string[] seccion_tipo = ObtenerSeccionYTipo(mySapModel, barraControl);
                            secciontipo[grupo] = (seccion_tipo[0], seccion_tipo[1]);
                        }

                        foreach (var propiedad in secciones)
                        {
                            string grupo = propiedad.Key;
                            string barraControl = propiedad.Value.barraControl;
                            string[] listaperfiles = propiedad.Value.listaperfiles;
                            eItemType tipo = propiedad.Value.tipo;
                            double ratio = 0;
                            if (vista.Pilares_conformados.IsChecked == true)
                            {
                                mySapModel.DesignColdFormed.StartDesign();
                                ratio = RatioGrupoConformado(grupo);
                            }
                            else if (vista.Pilares_laminados.IsChecked == true)
                            {
                                if (secciontipo[grupo].tipo == "Laminado")
                                {
                                    mySapModel.DesignSteel.StartDesign();
                                    ratio = RatioGrupoLaminado(grupo);
                                }
                                else if (secciontipo[grupo].tipo == "Conformado")
                                {
                                    mySapModel.DesignColdFormed.StartDesign();
                                    ratio = RatioGrupoConformado(grupo);
                                }
                            }
                            //double ratio = RatioGrupo(vista, grupo, barraControl, listaperfiles, tipo, seccion_tipo[1]);
                            ratios.Add(ratio);
                        }

                        List<bool> comprobacionPorGrupo = new List<bool>();

                        index = 0;

                        for (int i = 0; i < ratios.Count; i++)
                        {
                            double ratiomax = secciones.ElementAt(i).Value.ratiomax;
                            comprobacionPorGrupo.Add(ratios[i] < ratiomax);
                        }

                        if (!comprobacionPorGrupo.Contains(false))
                            comprobacion = true;

                        index = 0;

                        foreach (var propiedad in secciones)
                        {
                            string grupo = propiedad.Key;
                            string barraControl = propiedad.Value.barraControl;
                            string[] listaperfiles = propiedad.Value.listaperfiles;
                            eItemType tipo = propiedad.Value.tipo;
                            double ratiomax = propiedad.Value.ratiomax;
                            double ratio = ratios[index++];
                            if (ratio != 0 && ratio > ratiomax)
                            {
                                vista.Progreso.Items.Add(grupo + ": Perfil " + secciontipo[grupo].seccion + " no válido. Ratio: " + ratio.ToString("F2"));
                            }
                        }

                        index = 0;

                        foreach (var propiedad in secciones)
                        {
                            string grupo = propiedad.Key;
                            string barraControl = propiedad.Value.barraControl;
                            string[] listaperfiles = propiedad.Value.listaperfiles;
                            eItemType tipo = propiedad.Value.tipo;
                            double ratiomax = propiedad.Value.ratiomax;
                            double ratio = ratios[index++];
                            if (ratio != 0 && ratio > ratiomax)
                                CambiarSecciones(grupo, listaperfiles);
                        }
                        SAP.AnalysisSubclass.RunModel(mySapModel);
                    }
                    #endregion
                    //Resultados
                    #region
                    index = 0;
                    var resumen = new Dictionary<string, (string[] nombreBarras, eItemType tipo)>();

                    if (vista.Monoposte.IsChecked == true)
                    {
                        resumen["Pilares"] = (new[] { "Column_1" }, eItemType.Group);
                    }
                    else if (vista.Biposte.IsChecked == true)
                    {
                        resumen["Pilares Delanteros"] = (new[] { "Column_d_2" }, eItemType.Group);
                        resumen["Pilares Traseros"] = (new[] { "Column_i_2" }, eItemType.Group);
                    }
                    resumen["Vigas"] = (new[] { "Beam_1" }, eItemType.Group);
                    resumen["Correas"] = (new[] { "Purlin_1_1_CorreaExterior.Inferior" }, eItemType.Group);

                    if (vista.UnaDiagonal.IsChecked == true)
                    {
                        resumen["Diagonales"] = (new[] { "Diag_Dcha_1" }, eItemType.Group);
                    }
                    if (vista.DosDiagonal.IsChecked == true)
                    {
                        resumen["Diagonales Delanteras"] = (new[] { "Diag_Dcha_1" }, eItemType.Group);
                        resumen["Diagonales Traseras"] = (new[] { "Diag_Izda_1" }, eItemType.Group);
                    }
                    resumen["Estabilizadores"] = (new[] { "Arriostr_21" }, eItemType.Group);

                    foreach (var propiedad in resumen)
                    {
                        string elemento = propiedad.Key;
                        string[] nombreBarras = propiedad.Value.nombreBarras;
                        eItemType tipo = propiedad.Value.tipo;
                        if (ratios[index] != 0 && ratios[index] < 1)
                        {
                            Resultados(vista, elemento, nombreBarras, ratios[index]);
                        }
                        index++;
                    }
                    #endregion
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
        }

        public static void ObtenerMateriales (DimensionamientoRackAPP vista)
        {
            if(mySapModel != null)
            {
                int NumberNames = 0;
                string[] Materiales = new string[50];

                int ret = mySapModel.PropMaterial.GetNameList(ref NumberNames, ref Materiales, eMatType.Steel);

                vista.Ambiente.SelectedIndex = 0;


                if(ret == 0)
                {
                    for (int i = 0; i < Materiales.Count(); i++)
                    {
                        if (Materiales[i].Contains("S350GD"))
                        {
                            vista.Material_Vigas.Items.Add(Materiales[i]);
                            vista.Material_Diagonales.Items.Add(Materiales[i]);
                            vista.Material_Correas.Items.Add(Materiales[i]);
                            vista.Material_Estabilizadores.Items.Add(Materiales[i]);
                        }
                        if (Materiales[i].Contains("S420GD"))
                        {
                            vista.Material_Vigas.Items.Add(Materiales[i]);
                            vista.Material_Diagonales.Items.Add(Materiales[i]);
                            vista.Material_Correas.Items.Add(Materiales[i]);
                            vista.Material_Estabilizadores.Items.Add(Materiales[i]);
                        }
                        if (Materiales[i].Contains("S450GD"))
                        {
                            vista.Material_Vigas.Items.Add(Materiales[i]);
                            vista.Material_Diagonales.Items.Add(Materiales[i]);
                            vista.Material_Correas.Items.Add(Materiales[i]);
                            vista.Material_Estabilizadores.Items.Add(Materiales[i]);
                        }
                        if (Materiales[i].Contains("S355JR"))
                        {
                            vista.Material_Pilares.Items.Add(Materiales[i]);
                            vista.Material_Vigas.Items.Add(Materiales[i]);
                            vista.Material_Diagonales.Items.Add(Materiales[i]);
                            vista.Material_Correas.Items.Add(Materiales[i]);
                            vista.Material_Estabilizadores.Items.Add(Materiales[i]);
                        }
                        if (Materiales[i].Contains("S460JR"))
                        {
                            vista.Material_Pilares.Items.Add(Materiales[i]);
                            vista.Material_Vigas.Items.Add(Materiales[i]);
                            vista.Material_Diagonales.Items.Add(Materiales[i]);
                            vista.Material_Correas.Items.Add(Materiales[i]);
                            vista.Material_Estabilizadores.Items.Add(Materiales[i]);
                        }
                    }
                    vista.Material_Pilares.SelectedIndex=0;
                    vista.Material_Vigas.SelectedIndex = 0;
                    vista.Material_Diagonales.SelectedIndex = 0;
                    vista.Material_Correas.SelectedIndex = 0;
                    vista.Material_Estabilizadores.SelectedIndex = 0;
                }
            }
        }

        public static void AgregarPerfilesPorAmbiente(ComboBox combo, IEnumerable<string> perfiles, string material, string ambiente)
        {
            foreach (var perfil in perfiles)
            {
                string[] partes = perfil.Split('/').Select(p => p.Trim()).ToArray();

                if (partes.Length < 2 || partes[1] != material) continue;

                if (ambiente == "Normal" && partes.Length == 2)
                    combo.Items.Add(perfil);
                else if (ambiente.Contains("Ligeramente") && partes.Length == 3 && partes[2].Contains("-0.5"))
                    combo.Items.Add(perfil);
                else if (ambiente.Contains("Altamente") && partes.Length == 3 && partes[2].Contains("-1"))
                    combo.Items.Add(perfil);
            }
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
    }
}
