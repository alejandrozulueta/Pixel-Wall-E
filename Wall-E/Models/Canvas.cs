namespace Visual.Models
{
    public class Canvas
    {
        public int canvasRows = 20;
        public int canvasCols = 20;
        public Cell[,] canvasCells;

        public Canvas() 
        { 
            canvasCells = new Cell[canvasRows, canvasCols];

            for (int i = 0; i < canvasRows; i++)
            {
                for (int j = 0; j < canvasCols; j++)
                {
                    canvasCells[i, j] = new Cell();
                }
            }
        }
    }
}
