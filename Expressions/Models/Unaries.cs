using Expressions.Interfaces;

namespace Expressions.Models;

public abstract class UnaryExpression<T>(Expression<T> expression) : Expression<T>
{
    public IExpression<T> Expression { get; protected set; } = expression;
}

public class Factorial(Expression<long> expression) : UnaryExpression<long>(expression)
{
    public override long Accept() => Fact(Expression.Accept());

    protected static long Fact(long num)
    {
        if (num < 0)
            throw new InvalidOperationException();
        long result = 1;
        while (num > 1)
            result *= num--;
        return result;
    }
}
