using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        if (go.transform.childCount > 0)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                go.transform.GetChild(i).gameObject.SetLayerRecursively(layer);
            }
        }
        go.layer = layer;
    }

    public static void CopyTransformFrom(this Transform t1, Transform t2)
    {
        t1.position = t2.position;
        t1.rotation = t2.rotation;
        t1.localScale = t2.localScale;
    }

    /// <summary>
    /// Returns a Vector3 with the same value in x, y and z
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Vector2 NewUniformVector2(this Vector3 vec, float val)
    {
        return new Vector2(val, val);
    }

    /// <summary>
    /// Returns a Vector3 with the same value in x, y and z
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Vector3 NewUniformVector3(this Vector3 vec, float val)
    {
        return new Vector3(val, val, val);
    }

    public static Vector2 NewRandomVector2(this Vector3 vec)
    {
        return new Vector2(Random.value, Random.value);
    }

    public static Vector2 NewRandomVector3(this Vector3 vec)
    {
        return new Vector3(Random.value, Random.value, Random.value);
    }

    /// <summary>
    /// Returns an opaque Colour with the same value in r, g and b
    /// </summary>
    /// <param name="color"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Color NewUniformColor(this Color color, float val)
    {
        return new Color(val, val, val);
    }

    /// <summary>
    /// Returns a translucent Colour with the same value in r, g and b
    /// </summary>
    /// <param name="color"></param>
    /// <param name="val"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static Color NewUniformColor(this Color color, float val, float alpha)
    {
        return new Color(val, val, val, alpha);
    }
    
    /// <summary>
    /// Returns true if string doesn't have anything to use
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsNullEmptyOrWhiteSpace(this string input)
    {
        return string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input);
    }
}
