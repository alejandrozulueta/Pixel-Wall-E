using Core.Exceptions;
using Expressions.Enum;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Visual.Data;
using Visual.Enums;
using Visual.Interfaces;
using System.IO;

namespace Visual
{
    public partial class MainWindow : Window, IPaint, IPrinteable
    {
        private void Excecute()
        {
            SB.Clear();
            Canvas = new CanvasData(Canvas.Dimension);
            Brush = new BrushData(0, 0); 
            try
            {
                main.ExecuteCode(codeInfo!);
            }
            catch (ExceptionWL ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Execution Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                var error = ex;

                UnderLineErrors(ErrorTypes.Execution, error.Location.Row, error.Location.InitCol, error.Location.EndCol);

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

            foreach (var error in exceptions)
            {
                UnderLineErrors(ErrorTypes.Compilation, error.Location.Row, error.Location.InitCol, error.Location.EndCol);
            }

            Errors.Text = errors.ToString();
            ErrorsLine.Text = lines.ToString();
        }

        private void UnderLineErrors(ErrorTypes error, int line, int startChar, int endChar)
        {

            var paragraph = CodeEditor.Document.Blocks.ElementAt(line - 1) as Paragraph;
            if (paragraph == null) return;

            TextPointer startPointer = paragraph.ContentStart.GetPositionAtOffset(startChar);
            TextPointer endPointer = paragraph.ContentStart.GetPositionAtOffset(endChar);

            if (startPointer == null || endPointer == null) return;

            TextRange errorRange = new TextRange(startPointer, endPointer);

            switch (error)
            {
                case ErrorTypes.Compilation:
                    var redUnderline = (TextDecorationCollection)this.FindResource("RedUnderline");
                    errorRange.ApplyPropertyValue(Inline.TextDecorationsProperty, redUnderline);
                    break;
                case ErrorTypes.Execution:
                    errorRange.ApplyPropertyValue(Inline.BackgroundProperty, Brushes.Yellow);
                    break;
            }

        }

        private void HideUnderLineErrors()
        {
            TextRange documentRange = new TextRange(
                CodeEditor.Document.ContentStart,
                CodeEditor.Document.ContentEnd
            );
            documentRange.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            documentRange.ApplyPropertyValue(Inline.BackgroundProperty, null);
        }

        private void UpdateLineNumbers()
        {
            int lineCount = CodeEditor.Document.Blocks.Count;

            StringBuilder lineNumbersText = new();
            for (int i = 1; i <= lineCount; i++)
            {
                lineNumbersText.AppendLine(i.ToString());
            }

            Lines.Text = lineNumbersText.ToString();
        }

        private void DrawGrid()
        {

            string exc = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo directory = new DirectoryInfo(exc);

            directory = directory.Parent!.Parent!.Parent!.Parent!; ;
            string asset = System.IO.Path.Combine(directory.FullName, "Assets\\Wall_E.png");
            
            DrawCanvas.Children.Clear();

            double widthCells = DrawCanvas.ActualWidth / Canvas.Dimension;
            double heightCells = DrawCanvas.ActualHeight / Canvas.Dimension;

            for (int i = 0; i < Canvas.Dimension; i++)
            {
                for (int j = 0; j < Canvas.Dimension; j++)
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

                    if (Brush == null)
                        continue;

                    if (i == Brush!.CurrentX && j == Brush.CurrentY)
                    {
                        Image figureImage = new Image
                        {
                            Width = widthCells,   
                            Height = heightCells,
                            Source = new BitmapImage(new Uri(asset))
                        };

                        double scaleFactor = 1.7;

                        figureImage.RenderTransformOrigin = new Point(0.5, 0.5);
                        figureImage.RenderTransform = new ScaleTransform(scaleFactor, scaleFactor);

                        System.Windows.Controls.Canvas.SetLeft(figureImage, j * widthCells);
                        System.Windows.Controls.Canvas.SetTop(figureImage, i * heightCells);

                        DrawCanvas.Children.Add(figureImage);
                    }
                }
            }
        }

        private void Resize(int Dimension = 20)
        {
            Canvas = new CanvasData(Dimension);
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
            string currentText = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text;

            TextPointer caretPosition = CodeEditor.CaretPosition;
            int caretIndex = new TextRange(CodeEditor.Document.ContentStart, caretPosition).Text.Length;

            List<string> Sugg = [];
            int horizontalMargin = 5;

            if (SuggestionPopup.IsOpen == true)
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(currentText) || caretPosition.CompareTo(CodeEditor.Document.ContentStart) == 0)
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            string wordToSuggest = GetWord(currentText, caretIndex);

            Sugg = GetSuggest(codeInfo, wordToSuggest);

            if (Sugg.Count == 0 || string.IsNullOrWhiteSpace(wordToSuggest))
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            SuggestionListBox.ItemsSource = Sugg;
            SuggestionListBox.SelectedIndex = 0;

            Rect caretVisualRect = caretPosition.GetCharacterRect(LogicalDirection.Forward);

            if (caretVisualRect.IsEmpty || caretVisualRect.Top == 0.0)
            {
                TextPointer prevCharPosition = caretPosition.GetPositionAtOffset(-1, LogicalDirection.Backward);
                if (prevCharPosition != null)
                {
                    caretVisualRect = prevCharPosition.GetCharacterRect(LogicalDirection.Forward);
                }
            }

            Point placementPoint = new Point(caretVisualRect.Left + horizontalMargin, caretVisualRect.Bottom);

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

            if (startIndex < 0)
                startIndex = 0;
            
            return text[startIndex..caretIndex];
        }

        private void ApplySuggestion()
        {
            if (SuggestionListBox.SelectedItem is not string selectedSugg)
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            TextPointer caretPosition = CodeEditor.CaretPosition;
            string currentTextForAnalysis = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text;
            int caretIndexForAnalysis = new TextRange(CodeEditor.Document.ContentStart, caretPosition).Text.Length;

            string wordToReplace = GetWord(currentTextForAnalysis, caretIndexForAnalysis);

            if (string.IsNullOrEmpty(wordToReplace))
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            TextPointer endOfWord = caretPosition;
            TextPointer startOfWord = caretPosition.GetPositionAtOffset(-wordToReplace.Length);

            if (startOfWord == null) return;

            TextRange wordRange = new(startOfWord, endOfWord)
            {
                Text = selectedSugg
            };

            CodeEditor.CaretPosition = wordRange.End;

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
