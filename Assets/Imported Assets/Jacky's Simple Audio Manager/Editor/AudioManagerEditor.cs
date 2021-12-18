using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace JSAM.JSAMEditor
{
    /// <summary>
    /// Thank god to brownboot67 for his advice
    /// https://forum.unity.com/threads/custom-editor-not-saving-changes.424675/
    /// </summary>
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        AudioManager myScript;

        //static bool showAdvancedSettings;

        static bool showHowTo;

        int libraryIndex = 0;

        SerializedProperty listener;
        SerializedProperty sourcePrefab;

        SerializedProperty library;
        SerializedProperty settings;
        SerializedProperty mixer;

        List<string> excludedProperties = new List<string> { "m_Script" };

        private void OnEnable()
        {
            myScript = (AudioManager)target;

            myScript.EstablishSingletonDominance();

            listener = serializedObject.FindProperty("listener");
            excludedProperties.Add("listener");

            sourcePrefab = serializedObject.FindProperty("sourcePrefab");
            excludedProperties.Add("sourcePrefab");

            library = serializedObject.FindProperty("library");
            settings = serializedObject.FindProperty("settings");

            mixer = settings.FindPropertyRelative("Mixer");

            Application.logMessageReceived += UnityDebugLog;

            LoadLibraries();
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= UnityDebugLog;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
                    
            GUIContent blontent;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (AudioManager.Instance == myScript)
            {
                JSAMEditorHelper.BeginColourChange(Color.green);
                EditorGUILayout.LabelField("Looks good! This is the active AudioManager!", EditorStyles.label.ApplyTextAnchor(TextAnchor.MiddleCenter));
                JSAMEditorHelper.EndColourChange();
            }
            else
            {
                JSAMEditorHelper.BeginColourChange(Color.red);
                EditorGUILayout.LabelField("This is NOT the active AudioManager!", EditorStyles.boldLabel.ApplyTextAnchor(TextAnchor.MiddleCenter));
                JSAMEditorHelper.EndColourChange();
            }
            EditorGUILayout.EndVertical();

            //EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            blontent = new GUIContent("Library");
            EditorGUI.BeginChangeCheck();
            libraryIndex = EditorGUILayout.Popup(blontent, libraryIndex, AudioLibraryEditor.projectLibrariesNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                library.objectReferenceValue = AudioLibraryEditor.projectLibraries[libraryIndex];
            }
            blontent = new GUIContent(" Open ");
            if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                if (library.objectReferenceValue != null)
                {
                    JSAMSettings.Settings.SelectedLibrary = library.objectReferenceValue as AudioLibrary;
                }
                AudioLibraryEditor.Init();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            blontent = new GUIContent("Settings");
            EditorGUILayout.PropertyField(settings);
            EditorGUI.BeginDisabledGroup(settings.objectReferenceValue == null);
            blontent = new GUIContent(" Open ");
            if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                if (settings.objectReferenceValue != null)
                {
                    JSAMSettings.Settings.SelectedSettings = settings.objectReferenceValue as AudioManagerSettings;
                }
                AudioManagerSettingsEditor.Init();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(mixer);
            //EditorGUI.BeginDisabledGroup(mixer.objectReferenceValue == null);
            //blontent = new GUIContent(" Open ");
            //if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            //{
            //    Selection.activeObject = mixer.objectReferenceValue;
            //    EditorGUIUtility.PingObject(mixer.objectReferenceValue);
            //    EditorApplication.ExecuteMenuItem("Window/Audio/Audio Mixer");
            //    Selection.activeObject = target;
            //}
            //EditorGUI.EndDisabledGroup();
            //EditorGUILayout.EndHorizontal();

            DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());

            //if (myScript.GetListenerInternal() == null || showAdvancedSettings)
            {
                EditorGUILayout.PropertyField(listener);
            }

            #region Source Prefab Helper
            if (!myScript.SourcePrefabExists())
            {
                EditorGUILayout.PropertyField(sourcePrefab);

                EditorGUILayout.HelpBox("Reference to Source Prefab is missing! This prefab is required to make " +
                        "AudioManager function. Click the button below to have AudioManager reapply the default reference.", MessageType.Warning);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reapply Default AudioSource Prefab"))
                {
                    string[] GUIDs = AssetDatabase.FindAssets("Audio Channel t:GameObject");

                    GameObject fallback = null;

                    foreach (string s in GUIDs)
                    {
                        GameObject theObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(GameObject)) as GameObject;
                        if (theObject.GetComponent<AudioSource>())
                        {
                            fallback = theObject;
                            break;
                        }
                    }
                    if (fallback != null) // Check has succeeded in finding the default reference
                    {
                        sourcePrefab.objectReferenceValue = fallback;
                    }
                    else // Check has failed to turn up results
                    {
                        GameObject newPrefab = new GameObject("Audio Channel");
                        AudioSource theSource = newPrefab.AddComponent<AudioSource>();
                        theSource.rolloffMode = AudioRolloffMode.Logarithmic;
                        theSource.minDistance = 0.5f;
                        theSource.maxDistance = 7;

                        // Look for AudioManager so we can put the new prefab next to it
                        string assetPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("Audio Manager t:GameObject")[0]);
                        assetPath = assetPath.Substring(0, assetPath.LastIndexOf("/") + 1);
                        assetPath += "Audio Channel.prefab";
                        bool success = false;
                        PrefabUtility.SaveAsPrefabAsset(newPrefab, assetPath, out success);
                        if (success)
                        {
                            sourcePrefab.objectReferenceValue = newPrefab;
                            EditorUtility.DisplayDialog("Success", "AudioManager's default source prefab was missing. So a new one was recreated in it's place. " +
                                "If AudioManager doesn't immediately update with the Audio Source prefab in place, click the button again or recompile your code.", "OK");
                        }
                        DestroyImmediate(newPrefab);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (myScript.SourcePrefabExists()/* && showAdvancedSettings*/)
            {
                EditorGUILayout.PropertyField(sourcePrefab);
            }
            #endregion

            EditorGUILayout.Space();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }

            #region Quick Reference Guide
            showHowTo = EditorCompatability.SpecialFoldouts(showHowTo, "Quick Reference Guide");
            if (showHowTo)
            {
                JSAMEditorHelper.RenderHelpbox("Overview");
                JSAMEditorHelper.RenderHelpbox("This component is the backbone of the entire JSAM Audio Manager system and ideally should occupy it's own gameobject.");
                JSAMEditorHelper.RenderHelpbox("Remember to mouse over the various menu options in this and other JSAM windows to learn more about them!");
                JSAMEditorHelper.RenderHelpbox("Please ensure that you don't have multiple AudioManagers in one scene.");
                JSAMEditorHelper.RenderHelpbox(
                    "If you have any questions, suggestions or bug reports, feel free to open a new issue " +
                    "on Github repository's Issues page or send me an email directly!"
                    );

                EditorGUILayout.Space();

                JSAMEditorHelper.RenderHelpbox("Tips");
                JSAMEditorHelper.RenderHelpbox(
                    "The Github Repository is usually more up to date with bug fixes " + 
                    "than what's shown on the Unity Asset Store, so give it a look just in case!"
                    );
                JSAMEditorHelper.RenderHelpbox(
                    "Here are some helpful links, more of which can be found under/nWindows -> JSAM -> JSAM Startup"
                    );
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Report a Bug", "Click on me to go to the bug report page in a new browser window"), new GUILayoutOption[] { GUILayout.MinWidth(100) }))
                {
                    Application.OpenURL("https://github.com/jackyyang09/Simple-Unity-Audio-Manager/issues");
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Github Releases", "Click on me to check out the latest releases in a new browser window"), new GUILayoutOption[] { GUILayout.MinWidth(100) }))
                {
                    Application.OpenURL("https://github.com/jackyyang09/Simple-Unity-Audio-Manager/releases");
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Email", "You can find me at jackyyang267@gmail.com"), new GUILayoutOption[] { GUILayout.MinWidth(100) }))
                {
                    Application.OpenURL("mailto:jackyyang267@gmail.com");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorCompatability.EndSpecialFoldoutGroup();
            #endregion  
        }

        static void UnityDebugLog(string message, string stackTrace, LogType logType)
        {
            // Code from this steffen-itterheim
            // https://answers.unity.com/questions/482765/detect-compilation-errors-in-editor-script.html
            // if we receive a Debug.LogError we can assume that compilation failed
            if (logType == LogType.Error)
                EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Just hides the fancy loading bar lmao
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            EditorUtility.ClearProgressBar();
        }

        void LoadLibraries()
        {
            var libraries = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<AudioLibrary>(JSAMSettings.Settings.LibraryPath);

            AudioLibraryEditor.projectLibraries = new List<AudioLibrary>();
            AudioLibraryEditor.projectLibrariesNames = new List<string>();
            for (int i = 0; i < libraries.Count; i++)
            {
                AudioLibraryEditor.projectLibraries.Add(libraries[i]);
                AudioLibraryEditor.projectLibrariesNames.Add(libraries[i].name);
            }

            libraryIndex = AudioLibraryEditor.projectLibraries.IndexOf(library.objectReferenceValue as AudioLibrary);
        }

        [MenuItem("GameObject/Audio/JSAM/Audio Manager", false, 1)]
        public static void AddAudioManager()
        {
            AudioManager existingAudioManager = FindObjectOfType<AudioManager>();
            if (!existingAudioManager)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("Audio Manager t:GameObject")[0]);
                GameObject newManager = (GameObject)Instantiate(AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)));
                if (Selection.activeTransform != null)
                {
                    newManager.transform.parent = Selection.activeTransform;
                    newManager.transform.localPosition = Vector3.zero;
                }
                newManager.name = newManager.name.Replace("(Clone)", string.Empty);
                EditorGUIUtility.PingObject(newManager);
                Selection.activeGameObject = newManager;
                Undo.RegisterCreatedObjectUndo(newManager, "Added new AudioManager");
            }
            else
            {
                EditorGUIUtility.PingObject(existingAudioManager);
                Debug.Log("AudioManager already exists in this scene!");
            }
        }
    }
}