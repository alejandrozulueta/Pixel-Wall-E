using Core.Exceptions;
using Core.Models;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Parser.Models;

public static class Lexer
{
    public static Tokens[] Tokenizer(string input, out List<ExceptionWL> exceptions)
    {
        Regex regex = new(
            @"[a-zA-Z_][a-zA-Z0-9_]*[\t ]*\r\n"
                + @"|[a-zA-Z_][a-zA-Z0-9_]*[\t ]*\n"
                + @"|([a-zA-Z_][a-zA-Z0-9_]*)"
                + @"|\d+(\.\d+)?"
                + @"|""([^""]*)"""
                + @"|""[^""\r\n]*"
                + @"|"".*\r\n"
                + @"|"".*\n"
                + @"|(==|>=|<=|!= |<-)"
                + @"|([\[\]\(\)\+\-\*/%^><\&\|,!])"
                + @"|[\t ]"
                + @"|\r\n|\n"
                + @"|."
        );

        List<Tokens> tokens = [];
        exceptions = [];
        int count = 0;
        int column = 0;
        int line = 1;
        foreach (var match in regex.Matches(input).Cast<Match>())
        {
            var lex = match.Value;
            TokenType tokenType = GetTokenType(ref lex);
            if (!string.IsNullOrEmpty(lex.Trim()) || tokenType is TokenType.EndOfLine)
            {
                if (lex == "-")
                {
                    var type = tokens.Count > 0 ? tokens[^1].Type : TokenType.InvalidToken;

                    if (!(type == TokenType.Num || type == TokenType.Bool || type == TokenType.String))
                    {
                        tokenType = TokenType.UnaryOperator;
                    }
                }

                if (tokenType == TokenType.InvalidToken)
                {
                    exceptions.Add(new SyntaxException("Carácter inválido", new Location(line, column, lex.Length)));
                    continue;
                }
                if (tokenType == TokenType.InvalidStringToken)
                {
                    exceptions.Add(new SyntaxException("Se espera un cierre de comillas", new Location(line, column, lex.Length)));
                    continue;
                }
                tokens.Add(new Tokens(tokenType, lex, line, column));
                
                column += match.Length;
                count += match.Length;
            }
            if (tokenType is TokenType.EndOfLine or TokenType.Label)
            {
                column = 0;
                line++;
            }
        }
        if (tokens[^1].Type == TokenType.Identifier && tokens[^1].Location.InitCol == 0)
            tokens[^1].Type = TokenType.Label;
        if (tokens[^1].Type == TokenType.Label && tokens[^1].Location.InitCol != 0)
            tokens[^1].Type = TokenType.Identifier;
        if (tokens[^1].Type != TokenType.EndOfLine)
            tokens.Add(new Tokens(TokenType.EndOfLine, "$", line, column));
        tokens.Add(new Tokens(TokenType.EOS, "$", line, 0));

        if (tokens[0].Identifier != "Spawn")
            exceptions.Add(new SyntaxException("Se esperaba 'Spawn' al inicio del código", tokens[0].Location));

        return [.. tokens];
    }
    private static TokenType GetTokenType(ref string lex)
    {
        var tokenType = lex switch
        {
            "+" or "-" or "*" or "/" or "%" => TokenType.BinaryOperator,
            "==" or ">=" or "<=" or "!=" => TokenType.BinaryOperator,
            ">" or "<" => TokenType.BinaryOperator,
            "<-" => TokenType.AssingOperator,
            "!" or "-" => TokenType.UnaryOperator,
            "(" => TokenType.OpenParenthesis,
            ")" => TokenType.CloseParenthesis,
            "[" => TokenType.OpenBracket,
            "]" => TokenType.CloseBracket,
            "^" => TokenType.BinaryOperator,
            "&" or "|" => TokenType.BinaryOperator,
            "GoTo" => TokenType.Goto,
            "\r\n" or "\n" => TokenType.EndOfLine,
            _ => TokenType.Identifier,
        };

        if (tokenType != TokenType.Identifier)
            return tokenType;
        if (double.TryParse(lex, out double _))
            return TokenType.Num;
        if (bool.TryParse(lex, out bool _))
            return TokenType.Bool;
        if (lex[0] == '"' && lex.Length > 1)
        {
            if (lex[^1] != '"')
            {
                lex = lex[1..];
                return TokenType.InvalidStringToken;
            }
            lex = lex[1..^1];
            return TokenType.String;
        }
        if (lex[^1] == '\n')
        {
            lex = lex.Trim();
            return TokenType.Label;
        }

        var @char = lex[0];

        if (@char == ',')
            return TokenType.Comma;    
        
        if (!Char.IsLetter(@char) && @char != '_')
            return TokenType.InvalidToken;
        
        return tokenType;
    }
}
