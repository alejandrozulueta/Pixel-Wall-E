using Expressions.Models;

namespace Expressions.Interfaces;

public interface IInstruction
{
    void Accept(IExpressionsVisitor visitor);
}

public interface IExpression : IInstruction
{
    new Values Accept(IExpressionsVisitor visitor);
}
