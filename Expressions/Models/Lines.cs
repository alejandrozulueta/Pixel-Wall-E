using Core.Models;
using Expressions.Interfaces;

namespace Expressions.Models;

public class Assign(string name, IExpression expression, Location location) : Instruction(location)
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.AssingVisit(name, expression.Accept(visitor));
}

public class ActionInstruction(string action, IExpression[] @params, Location location) : Instruction(location)
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.ActionVisit(action, [.. @params.Select(x => x.Accept(visitor))], Location);
}

public class BlockInstruction(IInstruction[] expressions, Location location) : Instruction(location)
{
    public override void Accept(IExpressionsVisitor visitor) => visitor.BlockVisit(expressions);
}

public class LabelExpression(string identifier, int index, Location location) : Instruction(location), IInstruction
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.LabelVisit(identifier, index, Location);
}

public class GotoExpression(string labelName, IExpression cond, Location location) : Instruction(location), IInstruction
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.GotoVisit(labelName, cond.Accept(visitor), Location);

}
