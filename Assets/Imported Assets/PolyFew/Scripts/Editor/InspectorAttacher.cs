
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;




namespace BrainFailProductions.PolyFew
{
    //[CustomEditor(typeof(GameObject))]
    public class InspectorAttacher //: DecoratorEditor
    {
        //public InspectorAttacher() : base("GameObjectInspector") { }

#pragma warning disable

        private bool resetFlags = true;
        private static HideFlags oldFlags;

        
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmoForMyScript(Transform scr, GizmoType gizmoType)
        {
            if(PolyfewMenu.IsAutoAttachEnabled())
            {
                AttachInspector();
            }        
        }
    


        public static void AttachInspector()
        {

            
            //Debug.Log("DrawGizmoForMyScript on InspectorAttacher on PolyFew HasHost?  " + (Selection.activeGameObject.GetComponent<PolyFewHost>() != null));
            if (Selection.activeGameObject == null) { return; }
            if (Selection.activeTransform == null || Selection.activeTransform is RectTransform) { return; }
            if (Selection.activeGameObject.GetComponent<PolyFewHost>() != null) { return; }
            if (Application.isPlaying) { return; }


            // Attach the inspector hosting script
            if (Selection.activeGameObject != null)
            {
                //Debug.Log("Adding hosting script to gameobject  " +Selection.activeGameObject.name);
                PolyFewHost host = Selection.activeGameObject.AddComponent(typeof(PolyFewHost)) as PolyFewHost;

                host.hideFlags = HideFlags.DontSave;

#pragma warning disable

                int moveUp = Selection.activeGameObject.GetComponents<Component>().Length - 2;


                var backup = Selection.activeGameObject.GetComponent<LODBackupComponent>();

                if (backup) { backup.hideFlags = HideFlags.HideInInspector; }

            }

        }


        void OnEnable()
        {
        }

        void OnDisable()
        {
        }


    }


}

