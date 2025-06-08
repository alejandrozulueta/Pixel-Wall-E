using Parser.Models;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Visual.Controllers;
using Visual.Data;
using Visual.Interfaces;

namespace Visual
{
    public partial class MainWindow : Window, IPaint
    {
        private const string SETTINGS_KEY = "Settings";
        public CanvasData Canvas { get; set; }
        public BrushData? Brush { get; set; }

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
            func = new(this);
            act = new(this);
            main = new(func, act);

            errorsPanelHeight = new GridLength(120, GridUnitType.Pixel);
            ShowErrorPanel();

            SuggestionPopup.PlacementTarget = CodeEditor;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            string code = CodeEditor.Text;

            if (code == "")
                return;

            codeInfo = new CodeInfo(func.GetFuncs(), act.GetActs(), code);

            GetSuggest(codeInfo);

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

        private void ResizeEvent(object sender, RoutedEventArgs e) =>
            Resize();
        
        private void CodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (codeInfo is null || Errors.Text != "")
                return;

            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Key == Key.Space)
            {
                GetSuggest(codeInfo);
                return;
            }

            if (e.Key != Key.Enter)
                return;

            Canvas = new CanvasData(Canvas.Rows, Canvas.Cols);
            Brush = null;
            try
            {
                main.ExecuteCode(codeInfo!);
            }
            catch (Exception ex) 
            {
                // PopUp
                return;
            }
            DrawGrid();
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

        private void Resize(int rows = 20, int cols = 20)
        {
            Canvas = new CanvasData(rows, cols);
            DrawGrid();
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

        private void GetSuggest(CodeInfo codeInfo)
        {
            string currentText = CodeEditor.Text;
            int caretIndex = CodeEditor.CaretIndex;
            List<string> Sugg = [];
            int horizontalMargin = 15;

            SuggestionPopup.IsOpen = false;

            if (string.IsNullOrWhiteSpace(currentText) || caretIndex == 0)
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            string wordToSuggest = GetWord(currentText, caretIndex);

            Sugg = GetSugg(codeInfo, wordToSuggest);

            if (!Sugg.Any())
                return;

            if (string.IsNullOrWhiteSpace(wordToSuggest))
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            SuggestionListBox.ItemsSource = Sugg;
            SuggestionListBox.SelectedIndex = -1;

            Rect caretVisualRect = CodeEditor.GetRectFromCharacterIndex(caretIndex);

            Point placementPoint;


            if (caretIndex > 0)
            {
                Rect prevCharRect = CodeEditor.GetRectFromCharacterIndex(caretIndex - 1);
                placementPoint = new Point(prevCharRect.Right, prevCharRect.Top);
            }

            placementPoint = new Point(caretVisualRect.Left + horizontalMargin, caretVisualRect.Top);

            SuggestionPopup.CustomPopupPlacementCallback = GetPopupPlacementCallback(placementPoint);

            SuggestionPopup.IsOpen = true;

        }

        private List<string> GetSugg(CodeInfo codeInfo, string wordToSuggest)
        {
            List<string> suggs = [];

            var tokens = codeInfo.Tokens;

            foreach (var act in codeInfo.Context.Actions.Keys)
            {
                if (act.ToLower().StartsWith(wordToSuggest.ToLower()))
                    suggs.Add(act);
            }

            foreach (var func in codeInfo.Context.Functions.Keys)
            {
                if (func.ToLower().StartsWith(wordToSuggest.ToLower()))
                    suggs.Add(func);
            }


            for (int i = 0; i < tokens.Length - 1; i++)
            {
                var token = tokens[i];
                var indentifier = token.Identifier;

                if (indentifier == wordToSuggest)
                    break;

                if (tokens[i + 1].Type is not TokenType.AssingOperator)
                    continue;

                if (indentifier.ToLower().StartsWith(wordToSuggest.ToLower()) && !suggs.Contains(indentifier))
                    suggs.Add(indentifier);
            }

            return suggs;
        }

        private CustomPopupPlacementCallback GetPopupPlacementCallback(Point placementPoint)
        {
            return (popupSize, targetSize, offset) =>
            {
                return
                [
            new CustomPopupPlacement(placementPoint, PopupPrimaryAxis.Horizontal),
            new CustomPopupPlacement(new Point(placementPoint.X - popupSize.Width - CodeEditor.FontSize, placementPoint.Y), PopupPrimaryAxis.Horizontal),
            new CustomPopupPlacement(new Point(placementPoint.X, placementPoint.Y + CodeEditor.FontSize), PopupPrimaryAxis.Vertical),
            new CustomPopupPlacement(new Point(placementPoint.X, placementPoint.Y - popupSize.Height), PopupPrimaryAxis.Vertical)
                ];
            };
        }


        private string GetWord(string text, int caretIndex)
        {
            int startIndex = caretIndex - 1;

            while (startIndex >= 0)
            {
                char currentChar = text[startIndex];

                if (startIndex == 0)
                    break;

                if (char.IsWhiteSpace(currentChar) ||
                    currentChar == '(' || currentChar == ')' ||
                    currentChar == '{' || currentChar == '}' ||
                    currentChar == '[' || currentChar == ']' ||
                    currentChar == '.' || currentChar == ',' ||
                    currentChar == ';' || currentChar == ':')
                {
                    startIndex++;
                    break;
                }

                startIndex--;
            }
            return text[startIndex..caretIndex];
        }
    }

    public class Settings
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
