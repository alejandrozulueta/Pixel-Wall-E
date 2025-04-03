using Expressions.Interfaces;
using Expressions.Models;

namespace Expressions.Extensions;

public static class ExpressionExt
{
    public class ExpressionConvert<T>(IExpression<T> expression)
        : Expression<object?>,
            IExpression<object?>
    {
        private readonly IExpression<T> expression = expression;

        public override object? Accept() => expression.Accept();
    }

    public static IExpression<object?> ConvertToObject<T>(this IExpression<T> expression) =>
        new ExpressionConvert<T>(expression);
}
