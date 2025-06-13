using Core.Models;
using Expressions.Interfaces;

namespace Expressions.Models;

public class BinaryExpression(IExpression left, IExpression right, BinaryTypes type, Location location) 
    : Expression(location)
{
    public override Values Accept(IExpressionsVisitor visitor) =>
        visitor.BinaryVisit(left.Accept(visitor), right.Accept(visitor), type, Location);
}
