using Core.Models;

namespace Parser.Models;

public class Tokens(TokenType type, string identifier, int row, int col)
{
    public TokenType Type { get; set; } = type;
    public string Identifier => identifier;
    public Location Location { get; private set; } = new(row, col, identifier.Length);
}
