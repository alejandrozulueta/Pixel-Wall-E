using Expressions.Interfaces;
using Expressions.Models;
using Parser.Enums;
using Parser.Extensions;

namespace Parser.Models;

public class Parser
{
    private int tokenIndex;

    private delegate bool GetDelegateExpressions<T>(Tokens[] tokens, out IExpression<T>? exp);
    private delegate bool GetDelegateExpression<T>(
        Tokens[] tokens,
        IExpression<T> left,
        out IExpression<T>? exp
    );

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

    public IExpression Parse(Tokens[] tokens)
    {
        tokenIndex = 0;
        var node = GetBlockExpression(tokens);
        return tokens[tokenIndex].Type == TokenType.EOS
            ? node
            : throw new InvalidProgramException();
    }

    protected BlockExpression GetBlockExpression(Tokens[] tokens)
    {
        List<IExpression> lines = [];
        var change = true;

        while (change)
        {
            if (change = GetAssingExpression(tokens, out IExpression? exp))
                lines.Add(exp!);
            else if (change = GetActionExpression(tokens, out exp))
                lines.Add(exp!);
            else if (change = GetFuncExpression(tokens, out exp))
                lines.Add(exp!);
            // else if (GetLabelExpression(tokens, out exp))
            //     lines.Add(exp);
            // else if (GetGotoExpression(tokens, out exp))
            //     lines.Add(exp);
        }
        return new BlockExpression([.. lines]);
    }

    #region Lines Expressions

