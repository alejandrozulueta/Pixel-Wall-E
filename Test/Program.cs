using Expressions.Visitors;
using Parser.Models;

class Program
{
    private static void Main(string[] args)
    {
        var parser = new Parser.Models.Parser();
        var visit = new Execute();

        var tokens = Lexer.Tokenizer("a = 7 == 7");
        var node = parser.Parse(tokens);
        node.Accept(visit);

        tokens = Lexer.Tokenizer("b=(((a-1-2+2)/2)^2)/2");
        node = parser.Parse(tokens);
        node.Accept(visit);
    }
}
