using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Ventana_TEST.Styles
{
    public partial class ControlBarButton : UserControl
    {
        public ControlBarButton()
        {
            InitializeComponent();
            this.Loaded += MostrarTituloVentana;
        }

        private void MostrarTituloVentana(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                ControlBarTxt.Text = parentWindow.Title;

            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnlcControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                WindowInteropHelper helper = new WindowInteropHelper(parentWindow);
                SendMessage(helper.Handle, 0xA1, 0x2, 0); // 0xA1 = WM_NCLBUTTONDOWN, 0x2 = HTCAPTION
            }
        }

        private void pnlControlBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.WindowState = parentWindow.WindowState == WindowState.Normal
                    ? WindowState.Maximized
                    : WindowState.Normal;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.WindowState = WindowState.Minimized;
            }
        }
    }
}
