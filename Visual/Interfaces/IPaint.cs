using Visual.Data;

namespace Visual.Interfaces
{
    public interface IPaint
    {
        public CanvasData Canvas { get; set; }
        public BrushData? Brush { get; set; }
    }
}