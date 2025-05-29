using Expressions.Models;
using Expressions.Visitors;
using Parser.Models;
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
using Visual.Models;
using Wall_E.Controllers;

namespace Visual
{
    public partial class Form1 : Form
    {
        public Canvas Canvas { get; protected set; }
        FuncControler func; 
        ActionControler act;
        MainController main;

        public Form1()
        {
            InitializeComponent();
            Canvas = new Canvas();
            func = new(Canvas);
            act = new(Canvas);
            main = new(func, act);
            
            canvasPictureBox.Invalidate();
        }

        private void SplitContainer1_SplitterMoved(object sender, SplitterEventArgs e) { }

        private void RichTextBox1_TextChanged(object sender, EventArgs e) { }

        private void CanvasPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            float calculatedCellWidth = (float)canvasPictureBox.Width / Canvas.canvasCols;
            float calculatedCellHeight = (float)canvasPictureBox.Height / Canvas.canvasRows;

            float totalGridVisualWidth = Canvas.canvasCols * calculatedCellWidth;
            float totalGridVisualHeight = Canvas.canvasRows * calculatedCellHeight;
            float offsetX = (canvasPictureBox.Width - totalGridVisualWidth) / 2;
            float offsetY = (canvasPictureBox.Height - totalGridVisualHeight) / 2;

            for (int r = 0; r < Canvas.canvasRows; r++)
            {
                for (int c = 0; c < Canvas.canvasCols; c++)
                {
                    Cell celdaActual = Canvas.canvasCells[r, c];

                    using (SolidBrush cellBrush = new SolidBrush(celdaActual.Color))
                    {
                        float xPos = offsetX + (c * calculatedCellWidth);
                        float yPos = offsetY + (r * calculatedCellHeight);

                        g.FillRectangle(
                            cellBrush,
                            xPos,
                            yPos,
                            calculatedCellWidth,
                            calculatedCellHeight
                        );
                    }
                }

                FillBorder(g, calculatedCellWidth, calculatedCellHeight, offsetX, offsetY);
            }
        }

        private void FillBorder(
            Graphics g,
            float calculatedCellWidth,
            float calculatedCellHeight,
            float offsetX,
            float offsetY
        )
        {
            float totalGridVisualWidth = Canvas.canvasCols * calculatedCellWidth;
            float totalGridVisualHeight = Canvas.canvasRows * calculatedCellHeight;
            using (Pen gridPen = new Pen(Color.LightGray)) // Color de la cuadrícula
            {
                // Líneas Verticales
                for (int c = 0; c <= Canvas.canvasCols; c++)
                {
                    float xPos = offsetX + (c * calculatedCellWidth);
                    g.DrawLine(gridPen, xPos, offsetY, xPos, offsetY + totalGridVisualHeight);
                }
                // Líneas Horizontales
                for (int r = 0; r <= Canvas.canvasRows; r++)
                {
                    float yPos = offsetY + (r * calculatedCellHeight);
                    g.DrawLine(gridPen, offsetX, yPos, offsetX + totalGridVisualWidth, yPos);
                }
            }
        }

        private void CodeEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string code = codeEditor.Text; // Mandar para el pasrser

                main.ExecuteCode(code);

                canvasPictureBox.Invalidate();
            }
        }
    }
}
