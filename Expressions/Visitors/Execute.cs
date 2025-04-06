using System.Reflection;
using Expressions.Interfaces;
using Expressions.Models;

namespace Expressions.Visitors;

public class Execute : IExpressionsVisitor
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
            _ => throw new NotFiniteNumberException(),
        };

    public bool ComparerVisit<T>(T operand1, T operand2, ComparerTypes opType)
        where T : notnull
    {
        throw new NotImplementedException();
    }

    public T UnaryVisit<T>(T operand, UnaryTypes opType)
        where T : notnull
    {
        throw new NotImplementedException();
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
