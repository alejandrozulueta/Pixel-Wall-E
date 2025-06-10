using System.Drawing;
using System.Reflection;
using Visual.Attributes;
using Visual.Data;
using Visual.Extensions;
using Visual.Interfaces;

namespace Visual.Controllers
{
    public class ActionControler(IPaint paint)
    {
        IPaint _paint = paint;
        private static Dictionary<string, ActionInfo>? dict;

        [AttributeDefined("VisualActs")]
        public void Spawn(int x, int y)
        {
            IsValid(x, y, _paint.Canvas.Cols, _paint.Canvas.Rows);

            if (_paint.Brush is not null)
                throw new InvalidOperationException("Wall_E ya se ha iniciado");
            _paint.Brush = new BrushData(x, y);
            return;
        }

        [AttributeDefined("VisualActs")]
        public void Color(string color)
        {
            _paint.Brush!.CurrentColor = color.ToColor();   
        }

        [AttributeDefined("VisualActs")]
        public void Size(int size)
        {
            size -= (size + 1) % 2;
            _paint.Brush!.Size = size;
        }

        [AttributeDefined("VisualActs")]
        public void DrawLine(int dirX, int dirY, int distance)
        {
            ValidDir(dirX, dirY);

            int startLineX = _paint.Brush!.CurrentX;
            int startLineY = _paint.Brush!.CurrentY;

            int cx = startLineX + dirX * distance;
            int cy = startLineY + dirY * distance;

            IsValid(cx, cy, _paint.Canvas.Cols, _paint.Canvas.Rows);
            
            for (int i = 0; i <= distance; i++)
            {
                int currentCenterX = startLineX + dirX * i;
                int currentCenterY = startLineY + dirY * i;
                PaintPixel(_paint.Canvas!, currentCenterX, currentCenterY, _paint.Brush.CurrentColor, _paint.Brush.Size);
            }
            _paint.Brush.CurrentX = cx; 
            _paint.Brush.CurrentY = cy;
        }

        [AttributeDefined("VisualActs")]
        public void DrawCircle(int dirX, int dirY, int radius)
        {
            ValidDir(dirX, dirY);

            radius += (radius + 1) % 2;

            int circleCenterX = _paint.Brush!.CurrentX + dirX * radius;
            int circleCenterY = _paint.Brush!.CurrentY + dirY * radius;

            IsValid(circleCenterX, circleCenterY, _paint.Canvas.Cols, _paint.Canvas.Rows);
            
            _paint.Brush!.CurrentX = circleCenterX;
            _paint.Brush!.CurrentY = circleCenterY;

            int x = radius;
            int y = 0;
            int p = 1 - radius;

            CircleOctants(_paint.Canvas!, circleCenterX, circleCenterY, x, y);

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

                CircleOctants(_paint.Canvas!, circleCenterX, circleCenterY, x, y);
            }
        }

        [AttributeDefined("VisualActs")]
        public void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
        {
            ValidDir(dirX, dirY);

            int rectCenterX = _paint.Brush!.CurrentX + dirX * distance;
            int rectCenterY = _paint.Brush!.CurrentY + dirY * distance;

            IsValid(rectCenterX, rectCenterY, _paint.Canvas.Cols, _paint.Canvas.Rows);

            _paint.Brush!.CurrentX = rectCenterX;
            _paint.Brush!.CurrentY = rectCenterY;

            width += (width + 1) % 2; 
            height += (height + 1) % 2; 

            int halfWidth = width / 2;
            int halfHeight = height / 2;

            int interiorLeft = rectCenterX - (width - 1) / 2;
            int interiorTop = rectCenterY - (height - 1) / 2;

            int left = interiorLeft - 1;
            int right = interiorLeft + width;
            int top = interiorTop - 1;
            int bottom = interiorTop + height;

            Color color = _paint.Brush!.CurrentColor; ;
            int size = _paint.Brush.Size;

            for (int x = left; x <= right; x++)
            {
                PaintPixel(_paint.Canvas, x, top, color, size);
            }

            for (int x = left; x <= right; x++)
            {
                PaintPixel(_paint.Canvas, x, bottom, color, size);
            }

            for (int y = top + 1; y < bottom; y++) 
            {
                PaintPixel(_paint.Canvas, left, y, color, size);
            }

            for (int y = top + 1; y < bottom; y++) 
            {
                PaintPixel(_paint.Canvas, right, y, color, size);
            }
        }

        [AttributeDefined("VisualActs")]
        public void Fill()
        {
            Color targetColor = _paint.Canvas.CellsColor[_paint.Brush!.CurrentY, _paint.Brush!.CurrentX];
            Color fillColor = _paint.Brush.CurrentColor;
            int size = _paint.Brush.Size;

            RecursiveFill(_paint.Canvas, _paint.Brush!.CurrentY, _paint.Brush!.CurrentX, targetColor, fillColor, size);
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
            Color color = _paint.Brush!.CurrentColor; ;
            int size = _paint.Brush.Size;

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

        private void IsValid(int x, int y, uint canvasCols, uint canvasRows) 
        {
            if(!(x >= 0 && x < canvasCols && y >= 0 && y < canvasRows)) 
            { 
                throw new InvalidOperationException("Wall_E se sale del canvas");
            }
        }

        private void ValidDir(int x, int y)
        {
            if ((x != Math.Abs(1) && x != 0) || (y != Math.Abs(1) && y != 0)) 
            { 
                throw new InvalidOperationException("Dirección inválida, debe ser -1, 0 o 1 en cada eje");
            }
        }
    }
}