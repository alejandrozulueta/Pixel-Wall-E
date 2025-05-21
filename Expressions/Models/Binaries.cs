using Expressions.Interfaces;

namespace Expressions.Models;

public class BinaryExpression(IExpression left, IExpression right, BinaryTypes type) : Expression
{
    public override Values Accept(IExpressionsVisitor visitor) =>
        visitor.BinaryVisit(left.Accept(visitor), right.Accept(visitor), type);
}
