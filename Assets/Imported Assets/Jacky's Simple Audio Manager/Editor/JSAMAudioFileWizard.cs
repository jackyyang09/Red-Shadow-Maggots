﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using System;

namespace JSAM.JSAMEditor
{
    public enum AudioFileType
    {
        Music,
        Sound
    }

    public class JSAMAudioFileWizard : ScriptableObject
    {
        public List<AudioClip> files = new List<AudioClip>();
        public AudioFileType fileType = AudioFileType.Sound;
        public string outputFolder = "Assets";

        static string helperAssetName = "JSAMAudioFileWizard.asset";

        public static string HelperSavePath
        {
            get
            {
                string path = JSAMSettings.Settings.PackagePath;
                path += "/Editor/Preferences";
                return path;
            }
        }

        public static JSAMAudioFileWizard Settings
        {
            get
            {
                var settings = AssetDatabase.LoadAssetAtPath<JSAMAudioFileWizard>(HelperSavePath + "/" + helperAssetName);
                if (settings == null)
                {
                    if (!AssetDatabase.IsValidFolder(HelperSavePath))
                    {
                        JSAMEditorHelper.GenerateFolderStructureAt(HelperSavePath, true);
                    }
                    settings = ScriptableObject.CreateInstance<JSAMAudioFileWizard>();
                    JSAMEditorHelper.CreateAssetSafe(settings, HelperSavePath + "/" + helperAssetName);
                    AssetDatabase.SaveAssets();
                }
                return settings;
            }
        }
    }

    public class JSAMAudioFileWizardEditor : JSAMSerializedEditorWindow<JSAMAudioFileWizard, JSAMAudioFileWizardEditor>
    {
        Vector2 scroll;

        List<Preset> soundPresets = new List<Preset>();
        List<Preset> musicPresets = new List<Preset>();
        List<string> soundPresetNames;
        List<string> musicPresetNames;

        int selectedSoundPreset = 0;
        int selectedMusicPreset = 0;

        Dictionary<Preset, PropertyModification> presetToProp = new Dictionary<Preset, PropertyModification>();

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/JSAM/Audio File Wizard")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            Window.Show();
            window.SetWindowTitle();
            window.DesignateSerializedProperties();   
        }

