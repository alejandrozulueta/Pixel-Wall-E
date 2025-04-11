using Expressions.Interfaces;

namespace Expressions.Models;

public class Assign<T>(string name, IExpression<T> expression) : Expression
    where T : notnull
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.AssingVisit(name, expression.Accept(visitor));
}

public class ActionExpresion(Action<dynamic[]> action, IExpression<dynamic>[] @params) : Expression
{
    public override void Accept(IExpressionsVisitor visitor) =>
        visitor.ActionVisit(action, [.. @params.Select(x => x.Accept(visitor))]);
}

public class BlockExpression(IExpression[] expressions) : Expression
{
    public override void Accept(IExpressionsVisitor visitor) => visitor.BlockVisit(expressions);
}
