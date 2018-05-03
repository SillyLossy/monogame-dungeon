using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeon.Game.Common
{
    public static class DictionaryExtensions
    {

        public static TKey KeyWithMinValueFromKeySet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, ISet<TKey> set, TValue initialMin) where TValue : IComparable<TValue>
        {
            var keys = new HashSet<TKey>(dictionary.Keys);

            TKey minKey = keys.First();
            TValue minValue = initialMin;

            keys.IntersectWith(set);

            foreach (var key in keys)
            {
                TValue value = dictionary[key];
                if (minValue.CompareTo(value) < 0)
                {
                    continue;
                }
                minKey = key;
                minValue = value;
            }

            return minKey;
        }
    }
}