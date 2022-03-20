#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BrainFailProductions.PolyFew
{
    public class PolyfewMenu : MonoBehaviour
    {


        [MenuItem("Window/Brainfail Products/PolyFew/Enable Auto UI Attaching", false, 0)]
        static void EnableAutoUIAttaching()
        {
            //EditorPrefs.DeleteKey("polyfewAutoAttach");return;
            EditorPrefs.SetBool("polyfewAutoAttach", true);
            InspectorAttacher.AttachInspector();
        }

        
        [MenuItem("Window/Brainfail Products/PolyFew/Disable Auto UI Attaching", false, 1)]
        static void DisableAutoUIAttaching()
        {
            EditorPrefs.SetBool("polyfewAutoAttach", false);
        }


        [MenuItem("Window/Brainfail Products/PolyFew/Attach PolyFew to Object", false, 2)]
        static void AttachPolyFewToObject()
        {
            EditorPrefs.SetBool("polyfewAutoAttach", false);
            InspectorAttacher.AttachInspector();
        }



        //[MenuItem("Window/Brainfail Products/PolyFew/Cleanup Missing Scripts")]
        //static void CleanupMissingScripts()
        //{
        //    int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;

        //    for (int a = 0; a < UnityEngine.SceneManagement.SceneManager.sceneCount; a++)
        //    {

        //        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(a);

        //        var rootGameObjects = scene.GetRootGameObjects();

        //        if(rootGameObjects != null && rootGameObjects.Length > 0)
        //        {

        //            List<GameObject> allObjectsinScene = new List<GameObject>();


        //            EditorUtility.DisplayProgressBar("Preprocessing", $"Fetching GameObjects in active scene \"{scene.name}\"", 0);

        //            foreach (var gameObject in rootGameObjects)
        //            {
        //                var childObjects = gameObject.GetComponentsInChildren<Transform>();

        //                if(childObjects != null && childObjects.Length > 0)
        //                {
        //                    foreach(var obj in childObjects)
        //                    {
        //                        if (obj != null) { allObjectsinScene.Add(obj.gameObject); }
        //                    }         
        //                }

        //            }

        //            EditorUtility.ClearProgressBar();


        //            for (int b = 0; b < allObjectsinScene.Count; b++)
        //            {

        //                var gameObject = allObjectsinScene[b];

        //                EditorUtility.DisplayProgressBar("Removing missing script references", $"Inspecting GameObject  {b+1}/{allObjectsinScene.Count} in active scene \"{scene.name}\"", (float)(b) / allObjectsinScene.Count);

        //                // We must use the GetComponents array to actually detect missing components
        //                var components = gameObject.GetComponents<Component>();

        //                // Create a serialized object so that we can edit the component list
        //                var serializedObject = new SerializedObject(gameObject);
        //                // Find the component list property
        //                var prop = serializedObject.FindProperty("m_Component");

        //                // Track how many components we've removed
        //                int r = 0;

        //                // Iterate over all components
        //                for (int c = 0; c < components.Length; c++)
        //                {
        //                    // Check if the ref is null
        //                    if (components[c] == null)
        //                    {
        //                        // If so, remove from the serialized component array
        //                        prop.DeleteArrayElementAtIndex(c - r);
        //                        // Increment removed count
        //                        r++;
        //                    }
        //                }

        //                // Apply our changes to the game object
        //                serializedObject.ApplyModifiedProperties();

        //            }

        //            EditorSceneManager.MarkSceneDirty(scene);

        //            EditorUtility.ClearProgressBar();
        //        }

        //        EditorUtility.ClearProgressBar();
        //    }

        //    EditorUtility.ClearProgressBar();

        //    EditorUtility.DisplayDialog("Operation Completed", "Successfully removed missing script references. Please save all currently open scenes to keep these changes persistent", "Ok");

        //}



#if UNITY_2019_1_OR_NEWER

        [MenuItem("Window/Brainfail Products/PolyFew/Cleanup Missing Scripts")]
        static void CleanupMissingScripts()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;

            for (int a = 0; a < UnityEngine.SceneManagement.SceneManager.sceneCount; a++)
            {

                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(a);

                var rootGameObjects = scene.GetRootGameObjects();

                if (rootGameObjects != null && rootGameObjects.Length > 0)
                {

                    List<GameObject> allObjectsinScene = new List<GameObject>();


                    EditorUtility.DisplayProgressBar("Preprocessing", $"Fetching GameObjects in active scene \"{scene.name}\"", 0);

                    foreach (var gameObject in rootGameObjects)
                    {
                        var childObjects = gameObject.GetComponentsInChildren<Transform>();

                        if (childObjects != null && childObjects.Length > 0)
                        {
                            foreach (var obj in childObjects)
                            {
                                if (obj != null) { allObjectsinScene.Add(obj.gameObject); }
                            }
                        }

                    }

                    EditorUtility.ClearProgressBar();


                    for (int b = 0; b < allObjectsinScene.Count; b++)
                    {

                        var gameObject = allObjectsinScene[b];

                        EditorUtility.DisplayProgressBar("Removing missing script references", $"Inspecting GameObject  {b + 1}/{allObjectsinScene.Count} in active scene \"{scene.name}\"", (float)(b) / allObjectsinScene.Count);

                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    }

                    EditorSceneManager.MarkSceneDirty(scene);

                    EditorUtility.ClearProgressBar();
                }

                EditorUtility.ClearProgressBar();
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("Operation Completed", "Successfully removed missing script references. Please save all currently open scenes to keep these changes persistent", "Ok");

        }

#endif 


        public static bool IsAutoAttachEnabled()
        {
            bool isAutoAttach;

            if (!EditorPrefs.HasKey("polyfewAutoAttach"))
            {
                EditorPrefs.SetBool("polyfewAutoAttach", true);
                isAutoAttach = true;
            }
            else
            {
                isAutoAttach = EditorPrefs.GetBool("polyfewAutoAttach");
            }

            return isAutoAttach;
        }


        #region VALIDATORS

        [MenuItem("Window/Brainfail Products/PolyFew/Enable Auto UI Attaching", true)]
        static bool CheckEnableAttachingButton()
        {
            bool isAutoAttach = IsAutoAttachEnabled();

            if (isAutoAttach) { return false; }
            else { return true; }
        }

        [MenuItem("Window/Brainfail Products/PolyFew/Disable Auto UI Attaching", true)]
        static bool CheckDisableAttachingButton()
        {
            bool isEnableButtOn  = CheckEnableAttachingButton();

            if (isEnableButtOn) { return false; }
            else { return true; }
        }

        #endregion VALIDATORS
    }

}

#endif
