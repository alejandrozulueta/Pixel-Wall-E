namespace Parser.Models;

public record struct Location
{
    public int Row { get; private set; }
    public int InitCol { get; private set; }
    public int EndCol { get; private set; }

    public Location(int row, int col, int length)
    {
        Row = row;
        InitCol = col;
        EndCol = col + length;
    }

    public static Location operator +(Location left, Location right) =>
        new(left.Row, left.InitCol, right.EndCol);
}