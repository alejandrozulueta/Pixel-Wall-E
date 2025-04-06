using Expressions.Interfaces;

namespace Expressions.Models;

public abstract class Expression : IExpression
{
    public abstract void Accept(IExpressionsVisitor visitor);
}

public abstract class Expression<T> : IExpression, IExpression<T>
{
    public abstract T Accept(IExpressionsVisitor visitor);

    void IExpression.Accept(IExpressionsVisitor visitor) => Accept(visitor);
}
