using System;
using System.Collections.Generic;

namespace AzureLib.Base;

public static class DictionaryExtensions
{
    public static IList<KeyValuePair<TKey, TValue>> RemoveWhere<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary, 
        Func<TKey, TValue, bool> matcher)
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));
        ArgumentNullException.ThrowIfNull(matcher, nameof(matcher));

        List<KeyValuePair<TKey, TValue>> matchesKeys = new List<KeyValuePair<TKey, TValue>>();
        foreach (KeyValuePair<TKey, TValue> item in dictionary)
        {
            if (matcher(item.Key, item.Value))
            {
                matchesKeys.Add(item);
            }
        }

        matchesKeys.ForEach(item => dictionary.Remove(item.Key));
        return matchesKeys;
    }
}
