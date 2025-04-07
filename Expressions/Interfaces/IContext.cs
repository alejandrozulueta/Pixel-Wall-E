interface IContext
{
    Dictionary<string, dynamic> LocalVariables { get; }
    void Reset();
}
