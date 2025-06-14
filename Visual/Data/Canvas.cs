using System.Drawing;

namespace Visual.Data
{
    public class CanvasData
    {
        public int Dimension { get; set; }
        public Color[,] CellsColor { get; set; }

        public CanvasData(int dimension = 0) 
        {
            Dimension = dimension;
        
            CellsColor = new Color[Dimension, Dimension];

            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    CellsColor[i, j] = Color.Transparent;
                }
            }
        }
    }
}
