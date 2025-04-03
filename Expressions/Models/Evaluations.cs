using Expressions.Interfaces;

namespace Expressions.Models;

public class ActionExpresion(Action<object?[]> func, IExpression<object?>[] @params) : Expression
{
    public IExpression<object?>[] Params { get; protected set; } = @params;
    public Action<object?[]> Func { get; protected set; } = func;

    public override void Accept() => Func([.. Params.Select(x => x.Accept())]);
}

public class FuncExpresion<T>(Func<object?[], T> func, IExpression<object?>[] @params)
    : Expression<T>
{
    public IExpression<object?>[] Params { get; protected set; } = @params;
    public Func<object?[], T> Func { get; protected set; } = func;

    public override T Accept() => Func([.. Params.Select(x => x.Accept())]);
}

public class ValueExpression<T>(T value) : Expression<T>, IExpression<T>
{
    public T Value { get; protected set; } = value;

    public override T Accept() => Value;
}
