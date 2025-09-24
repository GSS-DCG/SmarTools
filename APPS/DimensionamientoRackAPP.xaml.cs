using ModernUI.View;
using SmarTools.Model.Applications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmarTools.APPS
{
    /// <summary>
    /// Lógica de interacción para DimensionamientoRackAPP.xaml
    /// </summary>
    public partial class DimensionamientoRackAPP : Window
    {
        public DimensionamientoRackAPP()
        {
            InitializeComponent();
            Herramientas.AbrirArchivoSAP2000();
            DimensionamientoRack.ObtenerMateriales(this);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnlcControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal) { this.WindowState = WindowState.Maximized; } else { this.WindowState = WindowState.Normal; }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnDimensionar_Click(object sender, RoutedEventArgs e)
        {
            DimensionamientoRack.Dimensionar(this);
        }

        private void btnFiltrarPerfiles_Click(object sender, RoutedEventArgs e)
        {
            DimensionamientoRack.FiltrarPerfiles(this);
        }

        private void btnAsignarPerfiles_Click(object sender, RoutedEventArgs e)
        {
            DimensionamientoRack.AsignarPerfiles(this);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == Monoposte && Monoposte.IsChecked == true)
            {
                Biposte.IsChecked = false;

                TextPilaresDelanteros.Visibility = Visibility.Visible;
                PilaresDelanteros.Visibility = Visibility.Visible;
                TextPilaresTraseros.Visibility = Visibility.Collapsed;
                PilaresTraseros.Visibility = Visibility.Collapsed;
                TextPilaresDelanteros.Content = "Pilares";

                Grid.SetRow(TextVigas, 1);
                Grid.SetRow(Vigas, 1);
                Grid.SetRow(TextCorreas, 2);
                Grid.SetRow(Correas, 2);

                if (SinDiagonal.IsChecked == true)
                {
                    UnaDiagonal.IsChecked = false;
                    DosDiagonal.IsChecked = false;

                    TextDiagonalesDelanteras.Visibility = Visibility.Collapsed;
                    DiagonalesDelanteras.Visibility = Visibility.Collapsed;
                    TextDiagonalesTraseras.Visibility = Visibility.Collapsed;
                    DiagonalesTraseras.Visibility = Visibility.Collapsed;

                    Grid.SetRow(TextEstabilizador, 3);
                    Grid.SetRow(Estabilizador, 3);

                }
                else if (UnaDiagonal.IsChecked == true)
                {
                    SinDiagonal.IsChecked = false;
                    DosDiagonal.IsChecked = false;

                    TextDiagonalesDelanteras.Visibility = Visibility.Visible;
                    DiagonalesDelanteras.Visibility = Visibility.Visible;
                    TextDiagonalesTraseras.Visibility = Visibility.Collapsed;
                    DiagonalesTraseras.Visibility = Visibility.Collapsed;
                    TextDiagonalesDelanteras.Content = "Diagonales";

                    Grid.SetRow(TextDiagonalesDelanteras, 3);
                    Grid.SetRow(DiagonalesDelanteras, 3);
                    Grid.SetRow(TextEstabilizador, 4);
                    Grid.SetRow(Estabilizador, 4);


                }
                else if (DosDiagonal.IsChecked == true)
                {
                    SinDiagonal.IsChecked = false;
                    UnaDiagonal.IsChecked = false;

                    TextDiagonalesDelanteras.Visibility = Visibility.Visible;
                    DiagonalesDelanteras.Visibility = Visibility.Visible;
                    TextDiagonalesTraseras.Visibility = Visibility.Visible;
                    DiagonalesTraseras.Visibility = Visibility.Visible;
                    TextDiagonalesDelanteras.Content = "Diagonales Delanteras";

                    Grid.SetRow(TextDiagonalesDelanteras, 3);
                    Grid.SetRow(DiagonalesDelanteras, 3);
                    Grid.SetRow(TextDiagonalesTraseras, 4);
                    Grid.SetRow(DiagonalesTraseras, 4);
                    Grid.SetRow(TextEstabilizador, 5);
                    Grid.SetRow(Estabilizador, 5);
                }

            }
            else if (sender == Biposte && Biposte.IsChecked == true)
            {
                Monoposte.IsChecked = false;

                TextPilaresDelanteros.Visibility = Visibility.Visible;
                PilaresDelanteros.Visibility = Visibility.Visible;
                TextPilaresTraseros.Visibility = Visibility.Visible;
                PilaresTraseros.Visibility = Visibility.Visible;
                TextPilaresDelanteros.Content = "Pilares Delanteros";

                Grid.SetRow(TextVigas, 2);
                Grid.SetRow(Vigas, 2);
                Grid.SetRow(TextCorreas, 3);
                Grid.SetRow(Correas, 3);

                if (SinDiagonal.IsChecked == true)
                {
                    UnaDiagonal.IsChecked = false;
                    DosDiagonal.IsChecked = false;

                    TextDiagonalesDelanteras.Visibility = Visibility.Collapsed;
                    DiagonalesDelanteras.Visibility = Visibility.Collapsed;
                    TextDiagonalesTraseras.Visibility = Visibility.Collapsed;
                    DiagonalesTraseras.Visibility = Visibility.Collapsed;

                    Grid.SetRow(TextEstabilizador, 4);
                    Grid.SetRow(Estabilizador, 4);

                }
                else if (UnaDiagonal.IsChecked == true)
                {
                    SinDiagonal.IsChecked = false;
                    DosDiagonal.IsChecked = false;

                    TextDiagonalesDelanteras.Visibility = Visibility.Visible;
                    DiagonalesDelanteras.Visibility = Visibility.Visible;
                    TextDiagonalesTraseras.Visibility = Visibility.Collapsed;
                    DiagonalesTraseras.Visibility = Visibility.Collapsed;
                    TextDiagonalesDelanteras.Content = "Diagonales";

                    Grid.SetRow(TextDiagonalesDelanteras, 3);
                    Grid.SetRow(DiagonalesDelanteras, 3);
                    Grid.SetRow(TextEstabilizador, 4);
                    Grid.SetRow(Estabilizador, 4);


                }
                else if (DosDiagonal.IsChecked == true)
                {
                    SinDiagonal.IsChecked = false;
                    UnaDiagonal.IsChecked = false;

                    TextDiagonalesDelanteras.Visibility = Visibility.Visible;
                    DiagonalesDelanteras.Visibility = Visibility.Visible;
                    TextDiagonalesTraseras.Visibility = Visibility.Visible;
                    DiagonalesTraseras.Visibility = Visibility.Visible;
                    TextDiagonalesDelanteras.Content = "Diagonales Delanteras";

                    Grid.SetRow(TextDiagonalesDelanteras, 3);
                    Grid.SetRow(DiagonalesDelanteras, 3);
                    Grid.SetRow(TextDiagonalesTraseras, 4);
                    Grid.SetRow(DiagonalesTraseras, 4);
                    Grid.SetRow(TextEstabilizador, 5);
                    Grid.SetRow(Estabilizador, 5);
                }
            }
            else if (sender == SinDiagonal && SinDiagonal.IsChecked == true)
            {
                UnaDiagonal.IsChecked = false;
                DosDiagonal.IsChecked = false;

                TextDiagonalesDelanteras.Visibility = Visibility.Collapsed;
                DiagonalesDelanteras.Visibility = Visibility.Collapsed;
                TextDiagonalesTraseras.Visibility = Visibility.Collapsed;
                DiagonalesTraseras.Visibility = Visibility.Collapsed;

                if (Monoposte.IsChecked == true)
                {
                    Grid.SetRow(TextEstabilizador, 3);
                    Grid.SetRow(Estabilizador, 3);
                }
                else if (Biposte.IsChecked == true)
                {
                    Grid.SetRow(TextEstabilizador, 4);
                    Grid.SetRow(Estabilizador, 4);
                }
            }
            else if (sender == UnaDiagonal && UnaDiagonal.IsChecked == true)
            {
                SinDiagonal.IsChecked = false;
                DosDiagonal.IsChecked = false;

                TextDiagonalesDelanteras.Visibility = Visibility.Visible;
                DiagonalesDelanteras.Visibility = Visibility.Visible;
                TextDiagonalesTraseras.Visibility = Visibility.Collapsed;
                DiagonalesTraseras.Visibility = Visibility.Collapsed;
                TextDiagonalesDelanteras.Content = "Diagonales";

                if (Monoposte.IsChecked == true)
                {
                    Grid.SetRow(TextDiagonalesDelanteras, 3);
                    Grid.SetRow(DiagonalesDelanteras, 3);
                    Grid.SetRow(TextEstabilizador, 4);
                    Grid.SetRow(Estabilizador, 4);
                }
                else if (Biposte.IsChecked == true)
                {
                    Grid.SetRow(TextDiagonalesDelanteras, 4);
                    Grid.SetRow(DiagonalesDelanteras, 4);
                    Grid.SetRow(TextEstabilizador, 5);
                    Grid.SetRow(Estabilizador, 5);
                }

            }
            else if (sender == DosDiagonal && DosDiagonal.IsChecked == true)
            {
                SinDiagonal.IsChecked = false;
                UnaDiagonal.IsChecked = false;

                TextDiagonalesDelanteras.Visibility = Visibility.Visible;
                DiagonalesDelanteras.Visibility = Visibility.Visible;
                TextDiagonalesTraseras.Visibility = Visibility.Visible;
                DiagonalesTraseras.Visibility = Visibility.Visible;
                TextDiagonalesDelanteras.Content = "Diagonales Delanteras";

                if (Monoposte.IsChecked == true)
                {
                    Grid.SetRow(TextDiagonalesDelanteras, 3);
                    Grid.SetRow(DiagonalesDelanteras, 3);
                    Grid.SetRow(TextDiagonalesTraseras, 4);
                    Grid.SetRow(DiagonalesTraseras, 4);
                    Grid.SetRow(TextEstabilizador, 5);
                    Grid.SetRow(Estabilizador, 5);
                }
                else if (Biposte.IsChecked == true)
                {
                    Grid.SetRow(TextDiagonalesDelanteras, 4);
                    Grid.SetRow(DiagonalesDelanteras, 4);
                    Grid.SetRow(TextDiagonalesTraseras, 5);
                    Grid.SetRow(DiagonalesTraseras, 5);
                    Grid.SetRow(TextEstabilizador, 6);
                    Grid.SetRow(Estabilizador, 6);
                }
            }
            else if (sender == Pilares_laminados && Pilares_laminados.IsChecked == true)
            {
                Pilares_conformados.IsChecked = false;
            }
            else if (sender == Pilares_conformados && Pilares_conformados.IsChecked == true)
            {
                Pilares_laminados.IsChecked=false;
            }
        }
    }
}
