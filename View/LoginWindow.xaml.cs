using Microsoft.VisualBasic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernUI.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        static class Globales 
        {
            public static string _txtUser;
            public static string _txtPassword;
        }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }

        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUser.Text != null && txtPassword.Text != null)
            {

                if(txtPassword.Text == HerramientasAuxiliares.login_sizesmart(txtUser.Text))
                {
                    MainView mainView = new MainView();
                    mainView.Show();
                    this.Close();
                }
                else
                {
                    txtPassword.Clear();
                    ContraseñaIncorrecta.Text = "Contraseña incorrecta";
                }
            }

            else
            {
                txtPassword.Clear();
                ContraseñaIncorrecta.Text = "Contraseña incorrecta";
            }
        }
    }

    public class HerramientasAuxiliares
    {
        public static string login_sizesmart(string usuario)
        {
            double num = 0.0;
            char[] array = usuario.ToCharArray();

            for (int i = 0; i < array.Length; i++)
            {
                num += (double)(int)(Math.Pow((int)array[i], 0.2) * 10000.0);
            }
            CultureInfo cultureInfo = new CultureInfo("es-ES");
            DateTime now = DateTime.Now;
            int month = now.Month;
            int year = now.Year;
            num = (int)(num * (double)(month + 10) / (double)(year - 2000));

            return (num * 3).ToString();
        }
    }
}