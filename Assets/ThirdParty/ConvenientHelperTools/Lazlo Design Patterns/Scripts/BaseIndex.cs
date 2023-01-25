using UnityEngine;

namespace Lazlo
{
    public abstract class BaseIndex : ScriptableObject
    {
        protected static T GetOrLoad<T>(ref T _instance) where T : BaseIndex
        {
            if (_instance == null)
            {
                var name = typeof(T).Name;

                _instance = Resources.Load<T>(name);

                if (_instance == null)
                {
                    Debug.LogWarning($"Failed to load index: '{name}'.\nIndex file must be placed at: Resources/{name}.asset");
                }
            }

            return _instance;
        }
    }
}