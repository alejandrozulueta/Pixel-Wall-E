using System.Drawing;

namespace Visual.Data
{
    public class CanvasData
    {
        public int Rows { get; set; } 
        public int Cols { get; set; }
        public Color[,] CellsColor { get; set; }

        public CanvasData(int rows = 0, int cols = 0) 
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
