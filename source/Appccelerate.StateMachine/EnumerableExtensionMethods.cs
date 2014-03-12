using System;
using System.Collections.Generic;
using System.Linq;

namespace Appccelerate.StateMachine
{
    public static class EnumerableExtensionMethods
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }
    }
}