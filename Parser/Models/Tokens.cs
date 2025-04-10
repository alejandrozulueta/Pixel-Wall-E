namespace Parser.Models;

public enum TokenType
{
    Identifier,
    Label,
    Value,
    BinaryOperator,
    UnaryOperator,
    AssingOperator,
    OpenParenthesis,
    CloseParenthesis,
}

public class Tokens(TokenType type, string identifier, int row, int col)
{
    public TokenType Type => type;
    public string Identifier => identifier;
    public int Row => row;
    public int Col => col;
}