        new protected void OnEnable()
        {
            base.OnEnable();
            DesignateSerializedProperties();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            LoadPresets();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        protected override void SetWindowTitle()
        {
            window.titleContent = new GUIContent("Audio File Wizard");
        }

        SerializedProperty files;
        SerializedProperty fileType;
        SerializedProperty outputFolder;

        protected override void DesignateSerializedProperties()
        {
            asset = JSAMAudioFileWizard.Settings;
            serializedObject = new SerializedObject(asset);

            files = FindProp(nameof(asset.files));
            fileType = FindProp(nameof(asset.fileType));
            outputFolder = FindProp(nameof(asset.outputFolder));
        }

        private void OnGUI()
        {
            serializedObject.Update();

            GUIContent blontent;

            HandleDragAndDrop();

            EditorGUILayout.BeginVertical(GUI.skin.box, new GUILayoutOption[] { GUILayout.MinHeight(70), GUILayout.MaxHeight(200) });
            scroll = EditorGUILayout.BeginScrollView(scroll);

            if (files.arraySize == 0)
            {
                blontent = new GUIContent("Drag AudioClips here");
                GUIStyle style = GUI.skin.label.ApplyTextAnchor(TextAnchor.MiddleCenter).SetFontSize(48);
                EditorGUILayout.LabelField(blontent, style, new GUILayoutOption[] { GUILayout.MinHeight(70), GUILayout.MaxHeight(200) });
            }
            else
            {
                List<AudioClip> toBeRemoved = new List<AudioClip>();
                for (int i = 0; i < files.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    var element = files.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (element == null) continue;
                    blontent = new GUIContent(element.name/*, AssetDatabase.GetAssetPath(element)*/);
                    EditorGUILayout.LabelField(blontent);

                    JSAMEditorHelper.BeginColourChange(Color.red);
                    blontent = new GUIContent("X", "Remove this AudioClip from the list");
                    if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        files.DeleteArrayElementAtIndex(i);
                        files.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                    JSAMEditorHelper.EndColourChange();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            blontent = new GUIContent("Clear Files", "Remove all files from the above list. This process can be undone with Edit -> Undo");
            if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                files.ClearArray();
            }

            blontent = new GUIContent("Audio File Type", "The type of Audio File you want to generate from these Audio Clips");
            EditorGUILayout.PropertyField(fileType, blontent);

            Preset selectedPreset = null;

            EditorGUILayout.BeginHorizontal();
            blontent = new GUIContent("Preset to Apply", "Audio File objects created through the Audio File Wizard will be created using this preset as a template.");
            // Music
            if (fileType.enumValueIndex == 0)
            {
                if (musicPresets.Count == 0)
                {
                    using (new EditorGUI.DisabledScope(true))
                        selectedMusicPreset = EditorGUILayout.Popup(blontent, selectedMusicPreset, new string[] { "<None>" });
                }
                else
                {
                    selectedMusicPreset = EditorGUILayout.Popup(blontent, selectedMusicPreset, musicPresetNames.ToArray());
                    selectedPreset = musicPresets[selectedMusicPreset];
                }
            }
            // Sound
            else
            {
                if (soundPresets.Count == 0)
                {
                    using (new EditorGUI.DisabledScope(true))
                        selectedSoundPreset = EditorGUILayout.Popup(blontent, selectedSoundPreset, new string[] { "<None>" });
                }
                else
                {
                    selectedSoundPreset = EditorGUILayout.Popup(blontent, selectedSoundPreset, soundPresetNames.ToArray());
                    selectedPreset = soundPresets[selectedSoundPreset];
                }
            }

            blontent = new GUIContent("Refresh Presets", "Reload your assets from your configured preset folder. " +
                "\nPress this button if you recently created a new Audio File preset and don't see it in the drop-down menu. " +
                "\nYou can change your preset folder location in Project Settings -> Audio - JSAM");
            if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                LoadPresets();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (selectedPreset != null)
            {
                blontent = new GUIContent(presetToProp[selectedPreset].value);
            }
            else
            {
                blontent = new GUIContent("No preset selected");
            }

            // Text shows up black for some reason? Why?
            //var skin = JSAMEditorHelper.ApplyTextAnchorToStyle(GUI.skin.box, TextAnchor.UpperLeft);

            var skin = GUI.skin.box.ApplyTextAnchor(TextAnchor.UpperLeft).SetTextColor(GUI.skin.label.normal.textColor);
            EditorGUILayout.LabelField(new GUIContent("Preset Description"), blontent, skin, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            EditorGUILayout.EndHorizontal();

            JSAMEditorHelper.SmartFolderField(outputFolder);

            blontent = new GUIContent("Generate Audio File Objects", 
                "Create audio file objects with the provided Audio Clips according to the selected preset. " +
                "Audio File objects will be saved to the output folder.");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            bool cantGenerate = files.arraySize == 0 || selectedPreset == null;
            using (new EditorGUI.DisabledScope(cantGenerate))
            {
                if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    if (fileType.enumValueIndex == 0)
                    {
                        GenerateAudioFileObjects<JSAMMusicFileObject>(selectedPreset);
                    }
                    else
                    {
                        GenerateAudioFileObjects<JSAMSoundFileObject>(selectedPreset);
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnUndoRedoPerformed()
        {
            if (serializedObject != null)
            {
                Window.Repaint();
            }
        }

        void LoadPresets()
        {
            soundPresets = new List<Preset>();
            soundPresetNames = new List<string>();

            musicPresets = new List<Preset>();
            musicPresetNames = new List<string>();

            var presets = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<Preset>(JSAMSettings.Settings.PresetsPath);

            for (int i = 0; i < presets.Count; i++)
            {
                var testMusic = CreateInstance<JSAMMusicFileObject>();
                var testSound = CreateInstance<JSAMSoundFileObject>();

                if (presets[i].CanBeAppliedTo(testMusic))
                {
                    musicPresets.Add(presets[i]);
                    musicPresetNames.Add(presets[i].name);
                }
                else if (presets[i].CanBeAppliedTo(testSound))
                {
                    soundPresets.Add(presets[i]);
                    soundPresetNames.Add(presets[i].name);
                }

                presetToProp[presets[i]] = presets[i].FindProp("presetDescription");
            }
        }

        void GenerateAudioFileObjects<T>(Preset preset) where T : BaseAudioFileObject
        {
            string folder = outputFolder.stringValue;
            for (int i = 0; i < asset.files.Count; i++)
            {
                var newObject = CreateInstance<T>();
                preset.ApplyTo(newObject);
                SerializedObject newSO = new SerializedObject(newObject);
                newSO.FindProperty("file").objectReferenceValue = asset.files[i];
                newSO.ApplyModifiedProperties();
                string finalPath = folder + "/" + asset.files[i].name + ".asset";
                JSAMEditorHelper.CreateAssetSafe(newObject, finalPath);
            }
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folder));
        }

        void HandleDragAndDrop()
        {
            Rect dragRect = Window.position;
            dragRect.x = 0;
            dragRect.y = 0;

            if (dragRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    List<AudioClip> duplicates = new List<AudioClip>();

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        string filePath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[i]);
                        var clips = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<AudioClip>(filePath);

                        for (int j = 0; j < clips.Count; j++)
                        {
                            if (asset.files.Contains(clips[j]))
                            {
                                duplicates.Add(clips[j]);
                                continue;
                            }

                            files.AddNewArrayElement().objectReferenceValue = clips[j];
                        }
                    }

                    Event.current.Use();

                    if (duplicates.Count > 0)
                    {
                        string multiLine = string.Empty;
                        for (int i = 0; i < duplicates.Count; i++)
                        {
                            multiLine = duplicates[i].name + "\n";
                        }
                        EditorUtility.DisplayDialog("Duplicate Audio Clips!",
                            "The following Audio File Objects are already present in the wizard and have been skipped.\n" + multiLine,
                            "OK");
                    }

                    if (serializedObject.hasModifiedProperties)
                    {
                        serializedObject.ApplyModifiedProperties();
                        Window.Repaint();
                    }
                }
            }
        }
    }
}