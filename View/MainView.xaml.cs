using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using SmarTools.ViewModel;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using SAP2000v1;
using Microsoft.Win32;
using Microsoft.VisualBasic;
using System.Windows.Forms;

namespace ModernUI.View
{
    /// <summary>
    /// Lógica de interacción para MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public static class Globales
        {
            public static string _version = "Version_0.0";

            public static Inicio _inicio = new Inicio();
            public static TracSmart1V _TracSmart1V = new TracSmart1V();
            public static TracSmart2V _TracSmart2V = new TracSmart2V();
            public static RackSmart _RackSmart = new RackSmart();
            public static Ajustes _Ajustes = new Ajustes();
            public static FontAwesome.Sharp.IconImage _BellNotification = new FontAwesome.Sharp.IconImage();

            public static cHelper _myHelper;
            public static cOAPI _mySapObject;
            public static cSapModel _mySapModel;

        }
        public MainView()
        {
            InitializeComponent();
            Caption_Text.Text = "Inicio";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Home;
            MainViewContentControl.Children.Add(Globales._inicio);
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            VersionInfoText.Text = Globales._version.Replace("_", " ");

            Globales._BellNotification = BellNotification;

            //Ejecutamos de manera asincrona el SAP2000
            this.Loaded += MainView_Loaded;
        }

        private async void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            await Herramientas.ConexionSAP2000Async();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnlcControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void pnlControlBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            try { Globales._mySapObject.Unhide(); } catch { }
            try { Globales._mySapObject.ApplicationExit(false); } catch { }
            Globales._mySapModel = null;
            Globales._mySapObject = null;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal) { this.WindowState = WindowState.Maximized; } else { this.WindowState = WindowState.Normal; }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnSettingsIcon_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "Ajustes";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Gear;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._Ajustes);
        }

        private void btnBellIcon_Click(object sender, RoutedEventArgs e)
        {
            /////////
            ///WIP///
            /////////
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "Inicio";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Home;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._inicio);

            Globales._inicio.InfoVersionView.Text = SmarTools.ViewModel.Inicio.InfoVersion();
        }

        private void btnTracSmart1V_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "TracSmart+ 1P";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.SolarPanel;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._TracSmart1V);
        }

        private void btnTracSmart2V_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "TracSmart+ 2P";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.SolarPanel;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._TracSmart2V);
        }

        private void btnRacksmart_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "RackSmarT";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.SolarPanel;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._RackSmart);
        }

        private void btnAjustes_Click(object sender, RoutedEventArgs e)
        {
            Caption_Text.Text = "Ajustes";
            Caption_Icon.Icon = FontAwesome.Sharp.IconChar.Gear;
            MainViewContentControl.Children.Clear();
            MainViewContentControl.Children.Add(Globales._Ajustes);
        }
    }
    public class Herramientas
    {
        public static void NotificacionCampana()
        {
            string ruta = @"Z:\300Logos\Version.txt";
            string version = File.ReadAllText(ruta);

            if (version != ModernUI.View.MainView.Globales._version)
            {
                ModernUI.View.MainView.Globales._BellNotification.Visibility = Visibility.Visible;
            }
            else
            {
                ModernUI.View.MainView.Globales._BellNotification.Visibility = Visibility.Hidden;
            }
        }

        public static async Task ConexionSAP2000Async()
        {
            await Task.Run(() =>
            {
                cHelper myHelper;
                cOAPI mySapObject;
                cSapModel mySapModel;

                string ProgramPath = @"C:\Program Files\Computers and Structures\SAP2000 25\SAP2000.exe";

                try
                {
                    myHelper = (cHelper)Activator.CreateInstance(Type.GetTypeFromProgID("SAP2000v1.Helper", true));
                    mySapObject = myHelper.CreateObject(ProgramPath);
                    mySapObject.ApplicationStart(eUnits.kN_m_C);

                    mySapModel = mySapObject.SapModel;

                    // Guardar en variables globales (esto debe hacerse en el hilo principal si afecta a la UI)
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainView.Globales._myHelper = myHelper;
                        MainView.Globales._mySapObject = mySapObject;
                        MainView.Globales._mySapModel = mySapModel;
                    });

                    mySapObject.Hide();
                }
                catch (Exception ex)
                {
                    // Puedes mostrar un mensaje si quieres
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show($"Error al iniciar SAP2000: {ex.Message}");
                    });
                }
            });
        }

        //Buscar Rutas en el explorador de windows
        public static string BuscarArchivo()
        {
            Microsoft.Win32.FileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "Seleccionar archivo";
            openFileDialog.Filter = "SAP2000 (*.sdb)|*.sdb" + "|Todos los archivos (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string BuscarCarpeta()
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

            DialogResult result = openFolderDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                return openFolderDialog.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        //Abrir Archivos en SAP2000
        public static int AbrirArchivoSAP2000()
        {
            int ret = 0;
            string ruta_abrir = BuscarArchivo();
            MessageBoxResult resultado = MessageBoxResult.Yes;

            while (ruta_abrir == string.Empty)
            {
                resultado = System.Windows.MessageBox.Show("No se ha seleccionado un archivo de SAP2000\n ¿Cerrar SAP2000?", "ERROR", System.Windows.MessageBoxButton.YesNo);

                if (resultado == MessageBoxResult.Yes) 
                {
                    ret = MainView.Globales._mySapObject.Hide();

                    return ret;
                }
                else if (resultado == MessageBoxResult.No)
                {
                    ruta_abrir = BuscarArchivo();
                }
            }

            ret = AbrirArchivo(ruta_abrir);

            return ret;
        } 

        private static int AbrirArchivo(string ruta_archivo)
        {
            int ret = 0;

            ret = MainView.Globales._mySapObject.Unhide();

            ret = MainView.Globales._mySapModel.InitializeNewModel(eUnits.kN_m_C);

            ret = MainView.Globales._mySapModel.File.OpenFile(ruta_archivo);

            ret = CambiarUnidadesSAP2000();

            return ret;
        }

        //Guardar archivos en SAP2000
        public static int GuardarArchivoSAP2000()
        {
            int ret = 0;
            string ruta_guardado = BuscarCarpeta();
            MessageBoxResult resultado = MessageBoxResult.None;

            while (resultado != MessageBoxResult.Yes)
            {
                resultado = System.Windows.MessageBox.Show("No se ha seleccionado un archivo de SAP2000\n ¿Cerrar SAP2000?", "ERROR", System.Windows.MessageBoxButton.YesNo);

                if (resultado == MessageBoxResult.Yes)
                {
                    ret = MainView.Globales._mySapObject.Hide();

                    return ret;
                }
                else if (resultado == MessageBoxResult.No)
                {
                    ruta_guardado = BuscarArchivo();
                }
            }

            ret = GuardarArchivo(ruta_guardado);

            return ret;
        }

        private static int GuardarArchivo(string ruta_guardado_archivo)
        {
            int ret = 0;

            ret = MainView.Globales._mySapModel.SetPresentUnits(eUnits.kN_m_C);

            return ret;
        }

        //Edicion Opciones SAP2000
        public static int CambiarUnidadesSAP2000()
        {
            int ret = 0;

            ret = MainView.Globales._mySapModel.SetPresentUnits(eUnits.kN_m_C);

            return ret;
        }
    }
}
