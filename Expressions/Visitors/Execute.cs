using Core.Exceptions;
using Core.Models;
using Expressions.Interfaces;
using Expressions.Models;
using System;
using System.Reflection;
using System.Xml.Linq;

namespace Expressions.Visitors;

public class Execute(Context Context) : IExpressionsVisitor
{
    private bool gotoFlag;
    private string? targetLabel;

    public void Visit(IInstruction node) => node.Accept(this);

    public Values BinaryVisit(Values operand1, Values operand2, BinaryTypes opType, Location location) =>
        opType switch
        {
            BinaryTypes.Sum => operand1 + operand2,
            BinaryTypes.Sub => operand1 - operand2,
            BinaryTypes.Mult => operand1 * operand2,
            BinaryTypes.Div => operand1 / operand2,
            BinaryTypes.Modul => operand1 % operand2,
            BinaryTypes.And => operand1 & operand2,
            BinaryTypes.Or => operand1 | operand2,
            BinaryTypes.Pow => operand1 ^ operand2,
            BinaryTypes.Equal => operand1 == operand2,
            BinaryTypes.Inequal => operand1 != operand2,
            BinaryTypes.Less => operand1 < operand2,
            BinaryTypes.Greater => operand1 > operand2,
            BinaryTypes.LessEqual => operand1 <= operand2,
            BinaryTypes.GreaterEqual => operand1 >= operand2,
            _ => throw new NotImplementedException(),
        };

    public Values UnaryVisit(Values operand, UnaryTypes opType, Location location)
    {
        return opType switch
        {
            UnaryTypes.Not => !operand,
            UnaryTypes.Neg => -operand,
            _ => throw new NotImplementedException(),
        };
    }

    public Values ValueVisit(Values value) => value;

    public void AssingVisit(string name, Values value) =>
        Context.CurrentScope!.Variables[name] = value;

    public Values VariableVisit(string name, Location location) =>
        Context.CurrentScope!.TryGetVariable(name, out Values? value)
            ? value!
            : throw new Exception();

    public void ActionVisit(string action, Values[] value, Location location) 
    {
        try
        {
            Context.GetAction(action)(value);
        }

        catch (TargetInvocationException ex)
        { 
            throw new Exception(ex.InnerException!.Message);
        }
    }

    public Values FuncVisit(string func, Values[] value, Location location)
    {
        try
        {
            return Context.GetFunction(func)(value); 
        }

        catch (TargetInvocationException ex)
        {
            throw new Exception(ex.InnerException!.Message);
        }
    }

    public void GotoVisit(string label, Values cond, Location location) =>
        targetLabel = (gotoFlag = cond.Value ?? false) ? label : null;

    public void LabelVisit(string label, int index, Location location) => Context.CurrentScope!.Labels[label] = index;

    public void BlockVisit(IInstruction[] expressions)
    {
        Context.PushScope();
        SearchLabel(expressions);
        for (int i = 0; i < expressions.Length; i++)
        {
            try
            {
                expressions[i].Accept(this);
            }

            catch (Exception ex)
            {
                throw new ExceptionWL(ex.Message, expressions[i].Location);
            }

            if (!gotoFlag)
                continue;
            if (!Context.CurrentScope!.TryGetLabel(targetLabel!, out int temp))
                break;
            ResetGotoFlag(temp, out i);
        }
        Context.PopScope();
    }

    public void ResetGotoFlag(int gotoIndex, out int index)
    {
        index = gotoIndex;
        gotoFlag = false;
        targetLabel = null;
    }

    private void SearchLabel(IInstruction[] expressions)
    {
        foreach (var item in expressions)
        {
            if (item is not LabelExpression label)
                continue;
            label.Accept(this);
        }
    }
}
