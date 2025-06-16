using Expressions.Enum;
using Parser.Enums;
using Core.Exceptions;
using Core.Models;
using Parser.Extensions;
using System;
using System.Runtime.InteropServices;

namespace Parser.Models;

public class Parser
{
    private int tokenIndex;

    private delegate bool GetDelegateExpressions(Tokens[] tokens, out IExpression? exp);
    private delegate bool GetDelegateExpression(
        Tokens[] tokens,
        IExpression left,
        out IExpression? exp
    );

    private List<ExceptionWL> exceptions = [];

    public static readonly Dictionary<string, OpFunc> OpFuncs = new()
    {
        { "+", OpFunc.Shift },
        { "-", OpFunc.Reduce },
        { "*", OpFunc.Shift },
        { "/", OpFunc.Reduce },
        { "%", OpFunc.Reduce },
        { "^", OpFunc.Shift },
        { "|", OpFunc.Shift },
        { "&", OpFunc.Shift },
        { "!", OpFunc.Shift },
        { "==", OpFunc.Reduce },
        { "!=", OpFunc.Reduce },
        { "<", OpFunc.Reduce },
        { ">", OpFunc.Reduce },
        { ">=", OpFunc.Reduce },
        { "<=", OpFunc.Reduce },
    };

    public IInstruction Parse(Tokens[] tokens, out List<ExceptionWL> exceptions)
    {
        tokenIndex = 0;
        var node = GetBlockExpression(tokens);

        exceptions = this.exceptions;
        return tokens[tokenIndex].Type == TokenType.EOS
            ? node
            : throw new InvalidProgramException();
    }

    protected BlockInstruction GetBlockExpression(Tokens[] tokens)
    {
        List<IInstruction> lines = [];
        var change = true;
        var startToken = tokens[tokenIndex];
        while (change && tokens[tokenIndex].Type != TokenType.EOS)
        {
            var exist = exceptions.Count;
            var count = lines.Count;
            if (change = GetAssingInstruction(tokens, out IInstruction? exp))
                lines.Add(exp!);
            else if (change = AddExc(exist) && GetCallFunction(tokens, out exp))
                lines.Add(exp!);
            else if (change = AddExc(exist) && GetLabelExpression(tokens, lines.Count, out exp))
                lines.Add(exp!);
            else if (change = AddExc(exist) && GetGotoExpression(tokens, out exp))
                lines.Add(exp!);
            if (!change && AddExc(exist) && tokens[tokenIndex].Type != TokenType.EndOfLine)
                exceptions.Add(new GramaticalExceptions("Se esperaba una asignacion/llamada a una función", tokens[tokenIndex].Location));

            change = change || GetEmptyLine(tokens);
        }
        return new BlockInstruction([.. lines], startToken.Location + tokens[tokenIndex].Location);
    }

    private bool AddExc(int exist) =>
        exist == exceptions.Count;

    private bool GetEmptyLine(Tokens[] tokens)
    {
        Tokens token = tokens[tokenIndex++];
        while (token.Type != TokenType.EndOfLine && token.Type != TokenType.EOS)
            token = tokens[tokenIndex++];

        var result = token.Type == TokenType.EndOfLine;
        if (!result)
            tokenIndex--;
        return result;
    }

    private bool GetLabelExpression(Tokens[] tokens, int index, out IInstruction? exp)
    {
        var name = tokens[tokenIndex].Identifier;
        var startIndex = tokenIndex;
        if (tokens[tokenIndex++].Type != TokenType.Label)
            return ResetDefault(startIndex, out exp);
        var node = new LabelExpression(name, index, tokens[tokenIndex].Location);
        return GetDefault(node, out exp);
    }

    private bool GetGotoExpression(Tokens[] tokens, out IInstruction? exp)
    {
        var message = TemplatesErrors.EXPECTEDERROR_2;
        var message_2 = TemplatesErrors.EXPECTEDERROR_1;
        GotoExpression node;

        var startIndex = tokenIndex;
        if (tokens[tokenIndex++].Type != TokenType.Goto)
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.OpenBracket)
            return ResetDefault(startIndex, out exp, string.Format(message, '[', tokens[tokenIndex - 1].Identifier), tokens[tokenIndex - 1].Location);
        var labelName = tokens[tokenIndex].Identifier;
        if (tokens[tokenIndex++].Type != TokenType.Identifier)
            return ResetDefault(startIndex, out exp, string.Format(message, "Indentificador", tokens[tokenIndex - 1].Identifier), tokens[tokenIndex - 1].Location);
        if (tokens[tokenIndex++].Type != TokenType.CloseBracket)
            return ResetDefault(startIndex, out exp, string.Format(message, "]", tokens[tokenIndex - 1].Identifier), tokens[tokenIndex - 1].Location);

