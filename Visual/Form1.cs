using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual
{
    public partial class Form1 : Form
    {
        public Canvavs canvas;
        public Form1()
        {
            InitializeComponent();
            canvas = new Canvavs();
            canvasPictureBox.Invalidate();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void canvasPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            float calculatedCellWidth = (float)canvasPictureBox.Width / canvas.canvasCols;
            float calculatedCellHeight = (float)canvasPictureBox.Height / canvas.canvasRows;

            float totalGridVisualWidth = canvas.canvasCols * canvas.cellSize;
            float totalGridVisualHeight = canvas.canvasRows * canvas.cellSize;
            float offsetX = (canvasPictureBox.Width - totalGridVisualWidth) / 2;
            float offsetY = (canvasPictureBox.Height - totalGridVisualHeight) / 2;

            for (int r = 0; r < canvas.canvasRows; r++)
            {
                for (int c = 0; c < canvas.cellSize; c++) 
                {
                    Cell celdaActual = canvas.canvasCells[r, c];

                    using (SolidBrush cellBrush = new SolidBrush(celdaActual.Color))
                    {
                        float xPos = offsetX + (c * canvas.cellSize);
                        float yPos = offsetY + (r * canvas.cellSize);

                        g.FillRectangle(cellBrush, xPos, yPos, canvas.cellSize, canvas.cellSize);
                    }
                }

                if (canvas.cellSize > 2) 
                {
                    using (Pen gridPen = new Pen(Color.LightGray)) // Color de la cuadrícula
                    {
                        // Líneas Verticales
                        for (int c = 0; c <= canvas.canvasCols; c++)
                        {
                            float xPos = offsetX + (c * canvas.cellSize);
                            g.DrawLine(gridPen, xPos, offsetY, xPos, offsetY + totalGridVisualHeight);
                        }
                        // Líneas Horizontales
                        for (int r = 0; r <= canvas.canvasRows; r++)
                        {
                            float yPos = offsetY + (r * canvas.cellSize);
                            g.DrawLine(gridPen, offsetX, yPos, offsetX + totalGridVisualWidth, yPos);
                        }
                    }
                }

            }
        }

        private void codeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string codigoAEjecutar = codeEditor.Text; // Mandar para el pasrser

                // Code Parser

                canvasPictureBox.Invalidate();
            }
        }
    }

    public class Cell
    {
        public Color Color { get; set; } = Color.White; 
    }

    public class Canvavs
    {
        public int canvasRows = 20;
        public int canvasCols = 20;
        public int cellSize = 20
        public Cell[,] canvasCells;

        public Canvavs() 
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
