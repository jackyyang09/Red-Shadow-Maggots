using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Doesn't persist between scenes and uses Lazy Load
/// </summary>
/// <typeparam name="T"></typeparam>
public class BasicSingleton<T> : MonoBehaviour where T : Component
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
            }
            return instance;
        }
    }
}