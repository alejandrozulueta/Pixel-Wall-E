using Expressions.Visitors;

namespace Expressions.Models;

public class Context(
    Dictionary<string, Action<Values[]>>? actions,
    Dictionary<string, Func<Values[], Values>>? functions
)
{
    public Scope? CurrentScope { get; set; } = null;
    public Dictionary<string, Action<Values[]>> Actions { get; set; } = actions ?? [];
    public Dictionary<string, Func<Values[], Values>> Functions { get; set; } = functions ?? [];

    public Action<Values[]> GetAction(string action)
    {
        if (Actions.TryGetValue(action, out Action<Values[]>? act))
            return act;
        return @params => GetFunction(action)(@params);
    }

    internal Func<Values[], Values> GetFunction(string function)
    {
        if (Functions.TryGetValue(function, out Func<Values[], Values>? func))
            return func;
        throw new Exception();
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
