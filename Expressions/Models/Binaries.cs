using Expressions.Interfaces;

namespace Expressions.Models;

public class BinaryExpression<T>(IExpression<T> left, IExpression<T> right, BinaryTypes type)
    : Expression<T>
    where T : notnull
{
    public override T Accept(IExpressionsVisitor visitor) =>
        visitor.BinaryVisit(left.Accept(visitor), right.Accept(visitor), type);
}

public class BinaryExpression<T, K>(IExpression<T> left, IExpression<T> right, BinaryTypes type)
    : Expression<K>
    where T : notnull
    where K : notnull
{
    public override K Accept(IExpressionsVisitor visitor) =>
        visitor.BinaryVisit<T, K>(left.Accept(visitor), right.Accept(visitor), type);
}

public enum BinaryTypes
{
    Sum,
    Sub,
    Mult,
    Div,
    Pow,
    Modul,
    And,
    Or,
    Equal,
    Inequal,
    Less,
    Greater,
    LessEqual,
    GreaterEqual,
}
