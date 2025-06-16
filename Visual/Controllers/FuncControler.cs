using System.Reflection;
using System.Windows.Controls;
using Visual.Attributes;
using Visual.Data;
using Visual.Extensions;
using Visual.Interfaces;

namespace Visual.Controllers
{
    public class FuncControler(IPaint paint)
    {
        IPaint _paint = paint;
        private static Dictionary<string, FuncInfo>? dict;

        [AttributeDefined("VisualFuncs")]
        public int GetActualX()
            => _paint.Brush!.CurrentX;

        [AttributeDefined("VisualFuncs")]
        public int GetActualY()
            => _paint.Brush!.CurrentY;

        [AttributeDefined("VisualFuncs")]
        public int GetCanvasSize()
            => _paint.Canvas.Dimension;
        
        [AttributeDefined("VisualFuncs")]
        public int GetColorCount(string color, int x1, int y1, int x2, int y2)
        {
            int count = 0;
            var targetColor = color.ToColor();
            for (int y = y2; y >= y1; y--)
            {
                for (int x = x1; x <= x2; x++)
                {
                    if (_paint.Canvas.CellsColor[y, x] == targetColor)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        [AttributeDefined("VisualFuncs")]
        public bool IsBrushColor(string color)
            => color.ToColor() == _paint.Brush!.CurrentColor;
        
        [AttributeDefined("VisualFuncs")]
        public bool IsSize(int size)
            => _paint.Brush!.Size == size;
        
        [AttributeDefined("VisualFuncs")]
        public bool IsCanvasColor(string color, int vertical, int horizontal)
            => _paint.Canvas.CellsColor[horizontal, vertical] == color.ToColor();
        
        public Dictionary<string, FuncInfo> GetFuncs()
        {
            dict = dict ?? typeof(FuncControler).GetMethods()
                .Where(x => x.GetCustomAttribute<AttributeDefined>()?.Name == "VisualFuncs")
                .Select(x =>
                {
                    Func<Values[], Values> @delegate = (args) =>
                {
                    ParameterInfo[] originalParams = x.GetParameters();
                    object[] invokeArgs = new object[originalParams.Length];

                    for (int i = 0; i < invokeArgs.Length; i++)
                    {
                        invokeArgs[i] = Convert.ChangeType(args[i].Value, originalParams[i].ParameterType);
                    }

                    object? result = x.Invoke(this, invokeArgs);
                    return new Values(x.ReturnType.ToValueType(), result);

                };
                    return new { Key = x.Name, Value = new FuncInfo(@delegate, x.GetParameters(), x.ReturnType) };

                })
                .ToDictionary(item => item.Key, item => item.Value);

            return dict;
        }
    }
}