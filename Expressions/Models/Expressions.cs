using Expressions.Interfaces;

namespace Expressions.Models;

public abstract class Expression : IExpression
{
    public virtual void Accept() { }
}

public abstract class Expression<T> : Expression, IExpression<T>
{
    public new abstract T Accept();
}
