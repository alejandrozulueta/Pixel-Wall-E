using Expressions.Models;
using Parser.Models;

namespace Parser.Extensions;

public static class StringExtensions
{
    public static BinaryTypes GetBinaryType(this string identifier)
    {
        return identifier switch
        {
            "+" => BinaryTypes.Sum,
            "-" => BinaryTypes.Sub,
            "*" => BinaryTypes.Mult,
            "/" => BinaryTypes.Div,
            "^" => BinaryTypes.Pow,
            "%" => BinaryTypes.Modul,
            "==" => BinaryTypes.Equal,
            "!=" => BinaryTypes.Inequal,
            "<" => BinaryTypes.Less,
            ">" => BinaryTypes.Greater,
            ">=" => BinaryTypes.GreaterEqual,
            "<=" => BinaryTypes.LessEqual,
            _ => throw new NotImplementedException(),
        };
    }

    public static UnaryTypes GetUnaryType(this string identifier)
    {
        return identifier switch
        {
            "!" => UnaryTypes.Not,
            "-" => UnaryTypes.Neg,
            _ => throw new NotImplementedException(),
        };
    }
}
