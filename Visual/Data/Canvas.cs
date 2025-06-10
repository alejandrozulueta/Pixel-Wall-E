using System.Drawing;

namespace Visual.Data
{
    public class CanvasData
    {
        public uint Rows { get; set; } 
        public uint Cols { get; set; }
        public Color[,] CellsColor { get; set; }

        public CanvasData(uint rows = 0, uint cols = 0) 
        {
            Rows = rows;
            Cols = cols;
        
            CellsColor = new Color[Rows, Cols];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    CellsColor[i, j] = Color.Transparent;
                }
            }
        }
    }
}
