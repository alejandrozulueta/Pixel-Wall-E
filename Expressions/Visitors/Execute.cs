using Expressions.Interfaces;
using Expressions.Models;

namespace Expressions.Visitors;

public class Execute : IExpressionsVisitor, IContext
{
    public Dictionary<string, dynamic> LocalVariables { get; protected set; }

    public Execute() => LocalVariables = [];

    public void Reset() => LocalVariables.Clear();

    public void Visit(IExpression node) => node.Accept(this);

    public T BinaryVisit<T>(T operand1, T operand2, BinaryTypes opType)
        where T : notnull =>
        opType switch
        {
            BinaryTypes.Sum => (T)((dynamic)operand1 + (dynamic)operand2),
            BinaryTypes.Sub => (T)((dynamic)operand1 - (dynamic)operand2),
            BinaryTypes.Mult => (T)((dynamic)operand1 * (dynamic)operand2),
            BinaryTypes.Div => (T)((dynamic)operand1 / (dynamic)operand2),
            BinaryTypes.And => (T)((dynamic)operand1 && (dynamic)operand2),
            BinaryTypes.Or => (T)((dynamic)operand1 || (dynamic)operand2),
            _ => throw new NotImplementedException(),
        };

    public K BinaryVisit<T, K>(T operand1, T operand2, BinaryTypes opType)
        where T : notnull
        where K : notnull =>
        opType switch
        {
            BinaryTypes.Equal => (K)((dynamic)operand1 == (dynamic)operand2),
            BinaryTypes.Inequal => (K)((dynamic)operand1 != (dynamic)operand2),
            BinaryTypes.Less => (K)((dynamic)operand1 < (dynamic)operand2),
            BinaryTypes.Greater => (K)((dynamic)operand1 > (dynamic)operand2),
            BinaryTypes.LessEqual => (K)((dynamic)operand1 <= (dynamic)operand2),
            BinaryTypes.GreaterEqual => (K)((dynamic)operand1 >= (dynamic)operand2),
            _ => throw new NotFiniteNumberException(),
        };

    public T UnaryVisit<T>(T operand, UnaryTypes opType)
        where T : notnull
    {
        return opType switch
        {
            UnaryTypes.Not => (T)!(dynamic)operand,
            _ => throw new NotImplementedException(),
        };
    }

    public T ValueVisit<T>(T value)
        where T : notnull => value;

    public void AssingVisit<T>(string name, T value)
        where T : notnull => LocalVariables[name] = value;

    public T VariableVisit<T>(string name)
        where T : notnull => LocalVariables[name];

    public void ActionVisit(Action<dynamic[]> action, dynamic[] value) => action(value);

    public T FuncVisit<T>(Func<dynamic[], T> func, dynamic[] value)
        where T : notnull => func(value);
}
