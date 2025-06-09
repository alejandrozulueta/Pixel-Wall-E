using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Parser.Models;

public static class Lexer
{
    public static Tokens[] Tokenizer(string input, out List<Exception> exceptions)
    {
        Regex regex = new(
            @"[a-zA-Z_][a-zA-Z0-9_]*[\t ]*\r\n"
                + @"|[a-zA-Z_][a-zA-Z0-9_]*[\t ]*\n"
                + @"|([a-zA-Z_][a-zA-Z0-9_]*)"
                + @"|\d+(\.\d+)?"
                + @"|""([^""]*)"""
                + @"|"".*\r\n"
                + @"|"".*\n"
                + @"|(==|>=|<=|!=)"
                + @"|([\[\]\(\)\+\-\*/%^=><\&\|,!])"
                + @"|[\t ]"
                + @"|\r\n|\n"
        );

        List<Tokens> tokens = [];
        exceptions = [];
        Exception? exc;
        int count = 0;
        int column = 0;
        int line = 0;
        foreach (var match in regex.Matches(input).Cast<Match>())
        {
            var lex = match.Value;
            if (GetException(match, count, out exc))
                exceptions.Add(exc!);
            column += match.Length;
            count += match.Length;
            TokenType tokenType = GetTokenType(ref lex);
            if (!string.IsNullOrEmpty(lex.Trim()) || tokenType is TokenType.EndOfLine)
            {
                if (lex == "-")
                {
                    var type = tokens.Last().Type;

                    if (!(type == TokenType.Num || type == TokenType.Bool || type == TokenType.String))
                    {
                        tokenType = TokenType.UnaryOperator;
                    }
                }

                tokens.Add(new Tokens(tokenType, lex, line, column));
            }
            if (tokenType is TokenType.EndOfLine or TokenType.Label)
            {
                column = 0;
                line++;
            }
        }
        if (InvalidCharacter(count, input.Length, out exc))
            exceptions.Add(exc!);
        if (tokens[^1].Type != TokenType.EndOfLine)
            tokens.Add(new Tokens(TokenType.EndOfLine, "$", line, column));
        tokens.Add(new Tokens(TokenType.EOS, "$", line + 1, 0));

        return [.. tokens];
    }

    private static bool GetException(Match match, int count, out Exception? exc)
    {
        var start = match.Value[0];
        var end = match.Value[^1];

        if (start == '"' && start != end)
        {
            exc = new SyntaxErrorException("Se esperaba una \"");
            return true;
        }

        return InvalidCharacter(count, match.Index, out exc);
    }

    private static bool InvalidCharacter(int start, int end, out Exception? exc)
    {
        if (start != end)
        {
            exc = new SyntaxErrorException("Carácter inválido");
            return true;
        }

        exc = null;
        return false;
    }

    private static TokenType GetTokenType(ref string lex)
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
            "[" => TokenType.OpenBracket,
            "]" => TokenType.CloseBracket,
            "^" => TokenType.BinaryOperator,
            "&" or "|" => TokenType.BinaryOperator,
            "goto" => TokenType.Goto,
            "\r\n" or "\n" => TokenType.EndOfLine,
            _ => TokenType.Identifier,
        };

        if (tokenType != TokenType.Identifier)
            return tokenType;
        if (double.TryParse(lex, out double _))
            return TokenType.Num;
        if (bool.TryParse(lex, out bool _))
            return TokenType.Bool;
        if (lex[0] == '"')
        {
            lex = lex[1..^1];
            return TokenType.String;
        }
        if (lex[^1] == '\n')
        {
            lex = lex.Trim();
            return TokenType.Label;
        }
        return tokenType;
    }
}
