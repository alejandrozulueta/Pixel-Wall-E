using Expressions.Interfaces;

namespace Expressions.Models;

public class UnaryExpression(IExpression expression, UnaryTypes type) : Expression
{
    public override Values Accept(IExpressionsVisitor visitor) =>
        visitor.UnaryVisit(expression.Accept(visitor), type);
}
