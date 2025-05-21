using Expressions.Interfaces;
using Expressions.Models;
using Parser.Enums;
using Parser.Extensions;

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

    public IInstruction Parse(Tokens[] tokens)
    {
        tokenIndex = 0;
        var node = GetBlockExpression(tokens);
        return tokens[tokenIndex].Type == TokenType.EOS
            ? node
            : throw new InvalidProgramException();
    }

    protected BlockInstruction GetBlockExpression(Tokens[] tokens)
    {
        List<IInstruction> lines = [];
        var change = true;

        while (change)
        {
            if (change = GetAssingInstruction(tokens, out IInstruction? exp))
                lines.Add(exp!);
            else if (change = GetCallFunction(tokens, out exp))
                lines.Add(exp!);
            else if (change = GetLabelExpression(tokens, lines.Count, out exp))
                lines.Add(exp!);
            else if (change = GetGotoExpression(tokens, out exp))
                lines.Add(exp!);
            change = change || GetEmptyLine(tokens);
        }
        return new BlockInstruction([.. lines]);
    }

    private bool GetEmptyLine(Tokens[] tokens)
    {
        Tokens token;
        do token = tokens[tokenIndex++];
        while (token.Type != TokenType.EndOfLine && token.Type != TokenType.EOS);
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
        var node = new LabelExpression(name, index);
        return GetDefault(node, out exp);
    }

    private bool GetGotoExpression(Tokens[] tokens, out IInstruction? exp)
    {
        var startIndex = tokenIndex;
        if (tokens[tokenIndex++].Type != TokenType.Goto)
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.OpenBracket)
            return ResetDefault(startIndex, out exp);
        var labelName = tokens[tokenIndex].Identifier;
        if (tokens[tokenIndex++].Type != TokenType.Identifier)
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.CloseBracket)
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.OpenParenthesis)
            return ResetDefault(startIndex, out exp);
        if (!GetBooleanExpression(tokens, out IExpression? @bool))
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.CloseParenthesis)
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.EndOfLine)
            return ResetDefault(startIndex, out exp);
        var node = new GotoExpression(labelName, @bool!);
        return GetDefault(node, out exp);
    }

    #region Lines Expressions

    private bool GetAssingInstruction(Tokens[] tokens, out IInstruction? exp)
    {
        int startIndex = tokenIndex;
        if (tokens[tokenIndex++].Type != TokenType.Identifier)
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.AssingOperator)
            return ResetDefault(startIndex, out exp);
        if (
            !GetExpressionType(
                tokens,
                out IExpression? value,
                GetBooleanExpression,
                GetStringExpression,
                GetNumericExpression
            )
        )
            return ResetDefault(startIndex, out exp);
        return AssingDefault(tokens[startIndex].Identifier, value, out exp);
    }

    private bool GetCallFunction(Tokens[] tokens, out IInstruction? exp)
    {
        int startIndex = tokenIndex;
        if (!CheckFunction(tokens, out IExpression[]? expressions))
            return ResetDefault(startIndex, out exp);
        if (tokens[tokenIndex++].Type != TokenType.EndOfLine)
            return ResetDefault(startIndex, out exp);
        return ActionDefault(tokens[startIndex].Identifier, expressions!, out exp);
    }

    private bool GetCallFunction(Tokens[] tokens, out IExpression? exp)
    {
        int startIndex = tokenIndex;
        if (!CheckFunction(tokens, out IExpression[]? expressions))
            return ResetDefault(startIndex, out exp);
        return FunctDefault(tokens[tokenIndex++].Identifier, expressions!, out exp);
    }

    private bool CheckFunction(Tokens[] tokens, out IExpression[]? expressions)
    {
        int startIndex = tokenIndex;
        List<IExpression> @params = [];
        if (tokens[tokenIndex++].Type != TokenType.Identifier)
            return ResetDefault(startIndex, out expressions);
        if (tokens[tokenIndex++].Type != TokenType.OpenParenthesis)
            return ResetDefault(startIndex, out expressions);

        do
        {
            if (!GetExpressionType(tokens, out IExpression? value))
                return ResetDefault(startIndex, out expressions);
            @params.Add(value!);
        } while (tokens[tokenIndex++].Identifier != ",");

        if (tokens[tokenIndex - 1].Identifier != ")")
            return ResetDefault(startIndex, out expressions);
        return GetDefault([.. @params], out expressions);
    }

    #endregion

    #region Values Expressions

    private bool GetExpressionType(
        Tokens[] tokens,
        out IExpression? exp,
        params GetDelegateExpressions[] getDelegates
    )
    {
        var startIndex = tokenIndex;
        foreach (var getDelegate in getDelegates)
        {
            if (!getDelegate(tokens, out exp))
                continue;
            if (tokens[tokenIndex++].Type == TokenType.EndOfLine)
                return true;
            tokenIndex = startIndex;
        }
        return ResetDefault(startIndex, out exp);
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

    private bool GetLUNExpression(Tokens[] tokens, out IExpression? exp) =>
        GetLUExpression(tokens, out exp, GetTNExpression, "-");

    private bool GetTNExpression(Tokens[] tokens, out IExpression? exp) =>
        GetTermExpression<double>(tokens, TokenType.Num, out exp, GetNumericExpression);

    #endregion

    #region BooleanExpression

    private bool GetOrExpressions(Tokens[] tokens, out IExpression? binaryExp) =>
        GetExpression(tokens, out binaryExp, GetAndExpressions, "|");

    private bool GetAndExpressions(Tokens[] tokens, out IExpression? exp) =>
        GetExpression(tokens, out exp, GetComparerExpression, "&");

    private bool GetComparerExpression(Tokens[] tokens, out IExpression? binaryExp) =>
        GetExpression(tokens, out binaryExp, GetComparerType, "==", "!=", "<=", ">=", ">", "<");

    private bool GetComparerType(Tokens[] tokens, out IExpression? exp) =>
        GetStringExpression(tokens, out exp)
        || GetNotExpressions(tokens, out exp)
        || GetNumericExpression(tokens, out exp);

    private bool GetNotExpressions(Tokens[] tokens, out IExpression? exp) =>
        GetLUExpression(tokens, out exp, GetTBExpression, "!");

    private bool GetTBExpression(Tokens[] tokens, out IExpression? exp) =>
        GetTermExpression<bool>(tokens, TokenType.Bool, out exp, GetBooleanExpression);

    #endregion

    #region StringExpression

    private bool GetSumStringExpression(Tokens[] tokens, out IExpression? exp) =>
        GetExpression(tokens, out exp, GetTSExpression, "+");

    private bool GetTSExpression(Tokens[] tokens, out IExpression? exp) =>
        GetTermExpression<string>(tokens, TokenType.String, out exp, GetStringExpression);

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
        var node = new BinaryExpression(left, right!, op.GetBinaryType());
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
        var node = new BinaryExpression(left, right!, op.GetBinaryType());
        if (!ConstructExpression(tokens, node, out exp, getSubExpressions, @params))
            return GetDefault(node, out exp);
        return true;
    }

    // private bool GetRUExpression(
    //     Tokens[] tokens,
    //     out IExpression? exp,
    //     GetDelegateExpressions getExpressions,
    //     params string[] @params
    // )
    // {
    //     var startIndex = tokenIndex;
    //     if (!getExpressions(tokens, out IExpression? value))
    //         return ResetDefault(startIndex, out exp);
    //     if (!GetRUExpressions(tokens, value!, out exp, @params))
    //         return GetDefault(value!, out exp);
    //     return true;
    // }

    private bool GetLUExpression(
        Tokens[] tokens,
        out IExpression? exp,
        GetDelegateExpressions getExpressions,
        params string[] @params
    )
    {
        var startIndex = tokenIndex;
        if (getExpressions(tokens, out IExpression? value))
            return GetDefault(value!, out exp);
        if (!GetRUExpressions(tokens, value!, out exp, @params))
            return ResetDefault(startIndex, out exp);
        return true;
    }

    private bool GetRUExpressions(
        Tokens[] tokens,
        IExpression num,
        out IExpression? exp,
        params string[] @params
    )
    {
        var startIndex = tokenIndex;
        if (!MatchToken(tokens, @params, out string? op))
            return ResetDefault(startIndex, out exp);
        var node = new UnaryExpression(num, op!.GetUnaryType());
        if (!GetRUExpressions(tokens, node, out exp))
            return GetDefault(node, out exp);
        return true;
    }

    private bool GetTermExpression<T>(
        Tokens[] tokens,
        TokenType type,
        out IExpression? termExp,
        GetDelegateExpressions getExpressions
    )
        where T : IParsable<T>
    {
        var startIndex = tokenIndex;
        var token = tokens[tokenIndex++];
        if (token.Type == type && T.TryParse(token.Identifier, null, out T? num1))
            return GetDefault(
                new ValueExpression(new Values(typeof(T).ToValueType(), num1)),
                out termExp
            );
        if (GetCallFunction(tokens, out IExpression? function))
            return GetDefault(function, out termExp);
        if (token.Type == TokenType.Identifier)
            return GetDefault(new VariableExpression(token.Identifier), out termExp);
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

    private bool AssingDefault(string name, IExpression? value, out IInstruction? exp)
    {
        exp = new Assign(name, value!);
        return true;
    }

    private bool ActionDefault(string action, IExpression?[] values, out IInstruction? exp)
    {
        exp = new ActionInstruction(action, values!);
        return true;
    }

    private bool FunctDefault(string name, IExpression?[] values, out IExpression? exp)
    {
        exp = new FuncExpresion(name, values!);
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
