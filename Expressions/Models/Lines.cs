using Expressions.Interfaces;

namespace Expressions.Models;

public class Assign(string name, IExpression expression) : Instruction
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.AssingVisit(name, expression.Accept(visitor));
}

public class ActionInstruction(string action, IExpression[] @params) : Instruction
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.ActionVisit(action, [.. @params.Select(x => x.Accept(visitor))]);
}

public class BlockInstruction(IInstruction[] expressions) : Instruction
{
    public override void Accept(IExpressionsVisitor visitor) => visitor.BlockVisit(expressions);
}

public class LabelExpression(string identifier, int index) : Instruction, IInstruction
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.LabelVisit(identifier, index);
}

public class GotoExpression(string labelName, IExpression cond) : IInstruction
{
    public void Accept(IExpressionsVisitor visitor) =>
        visitor.GotoVisit(labelName, cond.Accept(visitor));
}
