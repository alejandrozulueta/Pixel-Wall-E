﻿using Expressions.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressions.Extensions
{
    public static class TypeExtension
    {
        public static ValueType ToValueType(this Type type)
        {
            return type switch
            {
                Type T when T == typeof(int) => ValueType.Double,
                Type T when T == typeof(double) => ValueType.Double,
                Type T when T == typeof(string) => ValueType.String,
                Type T when T == typeof(bool) => ValueType.Bool,
                Type T when T == typeof(object) => ValueType.Object,
                _ => throw new NotSupportedException($"Unsuported type: {type}")
            };
        }
    }
}
