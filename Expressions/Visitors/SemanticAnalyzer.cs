using Expressions.Interfaces;
using Expressions.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressions.Visitors
{
    public class SemanticAnalyzer(Context Context) : IExpressionsVisitor
    {
        List<Exception> Exceptions = [];
        private bool gotoFlag;
        private string? targetLabel;
        private string? message;

        public void Visit(IInstruction node) => node.Accept(this);

        public Values BinaryVisit(Values operand1, Values operand2, BinaryTypes opType) 
        {
            
            if(operand1.Type == ValueType.InvalidType || operand2.Type == ValueType.InvalidType) 
            { 
                return new Values(ValueType.InvalidType);
            }

            if(operand1.Type != operand2.Type) 
            {
                message = $"Operación no válida entre {operand1.Type} y {operand2.Type}";
                Exceptions.Add(new InvalidOperationException(message));
                return new Values(ValueType.InvalidType);
            }

            message = $"Operación {opType} no válida entre {operand1.Type}s";

            switch (opType) 
            {
                case BinaryTypes.Sum or BinaryTypes.Sub or BinaryTypes.Mult or BinaryTypes.Div:
                case BinaryTypes.Modul or BinaryTypes.Pow:
                    if(operand1.Type is not ValueType.Double) 
                    {
                        Exceptions.Add(new InvalidOperationException(message));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Double);

                case BinaryTypes.And or BinaryTypes.Or:
                    if(operand1.Type is not ValueType.Bool) 
                    {
                        Exceptions.Add(new InvalidOperationException(message));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Bool);

                case BinaryTypes.Equal or BinaryTypes.Inequal:
                    return new Values(ValueType.Bool);

                case BinaryTypes.Less or BinaryTypes.Greater or BinaryTypes.LessEqual or BinaryTypes.GreaterEqual:
                    if(operand1.Type is not ValueType.Double) 
                    {
                        Exceptions.Add(new InvalidOperationException(message));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Bool);

                default:
                    throw new NotImplementedException();
            }

        }

        public Values UnaryVisit(Values operand, UnaryTypes opType)
        {
            message = $"Operación {opType} no válida con {operand.Type}";

            switch (opType) 
            { 
                case UnaryTypes.Not:
                    if (operand.Type is not ValueType.Bool) 
                    { 
                        Exceptions.Add(new InvalidOperationException (message));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Bool);

                case UnaryTypes.Neg:
                    if (operand.Type is not ValueType.Double)
                    {
                        Exceptions.Add(new InvalidOperationException(message));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Double);

                default:
                    throw new NotImplementedException();
            }

        }

        public Values ValueVisit(Values value) => value;

        public void AssingVisit(string name, Values value) =>
            Context.CurrentScope!.Variables[name] = value;

        public Values VariableVisit(string name)
        {
            if(!Context.CurrentScope!.TryGetVariable(name, out Values? value)) 
            {
                message = $"El nombre {name} no existe en el contexto actual";
                Exceptions.Add (new InvalidOperationException (message));
                return new Values(ValueType.InvalidType);
            }

            return new Values(value!.Type, value);
        }

        public void ActionVisit(string action, Values[] value) => Context.GetAction(action)(value);

        public Values FuncVisit(string func, Values[] value) => Context.GetFunction(func)(value);

        public void GotoVisit(string label, Values cond) =>
            targetLabel = (gotoFlag = cond.Value ?? false) ? label : null;

        public void LabelVisit(string label, int index) => Context.CurrentScope!.Labels[label] = index;

        public void BlockVisit(IInstruction[] expressions)
        {
            Context.PushScope();
            SearchLabel(expressions);
            for (int i = 0; i < expressions.Length; i++)
            {
                expressions[i].Accept(this);
                if (!gotoFlag)
                    continue;
                if (!Context.CurrentScope!.TryGetLabel(targetLabel!, out int temp))
                    break;
                ResetGotoFlag(temp, out i);
            }
            Context.PopScope();
        }

        public bool GetExceptions(out List<Exception>? exceptions) 
        { 
            if(Exceptions.Count > 0) 
            {
                exceptions = Exceptions;
                return false; 
            }

            exceptions = null;
            return true;
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
}
