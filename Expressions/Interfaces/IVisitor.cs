using Expressions.Models;

namespace Expressions.Interfaces;

public interface IExpressionsVisitor
{
    void Visit(IExpression node);
    void BlockVisit(IExpression[] expressions);
    T UnaryVisit<T>(T operand, UnaryTypes opType)
        where T : notnull;
    T BinaryVisit<T>(T operand1, T operand2, BinaryTypes opType)
        where T : notnull;
    K BinaryVisit<T, K>(T operand1, T operand2, BinaryTypes type)
        where T : notnull
        where K : notnull;
    T ValueVisit<T>(T value)
        where T : notnull;
    void AssingVisit<T>(string name, T value)
        where T : notnull;
    T VariableVisit<T>(string name)
        where T : notnull;
    void ActionVisit(Action<dynamic[]> action, dynamic[] value);
    T FuncVisit<T>(Func<dynamic[], T> func, dynamic[] value)
        where T : notnull;
}
