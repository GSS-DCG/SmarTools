﻿using System;
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
using ModernUI.View;
using SmarTools.Model.Applications;

namespace SmarTools.APPS
{
    /// <summary>
    /// Lógica de interacción para Dimensionamiento1VAPP.xaml
    /// </summary>
    public partial class Dimensionamiento1VAPP : Window
    {
        public Dimensionamiento1VAPP()
        {
            InitializeComponent();
            Herramientas.AbrirArchivoSAP2000();
            Dimensionamiento1V.ObtenerMateriales(this);
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
            Dimensionamiento1V.Dimensionar1V(this);
        }

        private void btnFiltrarPerfiles_Click(object sender, RoutedEventArgs e)
        {
            Dimensionamiento1V.FiltrarPerfiles(this);
        }

        private void btnAsignarPerfiles_Click(object sender, RoutedEventArgs e)
        {
            Dimensionamiento1V.AsignarPerfiles(this);
        }
    }
}
