using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using SmarTools.Model.Repository;
using ModernUI.View;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.View;
using static OfficeOpenXml.ExcelErrorValue;

namespace SmarTools.Model.Applications
{
    internal class Dimensionamiento2V
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;

        public static void FiltrarPerfiles2V(Dimensionamiento2VAPP vista)
        {

            // Limpiar listas
            LimpiarListasPerfiles2V(vista.Pilar_motor, vista.Pilar_general, vista.Viga_B1, vista.Viga_secundaria);

            // Ejecutar análisis y obtener perfiles
            string[] Perfiles = SAP.ElementFinderSubclass.GetFrameSectionsInWeightOrder(mySapModel);

            // Variables de entrada
            string material_MP = "S355JR";
            string material_GP = vista.Material_GP.Text;
            string material_vigas = vista.Material_Vigas.Text;
            string material_SB = vista.Material_Secundarias.Text;
            string ambiente = vista.Ambiente.Text;

            // Filtrado
            string[] Perfil_Motor=
                vista.Slew_6in.IsChecked==true?Perfiles.Where(s => s.StartsWith("W6")).ToArray():
                vista.Slew_8in.IsChecked==true?Perfiles.Where(s => s.StartsWith("W8")|| s.StartsWith("IPE200")|| s.StartsWith("IPEA200")).ToArray():
                Array.Empty<string>();

            string[] Perfil_General =
                vista.Pilares_laminados.IsChecked == true 
                ? (vista.Pilares_W6.IsChecked == true
                        ? Perfiles.Where(s => s.StartsWith("W6")).ToArray() 
                        : vista.Pilares_W8.IsChecked == true
                            ? Perfiles.Where(s => s.StartsWith("W8")).ToArray()
                            : Array.Empty<string>())
                : Perfiles.Where(s => s.StartsWith("C-175x120x30") || s.StartsWith("C-195x120x30")).ToArray();

            string[] PerfilSHS = 
                vista.Tubo_140.IsChecked==true?Perfiles.Where(s => s.StartsWith("SHS-140") && s.EndsWith(material_vigas)).ToArray():
                vista.Tubo_150.IsChecked==true?Perfiles.Where(s => s.StartsWith("SHS-150") && s.EndsWith(material_vigas)).ToArray():
                Array.Empty<string>();

            string[] PerfilOH=
                vista.OH_75.IsChecked==true?Perfiles.Where(s=>s.StartsWith("OH-75x40x28,8") && s.EndsWith(material_SB)).ToArray():
                vista.OH_90.IsChecked==true?Perfiles.Where(s=>s.StartsWith("OH-90x40x28,8") && s.EndsWith(material_SB)).ToArray() :
                Array.Empty<string>();

            // Agregar perfiles a controles
            AgregarPerfilesPorAmbiente2V(vista.Pilar_motor, Perfil_Motor, material_MP, ambiente);
            vista.Pilar_motor.SelectedIndex = 0;

            AgregarPerfilesPorAmbiente2V(vista.Pilar_general, Perfil_General, material_GP, ambiente);
            vista.Pilar_general.SelectedIndex = 0;

            AgregarPerfilesAVigas2V(PerfilSHS, vista.Viga_B1);
            vista.Viga_B1.SelectedIndex = 0;

            foreach (var perfil in PerfilOH)
            {
                vista.Viga_secundaria.Items.Add(perfil);
            }
            vista.Viga_secundaria.SelectedIndex = 0;

        }

