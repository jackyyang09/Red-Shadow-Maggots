using UnityEditor;
using UnityEngine;

namespace MWU.Shared.Utilities
{
    /// <summary>
    /// Adds 'Create Empty at Root' shortcut for the GameObject / right-click context menu to create an empty gameobject at 0,0,0
    /// Original can be found here: https://gist.github.com/gekidoslair/359535dd108ce3dfac6b564ad37e42fe
    /// </summary>
    public class CreateEmptyAtRoot : MonoBehaviour
    {
        [MenuItem("GameObject/Create Empty at Root", false, 0)]
        public static void Create()
        {
            var go = new GameObject();
            go.name = "GameObject";
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(go, "Added new gameobject at root");
            Selection.activeTransform = go.transform;
        }
    }
}