//#if UNITY_EDITOR

using UnityEngine;

namespace BrainFailProductions.PolyFew
{
    public class PolyFewHost : MonoBehaviour
    {
        void Start()
        {
            if (!Application.isEditor || Application.isPlaying) { DestroyImmediate(this); }
        }
    }
}

//#endif
