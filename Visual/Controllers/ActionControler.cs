using System.Reflection;
using Visual.Attributes;
using Visual.Extensions;
using Visual.Data;

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
            for (int i = 0; i <= distance; i++)
            {
                int newX = brush!.CurrentX + dirX * i;
                int newY = brush!.CurrentX + dirX * i;

                canvas.CellsColor[newX, newY] = brush!.CurrentColor;
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