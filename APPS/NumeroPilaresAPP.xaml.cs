﻿using System;
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
using SmarTools.Model.Repository;
using ModernUI.View;
using SmarTools.Model.Applications;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace SmarTools.APPS
{
    /// <summary>
    /// Lógica de interacción para NumeroPilaresAPP.xaml
    /// </summary>
    public partial class NumeroPilaresAPP : Window
    {
        public NumeroPilaresAPP()
        {
            InitializeComponent();
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

        private void btnExaminarESMA_Click(object sender, RoutedEventArgs e)
        {
            string ruta = WindowsFunctions.SearchFile();
            RutaESMA.Text = ruta;
        }

        private void btnCalcular_Click(object sender, RoutedEventArgs e)
        {
            NumeroPilares.NumeroPilares1V(this);
        }
    }
}
