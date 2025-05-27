using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ListadosDeCalculo.Scripts;
using ModernUI.View;
using SAP2000v1;
using SmarTools.APPS;
using SmarTools.View;
using static OfficeOpenXml.ExcelErrorValue;

namespace SmarTools.Model.Applications
{
    class ComprobacionFlechasTracker
    {
        public static cHelper cHelper=MainView.Globales._myHelper;
        public static cOAPI mySapObject=MainView.Globales._mySapObject;
        public static cSapModel mySapModel=MainView.Globales._mySapModel;
        
        public static void ComprobarFlechas(ComprobacionFlechasTrackerAPP vista)
        {
            var loadingWindow = new Status();
            loadingWindow.Show();
            loadingWindow.UpdateLayout();

            try
            {
                Herramientas.AbrirArchivoSAP2000();
                mySapModel.SetPresentUnits(eUnits.N_mm_C);
                SAP.AnalysisSubclass.RunModel(mySapModel);
                SAP.AnalysisSubclass.SelectHypotesis(mySapModel, "SLS", false);

                DesplomePilares(vista);
                FlechaVigas(vista);
                FlechaSecundarias(vista);
                FlechaVoladizo(vista);
            }
            finally
            {
                loadingWindow.Close();
            }
        }

        public static void DesplomePilares(ComprobacionFlechasTrackerAPP vista)
        {
            //Variables para almacenar coordenadas del pilar motor (H), coordenadas y desplazamiento
            double X=0, Y=0, Z=0;
            int ret=mySapModel.PointElm.GetCoordCartesian("mps",ref X,ref Y,ref Z);
            double desplazamiento_motor = SAP.DesignSubclass.JointDisplacement(mySapModel, "mps");

            //Número de pilares, nombres de pilares y de nudos superiores
            int n_pilares = SAP.ElementFinderSubclass.TrackerSubclass.PileNumber(mySapModel);
            string[] pilar_n=SAP.ElementFinderSubclass.TrackerSubclass.NorthPiles(mySapModel);
            string[] pilar_s=SAP.ElementFinderSubclass.TrackerSubclass.SouthPiles(mySapModel);
            string[] gps_n = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilar_n, 2);
            string[] gps_s = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilar_s, 2);

            //Desplazamientos de las cabezas de los pilares
            double[] desplazamiento_gp_norte = new double[n_pilares];
            double[] desplazamiento_gp_sur=new double[n_pilares];
            for (int i = 1; i < n_pilares; i++)
            {
                desplazamiento_gp_norte[i-1] = SAP.DesignSubclass.JointDisplacement(mySapModel, gps_n[i-1]);
                desplazamiento_gp_sur[i-1]=SAP.DesignSubclass.JointDisplacement(mySapModel,gps_s[i-1]);
            }
            double[] desplomes = new double[1 + 2 * n_pilares];
            desplomes[0] = desplazamiento_motor;
            Array.Copy(desplazamiento_gp_norte, 0, desplomes, 1, n_pilares);
            Array.Copy(desplazamiento_gp_sur, 0, desplomes, n_pilares + 1, n_pilares);

            //Cálculo de flecha máxima admisible, flecha real y relación H/D
            double dmax = Z / 100;
            double dreal = desplomes.Max();
            double R=Z/dreal;

