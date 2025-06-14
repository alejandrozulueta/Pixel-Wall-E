using Core.Exceptions;
using Expressions.Enum;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Visual.Data;
using Visual.Interfaces;

namespace Visual
{
    public partial class MainWindow : Window, IPaint, IPrinteable
    {
        private void Excecute()
        {
            SB.Clear();
            Canvas = new CanvasData(Canvas.Rows, Canvas.Cols);
            Brush = null;
            try
            {
                main.ExecuteCode(codeInfo!);
            }
            catch (TargetInvocationException tie)
            {
                MessageBox.Show(
                    tie.InnerException?.Message,
                    "Execution Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Execution Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            DrawGrid();
            Ouput.Text = SB.ToString();
        }

        private void UpdateErrors(List<ExceptionWL> exceptions)
        {
            StringBuilder errors = new StringBuilder();
            StringBuilder lines = new StringBuilder();
            for (int i = 0; i < exceptions.Count; i++)
            {
                errors.AppendLine(exceptions[i].Message);
                lines.AppendLine(exceptions[i].Location.Row.ToString());
            }

            Errors.Text = errors.ToString();
            ErrorsLine.Text = lines.ToString();
        }

        private void UpdateLineNumbers()
        {
            int lineCount = CodeEditor.LineCount;

            StringBuilder lineNumbersText = new();
            for (int i = 1; i <= lineCount; i++)
            {
                lineNumbersText.AppendLine(i.ToString());
            }

            Lines.Text = lineNumbersText.ToString();
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
            if (!(Tap.Visibility == Visibility.Collapsed))
                return;

            EditorArea.Height = new GridLength(1, GridUnitType.Star);
            ErrorsArea.Height = errorsPanelHeight;

            Tap.Visibility = Visibility.Visible;
            Errors.Visibility = Visibility.Visible;
        }

        public void HideErrorsPanel()
        {
            Tap.Visibility = Visibility.Collapsed;
            Errors.Visibility = Visibility.Collapsed;
            EditorArea.Height = new GridLength(1, GridUnitType.Star);
            ErrorsArea.Height = GridLength.Auto;
        }

        private void ShowSuggest(CodeInfo codeInfo)
        {
            string currentText = CodeEditor.Text;
            int caretIndex = CodeEditor.CaretIndex;
            List<string> Sugg = [];
            int horizontalMargin = 15;

            if (SuggestionPopup.IsOpen == true)
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            SuggestionPopup.IsOpen = false;

            if (string.IsNullOrWhiteSpace(currentText) || caretIndex == 0)
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            string wordToSuggest = GetWord(currentText, caretIndex);

            Sugg = GetSuggest(codeInfo, wordToSuggest);

            if (Sugg.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(wordToSuggest))
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            SuggestionListBox.ItemsSource = Sugg;
            SuggestionListBox.SelectedIndex = -1;

            SuggestionListBox.SelectedIndex = 0;

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

        private List<string> GetSuggest(CodeInfo codeInfo, string wordToSuggest)
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

        private void ApplySuggestion()
        {
            if (SuggestionListBox.SelectedItem is not string selectedSugg)
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            string currentText = CodeEditor.Text;
            int caretIndex = CodeEditor.CaretIndex;

            string wordToReplace = GetWord(currentText, caretIndex);

            int startIndex = caretIndex - wordToReplace.Length;

            string newText = currentText.Remove(startIndex, wordToReplace.Length);
            newText = newText.Insert(startIndex, selectedSugg);

            CodeEditor.Text = newText;

            CodeEditor.CaretIndex = startIndex + selectedSugg.Length;

            SuggestionPopup.IsOpen = false;
            CodeEditor.Focus();
        }

        private void NavigateSuggestions(KeyEventArgs e)
        {
            if (!SuggestionPopup.IsOpen)
                return;

            switch (e.Key)
            {
                case Key.Down:
                    SuggestionListBox.SelectedIndex = (SuggestionListBox.SelectedIndex + 1) % SuggestionListBox.Items.Count;
                    SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                    e.Handled = true;
                    break;

                case Key.Up:
                    int newIndex = SuggestionListBox.SelectedIndex - 1;
                    if (newIndex < 0) newIndex = SuggestionListBox.Items.Count - 1;
                    SuggestionListBox.SelectedIndex = newIndex;
                    SuggestionListBox.ScrollIntoView(SuggestionListBox.SelectedItem);
                    e.Handled = true;
                    break;

                case Key.Enter:
                case Key.Tab:
                    ApplySuggestion();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    SuggestionPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }
    }
}