    private bool GetAssingExpression(Tokens[] tokens, out IExpression? exp)
    {
        int startIndex = tokenIndex;
        if (tokens[tokenIndex++].Type != TokenType.Identifier)
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.AssingOperator)
            return ResetDefault(startIndex, out exp);
        if (GetBooleanExpression(tokens, out IExpression<bool>? @bool))
            return AssingDefault(tokens[startIndex].Identifier, @bool, out exp);
        if (GetNumericExpression(tokens, out IExpression<int>? num))
            return AssingDefault(tokens[startIndex].Identifier, num, out exp);
        if (GetStringExpression(tokens, out IExpression<string>? value))
            return AssingDefault(tokens[startIndex].Identifier, value, out exp);
        return ResetDefault(startIndex, out exp);
    }

    private bool GetActionExpression(Tokens[] tokens, out IExpression? exp)
    {
        return ResetDefault(tokenIndex, out exp);
    }

    private bool GetFuncExpression(Tokens[] tokens, out IExpression? exp)
    {
        return ResetDefault(tokenIndex, out exp);
    }

    #endregion

    #region Value Expressions

    private bool GetNumericExpression(Tokens[] tokens, out IExpression<int>? num) =>
        GetBinaryNumericExpression(tokens, out num);

    private bool GetBooleanExpression(Tokens[] tokens, out IExpression<bool>? @bool) =>
        GetBinaryBooleanExpression(tokens, out @bool);

    private bool GetStringExpression(Tokens[] tokens, out IExpression<string>? value)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region NumericExpressions

    private bool GetBinaryNumericExpression(Tokens[] tokens, out IExpression<int>? binaryExp) =>
        GetAddExpressions(tokens, out binaryExp);

    private bool GetAddExpressions(Tokens[] tokens, out IExpression<int>? binaryExp) =>
        GetExpressions(tokens, out binaryExp, GetMultExpressions, "+", "-");

    private bool GetMultExpressions(Tokens[] tokens, out IExpression<int>? multExp) =>
        GetExpressions(tokens, out multExp, GetPowExpressions, "*", "/", "%");

    private bool GetPowExpressions(Tokens[] tokens, out IExpression<int>? powExp) =>
        GetExpressions(tokens, out powExp, GetLUNExpression, "^");

    private bool GetLUNExpression(Tokens[] tokens, out IExpression<int>? exp) =>
        GetLUExpression(tokens, out exp, GetTNExpression, "-");

    private bool GetTNExpression(Tokens[] tokens, out IExpression<int>? exp) =>
        GetTermExpression(tokens, out exp, GetNumericExpression);

    #endregion

    #region BooleanExpression

    private bool GetBinaryBooleanExpression(Tokens[] tokens, out IExpression<bool>? binaryExp) =>
        GetOrExpressions(tokens, out binaryExp);

    private bool GetOrExpressions(Tokens[] tokens, out IExpression<bool>? binaryExp) =>
        GetExpressions(tokens, out binaryExp, GetAndExpressions, "|");

    private bool GetAndExpressions(Tokens[] tokens, out IExpression<bool>? exp) =>
        GetExpressions(tokens, out exp, GetNotExpressions, "&");

    private bool GetNotExpressions(Tokens[] tokens, out IExpression<bool>? exp) =>
        GetLUExpression(tokens, out exp, GetTBExpression, "!");

    private bool GetTBExpression(Tokens[] tokens, out IExpression<bool>? exp) =>
        GetTermExpression(tokens, out exp, GetBinaryBooleanExpression)
        || GetComparerExpression(tokens, out exp);

    private bool GetComparerExpression(Tokens[] tokens, out IExpression<bool>? binaryExp) =>
        GetExpressions(
            tokens,
            out binaryExp,
            GetBinaryBooleanExpression,
            "==",
            "!=",
            "<=",
            ">=",
            ">",
            "<"
        );

    #endregion

    #region Default Methods

    private bool GetExpressions<T>(
        Tokens[] tokens,
        out IExpression<T>? binaryExp,
        GetDelegateExpressions<T> getExpressions,
        params string[] @params
    )
        where T : notnull
    {
        var startIndex = tokenIndex;
        if (!getExpressions.Invoke(tokens, out IExpression<T>? left))
            return ResetDefault(startIndex, out binaryExp);
        if (GetExpression(tokens, left!, out IExpression<T>? result, getExpressions, @params))
            return GetDefault(result!, out binaryExp);
        if (left is IExpression<T> value)
            return GetDefault(value, out binaryExp);
        return ResetDefault(startIndex, out binaryExp);
    }

    private bool GetExpression<T>(
        Tokens[] tokens,
        IExpression<T> left,
        out IExpression<T>? exp,
        GetDelegateExpressions<T> getSubExp,
        params string[] @params
    )
        where T : notnull
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

    private bool ShiftExpression<T>(
        Tokens[] tokens,
        IExpression<T> left,
        out IExpression<T>? exp,
        GetDelegateExpressions<T> getSubExpressions,
        string op,
        params string[] @params
    )
        where T : notnull
    {
        if (!GetExpressions(tokens, out IExpression<T>? right, getSubExpressions, @params))
            return ResetDefault(tokenIndex, out exp);
        var node = new BinaryExpression<T>(left, right!, op.GetBinaryType());
        return GetDefault(node, out exp);
    }

    private bool ReduceExpression<T>(
        Tokens[] tokens,
        IExpression<T> left,
        out IExpression<T>? exp,
        GetDelegateExpressions<T> getSubExpressions,
        string op,
        params string[] @params
    )
        where T : notnull
    {
        if (!getSubExpressions.Invoke(tokens, out IExpression<T>? right))
            return ResetDefault(tokenIndex, out exp);
        var node = new BinaryExpression<T>(left, right!, op.GetBinaryType());
        if (!GetExpression(tokens, node, out exp, getSubExpressions, @params))
            return GetDefault(node, out exp);
        return true;
    }

    // private bool GetRUExpression<T>(
    //     Tokens[] tokens,
    //     out IExpression<T>? exp,
    //     GetDelegateExpressions<T> getExpressions,
    //     params string[] @params
    // )
    //     where T : notnull, IParsable<T>
    // {
    //     var startIndex = tokenIndex;
    //     if (!getExpressions(tokens, out IExpression<T>? value))
    //         return ResetDefault(startIndex, out exp);
    //     if (!GetRUExpressions(tokens, value!, out exp, @params))
    //         return GetDefault(value!, out exp);
    //     return true;
    // }

    private bool GetLUExpression<T>(
        Tokens[] tokens,
        out IExpression<T>? exp,
        GetDelegateExpressions<T> getExpressions,
        params string[] @params
    )
        where T : notnull, IParsable<T>
    {
        var startIndex = tokenIndex;
        if (getExpressions(tokens, out IExpression<T>? value))
            return GetDefault(value!, out exp);
        if (!GetRUExpressions(tokens, value!, out exp, @params))
            return ResetDefault(startIndex, out exp);
        return true;
    }

    private bool GetRUExpressions<T>(
        Tokens[] tokens,
        IExpression<T> num,
        out IExpression<T>? exp,
        params string[] @params
    )
        where T : notnull
    {
        var startIndex = tokenIndex;
        if (!MatchToken(tokens, @params, out string? op))
            return ResetDefault(startIndex, out exp);
        var node = new UnaryExpression<T>(num, op!.GetUnaryType());
        if (!GetRUExpressions(tokens, node, out exp))
            return GetDefault(node, out exp);
        return true;
    }

    private bool GetTermExpression<T>(
        Tokens[] tokens,
        out IExpression<T>? termExp,
        GetDelegateExpressions<T> getExpressions
    )
        where T : notnull, IParsable<T>
    {
        var startIndex = tokenIndex;
        var token = tokens[tokenIndex++];
        if (T.TryParse(token.Identifier, null, out T? num1))
            return GetDefault(new ValueExpression<T>(num1), out termExp);
        if (token.Type == TokenType.Identifier)
            return GetDefault(new VariableExpression<T>(token.Identifier), out termExp);
        if (token.Type != TokenType.OpenParenthesis)
            return ResetDefault(startIndex, out termExp);
        if (!getExpressions(tokens, out termExp))
            return ResetDefault(startIndex, out termExp);
        if (tokens[tokenIndex++].Type != TokenType.CloseParenthesis)
            return ResetDefault(startIndex, out termExp);
        return true;
    }

    private bool MatchToken(Tokens[] tokens, string[] @params, out string? op)
    {
        var startIndex = tokenIndex;
        var identifier = tokens[tokenIndex++].Identifier;
        if (!@params.Contains(identifier))
            return ResetDefault(startIndex, out op);
        return GetDefault(identifier, out op);
    }

    private bool AssingDefault<T>(string name, IExpression<T>? value, out IExpression? exp)
        where T : notnull
    {
        exp = new Assign<T>(name, value!);
        return true;
    }

    private bool GetDefault<T>(T value, out T exp)
    {
        exp = value;
        return true;
    }

    private bool ResetDefault<T>(int index, out T? exp)
    {
        tokenIndex = index;
        exp = default;
        return false;
    }

    #endregion
}
