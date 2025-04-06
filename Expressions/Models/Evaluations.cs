using Expressions.Interfaces;

namespace Expressions.Models;

public class FuncExpresion<T>(Func<dynamic[], T> func, IExpression<dynamic>[] @params)
    : Expression<T>
    where T : notnull
{
    public override T Accept(IExpressionsVisitor visitor) =>
        visitor.FuncVisit(func, [.. @params.Select(x => x.Accept(visitor))]);
}

public class ValueExpression<T>(T value) : Expression<T>, IExpression<T>
    where T : notnull
{
    public override T Accept(IExpressionsVisitor visitor) => visitor.ValueVisit(value);
}
