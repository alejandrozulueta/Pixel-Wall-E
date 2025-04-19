using Expressions.Interfaces;

namespace Expressions.Models;

public class UnaryExpression<T>(IExpression<T> expression, UnaryTypes type) : Expression<T>
    where T : notnull
{
    public override T Accept(IExpressionsVisitor visitor) =>
        visitor.UnaryVisit(expression.Accept(visitor), type);
}

public enum UnaryTypes
{
    Not,
    Neg,
}
