using Expressions.Models;
using Core.Models; 

namespace Expressions.Interfaces;

public interface IExpressionsVisitor
{
    void Visit(IInstruction node);
    void BlockVisit(IInstruction[] expressions);
    Values UnaryVisit(Values operand, UnaryTypes opType, Location location);
    Values BinaryVisit(Values operand1, Values operand2, BinaryTypes opType, Location location);
    Values ValueVisit(Values value);
    void AssingVisit(string name, Values value);
    Values VariableVisit(string name, Location location);
    void ActionVisit(string action, Values[] value, Location location);
    Values FuncVisit(string func, Values[] value, Location location);
    void LabelVisit(string label, int index, Location location);
    void GotoVisit(string label, Values cond, Location location);
}
