using System;
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
using ListadosDeCalculo.Scripts;
using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.View;
using static OfficeOpenXml.ExcelErrorValue;

namespace SmarTools.Model.Applications
{
    internal class Dimensionamiento1V
    {
        public static cHelper cHelper = MainView.Globales._myHelper;
        public static cOAPI mySapObject = MainView.Globales._mySapObject;
        public static cSapModel mySapModel = MainView.Globales._mySapModel;
        public static string ruta = @"Z:\300Logos\03 Uniones\Uniones 1VR5.xlsx";

        public static void FiltrarPerfiles (Dimensionamiento1VAPP vista)
        {

            // Limpiar listas
            LimpiarListasPerfiles(vista.Pilar_motor, vista.Pilar_general,vista.Viga_B1, vista.Viga_B2, vista.Viga_B3, vista.Viga_B4,vista.Viga_secundaria);

            // Ejecutar análisis y obtener perfiles
            SAP.AnalysisSubclass.RunModel(mySapModel);
            string[] Perfiles = SAP.ElementFinderSubclass.GetFrameSectionsInWeightOrder(mySapModel);

            // Variables de entrada
            string material_MP = vista.Material_MP.Text;
            string material_GP = vista.Material_GP.Text;
            string material_vigas = vista.Material_Vigas.Text;
            string material_SB = vista.Material_Secundarias.Text;
            string ambiente = vista.Ambiente.Text;

            // Filtrado
            string[] PerfilW = Perfiles.Where(s => s.StartsWith("W6")).ToArray();

            string[] PerfilC = vista.Pilares_laminados.IsChecked == true
            ? Perfiles.Where(s => s.StartsWith("W6") || s.StartsWith("IPEA180")).ToArray()
            : Perfiles.Where(s => s.StartsWith("C-150") || s.StartsWith("C-200x100x30") || s.StartsWith("C-175")).ToArray();

            string[] PerfilSHS = Perfiles.Where(s => s.StartsWith("SHS-120") && s.EndsWith(material_vigas)).ToArray();

            string[] PerfilOH = Perfiles.Where(s =>
            (vista.OH_60.IsChecked == true && s.StartsWith("OH-60") && s.EndsWith(material_SB)) ||
            (vista.OH_65.IsChecked == true && s.StartsWith("OH-65") && s.EndsWith(material_SB))
            ).ToArray();

            // Agregar perfiles a controles
            AgregarPerfilesPorAmbiente(vista.Pilar_motor, PerfilW, material_MP, ambiente);
            SeleccionarPorDefecto(vista.Pilar_motor, "W6X9");

            AgregarPerfilesPorAmbiente(vista.Pilar_general, PerfilC, material_GP, ambiente);
            vista.Pilar_general.SelectedIndex = 0;

            AgregarPerfilesAVigas(PerfilSHS, vista.Viga_B1, vista.Viga_B2, vista.Viga_B3, vista.Viga_B4);
            vista.Viga_B1.SelectedIndex = 1;
            vista.Viga_B2.SelectedIndex = 0;
            vista.Viga_B3.SelectedIndex = 0;
            vista.Viga_B4.SelectedIndex = 0;

            foreach (var perfil in PerfilOH)
            {
                vista.Viga_secundaria.Items.Add(perfil);
            }
            vista.Viga_secundaria.SelectedIndex = 0;

        }

        public static void AsignarPerfiles(Dimensionamiento1VAPP vista)
        {
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            //Obtener los nombres de las secciones desde los combobox
            var secciones = new Dictionary<string, (string perfil, eItemType tipo)>
            {
                { "01 Pilares Centrales",(vista.Pilar_motor.Text,eItemType.Group) },
                { "02 Pilares Generales",(vista.Pilar_general.Text,eItemType.Group) },
                { "B-1_Motor",(vista.Viga_B1.Text,eItemType.Objects) },
                { "B1_Motor",(vista.Viga_B1.Text,eItemType.Objects) },
                { "B-1",(vista.Viga_B1.Text,eItemType.Objects) },
                { "B1",(vista.Viga_B1.Text,eItemType.Objects) },
                { "B-2",(vista.Viga_B2.Text,eItemType.Objects) },
                { "B2",(vista.Viga_B2.Text,eItemType.Objects) },
                { "B-3",(vista.Viga_B3.Text,eItemType.Objects) },
                { "B3",(vista.Viga_B3.Text,eItemType.Objects) },
                { "B-4",(vista.Viga_B4.Text,eItemType.Objects) },
                { "B4",(vista.Viga_B4.Text,eItemType.Objects) },
                { "05 Vigas Secundarias",(vista.Viga_secundaria.Text,eItemType.Group) }
            };

            //Asignar perfiles a cada grupo u objeto
            foreach(var propiedad in secciones)
            {
                string nombre = propiedad.Key;
                string perfil = propiedad.Value.perfil;
                eItemType tipo = propiedad.Value.tipo;

                mySapModel.FrameObj.SetSection(nombre, perfil, tipo);
            }

            vista.Progreso.Items.Add("Perfiles asignados correctamente");
        }

        public static void Dimensionar1V(Dimensionamiento1VAPP vista)
        {
            
        }

        public static void ObtenerMateriales(Dimensionamiento1VAPP vista)
        {
            if (mySapModel != null)
            {
                int NumberNames = 0;
                string[] Materiales = new string[50];

                int ret = mySapModel.PropMaterial.GetNameList(ref NumberNames, ref Materiales, eMatType.Steel);
                string[] MSec = Materiales.Where(s => s.Contains("S350GD")|| s.Contains("S420GD")).ToArray();

                vista.Ambiente.SelectedIndex = 0;

                if (ret == 0)
                {
                    for (int i = 0; i < Materiales.Count(); i++)
                    {
                        vista.Material_MP.Items.Add(Materiales[i]);
                        vista.Material_GP.Items.Add(Materiales[i]);
                        vista.Material_Vigas.Items.Add(Materiales[i]);
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

                    for (int i = 0; i < vista.Material_MP.Items.Count; i++)
                    {
                        if (vista.Material_MP.Items[i].ToString().Contains("S355"))
                        {
                            vista.Material_MP.SelectedIndex = i;
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

        public static void LimpiarListasPerfiles(params ComboBox[] listas)
        {
            foreach (var lista in listas)
            {
                lista.Items.Clear();
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

        public static void AgregarPerfilesAVigas(string[] perfiles, params ComboBox[] vigas)
        {
            foreach (var perfil in perfiles)
            {
                foreach (var viga in vigas)
                {
                    viga.Items.Add(perfil);
                }
            }
        }

        public static void SeleccionarPorDefecto(ComboBox combo, string contiene)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                combo.SelectedIndex = i;
                if (combo.Text.Contains(contiene)) break;
            }
        }

       

    }
}