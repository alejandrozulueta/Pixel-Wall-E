using Expressions.Interfaces;

namespace Expressions.Models;

public class BinaryExpression<T>(Expression<T> left, Expression<T> right, BinaryTypes type)
    : Expression<T>
    where T : notnull
{
    public override T Accept(IExpressionsVisitor visitor) =>
        visitor.BinaryVisit(left.Accept(visitor), right.Accept(visitor), type);
}

public enum BinaryTypes
{
    Sum,
    Sub,
    Mult,
    Div,
}

public enum ComparerTypes { }
