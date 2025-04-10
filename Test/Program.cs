using Expressions.Models;
using Expressions.Visitors;
using Parser.Models;

class Program
{
    private static void Main(string[] args)
    {
        _ = Lexer.Tokenizer("(()");
    }
}
