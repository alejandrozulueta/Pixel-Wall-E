using System.Reflection;
using Visual.Attributes;
using Visual.Extensions;
using Visual.Data;
using System.Drawing;

namespace Visual.Controllers
{
    public class ActionControler(CanvasData canvas, BrushData? brush)
    {
        private CanvasData canvas = canvas;
        private BrushData? brush = brush;
        private static Dictionary<string, ActionInfo>? dict;

        [AttributeDefined("VisualActs")]
        public void Spawn(int x, int y)
        {
            if (brush is not null)
                throw new InvalidOperationException("Wall_E ya se ha iniciado");
            brush = new BrushData(x, y);
            return;
        }

        [AttributeDefined("VisualActs")]
        public void Color(string color)
        {
            brush!.CurrentColor = color.ToColor();   
        }

        [AttributeDefined("VisualActs")]
        public void Size(int size)
        {
            if (size % 2 == 0)
                size--;
            brush!.Size = size;
        }

        [AttributeDefined("VisualActs")]
        public void DrawLine(int dirX, int dirY, int distance)
        {
            int startLineX = brush!.CurrentX;
            int startLineY = brush!.CurrentY;

            for (int i = 0; i <= distance; i++)
            {
                int currentCenterX = startLineX + dirX * i;
                int currentCenterY = startLineY + dirY * i;
                PaintPixel(canvas, currentCenterX, currentCenterY, brush.CurrentColor, brush.Size);
            }

            brush.CurrentX = startLineX + dirX * distance;
            brush.CurrentY = startLineY + dirY * distance;
        }

        [AttributeDefined("VisualActs")]
        public void DrawCircle(int dirX, int dirY, int radius)
        {
            int circleCenterX = brush!.CurrentX + dirX * radius;
            int circleCenterY = brush!.CurrentY + dirY * radius;

            brush!.CurrentX = circleCenterX;
            brush!.CurrentY = circleCenterY;

            int x = radius;
            int y = 0;
            int p = 1 - radius;

            CircleOctants(canvas, circleCenterX, circleCenterY, x, y);

            while (x > y)
            {
                y++;

                if (p <= 0)
                {
                    p = p + 2 * y + 1;
                }
                else 
                {
                    x--;
                    p = p + 2 * y - 2 * x + 1;
                }

                if (x < y)
                    break;

                CircleOctants(canvas, circleCenterX, circleCenterY, x, y);
            }
        }

        [AttributeDefined("VisualActs")]
        public void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
        {
            int rectCenterX = brush!.CurrentX + dirX * distance;
            int rectCenterY = brush!.CurrentY + dirY * distance;

            brush!.CurrentX = rectCenterX;
            brush!.CurrentY = rectCenterY;

            width = width % 2 != 0 ? width : width + 1;
            height = height % 2 != 0 ? height : height + 1;

            int halfWidth = width / 2;
            int halfHeight = height / 2;

            int interiorLeft = rectCenterX - (width - 1) / 2;
            int interiorTop = rectCenterY - (height - 1) / 2;

            int left = interiorLeft - 1;
            int right = interiorLeft + width;
            int top = interiorTop - 1;
            int bottom = interiorTop + height;

            Color color = brush!.CurrentColor; ;
            int size = brush.Size;

            for (int x = left; x <= right; x++)
            {
                PaintPixel(canvas, x, top, color, size);
            }

            for (int x = left; x <= right; x++)
            {
                PaintPixel(canvas, x, bottom, color, size);
            }

            for (int y = top + 1; y < bottom; y++) 
            {
                PaintPixel(canvas, left, y, color, size);
            }

            for (int y = top + 1; y < bottom; y++) 
            {
                PaintPixel(canvas, right, y, color, size);
            }
        }

        [AttributeDefined("VisualActs")]
        public void Fill()
        {
            Color targetColor = canvas.CellsColor[brush!.CurrentY, brush!.CurrentX];
            Color fillColor = brush.CurrentColor;
            int size = brush.Size;

            RecursiveFill(canvas, brush!.CurrentY, brush!.CurrentX, targetColor, fillColor, size);
        }


        private void PaintPixel(CanvasData canvas, int centerX, int centerY, Color color, int brushSize)
        {
            int brushOffset = (brushSize - 1) / 2;
            if (brushOffset < 0) brushOffset = 0;

            for (int offsetY = -brushOffset; offsetY <= brushOffset; offsetY++)
            {
                for (int offsetX = -brushOffset; offsetX <= brushOffset; offsetX++)
                {
                    int paintX = centerX + offsetX;
                    int paintY = centerY + offsetY;

                    if (paintY >= 0 && paintY < canvas.Rows &&
                        paintX >= 0 && paintX < canvas.Cols)
                    {
                        canvas.CellsColor[paintY, paintX] = color;   
                    }
                }
            }
        }

        private void CircleOctants(CanvasData canvas, int centerX, int centerY, int x, int y)
        {
            Color color = brush!.CurrentColor; ;
            int size = brush.Size;

            PaintPixel(canvas, centerX + x, centerY + y, color, size);
            PaintPixel(canvas, centerX - x, centerY + y, color, size);
            PaintPixel(canvas, centerX + x, centerY - y, color, size);
            PaintPixel(canvas, centerX - x, centerY - y, color, size);

            if (x != y)
            {
                PaintPixel(canvas, centerX + y, centerY + x, color, size);
                PaintPixel(canvas, centerX - y, centerY + x, color, size);
                PaintPixel(canvas, centerX + y, centerY - x, color, size);
                PaintPixel(canvas, centerX - y, centerY - x, color, size);
            }
        }
        private void RecursiveFill(CanvasData canvas, int y, int x, Color targetColor, Color fillColor, int size)
        {
            if (x < 0 || x >= canvas.Cols || y < 0 || y >= canvas.Rows)
                return;
            
            if (canvas.CellsColor[y, x] != targetColor)
               return;
            
            PaintPixel(canvas, x, y, fillColor, 1);

            RecursiveFill(canvas, x + 1, y, targetColor, fillColor, size);
            RecursiveFill(canvas, x - 1, y, targetColor, fillColor, size);
            RecursiveFill(canvas, x, y + 1, targetColor, fillColor, size);
            RecursiveFill(canvas, x, y - 1, targetColor, fillColor, size);
        }

        public Dictionary<string, ActionInfo> GetActs()
        {
            dict = dict ?? typeof(ActionControler).GetMethods()
                .Where(x => x.GetCustomAttribute<AttributeDefined>()?.Name == "VisualActs")
                .Select(x =>
                {
                    Action<Values[]> @delegate = (args) =>
                    {
                        ParameterInfo[] originalParams = x.GetParameters();
                        object[] invokeArgs = new object[originalParams.Length];

                        for (int i = 0; i < invokeArgs.Length; i++)
                        {
                            invokeArgs[i] = Convert.ChangeType(args[i].Value, originalParams[i].ParameterType);
                        }

                        x.Invoke(this, invokeArgs);
                    };
                    return new { Key = x.Name, Value = new ActionInfo(@delegate, x.GetParameters()) };
                })
                .ToDictionary(item => item.Key, item => item.Value);

            return dict;
        }

    }
}