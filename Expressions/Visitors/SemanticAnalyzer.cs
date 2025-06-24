using Core.Exceptions;
using Core.Models;
using Expressions.Enum;
using Expressions.Extensions;
using Expressions.Interfaces;
using Expressions.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Expressions.Visitors
{
    public class SemanticAnalyzer(Context Context) : IExpressionsVisitor
    {
        List<ExceptionWL> Exceptions = [];
        private string? message;

        public void Visit(IInstruction node) => node.Accept(this);

        public Values BinaryVisit(Values operand1, Values operand2, BinaryTypes opType, Location location) 
        {
            
            if(operand1.Type == ValueType.InvalidType || operand2.Type == ValueType.InvalidType) 
            { 
                return new Values(ValueType.InvalidType);
            }

            if(operand1.Type != operand2.Type) 
            {
                message = $"Operación no válida entre {operand1.Type} y {operand2.Type}";
                Exceptions.Add(new SemanticException(message, location));
                return new Values(ValueType.InvalidType);
            }

            message = $"Operación {opType} no válida entre {operand1.Type}s";

            switch (opType) 
            {
                case BinaryTypes.Sum or BinaryTypes.Sub or BinaryTypes.Mult or BinaryTypes.Div:
                case BinaryTypes.Modul or BinaryTypes.Pow:
                    if(operand1.Type is not ValueType.Double) 
                    {
                        Exceptions.Add(new SemanticException(message, location));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Double);

                case BinaryTypes.And or BinaryTypes.Or:
                    if(operand1.Type is not ValueType.Bool) 
                    {
                        Exceptions.Add(new SemanticException(message, location));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Bool);

                case BinaryTypes.Equal or BinaryTypes.Inequal:
                    return new Values(ValueType.Bool);

                case BinaryTypes.Less or BinaryTypes.Greater or BinaryTypes.LessEqual or BinaryTypes.GreaterEqual:
                    if(operand1.Type is not ValueType.Double) 
                    {
                        Exceptions.Add(new SemanticException(message, location));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Bool);

                default:
                    throw new NotImplementedException();
            }

        }

        public Values UnaryVisit(Values operand, UnaryTypes opType, Location location)
        {
            message = $"Operación {opType} no válida con {operand.Type}";

            switch (opType) 
            { 
                case UnaryTypes.Not:
                    if (operand.Type is not ValueType.Bool)
                    {
                        Exceptions.Add(new SemanticException(message, location));
                        return new Values(ValueType.InvalidType);
                    }

                    return new Values(ValueType.Bool);

                case UnaryTypes.Neg:
                    if (operand.Type is not ValueType.Double)
                    {
                        Exceptions.Add(new SemanticException(message, location));
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

        public Values VariableVisit(string name, Location location)
        {
            if(!Context.CurrentScope!.TryGetVariable(name, out Values? value)) 
            {
                message = $"El nombre {name} no existe en el contexto actual";
                Exceptions.Add (new SemanticException(message, location));
                return new Values(ValueType.InvalidType);
            }

            return new Values(value!.Type, value);
        }

        public void ActionVisit(string action, Values[] value, Location location) 
        {
            if (!Context.Actions.TryGetValue(action, out ActionInfo? _) && !Context.Functions.TryGetValue(action, out FuncInfo? _))
            {
                message = $"Método {action} no implementado";
                Exceptions.Add(new SemanticException(message, location));
                return;
            }

            ParameterInfo[] @params;

            @params = Context.GetOParamsInfo(action, Methods.Action);
            
            if (value.Length < @params.Length) 
            { 
                var param = @params[value.Length].Name;
                message = $"No se ha introducido el parámetro {param}";
                Exceptions.Add(new SemanticException(message, location));
                return;
            }

            if(value.Length > @params.Length) 
            {
                message = $"El método {action} no recibe {value.Length} parámetros";
                Exceptions.Add(new SemanticException(message, location));
                return;
            }

            for (int i = 0; i < @params.Length; i++)
            {
                var paramType = @params[i].ParameterType.ToValueType();

                if (paramType == ValueType.Object)
                    continue;

                var valueType = value[i].Type;
                if (paramType != valueType) 
                {
                    message = $"Se esperaba un tipo {paramType} y se ha introducido un tipo {valueType}";
                    Exceptions.Add(new SemanticException(message, location));
                    return;
                }
            }
        } 

        public Values FuncVisit(string func, Values[] value, Location location) 
        {
            Func<Values[], Values> funcDef;

            try
            {
                funcDef = Context.GetFunction(func);
            }

            catch (SemanticException e)
            {
                Exceptions.Add(e);
                return new Values(ValueType.InvalidType);
            }

            var info = funcDef.GetMethodInfo();
            var @params = Context.GetOParamsInfo(func, Methods.Function);
            var returnType = Context.GetOReturnType(func).ToValueType();

            if (value.Length < @params.Length)
            {
                var param = @params[value.Length].Name;
                message = $"No se ha introducido el parámetro {param}";
                return new Values(returnType);
            }

            if (value.Length > @params.Length)
            {
                message = $"El método {funcDef} no recibe {value.Length} parámetros";
                Exceptions.Add(new SemanticException(message, location));
                return new Values(returnType);
            }

            for (int i = 0; i < @params.Length; i++)
            {
                var paramType = @params[i].ParameterType.ToValueType();

                if (paramType == ValueType.Object)
                    continue;

                var valueType = value[i].Type;
                if (paramType != valueType)
                {
                    message = $"Se esperaba un tipo {paramType} y se ha introducido un tipo {valueType}";
                    Exceptions.Add(new SemanticException(message, location));
                    return new Values(returnType);
                }
            }

            return new Values(returnType);
        }

        public void GotoVisit(string label, Values cond, Location location) 
        {
            if (!Context.CurrentScope!.TryGetLabel(label, out int _))
            {
                message = $"La etiqueta {label} no existe en el contexto actual";
                Exceptions.Add(new SemanticException(message, location));
            }

            if (cond.Type != ValueType.Bool && cond.Type != ValueType.InvalidType)
            {
                message = $"Se esperaba un booleano";
                Exceptions.Add(new SemanticException(message, location));
            }
        }

        public void LabelVisit(string label, int index, Location location)
        {
            if (Context.CurrentScope!.TryGetLabel(label, out int _))
            {
                message = $"El label {label} ya existe en el contexto acual";
                Exceptions.Add(new SemanticException(message, location));
                return;
            }
            Context.CurrentScope!.Labels[label] = index;
        }

        public void BlockVisit(IInstruction[] expressions)
        {
            Context.PushScope();
            SearchLabel(expressions);
            for (int i = 0; i < expressions.Length; i++)
            {
                if (expressions[i] is LabelExpression)
                    continue;
                expressions[i].Accept(this);
            }
            Context.PopScope();
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

        public bool GetExceptions(out List<ExceptionWL>? exceptions) 
        { 
            if(Exceptions.Count > 0) 
            {
                exceptions = Exceptions;
                return false; 
            }

            exceptions = null;
            return true;
        }
    }
}
