using System.Data;
using System.Text.RegularExpressions;

namespace Parser.Models;

public static class Lexer
{
    public static Tokens[] Tokenizer(string input)
    {
        Regex regex = new(
            @"((.+)?![a-zA-Z_][a-zA-Z0-9_]*:\s*)"
                + @"|([a-zA-Z_][a-zA-Z0-9_]*)"
                + @"|\d+"
                + @"|(==|>=|<=)"
                + @"|("".*"")"
                + @"|([\[\]\(\)\+\-\*/%^=><])|\s+"
        );

        var split = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        List<Tokens> tokens = [];

        for (int i = 0; i < split.Length; i++)
        {
            int count = 0;

            foreach (var match in regex.Matches(split[i]).Cast<Match>())
            {
                if (split[i][count] != match.Value[0])
                    throw new InvalidExpressionException();
                count += match.Length;
                if (string.IsNullOrEmpty(match.Value.Trim()))
                    continue;
                var tokenType = GetTokenType(match.Value);

                tokens.Add(new Tokens(tokenType, match.Value, i, count));
            }
        }

        tokens.Add(new Tokens(TokenType.EOS, "$", split.Length, 0));
        return [.. tokens];
    }

    private static TokenType GetTokenType(string lex)
    {
        var tokenType = lex switch
        {
            "+" or "-" or "*" or "/" or "%" => TokenType.BinaryOperator,
            "==" or ">=" or "<=" or "!=" => TokenType.BinaryOperator,
            ">" or "<" => TokenType.BinaryOperator,
            "=" => TokenType.AssingOperator,
            "!" or "-" => TokenType.UnaryOperator,
            "(" => TokenType.OpenParenthesis,
            ")" => TokenType.CloseParenthesis,
            "^" => TokenType.BinaryOperator,
            _ => TokenType.Identifier,
        };

        if (tokenType != TokenType.Identifier)
            return tokenType;
        if (int.TryParse(lex, out int _))
            return TokenType.Value;
        if (bool.TryParse(lex, out bool _))
            return TokenType.Value;
        if (lex[0] == '"')
            return TokenType.Value;
        if (lex[^1] == ':')
            return TokenType.Label;
        return tokenType;
    }
}
