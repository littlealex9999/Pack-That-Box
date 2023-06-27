using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ArrayHelper<T>
{
    /// <summary>
    /// Returns a random element of the given array, or the default return value if the array contains nothing.
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T GetRandomElement(T[] array)
    {
        if (array.Length > 0) {
            return array[Random.Range(0, array.Length)];
        }

        return default(T);
    }
}
