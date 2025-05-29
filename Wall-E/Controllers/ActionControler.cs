using Expressions.Extensions;
using Expressions.Models;
using System.Collections.Generic;
using System.Reflection;
using Visual.Models;
using Wall_E.Attributes;
using Wall_E.Extensions;
using Wall_E.Models;
using Brush = Wall_E.Models.Brush;

namespace Wall_E.Controllers
{
    public class ActionControler
    {
        private Canvas canvas;
        private Brush? brush;

        public ActionControler(Canvas canvas)
        {
            this.canvas = canvas;
        }

        [AttributeDefined("VisualActs")]
        public void Spawn(int x, int y)
        {
            if(brush is null)
                brush = new Wall_E.Models.Brush(x, y);
            throw new InvalidOperationException("Wall_E ya se ha iniciado");
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
            for (int i = 0; i <= distance; i++)
            {
                int newX = brush!.CurrentX + dirX * i;
                int newY = brush!.CurrentX + dirX * i;

                canvas.canvasCells[newX, newY].Color = brush!.CurrentColor;
            }
        }

        [AttributeDefined("VisualActs")]
        public void DrawCircle(int dirX, int dirY, int radius)
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualActs")]
        public void Rectangle(int dirX, int dirY, int distance, int width, int height)
        {
            throw new NotImplementedException();
        }

        [AttributeDefined("VisualActs")]
        public void Fill()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, Action<Values[]>> GetActs()
        {
            return typeof(FuncControler).GetMethods()
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
                    return new { Key = x.Name, Value = @delegate };
                })
                .ToDictionary(item => item.Key, item => item.Value);


        }

    }
}