using Expressions.Interfaces;
using Expressions.Models;

namespace Parser.Models;

public class Parser
{
    private int tokenIndex;

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
        if (GetNumericExpression(tokens, out IExpression<int>? num))
            return AssingDefault(tokens[startIndex].Identifier, num, out exp);
        if (GetBooleanExpression(tokens, out IExpression<bool> @bool))
            return AssingDefault(tokens[startIndex].Identifier, @bool, out exp);
        if (GetStringExpression(tokens, out IExpression<string> value))
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

    private bool GetNumericExpression(Tokens[] tokens, out IExpression<int>? num)
    {
        var startIndex = tokenIndex;
        if (GetBinaryNumericExpression(tokens, out IExpression<int>? BinaryExp))
            return GetDefault(BinaryExp!, out num);
        if (GetUnaryNumericExpression(tokens, out IExpression<int>? UnaryExp))
            return GetDefault(UnaryExp!, out num);
        return ResetDefault(startIndex, out num);
    }

    private bool GetBooleanExpression(Tokens[] tokens, out IExpression<bool> @bool)
    {
        throw new NotImplementedException();
    }

    private bool GetStringExpression(Tokens[] tokens, out IExpression<string> value)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region NumericExpressions

    private bool GetBinaryNumericExpression(Tokens[] tokens, out IExpression<int>? binaryExp)
    {
        return GetSumExpressions(tokens, out binaryExp);
    }

    private bool GetSumExpressions(Tokens[] tokens, out IExpression<int>? binaryExp)
    {
        var startIndex = tokenIndex;
        if (!GetMultExpressions(tokens, out IExpression<int>? num1))
            return ResetDefault(startIndex, out binaryExp);
        if (GetSumExpression(tokens, num1!, out IExpression<int>? sum))
            return GetDefault(sum!, out binaryExp);
        return GetDefault(num1!, out binaryExp);
    }

    private bool GetMultExpressions(Tokens[] tokens, out IExpression<int>? multExp)
    {
        var startIndex = tokenIndex;
        if (!GetPowExpressions(tokens, out IExpression<int>? num1))
            return ResetDefault(startIndex, out multExp);
        if (GetMulExpression(tokens, num1!, out IExpression<int>? mult))
            return GetDefault(mult!, out multExp);
        return GetDefault(num1!, out multExp);
    }

    private bool GetPowExpressions(Tokens[] tokens, out IExpression<int>? powExp)
    {
        var startIndex = tokenIndex;
        if (!GetUnaryNumericExpression(tokens, out IExpression<int>? num1))
            return ResetDefault(startIndex, out powExp);
        if (GetPowExpression(tokens, num1!, out IExpression<int>? pow))
            return GetDefault(pow!, out powExp);
        return GetDefault(num1!, out powExp);
    }

    private bool GetSumExpression(Tokens[] tokens, IExpression<int> left, out IExpression<int>? sum)
    {
        var startIndex = tokenIndex;
        var opToken = tokens[tokenIndex++];
        if (opToken.Type != TokenType.BinaryOperator || opToken.Identifier != "+")
            return ResetDefault(startIndex, out sum);
        if (!GetSumExpressions(tokens, out IExpression<int>? right))
            return ResetDefault(startIndex, out sum);
        var node = new BinaryExpression<int>(left, right!, BinaryTypes.Sum);
        return GetDefault(node, out sum);
    }

    private bool GetMulExpression(Tokens[] tokens, IExpression<int> left, out IExpression<int>? mul)
    {
        var startIndex = tokenIndex;
        var opToken = tokens[tokenIndex++];
        if (opToken.Type != TokenType.BinaryOperator || opToken.Identifier != "*")
            return ResetDefault(startIndex, out mul);
        if (!GetMultExpressions(tokens, out IExpression<int>? right))
            return ResetDefault(startIndex, out mul);
        var node = new BinaryExpression<int>(left, right!, BinaryTypes.Mult);
        return GetDefault(node, out mul);
    }

