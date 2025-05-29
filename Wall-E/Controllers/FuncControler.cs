using Expressions.Extensions;
using Expressions.Models;
using System.Reflection;
using Visual.Models;
using Wall_E.Attributes;

namespace Wall_E.Controllers
{
    public class FuncControler
    {
        private Canvas canvas;

        public FuncControler(Canvas canvas) 
        { 
            this.canvas = canvas;    
        }

        [AttributeDefined("VisualFuncs")]
        public int GetActualX()
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualFuncs")]
        public int GetActualY()
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualFuncs")]
        public int GetCanvasSize()
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualFuncs")]
        public int GetColorCount(string color, int x1, int y1, int x2, int y2)
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualFuncs")]
        public bool IsBrushColor(string color)
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualFuncs")]
        public bool IsCanvasColor(string color, int vertical, int horizontal)
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualFuncs")]
        public bool IsColor(string color, int x, int y)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, Func<Values[], Values>> GetFuncs()
        {
            return typeof(FuncControler).GetMethods()
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
                    return new { Key = x.Name, Value = @delegate };

                })
                .ToDictionary(item => item.Key, item => item.Value);
        }
    }
}