            //Mostrar resultados
            vista.Resultado_pilares.Content = $"{dreal:F3} (H/{R:F0})";
            vista.Admisible_pilares.Content = $"{dmax:F3}";
            vista.Check_pilares.Content = (dreal <= dmax).ToString();
        }

        public static void FlechaVigas(ComprobacionFlechasTrackerAPP vista)
        {
            //Número de pilares, nombres de pilares y de nudos superiores
            int n_pilares = SAP.ElementFinderSubclass.TrackerSubclass.PileNumber(mySapModel);
            string[] pilar_n = SAP.ElementFinderSubclass.TrackerSubclass.NorthPiles(mySapModel);
            string[] pilar_s = SAP.ElementFinderSubclass.TrackerSubclass.SouthPiles(mySapModel);
            string[] gps_n = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilar_n, 2);
            string[] gps_s = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilar_s, 2);

            //Desbloqueamos el modelo y ponemos apoyos en las cabezas de los pilares
            SAP.AnalysisSubclass.UnlockModel(mySapModel);
            bool[] apoyo = [true,true,true,false,false,false];
            mySapModel.PointObj.SetRestraint("mps",ref apoyo);
            for (int i = 0; i < n_pilares; i++)
            {
                mySapModel.PointObj.SetRestraint(gps_n[i], ref apoyo);
                mySapModel.PointObj.SetRestraint(gps_s[i], ref apoyo);
            }
            SAP.AnalysisSubclass.RunModel(mySapModel);

            //Nombre de secundarias y punto de apoyo en la viga
            string[] sec_sup_norte=SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel);
            string[] sec_sup_sur = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel);
            string[] nudos_viga_norte = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, sec_sup_norte, 1);
            string[] nudos_viga_sur = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, sec_sup_sur, 1);
            double[] flecha_vigas_n=new double[sec_sup_norte.Length];
            double[] flecha_vigas_s=new double[sec_sup_sur.Length];

            //Calculamos el desplazamiento de los nudos de las vigas, y las flechas máximas de norte y sur
            for(int i=0;i<sec_sup_norte.Length;i++)
            {
                flecha_vigas_n[i]=SAP.DesignSubclass.JointDisplacement(mySapModel,nudos_viga_norte[i]);
            }
            for (int i = 0; i < sec_sup_sur.Length; i++)
            {
                flecha_vigas_s[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudos_viga_sur[i]);
            }
            double fmax_n=flecha_vigas_n.Max();
            double fmax_s=flecha_vigas_s.Max();

            //Obtenemos los índices donde ocurren esas flechas máximas, y la longitud de los vanos correspondientes
            int n1=Array.IndexOf(flecha_vigas_n, fmax_n);
            int n2=Array.IndexOf(flecha_vigas_s, fmax_s);
            double L_n = SAP.DesignSubclass.FindSpan(mySapModel, nudos_viga_norte[n1], gps_n);
            double L_s = SAP.DesignSubclass.FindSpan(mySapModel, nudos_viga_sur[n2], gps_s);

            //Flechas admisibles L/200, y flecha máxima
            double fadm_n = Math.Abs(L_n / 200);
            double fadm_s = Math.Abs(L_s / 200);
            double fvigas=Math.Max(fmax_n, fmax_s);

            //Determinamos la flecha más crítica y su vano asociado
            double L_critico = (fvigas == fmax_n) ? L_n : L_s;
            double fadm_critico=Math.Abs(L_critico / 200);
            double R = Math.Abs(L_critico / fvigas);

            //Mostrar resultado en los controles visuales
            vista.Resultado_vigas.Content = $"{fvigas:F3} (H/{R:F0})";
            vista.Admisible_vigas.Content = $"{fadm_critico:F3}";
            vista.Check_vigas.Content = (fvigas <= fadm_critico).ToString();

            //Quitamos las coacciones en los nudos de los pilares
            SAP.AnalysisSubclass.UnlockModel(mySapModel);
            bool[] nudo_libre= new bool[6];
            mySapModel.PointObj.SetRestraint("mps",ref nudo_libre);
            for (int i = 0;i<n_pilares;i++)
            {
                mySapModel.PointObj.SetRestraint(gps_n[i], ref nudo_libre);
                mySapModel.PointObj.SetRestraint(gps_s[i], ref nudo_libre);
            }
        }

        public static void FlechaSecundarias(ComprobacionFlechasTrackerAPP vista)
        {
            int ret = 0;
            double X=0, Y=0, Z=0;

            //Nombre de secundarias y punto de apoyo en la viga
            string[] sec_sup_norte = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel);
            string[] sec_sup_sur = SAP.ElementFinderSubclass.TrackerSubclass.SouthSecundaryBeams(mySapModel);
            string[] sec_inf_norte = SAP.ElementFinderSubclass.TrackerSubclass.NorthSecundaryBeams(mySapModel,false);
            string[] nudos_viga_norte = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, sec_sup_norte, 1);
            string[] nudos_viga_sur = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, sec_sup_sur, 1);
            string[] nudos_superiores_sec=SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel,sec_sup_norte, 2);
            string[] nudos_inferiores_sec = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, sec_inf_norte, 2);

            //Restricción completa a los nudos de las secundarias
            bool[] empotramiento = { true, true, true, true, true, true };
            SAP.AnalysisSubclass.UnlockModel(mySapModel);
            for (int i = 0;i<sec_sup_norte.Length;i++)
            {
                mySapModel.PointObj.SetRestraint(nudos_viga_norte[i],ref empotramiento);
            }
            for (int i = 0; i < sec_sup_sur.Length; i++)
            {
                mySapModel.PointObj.SetRestraint(nudos_viga_sur[i], ref empotramiento);
            }

            // Calcular vano entre dos puntos de referencia
            mySapModel.PointObj.GetCoordCartesian("vp5", ref X, ref Y, ref Z);
            double x1 = X, z1 = Z;

            mySapModel.PointObj.GetCoordCartesian("sbs5", ref X, ref Y, ref Z);
            double L = Math.Sqrt(Math.Pow(x1 - X, 2) + Math.Pow(z1 - Z, 2));

            // Calcular flecha admisible (2L/300)
            double fmax = 2 * L / 300;
            
            SAP.AnalysisSubclass.RunModel(mySapModel);

            // Calcular desplazamientos reales en vigas secundarias
            double[] fvigas_n=new double[nudos_viga_norte.Length];
            double[] fvigas_s = new double[nudos_viga_sur.Length];

            for (int i = 0; i < sec_sup_norte.Length; i++)
                fvigas_n[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudos_superiores_sec[i]);

            for (int i = 0; i < sec_sup_sur.Length; i++)
                fvigas_s[i] = SAP.DesignSubclass.JointDisplacement(mySapModel, nudos_inferiores_sec[i]);

            // Obtener flecha máxima real
            double fvigas = Math.Max(fvigas_n.Max(), fvigas_s.Max());

            // Calcular relación 2L/f
            double R = Math.Abs(2 * L / fvigas);

            // Mostrar resultados en Labels
            vista.Resultado_secundarias.Content = $"{fvigas:F3} (2L/{R:F0})";
            vista.Admisible_secundarias.Content = $"{fmax:F3}";
            vista.Check_secundarias.Content = (fvigas <= fmax).ToString();

            // Quitar restricciones después del análisis
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            bool[] nudo_libre = new bool[6]; // false por defecto

            for (int i = 0; i < sec_sup_norte.Length; i++)
                ret = mySapModel.PointObj.SetRestraint(nudos_viga_norte[i], ref nudo_libre);

            for (int i = 0; i < sec_sup_sur.Length; i++)
                ret = mySapModel.PointObj.SetRestraint(nudos_viga_sur[i], ref nudo_libre);

        }

        public static void FlechaVoladizo(ComprobacionFlechasTrackerAPP vista)
        {
            int ret = 0;
            double X = 0, Y = 0, Z = 0;
            //Datos geoméricos
            int nvigas = SAP.ElementFinderSubclass.TrackerSubclass.BeamNumber(mySapModel);
            int npilares= SAP.ElementFinderSubclass.TrackerSubclass.PileNumber(mySapModel)-1;
            string[] pilares_n=SAP.ElementFinderSubclass.TrackerSubclass.NorthPiles(mySapModel);
            string[] pilares_s = SAP.ElementFinderSubclass.TrackerSubclass.SouthPiles(mySapModel);
            string[] gps_n = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilares_n, 2);
            string[] gps_s = SAP.ElementFinderSubclass.TrackerSubclass.GetJoints(mySapModel, pilares_s, 2);

            // Definir extremos del voladizo
            string extremo_n = "B" + (nvigas + 1);
            string extremo_s = "B-" + (nvigas + 1);

            // Aplicar restricciones parciales (solo traslaciones)
            bool[] value = { true, true, true, false, false, false };
            SAP.AnalysisSubclass.UnlockModel(mySapModel);
            for (int i = 0; i < npilares; i++)
            {
                ret = mySapModel.PointObj.SetRestraint(gps_n[i], ref value);
                ret = mySapModel.PointObj.SetRestraint(gps_s[i], ref value);
            }

            // Ejecutar análisis si el modelo no está bloqueado
            SAP.AnalysisSubclass.RunModel(mySapModel);

            // Calcular desplazamientos en extremos del voladizo
            double dvol_n = SAP.DesignSubclass.JointDisplacement(mySapModel, extremo_n);
            double dvol_s = SAP.DesignSubclass.JointDisplacement(mySapModel, extremo_s);

            // Calcular longitud del voladizo norte
            mySapModel.PointObj.GetCoordCartesian(gps_n[npilares - 1], ref X, ref Y, ref Z);
            double y1 = Y;
            mySapModel.PointObj.GetCoordCartesian(extremo_n, ref X, ref Y, ref Z);
            double Ln = Y - y1;

            // Calcular longitud del voladizo sur
            mySapModel.PointObj.GetCoordCartesian(gps_s[npilares - 1], ref X, ref Y, ref Z);
            y1 = Y;
            mySapModel.PointObj.GetCoordCartesian(extremo_s, ref X, ref Y, ref Z);
            double Ls = Y - y1;

            // Calcular relaciones y flechas admisibles
            double R_n = Math.Abs(2 * Ln / dvol_n);
            double R_s = Math.Abs(2 * Ls / dvol_s);
            double fmax_n = Math.Abs(2 * Ln / 300);
            double fmax_s = Math.Abs(2 * Ls / 300);

            // Mostrar resultados en Labels
            if ((dvol_n / fmax_n) > (dvol_s / fmax_s))
            {
                vista.Resultado_voladizo.Content = $"{dvol_n:F3} (2L/{R_n:F0})";
                vista.Admisible_voladizo.Content = $"{fmax_n:F3}";
                vista.Check_voladizo.Content = (dvol_n <= fmax_n).ToString();
            }
            else
            {
                vista.Resultado_voladizo.Content = $"{dvol_s:F3} (2L/{R_s:F0})";
                vista.Admisible_voladizo.Content = $"{fmax_s:F3}";
                vista.Check_voladizo.Content = (dvol_s <= fmax_s).ToString();
            }

            // Desbloquear modelo si está bloqueado
            SAP.AnalysisSubclass.UnlockModel(mySapModel);

            // Quitar restricciones
            value = new bool[6]; // false por defecto

            for (int i = 0; i < npilares; i++)
            {
                ret = mySapModel.PointObj.SetRestraint(gps_n[i], ref value);
                ret = mySapModel.PointObj.SetRestraint(gps_s[i], ref value);
            }
        }
    }
}
