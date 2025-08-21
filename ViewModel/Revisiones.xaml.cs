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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ComponentModel;

namespace SmarTools.ViewModel
{
    /// <summary>
    /// Lógica de interacción para Revisiones.xaml
    /// </summary>
    public partial class Revisiones : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<string> Revisions1V { get; set; }= new ObservableCollection<string>();
        public ObservableCollection<string> Revisions2V { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> RevisionsCoeficientes { get; set; } = new ObservableCollection<string>();

        private string _revision1VSeleccionada;

        public string Revision1VSeleccionada
        {
            get => _revision1VSeleccionada;
            set
            {
                if(_revision1VSeleccionada != value)
                {
                    _revision1VSeleccionada = value;
                    OnPropertyChanged(nameof(Revision1VSeleccionada));
                }
            }
        }

        private string _revision2VSeleccionada;

        public string Revision2VSeleccionada
        {
            get => _revision2VSeleccionada;
            set
            {
                if (_revision2VSeleccionada != value)
                {
                    _revision2VSeleccionada = value;
                    OnPropertyChanged(nameof(Revision2VSeleccionada));
                }
            }
        }

        private string _revisionCoeficientesSeleccionada;

        public string RevisionCoeficientesSeleccionada
        {
            get => _revisionCoeficientesSeleccionada;
            set
            {
                if (_revisionCoeficientesSeleccionada != value)
                {
                    _revisionCoeficientesSeleccionada = value;
                    OnPropertyChanged(nameof(RevisionCoeficientesSeleccionada));
                }
            }
        }

        public Revisiones()
        {
            InitializeComponent();
            DataContext = this;
            CargarRevisiones();
        }
        
        private void CargarRevisiones()
        {
            //Uniones 1V
            string ruta = @"Z:\300SmarTools\03 Uniones";
            string patron = @"Uniones 1VR5_(\d{2})\.xlsx";

            if (Directory.Exists(ruta))
            {
                var archivos = Directory.GetFiles(ruta, "Uniones 1VR5_*.xlsx");

                var revisiones = archivos
                    .Select(System.IO.Path.GetFileName)
                    .Select(nombre =>
                    {
                        var match = Regex.Match(nombre, patron);
                        return match.Success ? match.Groups[1].Value : null;
                    })
                    .Where(xx => xx != null)
                    .Distinct()
                    .OrderBy(xx => xx)
                    .ToList();

                Revisions1V.Clear();
                foreach (var rev in revisiones)
                    Revisions1V.Add(rev);

                Revision1VSeleccionada = Revisions1V.LastOrDefault();
            }

            //Uniones 2V
            ruta = @"Z:\300SmarTools\03 Uniones";
            patron = @"Uniones 2VR4_(\d{2})\.xlsx";

            if (Directory.Exists(ruta))
            {
                var archivos = Directory.GetFiles(ruta, "Uniones 2VR4_*.xlsx");

                var revisiones = archivos
                    .Select(System.IO.Path.GetFileName)
                    .Select(nombre =>
                    {
                        var match = Regex.Match(nombre, patron);
                        return match.Success ? match.Groups[1].Value : null;
                    })
                    .Where(xx => xx != null)
                    .Distinct()
                    .OrderBy(xx => xx)
                    .ToList();

                Revisions2V.Clear();
                foreach (var rev in revisiones)
                    Revisions2V.Add(rev);

                Revision2VSeleccionada = Revisions2V.LastOrDefault();
            }

            //Coeficientes
            ruta = @"Z:\300SmarTools\04 Combinaciones";
            patron = @"Coeficientes_(\d{2})\.xlsx";

            if (Directory.Exists(ruta))
            {
                var archivos = Directory.GetFiles(ruta, "Coeficientes_*.xlsx");

                var revisiones = archivos
                    .Select(System.IO.Path.GetFileName)
                    .Select(nombre =>
                    {
                        var match = Regex.Match(nombre, patron);
                        return match.Success ? match.Groups[1].Value : null;
                    })
                    .Where(xx => xx != null)
                    .Distinct()
                    .OrderBy(xx => xx)
                    .ToList();

                RevisionsCoeficientes.Clear();
                foreach (var rev in revisiones)
                    RevisionsCoeficientes.Add(rev);

                RevisionCoeficientesSeleccionada = RevisionsCoeficientes.LastOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nombrePropiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}
