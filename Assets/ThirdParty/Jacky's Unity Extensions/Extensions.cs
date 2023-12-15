using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T[] GetKeysCached<T, U>(this Dictionary<T, U> d)
    {
        T[] keys = new T[d.Keys.Count];
        d.Keys.CopyTo(keys, 0);
        return keys;
    }

    public static RectTransform GetRectTransform(this MonoBehaviour mono)
    {
        return mono.transform as RectTransform;
    }

    public static T GetLast<T>(this List<T> list)
    {
        if (list.Count == 0) return default(T);
        else return list[list.Count - 1];
    }

    public static int ToInt(this bool b)
    {
        return System.Convert.ToInt32(b);
    }

    /// <summary>
    /// Multiplies by 100 and truncates to int
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static int FormatPercentage(this float f)
    {
        return (int)(f * 100);
    }
}