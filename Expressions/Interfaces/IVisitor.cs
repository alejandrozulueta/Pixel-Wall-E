using Expressions.Models;

namespace Expressions.Interfaces;

public interface IExpressionsVisitor
{
    void Visit(IInstruction node);
    void BlockVisit(IInstruction[] expressions);
    Values UnaryVisit(Values operand, UnaryTypes opType);
    Values BinaryVisit(Values operand1, Values operand2, BinaryTypes opType);
    Values ValueVisit(Values value);
    void AssingVisit(string name, Values value);
    Values VariableVisit(string name);
    void ActionVisit(string action, Values[] value);
    Values FuncVisit(string func, Values[] value);
    void LabelVisit(string label, int index);
    void GotoVisit(string label, Values cond);
}
