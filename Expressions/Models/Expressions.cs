using Expressions.Interfaces;

namespace Expressions.Models;

public abstract class Expression : IInstruction, IExpression
{
    public abstract Values Accept(IExpressionsVisitor visitor);

    void IInstruction.Accept(IExpressionsVisitor visitor) => Accept(visitor);
}

public abstract class Instruction : IInstruction
{
    public abstract void Accept(IExpressionsVisitor visitor);
}
