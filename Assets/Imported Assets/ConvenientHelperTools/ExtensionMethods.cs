using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// Returns a Vector3 with the same value in x, y and z
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Vector3 NewUniformVector(this Vector3 vec, float val)
    {
        return new Vector3(val, val, val);
    }

    /// <summary>
    /// Returns a Vector3 with the same value in x, y and z
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Vector3 NewUniformVector(this Vector3 vec, int val)
    {
        return new Vector3(val, val, val);
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
}
