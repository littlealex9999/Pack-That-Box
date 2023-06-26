using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ArrayHelper<T>
{
    public static T GetRandomElement(T[] array)
    {
        if (array.Length > 0) {
            return array[Random.Range(0, array.Length)];
        }

        return default(T);
    }
}
