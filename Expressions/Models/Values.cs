using Expressions.Interfaces;

namespace Expressions.Models;

public class Values(ValueType type, dynamic? value = null)
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
}
