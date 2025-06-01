using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Visual.Controllers;
using Visual.Data;

namespace Visual
{
    public partial class MainWindow : Window
    {
        private const string SETTINGS_KEY = "Settings";
        public CanvasData Canvas { get; protected set; }
        public BrushData? Brush { get; protected set; }

        private Settings settings;
        private FuncControler func;
        private ActionControler act;
        private MainController main;

        private GridLength errorsPanelHeight;

        private CodeInfo? codeInfo;

        public MainWindow()
        {
            InitializeComponent();
            settings = (Resources[SETTINGS_KEY] as Settings)!;
            
            Canvas = new CanvasData();
            Brush = null;
            func = new(Canvas, Brush);
            act = new(Canvas, Brush);
            main = new(func, act);

            errorsPanelHeight = new GridLength(120, GridUnitType.Pixel);
            ShowErrorPanel();
        }


        private void DrawGrid()
        {
          
            DrawCanvas.Children.Clear();

            double widthCells = DrawCanvas.ActualWidth / Canvas.Cols;
            double heightCells = DrawCanvas.ActualHeight / Canvas.Rows;

            for (int i = 0; i < Canvas.Rows; i++) 
            {
                for (int j = 0; j < Canvas.Cols; j++)
                {
                    var color = Canvas.CellsColor[i, j];
                    Rectangle celdaRect = new()
                    {
                        Width = widthCells,
                        Height = heightCells,
                        Fill = new SolidColorBrush(Color.FromArgb(
                            color.A,
                            color.R,
                            color.G,
                            color.B
                            )),
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5
                    };

                
                    System.Windows.Controls.Canvas.SetLeft(celdaRect, j * widthCells);
                    System.Windows.Controls.Canvas.SetTop(celdaRect, i * heightCells);

                    DrawCanvas.Children.Add(celdaRect);
                }
            }
        }

        private void ShowErrorPanel() 
        {
            if (!(Errors.Visibility == Visibility.Collapsed))
                return;

            EditorArea.Height = new GridLength(1, GridUnitType.Star);
            ErrorsArea.Height = errorsPanelHeight;
            Errors.Visibility = Visibility.Visible;
        }

        public void HideErrorsPanel()
        {
            Errors.Visibility = Visibility.Collapsed;

            EditorArea.Height = new GridLength(1, GridUnitType.Star);

            ErrorsArea.Height = GridLength.Auto;

        }

        private void CodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (codeInfo is null || Errors.Text != "")
                return;
            
            if (e.Key == Key.Enter)
            {
                main.ExecuteCode(codeInfo!);
                DrawGrid();
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            string code = CodeEditor.Text;

            if (code == "")
                return;

            codeInfo = new CodeInfo(func.GetFuncs(), act.GetActs(), code);

            Errors.Text = "";

            if (!main.TryCode(codeInfo, out List<Exception>? exceptions))
            {
                StringBuilder sb = new();

                foreach (var error in exceptions!)
                {
                    sb.AppendLine(error.Message);
                }
                Errors.Text = sb.ToString();
            }
        }
    }

    public class Settings
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}