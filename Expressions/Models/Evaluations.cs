using Expressions.Interfaces;
using Core.Models;

namespace Expressions.Models;

public class FuncExpresion(string func, IExpression[] @params, Location location) : Expression(location)
{
    public override Values Accept(IExpressionsVisitor visitor) =>
        visitor.FuncVisit(func, [.. @params.Select(x => x.Accept(visitor))], Location);
}

public class ValueExpression(Values value, Location location) : Expression(location), IExpression
{
    public override Values Accept(IExpressionsVisitor visitor) => visitor.ValueVisit(value);
}

public class VariableExpression(string name, Location location) : Expression(location), IExpression
{
    public override Values Accept(IExpressionsVisitor visitor) => visitor.VariableVisit(name, Location);
}
