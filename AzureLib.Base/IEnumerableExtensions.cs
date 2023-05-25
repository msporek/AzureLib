using System;
using System.Collections.Generic;

namespace AzureLib.Base;

public static class IEnumerableExtensions
{
    public static void ForEach<T>(IEnumerable<T> enumerable, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(enumerable);
        ArgumentNullException.ThrowIfNull(action);

        foreach (T item in enumerable)
        {
            action(item);
        }
    }
}
