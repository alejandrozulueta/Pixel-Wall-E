using Expressions.Interfaces;
using Core.Models;

namespace Expressions.Models;

public abstract class Expression(Location location) : IInstruction, IExpression
{
    public Location Location { get; set; } = location;
    public abstract Values Accept(IExpressionsVisitor visitor);
    void IInstruction.Accept(IExpressionsVisitor visitor) => Accept(visitor);
}

public abstract class Instruction(Location location) : IInstruction
{
    public Location Location { get; set; } = location;
    public abstract void Accept(IExpressionsVisitor visitor);
}
