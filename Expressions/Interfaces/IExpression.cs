namespace Expressions.Interfaces;

public interface IExpression
{
    void Accept(IExpressionsVisitor visitor);
}

public interface IExpression<T> : IExpression
{
    new T Accept(IExpressionsVisitor visitor);
}
