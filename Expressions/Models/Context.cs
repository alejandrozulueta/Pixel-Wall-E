using Expressions.Enum;
using Expressions.Visitors;
using System.Diagnostics;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Expressions.Models;

public class Context(
    Dictionary<string, FuncInfo> functions,
    Dictionary<string, ActionInfo> actions
)
{
    public Scope? CurrentScope { get; set; } = null;
    public Dictionary<string, ActionInfo> Actions { get; set; } = actions;
    public Dictionary<string, FuncInfo> Functions { get; set; } = functions;

    public Action<Values[]> GetAction(string action)
    {
        if (Actions.TryGetValue(action, out ActionInfo? act))
            return act.Acts;

        return @params => GetFunction(action)(@params);
    }

    public Func<Values[], Values> GetFunction(string function)
    {
        if (Functions.TryGetValue(function, out FuncInfo? func))
            return func!.Func;
        throw new NotImplementedException($"Método {function} no implementado");
    }

    public ParameterInfo[] GetOParamsInfo(string name, Methods methods) 
    {
        switch (methods)
        {
            case Methods.Function:
                Functions.TryGetValue(name, out FuncInfo? func);
                return func!.Info;
            case Methods.Action:
                Actions.TryGetValue(name, out ActionInfo? act);
                Functions.TryGetValue(name, out FuncInfo? actAsFunc);
                return act != null ? act.Info : actAsFunc!.Info;
            default:
                throw new NotImplementedException();
        }
    }

    public Type GetOReturnType(string name) 
    {
        Functions.TryGetValue(name, out FuncInfo? actAsFunc);
        return actAsFunc!.ReturnType;
    }

    public void PopScope()
    {
        if (CurrentScope == null)
            return;
        CurrentScope.Reset();
        CurrentScope = CurrentScope.Parent;
    }

    public void PushScope()
    {
        CurrentScope = new Scope([], [], CurrentScope);
    }
}

public class FuncInfo(Func<Values[], Values> func, ParameterInfo[] info, Type type)
{
    public Func<Values[], Values> Func { get; set; } = func;
    public ParameterInfo[] Info { get; set; } = info;
    public Type ReturnType { get; set; } = type;
}

public class ActionInfo(Action<Values[]> acts, ParameterInfo[] info)
{
    public Action<Values[]> Acts { get; set; } = acts;
    public ParameterInfo[] Info { get; set; } = info;
}