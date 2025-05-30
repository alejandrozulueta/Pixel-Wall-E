using System.Drawing;

namespace Visual.Data
{
    public class CanvasData
    {
        public int Rows { get; set; } = 20;
        public int Cols { get; set; } = 20;
        public Color[,] CellsColor { get; set; }

        public CanvasData() 
        { 
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
