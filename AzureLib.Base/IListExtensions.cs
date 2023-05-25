using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureLib.Base;

public static class IListExtensions
{
    public static void ForEach<TItem>(this IList<TItem> list, Action<TItem> action)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(action);

        foreach (TItem item in list)
        {
            action(item);
        }
    }

    public static IList<TResultItem> WhereSelect<TItem, TResultItem>(
        this IList<TItem> list, 
        Predicate<TItem> predicate,
        Func<TItem, TResultItem> selector)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(selector);

        return
            list
                .Where(item => predicate(item))
                .Select(item => selector(item))
                .ToList();
    }
}
