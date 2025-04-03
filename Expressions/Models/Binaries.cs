using Expressions.Interfaces;

namespace Expressions.Models;

public abstract class BinaryExpression<T>(Expression<T> left, Expression<T> right) : Expression<T>
{
    public IExpression<T> Left { get; protected set; } = left;
    public IExpression<T> Right { get; protected set; } = right;
}
