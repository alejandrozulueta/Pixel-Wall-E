using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wall_E.Extensions
{
    public static class StringExtension
    {
        public static Color ToColor(this string value) 
        { 
            return value switch 
            { 
                "Red" => Color.Red,
                "Blue" => Color.Blue,
                "Green" => Color.Green,
                "Yellow" => Color.Yellow,
                "Orange" => Color.Orange,
                "Purple" => Color.Purple,
                "Black" => Color.Black,
                "White" => Color.White,
                "Transparent" => Color.Transparent,
                _ => throw new NotImplementedException($"Color {value} no encontrado")
            };
        }
    }
}
