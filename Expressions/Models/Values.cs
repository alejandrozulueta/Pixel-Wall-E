using Expressions.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Expressions.Models;

public class Values(ValueType type, dynamic? value = null) : IParsable<Values>
{
    public ValueType Type { get; set; } = type;
    public dynamic? Value { get; set; } = value;

    public static Values operator +(Values value1, Values value2) =>
        new(value1.Type, value1.Value + value2.Value);

    public static Values operator -(Values value1, Values value2) =>
        new(value1.Type, value1.Value - value2.Value);

    public static Values operator *(Values value1, Values value2) =>
        new(value1.Type, value1.Value * value2.Value);

    public static Values operator /(Values value1, Values value2) =>
        new(value1.Type, value1.Value / value2.Value);

    public static Values operator %(Values value1, Values value2) =>
        new(value1.Type, value1.Value % value2.Value);

    public static Values operator ^(Values value1, Values value2) =>
        new(value1.Type, Math.Pow(value1.Value, value2.Value));

    public static Values operator ==(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value == value2.Value);

    public static Values operator !=(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value != value2.Value);

    public static Values operator >(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value > value2.Value);

    public static Values operator <(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value < value2.Value);

    public static Values operator >=(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value >= value2.Value);

    public static Values operator <=(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value <= value2.Value);

    public static Values operator &(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value & value2.Value);

    public static Values operator |(Values value1, Values value2) =>
        new(ValueType.Bool, value1.Value | value2.Value);

    public static Values operator !(Values value) => new(ValueType.Bool, !value.Value);

    public static Values operator -(Values value) => new(ValueType.Double, -value.Value);

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj)
        || (obj is not null && obj is Values other && other.Type == Type && other.Value == Value);

    public override int GetHashCode() => Value?.GetHashCode() ?? default;

    public static Values Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Values result)
    {
        if (int.TryParse(s, out int num))
            result = new Values(ValueType.Double, num);
        else if (bool.TryParse(s, out bool @bool))
            result = new Values(ValueType.Bool, @bool);
        else result = new Values(ValueType.String, s);
        return true;
    }
}
