using Core.Models;
using Expressions.Models;

namespace Expressions.Interfaces;

public interface IInstruction
{
    public Location Location { get; set; }
    void Accept(IExpressionsVisitor visitor);
}

public interface IExpression : IInstruction
{
    new Values Accept(IExpressionsVisitor visitor);
}
