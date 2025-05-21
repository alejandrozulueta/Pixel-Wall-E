namespace Parser.Models;

public enum TokenType
{
    Identifier,
    Label,
    Num,
    Bool,
    String,
    BinaryOperator,
    UnaryOperator,
    AssingOperator,
    OpenParenthesis,
    CloseParenthesis,
    EOS,
    Goto,
    OpenBracket,
    CloseBracket,
    EndOfLine,
}

public class Tokens(TokenType type, string identifier, int row, int col)
{
    public TokenType Type => type;
    public string Identifier => identifier;
    public int Row => row;
    public int Col => col;
}
