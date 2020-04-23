using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace EmergoEntertainment
{
    public static class LinqExtensions
    {
        // taken from: https://www.codeproject.com/Tips/494499/Implementing-Dictionary-RemoveAll
        public static void RemoveAll<K, V>(this IDictionary<K, V> dict, Func<K, V, bool> match)
        {
            foreach (var key in dict.Keys.ToArray()
                    .Where(key => match(key, dict[key])))
                dict.Remove(key);
        }
    }
}