        if (tokens[tokenIndex].Type == TokenType.EndOfLine)
        {
            tokenIndex++;
            node = new GotoExpression(labelName, new ValueExpression(new Values(ValueType.Bool, true), tokens[tokenIndex].Location),
                tokens[startIndex].Location + tokens[tokenIndex].Location);
            return GetDefault(node, out exp);
        }

        if (tokens[tokenIndex++].Type != TokenType.OpenParenthesis)
            return ResetDefault(startIndex, out exp, string.Format(message, "(", tokens[tokenIndex - 1].Identifier), tokens[tokenIndex - 1].Location);

        GetExpressionType(tokens, out IExpression? @bool);

        if (@bool is null)
            return ResetDefault(startIndex, out exp, string.Format(message_2, "booleano"), tokens[tokenIndex].Location);
        if (tokens[tokenIndex++].Type != TokenType.CloseParenthesis)
            return ResetDefault(startIndex, out exp, string.Format(message, ")", tokens[tokenIndex - 1].Identifier), tokens[tokenIndex - 1].Location);
        if (tokens[tokenIndex++].Type != TokenType.EndOfLine)
            return ResetDefault(startIndex, out exp, string.Format(message_2, "cambio de línea"), tokens[tokenIndex].Location);
        node = new GotoExpression(labelName, @bool!, tokens[startIndex].Location + tokens[tokenIndex].Location);
        return GetDefault(node, out exp);
    }

    #region Lines Expressions

    private bool GetAssingInstruction(Tokens[] tokens, out IInstruction? exp)
    {
        var message = TemplatesErrors.EXPECTEDERROR_1;
        int startIndex = tokenIndex;

        if (tokens[tokenIndex++].Type != TokenType.Identifier)
        {
            return ResetDefault(startIndex, out exp);
        }
        if (tokens[tokenIndex++].Type != TokenType.AssingOperator)
            return ResetDefault(startIndex, out exp);

        var exist = exceptions.Count;

        if (tokens[tokenIndex].Type == TokenType.EndOfLine && AddExc(exist))
            return ResetDefault(startIndex, out exp, string.Format(message, "asignación"), tokens[tokenIndex].Location);

        if (!GetExpressionType(tokens,
            out IExpression? value))
        {
            return ResetDefault(startIndex, out exp);
        }

        if (tokens[tokenIndex++].Type != TokenType.EndOfLine && AddExc(exist))
            return ResetDefault(startIndex, out exp, string.Format(message, "cambio de línea"), tokens[tokenIndex - 1].Location);
        return AssingDefault(tokens[startIndex], value, out exp);
    }

    private bool GetCallFunction(Tokens[] tokens, out IInstruction? exp)
    {
        var message = TemplatesErrors.EXPECTEDERROR_1;
        int startIndex = tokenIndex;

        var exist = exceptions.Count;
        if (tokens[tokenIndex++].Type != TokenType.Identifier || !CheckFunction(tokens, out IExpression[]? expressions))
            return ResetDefault(startIndex, out exp);

        if (tokens[tokenIndex++].Type != TokenType.EndOfLine && AddExc(exist))
            return ResetDefault(startIndex, out exp, string.Format(message, "cambio de línea"), tokens[tokenIndex - 1].Location);
        return ActionDefault(tokens[startIndex].Identifier, expressions!, 
            tokens[startIndex].Location + tokens[tokenIndex - 1].Location, out exp);
    }

    private bool GetCallFunction(Tokens[] tokens, out IExpression? exp)
    {
        int startIndex = tokenIndex - 1;
        if (!CheckFunction(tokens, out IExpression[]? expressions))
            return ResetDefault(tokenIndex, out exp);
        return FunctDefault(tokens[startIndex].Identifier, expressions!, 
            tokens[startIndex].Location + tokens[tokenIndex - 1].Location, out exp);
    }
    private bool CheckFunction(Tokens[] tokens, out IExpression[]? expressions)
    {
        var message = TemplatesErrors.EXPECTEDERROR_1;
        var message_2 = TemplatesErrors.EXPECTEDERROR_2;
        int startIndex = tokenIndex;
        List<IExpression> @params = [];
        if (tokens[tokenIndex++].Type != TokenType.OpenParenthesis)
            return ResetDefault(startIndex, out expressions, string.Format(message_2, "( o <-", tokens[tokenIndex - 1].Identifier), tokens[tokenIndex - 1].Location);
        do
        {
            if (tokens[tokenIndex].Type == TokenType.CloseParenthesis)
            {
                tokenIndex++;
                break;
            }

            if (tokens[tokenIndex].Type == TokenType.Comma)
            {
                exceptions.Add(new GramaticalExceptions(string.Format(message, "argumento"), tokens[tokenIndex].Location));
                continue;
            }

            GetExpressionType(
                tokens,
                out IExpression? value);
            @params.Add(value!);
        } while (tokens[tokenIndex++].Type == TokenType.Comma);

        if (tokens[tokenIndex - 1].Identifier != ")")
            return ResetDefault(startIndex, out expressions, string.Format(message, ")"), tokens[tokenIndex - 1].Location);
        return GetDefault([.. @params], out expressions);
    }

    #endregion

    #region Values Expressions

    public delegate bool DispatcherType(
        Tokens[] tokens,
        out IExpression? exp
    );

    private bool GetExpressionType(
        Tokens[] tokens,
        out IExpression? exp
    )
    {
        DispatcherType[] @delegates = [DispatcherTypeBool, DispatcherTypeNum, DispatcherTypeString];
        var exist = exceptions.Count;
        var startIndex = tokenIndex;
        var token = tokens[tokenIndex];
        var message = TemplatesErrors.EXPECTEDERROR_1;

        for (int i = 0;
            !ResetDefault(startIndex, out IExpression? _) &&
            i < @delegates.Length &&
            AddExc(exist);
            i++)
        {
            DispatcherType? @delegate = @delegates[i];
            if (@delegate.Invoke(tokens, out exp))
            {
                var left = exp;
                if (!DispatcherComparer(tokens, exp!, out exp))
                    return GetDefault(left, out exp);
                return true;
            }
        }

        var name = AddExc(exist) ? string.Format(message, "Número, Booleano o String") : null;
        Location? loc = AddExc(exist) ? token.Location : null;
        return ResetDefault(startIndex, out exp, name, loc);
    }

    private bool DispatcherTypeString(Tokens[] tokens, out IExpression? exp)
    {
        var startIndex = tokenIndex;
        var exist = exceptions.Count;
        if (!GetTermExpression(tokens, TokenType.String, out IExpression? termExp, GetExpressionType))
        {
            exceptions.RemoveRange(exist, exceptions.Count - exist);
            return ResetDefault(startIndex, out exp);
        }

        return tokens[tokenIndex].Identifier switch
        {
            "+" => ConstructExpression(tokens, termExp!, out exp, GetTSExpression, "+"),
            "==" or "!=" or "<=" => GetDefault(termExp, out exp),
            ">=" or ">" or "<" => GetDefault(termExp, out exp),
            "$" or ")" or "," or "\n" or "\r\n" => GetDefault(termExp, out exp),
            _ => ResetDefault(tokenIndex, out exp)
        };
    }

    private bool DispatcherTypeNum(Tokens[] tokens, out IExpression? exp)
    {
        var startIndex = tokenIndex;
        var exist = exceptions.Count;
        if (tokens[tokenIndex].Type == TokenType.UnaryOperator)
            return GetLUExpression(tokens, out exp, GetTNExpression, "-");
        if (!GetTermExpression(tokens, TokenType.Num, out IExpression? termExp, GetExpressionType))
        {
            exceptions.RemoveRange(exist, exceptions.Count - exist);
            return ResetDefault(startIndex, out exp);
        }

        return DispatcherTypeNumS(tokens, termExp, out exp);
    }

    private bool DispatcherTypeNumS(Tokens[] tokens, IExpression? termExp, out IExpression? exp)
    {
        return tokens[tokenIndex].Identifier switch
        {
            "+" or "-" => ConstructExpression(tokens, termExp!, out termExp, GetMultExpressions, "+", "-") & DispatcherTypeNumS(tokens, termExp!, out exp),
            "/" or "*" => ConstructExpression(tokens, termExp!, out termExp, GetPowExpressions, "/", "*") & DispatcherTypeNumS(tokens, termExp!, out exp),
            "^" => ConstructExpression(tokens, termExp!, out termExp, GetLUNExpression, "^") & DispatcherTypeNumS(tokens, termExp!, out exp),
            "==" or "!=" or "<=" => GetDefault(termExp, out exp),
            ">=" or ">" or "<" => GetDefault(termExp, out exp),
            "$" or ")" or "," or "\n" or "\r\n" => GetDefault(termExp, out exp),
            _ => ResetDefault(tokenIndex, out exp)
        };
    }

    private bool DispatcherTypeBool(Tokens[] tokens, out IExpression? exp)
    {
        var startIndex = tokenIndex;
        var exist = exceptions.Count;
        if (tokens[tokenIndex].Type == TokenType.UnaryOperator)
            return GetLUExpression(tokens, out exp, GetTBExpression, "!");
        if (!GetTermExpression(tokens, TokenType.Bool, out IExpression? termExp, GetExpressionType))
        {
            exceptions.RemoveRange(exist, exceptions.Count - exist);
            return ResetDefault(startIndex, out exp);
        }

        return DispatcherTypeBoolS(tokens, termExp!, out exp);
    }

    private bool DispatcherTypeBoolS(Tokens[] tokens, IExpression termExp, out IExpression? exp)
    {
        return tokens[tokenIndex].Identifier switch
        {
            "|" => ConstructExpression(tokens, termExp, out termExp!, GetAndExpressions, "|") & DispatcherTypeBoolS(tokens, termExp!, out exp),
            "&" => ConstructExpression(tokens, termExp, out termExp!, GetComparerExpression, "&") & DispatcherTypeBoolS(tokens, termExp!, out exp),
            "==" or "!=" or "<=" => GetDefault(termExp, out exp),
            ">=" or ">" or "<" => GetDefault(termExp, out exp),
            "$" or ")" or "," or "\n" or "\r\n" => GetDefault(termExp, out exp),
            _ => ResetDefault(tokenIndex, out exp)
        };
    }


    private bool DispatcherComparer(Tokens[] tokens, IExpression termExp, out IExpression? exp)
    {
        return tokens[tokenIndex].Identifier switch
        {
            "==" or "!=" or "<=" => ConstructExpression(tokens, termExp!, out exp, GetComparerType, "==", "!=", "<=", ">=", ">", "<"),
            ">=" or ">" or "<" => ConstructExpression(tokens, termExp!, out exp, GetComparerType, "==", "!=", "<=", ">=", ">", "<"),
            _ => ResetDefault(tokenIndex, out exp)
        };
    }

    private bool GetNumericExpression(Tokens[] tokens, out IExpression? num) =>
        GetAddExpressions(tokens, out num);

    private bool GetBooleanExpression(Tokens[] tokens, out IExpression? @bool) =>
        GetOrExpressions(tokens, out @bool);

    private bool GetStringExpression(Tokens[] tokens, out IExpression? value) =>
        GetSumStringExpression(tokens, out value);

    #endregion

    #region NumericExpressions

    private bool GetAddExpressions(Tokens[] tokens, out IExpression? addExp) =>
        GetExpression(tokens, out addExp, GetMultExpressions, "+", "-");

    private bool GetMultExpressions(Tokens[] tokens, out IExpression? multExp) =>
        GetExpression(tokens, out multExp, GetPowExpressions, "*", "/", "%");

    private bool GetPowExpressions(Tokens[] tokens, out IExpression? powExp) =>
        GetExpression(tokens, out powExp, GetLUNExpression, "^");

    private bool GetLUNExpression(Tokens[] tokens, out IExpression? exp)
    {
        var exist = exceptions.Count;
        return GetLUExpression(tokens, out exp, GetTNExpression, "-") ||
           AddExc(exist) && GetTNExpression(tokens, out exp);
    }

    private bool GetTNExpression(Tokens[] tokens, out IExpression? exp) =>
        GetTermExpression(tokens, TokenType.Num, out exp, GetNumericExpression);


    #endregion

    #region BooleanExpression

    private bool GetOrExpressions(Tokens[] tokens, out IExpression? binaryExp) =>
        GetExpression(tokens, out binaryExp, GetAndExpressions, "|");

    private bool GetAndExpressions(Tokens[] tokens, out IExpression? exp) =>
        GetExpression(tokens, out exp, GetComparerExpression, "&");

    private bool GetComparerExpression(Tokens[] tokens, out IExpression? binaryExp) =>
        GetExpression(tokens, out binaryExp, GetComparerType, "==", "!=", "<=", ">=", ">", "<");

    private bool GetComparerType(Tokens[] tokens, out IExpression? exp)
    {
        var exist = exceptions.Count;
        var startIndex = tokenIndex;
        var message = TemplatesErrors.EXPECTEDERROR_1;

        // if (AddExc(exist) && DispatcherTypeString(tokens, out exp))
        //     return true;
        // tokenIndex = startIndex;
        // if (AddExc(exist) && GetNotExpressions(tokens, out exp))
        //     return true;
        // exceptions.RemoveRange(exist, exceptions.Count - exist);
        // tokenIndex = startIndex;
        if (DispatcherTypeNum(tokens, out exp))
            return true;
        tokenIndex = startIndex;
        return ResetDefault(startIndex, out exp, string.Format(message, "dos literales para comparar"), tokens[startIndex].Location + tokens[tokenIndex].Location);
    }

    //private bool GetNotExpressions(Tokens[] tokens, out IExpression? exp)
    //{
    //    var exist = exceptions.Count;
    //    return GetLUExpression(tokens, out exp, GetTBExpression, "!") ||
    //           AddExc(exist) && GetTBExpression(tokens, out exp);
    //}

    private bool GetTBExpression(Tokens[] tokens, out IExpression? exp) =>
        GetTermExpression(tokens, TokenType.Bool, out exp, GetBooleanExpression);

    #endregion

    #region StringExpression

    private bool GetSumStringExpression(Tokens[] tokens, out IExpression? exp) =>
        GetExpression(tokens, out exp, GetTSExpression, "+");

    private bool GetTSExpression(Tokens[] tokens, out IExpression? exp) =>
        GetTermExpression(tokens, TokenType.String, out exp, GetStringExpression);

    #endregion

    #region Default Methods

    private bool GetExpression(
        Tokens[] tokens,
        out IExpression? binaryExp,
        GetDelegateExpressions getExpressions,
        params string[] @params
    )
    {
        var startIndex = tokenIndex;
        if (!getExpressions.Invoke(tokens, out IExpression? left))
            return ResetDefault(startIndex, out binaryExp);
        if (ConstructExpression(tokens, left!, out IExpression? result, getExpressions, @params))
            return GetDefault(result!, out binaryExp);
        if (left is IExpression value)
            return GetDefault(value, out binaryExp);
        return ResetDefault(startIndex, out binaryExp);
    }

    private bool ConstructExpression(
        Tokens[] tokens,
        IExpression left,
        out IExpression? exp,
        GetDelegateExpressions getSubExp,
        params string[] @params
    )
    {
        var startIndex = tokenIndex;
        if (!MatchToken(tokens, @params, out string? op))
            return ResetDefault(startIndex, out exp);
        var opFunc = OpFuncs[op!];
        return opFunc switch
        {
            OpFunc.Shift => ShiftExpression(tokens, left, out exp, getSubExp, op!, @params),
            OpFunc.Reduce => ReduceExpression(tokens, left, out exp, getSubExp, op!, @params),
            _ => ResetDefault(startIndex, out exp),
        };
    }

    private bool ShiftExpression(
        Tokens[] tokens,
        IExpression left,
        out IExpression? exp,
        GetDelegateExpressions getSubExpressions,
        string op,
        params string[] @params
    )
    {
        if (!GetExpression(tokens, out IExpression? right, getSubExpressions, @params))
            return ResetDefault(tokenIndex, out exp);
        var node = new BinaryExpression(left, right!, op.GetBinaryType(), left.Location + right!.Location);
        return GetDefault(node, out exp);
    }

    private bool ReduceExpression(
        Tokens[] tokens,
        IExpression left,
        out IExpression? exp,
        GetDelegateExpressions getSubExpressions,
        string op,
        params string[] @params
    )
    {
        if (!getSubExpressions.Invoke(tokens, out IExpression? right))
            return ResetDefault(tokenIndex, out exp);
        var node = new BinaryExpression(left, right!, op.GetBinaryType(), left.Location + right!.Location);
        if (!ConstructExpression(tokens, node, out exp, getSubExpressions, @params))
            return GetDefault(node, out exp);
        return true;
    }

    private bool GetLUExpression(
        Tokens[] tokens,
        out IExpression? exp,
        GetDelegateExpressions getExpressions,
        params string[] @params
    )
    {
        var startIndex = tokenIndex;
        var exist = exceptions.Count;
        List<string> operators = [];
        while (MatchToken(tokens, @params, out string? op))
            operators.Add(op!);
        if (operators.Count == 0)
            return ResetDefault(startIndex, out exp);
        if (!getExpressions(tokens, out exp))
            return ResetDefault(startIndex, out exp);
        IExpression? node = operators.Aggregate(exp!, (agg, str)
            => new UnaryExpression(agg!, str!.GetUnaryType(), agg!.Location));
        return GetDefault(node, out exp);
    }

    private bool GetTermExpression(
        Tokens[] tokens,
        TokenType type,
        out IExpression? termExp,
        GetDelegateExpressions getExpressions
    )
    {
        var startIndex = tokenIndex;
        var token = tokens[tokenIndex++];
        var message = TemplatesErrors.EXPECTEDERROR_2;
        if (Values.TryParse(token.Identifier, token.Type, out Values? value))
            return GetDefault(new ValueExpression(value!, token.Location), out termExp);
        if (token.Type == TokenType.Identifier)
        {
            if (GetCallFunction(tokens, out IExpression? function))
                return GetDefault(function, out termExp);
            return GetDefault(new VariableExpression(token.Identifier, token.Location), out termExp);
        }
        
        if (token.Type == TokenType.OpenParenthesis)
        {
            getExpressions(tokens, out termExp);
            if (tokens[tokenIndex++].Type != TokenType.CloseParenthesis)
                return ResetDefault(startIndex, out termExp, string.Format(message, ")", tokens[tokenIndex - 1].Identifier), tokens[tokenIndex].Location);
            return true;
        }
        return ResetDefault(startIndex, out termExp, string.Format(message, type, token.Type));
    }

    private bool MatchToken(Tokens[] tokens, string[] @params, out string? op)
    {
        var startIndex = tokenIndex;
        var identifier = tokens[tokenIndex++].Identifier;
        if (!@params.Contains(identifier))
            return ResetDefault(startIndex, out op);
        return GetDefault(identifier, out op);
    }

    private bool AssingDefault(Tokens name, IExpression? value, out IInstruction? exp)
    {
        exp = new Assign(name.Identifier, value!, name.Location);
        return true;
    }

    private bool ActionDefault(string action, IExpression?[] values, Location location, out IInstruction? exp)
    {
        exp = new ActionInstruction(action, values!, location);
        return true;
    }

    private bool FunctDefault(string name, IExpression?[] values, Location location, out IExpression? exp)
    {
        exp = new FuncExpresion(name, values!, location);
        return true;
    }

    private bool GetDefault<T>(T value, out T exp, string? name = null, Location? location = null)
    {
        if (location is Location nonNullLocation)
        {
            if (!string.IsNullOrEmpty(name))
                exceptions.Add(new GramaticalExceptions(name, nonNullLocation));
        }

        exp = value;
        return true;
    }

    private bool ResetDefault<T>(int index, out T? exp, string? name = null, Location? location = null)
    {
        if (location is Location nonNullLocation) 
        { 
            if (!string.IsNullOrEmpty(name))
                exceptions.Add(new GramaticalExceptions(name, nonNullLocation));
        }
        tokenIndex = index;
        exp = default;
        return false;
    }

    #endregion
}

public static class TemplatesErrors
{
    public const string EXPECTEDERROR_2 = "Se espera {0} y se recibió {1}";
    public const string EXPECTEDERROR_1 = "Se espera {0}";
}