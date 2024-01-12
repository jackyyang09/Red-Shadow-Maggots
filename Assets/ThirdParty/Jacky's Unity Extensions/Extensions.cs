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

    public static RectTransform GetRectTransform(this Transform t)
    {
        return t as RectTransform;
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
    /// Thanks ChatGPT
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static string FormatPercentage(this float f)
    {
        f *= 100f;
        return f.ToString("0.0") + "%"; // Adjust the number of # symbols as needed
    }

    public static string FormatToDecimal(this float f)
    {
        return Mathf.FloorToInt(f * 100f).ToString();
    }

    /// <summary>
    /// https://forum.unity.com/threads/layoutgroup-does-not-refresh-in-its-current-frame.458446/
    /// "I am going to become the joker" - Tomsterk
    /// </summary>
    /// <param name="root"></param>
    public static void RefreshLayoutGroupsImmediateAndRecursive(this GameObject g, Transform root)
    {
        foreach (var layoutGroup in root.GetComponentsInChildren<UnityEngine.UI.LayoutGroup>())
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetRectTransform());
        }
    }
}