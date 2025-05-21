using Expressions.Interfaces;

namespace Expressions.Models;

public class Values(ValueType type, dynamic? value)
{
    public ValueType Type { get; set; } = type;
    public dynamic? Value { get; set; } = value;
}
