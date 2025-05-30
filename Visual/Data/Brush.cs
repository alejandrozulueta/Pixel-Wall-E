using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual.Data
{
    public class BrushData(int x, int y)
    {
        public Color CurrentColor { get; set; } = Color.Transparent;
        public int CurrentX { get; set; } = x;
        public int CurrentY { get; set;} = y;
        public int Size { get; set; } = 1;
    }
}