    private bool GetPowExpression(Tokens[] tokens, IExpression<int> left, out IExpression<int>? pow)
    {
        var startIndex = tokenIndex;
        var opToken = tokens[tokenIndex++];
        if (opToken.Type != TokenType.BinaryOperator || opToken.Identifier != "^")
            return ResetDefault(startIndex, out pow);
        if (!GetPowExpressions(tokens, out IExpression<int>? right))
            return ResetDefault(startIndex, out pow);
        var node = new BinaryExpression<int>(left, right!, BinaryTypes.Pow);
        return GetDefault(node, out pow);
    }

    private bool GetUnaryNumericExpression(Tokens[] tokens, out IExpression<int>? exp)
    {
        return GetRUNExpression(tokens, out exp);
    }

    private bool GetRUNExpression(Tokens[] tokens, out IExpression<int>? exp)
    {
        var startIndex = tokenIndex;
        if (!GetLUNExpression(tokens, out IExpression<int>? value))
            return ResetDefault(startIndex, out exp);
        if (!GetRUNExpressions(tokens, value!, out exp))
            return GetDefault(value!, out exp);
        return true;
    }

    private bool GetLUNExpression(Tokens[] tokens, out IExpression<int>? exp)
    {
        var startIndex = tokenIndex;
        if (GetTermExpression(tokens, out IExpression<int>? value))
            return GetDefault(value!, out exp);
        if (!GetLUNExpressions(tokens, value!, out exp))
            return ResetDefault(startIndex, out exp);
        return true;
    }

    private bool GetRUNExpressions(Tokens[] tokens, IExpression<int> num, out IExpression<int>? exp)
    {
        var startIndex = tokenIndex;
        var token = tokens[tokenIndex++];
        if (token.Type != TokenType.UnaryOperator || token.Identifier != "!")
            return ResetDefault(startIndex, out exp);
        var node = new UnaryExpression<int>(num, UnaryTypes.Factorial);
        if (!GetRUNExpressions(tokens, node, out exp))
            return GetDefault(node, out exp);
        return true;
    }

    private bool GetLUNExpressions(Tokens[] tokens, IExpression<int> num, out IExpression<int>? exp)
    {
        var startIndex = tokenIndex;
        var token = tokens[tokenIndex++];
        if (token.Type != TokenType.UnaryOperator || token.Identifier != "-")
            return ResetDefault(startIndex, out exp);
        var node = new UnaryExpression<int>(num, UnaryTypes.Neg);
        if (!GetRUNExpressions(tokens, node, out exp))
            return GetDefault(node, out exp);
        return true;
    }

    private bool GetTermExpression(Tokens[] tokens, out IExpression<int>? termExp)
    {
        var startIndex = tokenIndex;
        var token = tokens[tokenIndex++];
        if (int.TryParse(token.Identifier, out int num1))
            return GetDefault(new ValueExpression<int>(num1), out termExp);
        if (token.Type == TokenType.Identifier)
            return GetDefault(new VariableExpression<int>(token.Identifier), out termExp);
        if (token.Type != TokenType.OpenParenthesis)
            return ResetDefault(startIndex, out termExp);
        if (!GetBinaryNumericExpression(tokens, out termExp))
            return ResetDefault(startIndex, out termExp);
        if (tokens[tokenIndex++].Type != TokenType.CloseParenthesis)
            return ResetDefault(startIndex, out termExp);
        return true;
    }

    #endregion

    #region Default Methods

    private bool AssingDefault<T>(string name, IExpression<T>? value, out IExpression? exp)
        where T : notnull
    {
        exp = new Assign<T>(name, value!);
        return true;
    }

    private bool GetDefault<T>(IExpression<T> value, out IExpression<T> exp)
        where T : notnull
    {
        exp = value;
        return true;
    }

    private bool ResetDefault(int index, out IExpression? exp)
    {
        tokenIndex = index;
        exp = null;
        return false;
    }

    private bool ResetDefault<T>(int index, out IExpression<T>? exp)
    {
        tokenIndex = index;
        exp = null;
        return false;
    }

    #endregion
}
