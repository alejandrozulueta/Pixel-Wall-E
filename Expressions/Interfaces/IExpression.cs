namespace Expressions.Interfaces;

public interface IExpression
{
    void Accept();
}

public interface IExpression<T> : IExpression
{
    new T Accept();
}
