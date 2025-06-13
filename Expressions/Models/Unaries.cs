using Core.Models;
using Expressions.Interfaces;

namespace Expressions.Models;

public class UnaryExpression(IExpression expression, UnaryTypes type, Location location) : Expression(location)
{
    public override Values Accept(IExpressionsVisitor visitor) =>
        visitor.UnaryVisit(expression.Accept(visitor), type, Location);
}