        public static void AsignarPerfiles2V(Dimensionamiento2VAPP vista)
        {
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            int nvigas = SAP.ElementFinderSubclass.TrackerSubclass.BeamNumber(mySapModel);
            string[] vigas = SAP.ElementFinderSubclass.TrackerSubclass.BeamNames(mySapModel, "04 Vigas Principales");

            //Obtener los nombres de las secciones desde los combobox
            var secciones = new Dictionary<string, (string perfil, eItemType tipo)>
            {
                { "01 Pilares Centrales",(vista.Pilar_motor.Text,eItemType.Group) },
                { "02 Pilares Generales",(vista.Pilar_general.Text,eItemType.Group) },
                { "05 Vigas Secundarias",(vista.Viga_secundaria.Text,eItemType.Group) }
            };

            var comboBoxes=new Dictionary<string, ComboBox>
            {
                {"B-1_Motor", vista.Viga_B1},
                {"B1_Motor", vista.Viga_B1},
                {"B-1", vista.Viga_B1},
                {"B1", vista.Viga_B1},
            };

            for(int i = 0; i < vigas.Length; i++)
            {
                if (comboBoxes.TryGetValue(vigas[i], out ComboBox combo))
                {
                    secciones[vigas[i]] = (combo.Text, eItemType.Objects);
                }
            }
            
            //Asignar perfiles a cada grupo u objeto
            foreach (var propiedad in secciones)
            {
                string nombre = propiedad.Key;
                string perfil = propiedad.Value.perfil;
                eItemType tipo = propiedad.Value.tipo;

                mySapModel.FrameObj.SetSection(nombre, perfil, tipo);
            }

            vista.Progreso.Items.Add("Perfiles asignados correctamente");
        }

