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

        public MainWindow()
        {
            InitializeComponent();
            settings = (Resources[SETTINGS_KEY] as Settings)!;
            
            Canvas = new CanvasData();
            Brush = null;
            func = new(Canvas, Brush);
            act = new(Canvas, Brush);
            main = new(func, act);

            DrawGrid();
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

        private void UpdateGrid(int row, int col, Color color) 
        { 
            
        }

        private void CodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string code = CodeEditor.Text;

                main.ExecuteCode(code);

                DrawGrid();
            }
        }
    }

    public class Settings
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}