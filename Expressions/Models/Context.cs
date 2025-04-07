public class Context(
    Dictionary<string, dynamic>? variables,
    Dictionary<string, Action<dynamic[]>>? actions,
    Dictionary<string, Func<dynamic, dynamic[]>>? functions
)
{
    public Dictionary<string, dynamic> Variables { get; set; } = variables ?? [];
    public Dictionary<string, Action<dynamic[]>> Actions { get; set; } = actions ?? [];
    public Dictionary<string, Func<dynamic, dynamic[]>> Functions { get; set; } = functions ?? [];

    public void Reset()
    {
        Variables.Clear();
        Actions.Clear();
        Functions.Clear();
    }
}