        public static void Dimensionar2V(Dimensionamiento2VAPP vista)
        {
            //Preparamos el modelo 
            vista.Progreso.Items.Clear();
            vista.Resultados.Items.Clear();

            var loadingWindow = new Status();

            if (vista.Pilar_motor.Items.Count == 0)
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

                    //Listas de perfiles
                    string[] perfiles_MP = new string[vista.Pilar_motor.Items.Count];
                    vista.Pilar_motor.Items.CopyTo(perfiles_MP, 0);
                    string[] perfiles_GP = new string[vista.Pilar_general.Items.Count];
                    vista.Pilar_general.Items.CopyTo(perfiles_GP, 0);
                    string[] perfiles_vigas = new string[vista.Viga_B1.Items.Count];
                    vista.Viga_B1.Items.CopyTo(perfiles_vigas, 0);
                    string[] perfiles_SB = new string[vista.Viga_secundaria.Items.Count];
                    vista.Viga_secundaria.Items.CopyTo(perfiles_SB, 0);
                    
                    //Añadir grupo de secundarias sin los refuerzos
                    string[] Sec_sup_norte=SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel,true);
                    string[] Sec_inf_norte = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel, false);
                    string[] Sec_sup_sur = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel, true);
                    string[] Sec_inf_sur = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel, false);
                    string[] secundarias =(Sec_sup_norte.Concat(Sec_inf_norte).Concat(Sec_sup_sur).Concat(Sec_inf_sur)).ToArray();

                    int NumberNames = 0;
                    string[] Names = new string[1];
                    mySapModel.GroupDef.GetNameList(ref NumberNames,ref Names);

                    if(NumberNames==14)
                    {
                        mySapModel.GroupDef.SetGroup("20 Vigas Secundarias");
                    }

                    mySapModel.SelectObj.ClearSelection();
                    for(int i = 0;i<secundarias.Length;i++)
                    {
                        mySapModel.FrameObj.SetGroupAssign(secundarias[i], "20 Vigas Secundarias", false, eItemType.Objects);
                    }

                    int nvigas = SAP.ElementFinderSubclass.TrackerSubclass.BeamNumber(mySapModel);
                    string[] vigas = SAP.ElementFinderSubclass.TrackerSubclass.BeamNames(mySapModel, "04 Vigas Principales");
                    int mitad = vigas.Length / 2;
                    string[] vigasNorte = vigas.Take(mitad).ToArray();
                    string[] vigasSur = vigas.Skip(mitad).ToArray();

                    var secciones = new Dictionary<string, (string barraControl, string[] listaperfiles, eItemType tipo, double ratiomax)>
                    {
                        { "01 Pilares Centrales",("Column_0", perfiles_MP, eItemType.Group,0.9) },
                        { "02 Pilares Generales",("Column_1", perfiles_GP, eItemType.Group,0.9) },
                        { "20 Vigas Secundarias",("SBsN_2", perfiles_SB,eItemType.Group,1) }
                    };

                    for (int i = 0; i < vigas.Length; i++)
                    {
                        secciones[vigas[i]] = (vigas[i], perfiles_vigas, eItemType.Objects, 1);
                    }

                    List<double> ratios = new List<double>();

                    bool comprobacion = false;
                    int index = 0;

                    mySapModel.SetPresentUnits(eUnits.kN_m_C);
                    SAP.AnalysisSubclass.RunModel(mySapModel);

                    while (comprobacion == false)
                    {
                        ratios.Clear();

                        foreach (var propiedad in secciones)
                        {
                            string grupo = propiedad.Key;
                            string barraControl = propiedad.Value.barraControl;
                            string[] listaperfiles = propiedad.Value.listaperfiles;
                            eItemType tipo = propiedad.Value.tipo;
                            double ratio = RatioGrupo2V(vista, grupo, barraControl, listaperfiles, tipo);
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
                        {
                            comprobacion = true;
                        }

                        index = 0;

                        SAP.AnalysisSubclass.UnlockModel(mySapModel);

                        foreach (var propiedad in secciones)
                        {
                            string grupo = propiedad.Key;
                            string barraControl = propiedad.Value.barraControl;
                            string[] listaperfiles = propiedad.Value.listaperfiles;
                            eItemType tipo = propiedad.Value.tipo;
                            double ratiomax = propiedad.Value.ratiomax;
                            double ratio = ratios[index];
                            if (ratio != 0 && ratio > ratiomax)
                            {
                                mySapModel.SelectObj.ClearSelection();
                                RatioSuperior2V(vista, grupo, barraControl, ratio, listaperfiles, tipo);
                            }
                            index++;
                        }

                        SAP.AnalysisSubclass.RunModel(mySapModel);
                    }

                    index = 0;

                    var resumen = new Dictionary<string, (string[] nombreBarras, eItemType tipo)>
                    {
                        { "Pilar motor",(new []{"Column_0"},eItemType.Group)},
                        {"Pilares generales",(new[]{"Column_1"},eItemType.Group)},
                        {"Vigas Secundarias",(new[]{"SBsN_2"},eItemType.Group)}
                    };

                    resumen["Viga motor"] = (new[] { vigasNorte[0], vigasSur[0] }, eItemType.Objects);

                    for (int i = 1; i < vigasNorte.Length; i++)
                    {
                        resumen["Viga B" + (i + 1)] = (new[] { vigasNorte[i], vigasSur[i] }, eItemType.Objects);
                    }

                    foreach (var propiedad in resumen)
                    {
                        string elemento = propiedad.Key;
                        string[] nombreBarras = propiedad.Value.nombreBarras;
                        eItemType tipo = propiedad.Value.tipo;

                        if (ratios[index] != 0 && ratios[index] < 1)
                        {
                            Resultados2V(vista, elemento, nombreBarras, ratios[index]);
                        }

                        index++;
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
        }

        public static void ObtenerMateriales2V(Dimensionamiento2VAPP vista)
        {
            if (mySapModel != null)
            {
                int NumberNames = 0;
                string[] Materiales = new string[50];

                int ret = mySapModel.PropMaterial.GetNameList(ref NumberNames, ref Materiales, eMatType.Steel);
                string[] MSec = Materiales.Where(s => s.Contains("S350GD") || s.Contains("S420GD")).ToArray();
                string[] Mat_MP = Materiales.Where(s => s.Contains("S355")).ToArray();
                string[] Mat_GP = Materiales.Where(s => s.Contains("S355") || s.Contains("S350GD")||s.Contains("S420GD")||s.Contains("S420MC")).ToArray();
                string[] Mat_MB = Materiales.Where(s => s.Contains("S355") || s.Contains("S350GD") || s.Contains("S420GD") || s.Contains("S420MC")).ToArray();

                vista.Ambiente.SelectedIndex = 0;

                if (ret == 0)
                {
                    for (int i = 0; i < Mat_GP.Count(); i++)
                    {
                        vista.Material_GP.Items.Add(Mat_GP[i]);
                    }
                    for (int i = 0; i < Mat_MB.Count(); i++)
                    {
                        vista.Material_Vigas.Items.Add(Mat_MB[i]);
                    }
                    for (int i = 0; i < MSec.Count(); i++)
                    {
                        vista.Material_Secundarias.Items.Add(MSec[i]);
                    }

                    for (int i = 0; i < vista.Material_GP.Items.Count; i++)
                    {
                        if (vista.Material_GP.Items[i].ToString().Contains("S355"))
                        {
                            vista.Material_GP.SelectedIndex = i;
                            break;
                        }
                    }

                    for (int i = 0; i < vista.Material_Vigas.Items.Count; i++)
                    {
                        if (vista.Material_Vigas.Items[i].ToString().Contains("S420"))
                        {
                            vista.Material_Vigas.SelectedIndex = i;
                            break;
                        }
                    }

                    for (int i = 0; i < vista.Material_Secundarias.Items.Count; i++)
                    {
                        if (vista.Material_Secundarias.Items[i].ToString().Contains("S350GD"))
                        {
                            vista.Material_Secundarias.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        public static void LimpiarListasPerfiles2V(params ComboBox[] listas)
        {
            foreach (var lista in listas)
            {
                lista.Items.Clear();
            }
        }

        public static void AgregarPerfilesPorAmbiente2V(ComboBox combo, IEnumerable<string> perfiles, string material, string ambiente)
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

        public static void AgregarPerfilesAVigas2V(string[] perfiles, params ComboBox[] vigas)
        {
            foreach (var perfil in perfiles)
            {
                foreach (var viga in vigas)
                {
                    viga.Items.Add(perfil);
                }
            }
        }

        public static void SeleccionarPorDefecto2V(ComboBox combo, string contiene)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                combo.SelectedIndex = i;
                if (combo.Text.Contains(contiene)) break;
            }
        }

        public static string[] ObtenerSeccionYtipo2V(cSapModel mySapModel, string barra)
        {
            //Variables necesarias para SAP2000
            int ret = 0;
            int numberItems = 0;
            int[] objectType = new int[1];
            string[] itemName = new string[1];
            string section = "";

            //Variable de salida
            string[] seccion_tipo = new string[2];

            //Interacción con SAP
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

        public static double RatioGrupo2V(Dimensionamiento2VAPP vista, string grupo, string barra, string[] listaperfiles, eItemType tipo)
        {
            string[] seccion_tipo = ObtenerSeccionYtipo2V(mySapModel, barra);
            mySapModel.SelectObj.ClearSelection();

            //Variables
            int numberItems = 0;
            int[] ObjectType = new int[1], ratioType = new int[1];
            string[] ObjectName = new string[1], ComboName = new string[1], ErrorSummary = new string[1], WarningSummary = new string[1], PropName = new string[1];
            double[] Ratio = new double[1], location = new double[1];

            switch (seccion_tipo[1])
            {
                case "Laminado":

                    mySapModel.DesignSteel.StartDesign();

                    if (grupo == barra)
                    {
                        mySapModel.FrameObj.SetSelected(barra, true, tipo);
                        mySapModel.DesignSteel.GetSummaryResults(grupo, ref numberItems, ref ObjectName, ref Ratio, ref ratioType, ref location, ref ComboName, ref ErrorSummary, ref WarningSummary, tipo);
                        if (barra.StartsWith("B"))
                        {
                            double[] aprTorsor = SAP.DesignSubclass.ShearTorsionInteractionCheck(mySapModel, barra);
                            if (aprTorsor[0] > 1 || aprTorsor[1] > 1)
                            {
                                return 2;
                            }
                        }
                        if (ErrorSummary.Contains("Section is too slender"))
                        {
                            return 2;
                        }
                        else
                        {
                            return Ratio.Max();
                        }
                    }
                    else
                    {
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
                    }

                case "Conformado":

                    mySapModel.DesignColdFormed.StartDesign();

                    if (grupo == barra)
                    {
                        mySapModel.FrameObj.SetSelected(barra, true, tipo);
                        mySapModel.DesignColdFormed.GetSummaryResults(grupo, ref numberItems, ref ObjectName, ref Ratio, ref ratioType, ref location, ref ComboName, ref ErrorSummary, ref WarningSummary, tipo);
                        if (barra.StartsWith("B"))
                        {
                            double[] aprTorsor = SAP.DesignSubclass.ShearTorsionInteractionCheck(mySapModel, barra);
                            if (aprTorsor[0] > 1 || aprTorsor[1] > 1)
                            {
                                return 2;
                            }
                        }
                        if (ErrorSummary.Contains("Section is too slender"))
                        {
                            return 2;
                        }
                        else
                        {
                            return Ratio.Max();
                        }
                    }
                    else
                    {
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
            }
            mySapModel.SelectObj.ClearSelection();

            return 0;
        }

        public static void RatioSuperior2V(Dimensionamiento2VAPP vista, string grupo, string barra, double Ratio, string[] listaperfiles, eItemType tipo)
        {
            string[] seccion_tipo = ObtenerSeccionYtipo2V(mySapModel, barra);
            string propname = "";

            switch (seccion_tipo[1])
            {
                case "Laminado":
                    mySapModel.DesignSteel.GetDesignSection(barra, ref propname);
                    break;
                case "Conformado":
                    mySapModel.DesignColdFormed.GetDesignSection(barra, ref propname);
                    break;
            }

            vista.Progreso.Items.Add("Perfil " + propname + " no válido. Ratio: " + Ratio.ToString("F2"));

            if (grupo == barra)
            {
                mySapModel.FrameObj.SetSelected(barra, true, tipo);
            }
            else
            {
                mySapModel.SelectObj.Group(grupo);
            }
            SAP.DesignSubclass.ChangeSection(mySapModel, listaperfiles);
        }

        public static void Resultados2V(Dimensionamiento2VAPP vista, string elemento, string[] nombreBarras, double ratio)
        {
            if (nombreBarras.Length == 2)//Vigas principales
            {
                double[] tuboNorte = SAP.AnalysisSubclass.GetSHSProperties(mySapModel, nombreBarras[0]);
                double[] tuboSur = SAP.AnalysisSubclass.GetSHSProperties(mySapModel, nombreBarras[1]);

                if (tuboNorte[1] != tuboSur[1])
                {
                    if (tuboNorte[1] > tuboSur[1])
                    {
                        string seccionNorte = SAP.ElementFinderSubclass.TrackerSubclass.BeamName(mySapModel, nombreBarras[0]);
                        mySapModel.SelectObj.ClearSelection();
                        mySapModel.FrameObj.SetSection(nombreBarras[1], seccionNorte, eItemType.Objects);

                    }
                    else
                    {
                        string seccionSur = SAP.ElementFinderSubclass.TrackerSubclass.BeamName(mySapModel, nombreBarras[1]);
                        mySapModel.SelectObj.ClearSelection();
                        mySapModel.FrameObj.SetSection(nombreBarras[1], seccionSur, eItemType.Objects);
                    }
                }
            }

            string[] seccion_tipo = ObtenerSeccionYtipo2V(mySapModel, nombreBarras[0]);

            vista.Resultados.Items.Add(elemento + ": " + seccion_tipo[0] + " Ratio: " + ratio.ToString("F2"));
        }
    }
}
