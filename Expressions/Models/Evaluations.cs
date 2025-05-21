using Expressions.Interfaces;

namespace Expressions.Models;

public class FuncExpresion(string func, IExpression[] @params) : Expression
{
    public override Values Accept(IExpressionsVisitor visitor) =>
        visitor.FuncVisit(func, [.. @params.Select(x => x.Accept(visitor))]);
}

public class ValueExpression(Values value) : Expression, IExpression
{
    public override Values Accept(IExpressionsVisitor visitor) => visitor.ValueVisit(value);
}

public class VariableExpression(string name) : Expression, IExpression
{
    public override Values Accept(IExpressionsVisitor visitor) => visitor.VariableVisit(name);
}
