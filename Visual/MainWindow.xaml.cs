using Core.Exceptions;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Visual.Controllers;
using Visual.Data;
using Visual.Interfaces;

namespace Visual
{
    public partial class MainWindow : Window, IPaint, IPrinteable
    {
        private const string SETTINGS_KEY = "Settings";
        public CanvasData Canvas { get; set; }
        public BrushData? Brush { get; set; }
        public StringBuilder SB { get; set; }

        private Settings settings;
        private FuncControler func;
        private ActionControler act;
        private MainController main;

        private GridLength errorsPanelHeight;

        private CodeInfo? codeInfo;

        private bool _isApplyingFormat = false;
        private bool _isHiddenErrors = false;

        public MainWindow()
        {
            InitializeComponent();
            settings = (Resources[SETTINGS_KEY] as Settings)!;

            Canvas = new CanvasData();
            SB = new StringBuilder();
            func = new(this);
            act = new(this, this);
            main = new(func, act);

            errorsPanelHeight = new GridLength(120, GridUnitType.Pixel);

            ShowErrorPanel();

            SuggestionPopup.PlacementTarget = CodeEditor;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
            => Resize(Canvas.Dimension);

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isApplyingFormat || _isHiddenErrors)
                return;
            
            _isApplyingFormat = true;

            string code = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text;

            UpdateLineNumbers();

            Errors.Text = "";
            ErrorsLine.Text = "";
            if (code == "")
                return;

            codeInfo = new CodeInfo(func.GetFuncs(), act.GetActs(), code);
            
            ShowSuggest(codeInfo);

            if (!main.TryCode(codeInfo, out List<ExceptionWL>? exceptions))
            {

                StringBuilder sb = new();
                UpdateErrors(exceptions!);
                foreach (var error in exceptions!)
                {
                    sb.AppendLine(error.Message);
                }
                Errors.Text = sb.ToString();
            }

            _isApplyingFormat = false;
        }

        private void CodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (codeInfo is null || Errors.Text != "")
                return;

            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            switch (e.Key)
            {
                case Key.Enter:
                    Excecute();
                    break;
            }
        }

        private void CodeEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HideUnderLineErrors();

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Space)
            {
                e.Handled = true;
                ShowSuggest(codeInfo!);
                return;
            }

            NavigateSuggestions(e);
        }

        private void SuggestionClick(object sender, MouseButtonEventArgs e)
        {
            ApplySuggestion();
        }

        private void MainKeyControl(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                if (Errors.Visibility == Visibility.Visible)
                {
                    HideErrorsPanel();
                    return;
                }

                ShowErrorPanel();
                return;
            }
        }

        private void ExecuteClick(object sender, RoutedEventArgs e)
            => Excecute();
        
        private void ResizeEvent(object sender, RoutedEventArgs e) 
            => Resize();

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                FileName = "code",
                DefaultExt = ".pw",
                Filter = "Archivos de Wall_E (*.pw)|*.pw|Todos los archivos (*.*)|*.*"
            };

            bool? resultado = saveFileDialog.ShowDialog();

            if (resultado == true)
            {
                string rutaArchivo = saveFileDialog.FileName;

                try
                {
                    var text = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text;
                    File.WriteAllText(rutaArchivo, text);

                    MessageBox.Show("¡Archivo guardado exitosamente!", "Guardado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                DefaultExt = ".pw",
                Filter = "Archivos de Wall_E (*.pw)|*.pw|Todos los archivos (*.*)|*.*"
            };

            bool? resultado = openFileDialog.ShowDialog();

            if (resultado == true)
            {
                string rutaArchivo = openFileDialog.FileName;

                try
                {
                    string contenidoArchivo = File.ReadAllText(rutaArchivo);

                    new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text = contenidoArchivo;             
                    MessageBox.Show("¡Archivo cargado exitosamente!", "Cargado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResizeClick(object sender, RoutedEventArgs e)
        {
            string inputDim_str;

            inputDim_str = Interaction.InputBox(
                "Por favor, ingrese la dimensión:",
                "Ingresar Dimensiión",
                "0"
            );

            if (string.IsNullOrEmpty(inputDim_str))
                return;

            if (int.TryParse(inputDim_str, out int valueDim) && valueDim >= 0)
            {
                MessageBox.Show($"Has ingresado el valor:\nDimensión = {valueDim}");

                Resize(valueDim);
            }
            else
            {
                MessageBox.Show("Uno o ambos valores no son números naturales válidos.", "Error de Entrada", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Settings
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
