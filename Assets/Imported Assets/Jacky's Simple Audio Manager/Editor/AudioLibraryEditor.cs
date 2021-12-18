﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Linq;

namespace JSAM.JSAMEditor
{
    public class AudioList
    {
        ReorderableList list;
        string category;
        bool isMusic;

        public AudioList()
        {

        }

        public AudioList(SerializedObject obj, SerializedProperty prop, string _category, bool _isMusic)
        {
            list = new ReorderableList(obj, prop, true, false, false, false);
            isMusic = _isMusic;
            if (isMusic)
            {
                list.drawElementCallback += DrawMusicElement;
            }
            else
            {
                list.drawElementCallback += DrawSoundElement;
            }
            list.headerHeight = 1;
            list.footerHeight = 0;
            category = _category;
        }

        private void DrawMusicElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var file = element.objectReferenceValue as BaseAudioFileObject;

            Rect prevRect = new Rect(rect);
            Rect currentRect = new Rect(prevRect);

            currentRect.xMax = currentRect.xMin + 20;
            GUIContent blontent = new GUIContent("T", "Change the category of this Audio File object");
            if (EditorGUI.DropdownButton(currentRect, blontent, FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent(AudioLibraryEditor.CATEGORY_NONE), category.Equals(string.Empty),
                    SetAudioFileCategory, new Tuple<BaseAudioFileObject, string>(file, string.Empty));
                menu.AddSeparator(string.Empty);
                var categories = AudioLibraryEditor.Asset.musicCategories;
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].Equals(string.Empty)) continue;
                    menu.AddItem(new GUIContent(categories[i]), category.Equals(categories[i]),
                        SetAudioFileCategory, new Tuple<BaseAudioFileObject, string>(file, categories[i]));
                }
                menu.ShowAsContext();
            }

            string enumName = string.Empty;
            bool registered = AudioLibraryEditor.Window.GetMusicEnumName(file, out enumName);
            using (new EditorGUI.DisabledScope(true))
            {
                prevRect = new Rect(currentRect);
                currentRect.xMin = prevRect.xMax + 5;
                currentRect.xMax = rect.xMax - 110;
                if (registered)
                {
                    blontent = new GUIContent(file.SafeName, enumName);
                }
                else
                {
                    blontent = new GUIContent(file.SafeName + "*", "Re-generate your Audio Library to get this Audio File's enum name");
                }
            }
            // Force a normal-colored label in a disabled scope
            JSAMEditorHelper.BeginColourChange(Color.clear);
            Rect decoyRect = EditorGUI.PrefixLabel(currentRect, blontent);
            JSAMEditorHelper.EndColourChange();
            if (!registered) JSAMEditorHelper.BeginColourChange(new Color(0.75f, 0.75f, 0.75f));
            EditorGUI.LabelField(currentRect, blontent);
            JSAMEditorHelper.EndColourChange();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(decoyRect, element, GUIContent.none);
            }

            using (new EditorGUI.DisabledScope(!registered))
            {
                prevRect = new Rect(currentRect);
                currentRect.xMin = prevRect.xMax + 2;
                currentRect.xMax = currentRect.xMax + 85;
                if (registered)
                {
                    blontent = new GUIContent("Copy Enum", "Click to copy this Audio File's enum name to your clipboard");
                }
                else
                {
                    blontent = new GUIContent("Copy Enum", "Re-generate your Audio Library to copy this Audio File's enum name");
                }
                if (GUI.Button(currentRect, blontent))
                {
                    JSAMEditorHelper.CopyToClipboard(enumName);
                }
            }

            JSAMEditorHelper.BeginColourChange(Color.red);
            prevRect = new Rect(currentRect);
            currentRect.xMin = prevRect.xMax + 2;
            currentRect.xMax = currentRect.xMax + 25;
            blontent = new GUIContent("X", "Remove this Audio File from the library, can be undone with Edit -> Undo");
            if (GUI.Button(currentRect, blontent))
            {
                AudioLibraryEditor.Window.RemoveAudioFile(file, isMusic);
            }
            JSAMEditorHelper.EndColourChange();
        }

        private void DrawSoundElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var file = element.objectReferenceValue as BaseAudioFileObject;

            Rect prevRect = new Rect(rect);
            Rect currentRect = new Rect(prevRect);

            currentRect.xMax = currentRect.xMin + 20;
            GUIContent blontent = new GUIContent("T", "Change the category of this Audio File object");
            if (EditorGUI.DropdownButton(currentRect, blontent, FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent(AudioLibraryEditor.CATEGORY_NONE), category.Equals(string.Empty),
                    SetAudioFileCategory, new Tuple<BaseAudioFileObject, string>(file, string.Empty));
                var categories = AudioLibraryEditor.Asset.soundCategories;
                menu.AddSeparator(string.Empty);
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].Equals(string.Empty)) continue;
                    menu.AddItem(new GUIContent(categories[i]), category.Equals(categories[i]),
                        SetAudioFileCategory, new Tuple<BaseAudioFileObject, string>(file, categories[i]));
                }
                menu.ShowAsContext();
            }

            string enumName = string.Empty;
            bool registered = AudioLibraryEditor.Window.GetSoundEnumName(file, out enumName);
            using (new EditorGUI.DisabledScope(true))
            {
                prevRect = new Rect(currentRect);
                currentRect.xMin = prevRect.xMax + 5;
                currentRect.xMax = rect.xMax - 110;
                if (registered)
                {
                    blontent = new GUIContent(file.SafeName, enumName);
                }
                else
                {
                    blontent = new GUIContent(file.SafeName + "*", "Re-generate your Audio Library to get this Audio File's enum name");
                }
            }
            // Force a normal-colored label in a disabled scope
            JSAMEditorHelper.BeginColourChange(Color.clear);
            Rect decoyRect = EditorGUI.PrefixLabel(currentRect, blontent);
            JSAMEditorHelper.EndColourChange();
            if (!registered) JSAMEditorHelper.BeginColourChange(new Color(0.75f, 0.75f, 0.75f));
            EditorGUI.LabelField(currentRect, blontent);
            JSAMEditorHelper.EndColourChange();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.PropertyField(decoyRect, element, GUIContent.none);
            }

            using (new EditorGUI.DisabledScope(!registered))
            {
                prevRect = new Rect(currentRect);
                currentRect.xMin = prevRect.xMax + 2;
                currentRect.xMax = currentRect.xMax + 85;
                if (registered)
                {
                    blontent = new GUIContent("Copy Enum", "Click to copy this Audio File's enum name to your clipboard");
                }
                else
                {
                    blontent = new GUIContent("Copy Enum", "Re-generate your Audio Library to copy this Audio File's enum name");
                }
                if (GUI.Button(currentRect, blontent))
                {
                    JSAMEditorHelper.CopyToClipboard(enumName);
                }
            }

            JSAMEditorHelper.BeginColourChange(Color.red);
            prevRect = new Rect(currentRect);
            currentRect.xMin = prevRect.xMax + 2;
            currentRect.xMax = currentRect.xMax + 25;
            blontent = new GUIContent("X", "Remove this Audio File from the library, can be undone with Edit -> Undo");
            if (GUI.Button(currentRect, blontent))
            {
                AudioLibraryEditor.Window.RemoveAudioFile(file, isMusic);
            }
            JSAMEditorHelper.EndColourChange();
        }

        public void Draw()
        {
            list.DoLayoutList();

            Rect rect = EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Category Options: ", new GUILayoutOption[] { GUILayout.Width(105) });

            GUIContent blontent = new GUIContent("Rename", "Change the name of this category, also changes the category field for all Audio File objects that share this category");
            if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                var utility = JSAMUtilityWindow.Init("Enter New Category Name", true, true);
                utility.AddField(new GUIContent("New Category Name"), category);
                JSAMUtilityWindow.onSubmitField += RenameCategory;
            }

            using (new EditorGUI.DisabledScope(AudioLibraryEditor.Window.CanCategoryMove(category, true, isMusic)))
            {
                blontent = new GUIContent("Move Up", "Swap the order of this category with the one above it");
                if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    AudioLibraryEditor.Window.MoveCategory(category, true, isMusic);
                }
            }

            using (new EditorGUI.DisabledScope(AudioLibraryEditor.Window.CanCategoryMove(category, false, isMusic)))
            {
                blontent = new GUIContent("Move Down", "Swap the order of this category with the one below it");
                if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    AudioLibraryEditor.Window.MoveCategory(category, false, isMusic);
                }
            }

            JSAMEditorHelper.BeginColourChange(Color.red);
            blontent = new GUIContent("Delete", "Remove this category and any Audio Files inside it from the library");
            if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                AudioLibraryEditor.Window.DeleteCategory(category, isMusic);
            }
            JSAMEditorHelper.EndColourChange();

            rect.xMin = rect.xMax - 100;
            GUI.Label(rect, "File Count: " + list.serializedProperty.arraySize,
                EditorStyles.label.ApplyTextAnchor(TextAnchor.MiddleRight));

            EditorGUILayout.EndHorizontal();
        }

        public void RenameCategory(string[] input)
        {
            AudioLibraryEditor.Window.RenameCategory(category, input[0], isMusic);
        }

        public void SetAudioFileCategory(object input)
        {
            Tuple<BaseAudioFileObject, string> tuple = (Tuple<BaseAudioFileObject, string>)input;
            AudioLibraryEditor.Window.ChangeAudioFileCategory(tuple.Item1, tuple.Item2, isMusic);
        }
    }

    [CustomEditor(typeof(AudioLibrary))]
    public class AudioLibraryInspector : Editor
    {
        bool DEBUG_MODE = false;
        public override void OnInspectorGUI()
        {
            if (DEBUG_MODE) DrawDefaultInspector();
            else
            {
                if (!AudioLibraryEditor.IsOpen)
                {
                    if (GUILayout.Button("Open Audio Library"))
                    {
                        AudioLibraryEditor.Init();
                    }
                }
            }
        }
    }

    public class AudioLibraryEditor : JSAMSerializedEditorWindow<AudioLibrary, AudioLibraryEditor>
    {
        public const string CATEGORY_NONE = "Uncategorized";

        List<string> newSoundNames = new List<string>();
        List<string> missingSoundNames = new List<string>();
        List<JSAMSoundFileObject> registeredSounds = new List<JSAMSoundFileObject>();
        /// <summary>
        /// Dictionary of category names to SerializedProperties that hold the list of sounds in that category
        /// </summary>
        Dictionary<string, SerializedProperty> categoryToSoundStructs = new Dictionary<string, SerializedProperty>();
        Dictionary<string, AudioList> reorderableSoundLists = new Dictionary<string, AudioList>();

        List<string> newMusicNames = new List<string>();
        List<string> missingMusicNames = new List<string>();
        List<JSAMMusicFileObject> registeredMusic = new List<JSAMMusicFileObject>();
        Dictionary<string, SerializedProperty> categoryToMusicStructs = new Dictionary<string, SerializedProperty>();
        Dictionary<string, AudioList> reorderableMusicLists = new Dictionary<string, AudioList>();

        public static int selectedLibrary = 0;
        public static List<string> projectLibrariesNames = null;
        public static List<AudioLibrary> projectLibraries = null;

        static Vector2 scrollProgress;

        public static bool renamingLibrary = false;

        public static int dragSelectedIndex = -1;

        const string SHOW_MUSIC = "JSAM_AUDIOLIBRARY_SHOWMUSIC";
        static bool showMusic
        {
            get
            {
                if (!EditorPrefs.HasKey(SHOW_MUSIC))
                {
                    EditorPrefs.SetBool(SHOW_MUSIC, false);
                }
                return EditorPrefs.GetBool(SHOW_MUSIC);
            }
            set
            {
                EditorPrefs.SetBool(SHOW_MUSIC, value);
            }
        }

        const string SHOW_HOWTO = "JSAM_AUDIOLIBRARY_SHOWHOWTO";
        static bool showHowTo
        {
            get
            {
                if (!EditorPrefs.HasKey(SHOW_HOWTO))
                {
                    EditorPrefs.SetBool(SHOW_HOWTO, false);
                }
                return EditorPrefs.GetBool(SHOW_HOWTO);
            }
            set
            {
                EditorPrefs.SetBool(SHOW_HOWTO, value);
            }
        }

        const string SHOW_CUSTOMNAMES = "JSAM_AUDIOLIBRARY_SHOWCUSTOMNAMES";
        static bool showCustomNames
        {
            get
            {
                if (!EditorPrefs.HasKey(SHOW_CUSTOMNAMES))
                {
                    EditorPrefs.SetBool(SHOW_CUSTOMNAMES, false);
                }
                return EditorPrefs.GetBool(SHOW_CUSTOMNAMES);
            }
            set
            {
                EditorPrefs.SetBool(SHOW_CUSTOMNAMES, value);
            }
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            var newAsset = AssetDatabase.LoadAssetAtPath<AudioLibrary>(assetPath);
            if (newAsset)
            {
                Init();
                if (newAsset != asset)
                {
                    window.AssignAsset(newAsset);
                }
                return true;
            }
            return false;
        }

        private void OnSelectionChange()
        {
            AudioLibrary newLibrary = Selection.activeObject as AudioLibrary;
            if (newLibrary)
            {
                AssignAsset(newLibrary);
            }
        }

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/JSAM/Audio Library")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            Window.Show();
            window.SetWindowTitle();
            window.Focus();

            if (AudioManager.Instance)
            {
                if (AudioManager.Instance.Library)
                {
                    Window.LoadLibraries(AudioManager.Instance.Library.name);
                }
            }
        }

        protected override void SetWindowTitle()
        {
            window.titleContent = new GUIContent("Audio Library");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            JSAMAssetModificationProcessor.OnJSAMAssetDeleted += OnJSAMAssetDeleted;
            Window.wantsMouseEnterLeaveWindow = true;
            LoadLibraries();
        }

        private void OnDisable()
        {
            JSAMAssetModificationProcessor.OnJSAMAssetDeleted -= OnJSAMAssetDeleted;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        void LoadLibraries(string toBeSelected = "")
        {
            var libraries = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<AudioLibrary>(JSAMSettings.Settings.LibraryPath);

            projectLibraries = new List<AudioLibrary>();
            projectLibrariesNames = new List<string>();
            for (int i = 0; i < libraries.Count; i++)
            {
                projectLibraries.Add(libraries[i]);
                projectLibrariesNames.Add(libraries[i].name);
            }

            if (toBeSelected.Equals(string.Empty))
            {
                if (asset == null)
                {
                    AssignAsset();
                }
            }
            else
            {
                selectedLibrary = projectLibrariesNames.IndexOf(toBeSelected);
                AssignAsset(projectLibraries[selectedLibrary]);
            }
        }

        void HandleAssetDeletion()
        {
            sounds.RemoveNullElementsFromArray();
            for (int i = soundCategoriesToList.arraySize - 1; i > -1; i--)
            {
                var files = soundCategoriesToList.GetArrayElementAtIndex(i).FindPropertyRelative("files");
                files.RemoveNullElementsFromArray();
            }

            music.RemoveNullElementsFromArray();
            for (int i = musicCategoriesToList.arraySize - 1; i > -1; i--)
            {
                var files = musicCategoriesToList.GetArrayElementAtIndex(i).FindPropertyRelative("files");
                files.RemoveNullElementsFromArray();
            }

            if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();

            markedForDeletion = false;
            Repaint();
        }

        void ReinitializeSerializedObject()
        {
            serializedObject = new SerializedObject(asset);
            DesignateSerializedProperties();
        }

        void AssignAsset(AudioLibrary newAsset = null)
        {
            if (newAsset != null) // Handle a basic reassign
            {
                asset = newAsset;
                JSAMSettings.Settings.SelectedLibrary = asset;
            }
            else // Fallback
            {
                // Read from the Settings file first
                AudioLibrary savedAsset = JSAMSettings.Settings.SelectedLibrary;
                if (savedAsset != null)
                {
                    asset = JSAMSettings.Settings.SelectedLibrary;
                }
                else
                {
                    // Write to Settings file if the project has libraries
                    if (projectLibraries.Count > 0)
                    {
                        asset = projectLibraries[0];
                        JSAMSettings.Settings.SelectedLibrary = asset;
                    }
                }
            }

            if (asset != null)
            {
                selectedLibrary = projectLibraries.IndexOf(asset);
                if (serializedObject == null)
                {
                    ReinitializeSerializedObject();
                }
                else if (serializedObject != null)
                {
                    if (serializedObject.targetObject != asset)
                    {
                        ReinitializeSerializedObject();
                    }
                }
            }
            else
            {
                serializedObject = null;
            }
        }

        bool markedForDeletion = true;
        private void OnJSAMAssetDeleted(string filePath)
        {
            AudioLibrary library = JSAMSettings.Settings.SelectedLibrary;
            if (filePath.Equals(AssetDatabase.GetAssetPath(library)))
            {
                serializedObject = null;
            }
            else
            {
                BaseAudioFileObject file = AssetDatabase.LoadAssetAtPath<BaseAudioFileObject>(filePath);
                if (asset.Sounds.Contains(file) || asset.Music.Contains(file))
                {
                    markedForDeletion = true;
                }
            }
        }

        SerializedProperty useCustomNames;
        SerializedProperty generatedName;

        SerializedProperty musicEnum;
        SerializedProperty musicEnumGenerated;
        SerializedProperty musicNamespace;
        SerializedProperty musicNamespaceGenerated;

        SerializedProperty soundEnum;
        SerializedProperty soundEnumGenerated;
        SerializedProperty soundNamespace;
        SerializedProperty soundNamespaceGenerated;

        SerializedProperty sounds;
        SerializedProperty soundCategories;
        SerializedProperty soundCategoriesToList;

        SerializedProperty music;
        SerializedProperty musicCategories;
        SerializedProperty musicCategoriesToList;

        protected override void DesignateSerializedProperties()
        {
            useCustomNames = FindProp(nameof(asset.useCustomNames));

            soundEnum = FindProp(nameof(asset.soundEnum));
            soundNamespace = FindProp(nameof(asset.soundNamespace));
            musicEnum = FindProp(nameof(asset.musicEnum));
            musicNamespace = FindProp(nameof(asset.musicNamespace));

            generatedName = FindProp(nameof(asset.generatedName));
            soundEnumGenerated = FindProp(nameof(asset.soundEnumGenerated));
            soundNamespaceGenerated = FindProp(nameof(asset.soundNamespaceGenerated));
            musicEnumGenerated = FindProp(nameof(asset.musicEnumGenerated));
            musicNamespaceGenerated = FindProp(nameof(asset.musicNamespaceGenerated));

            soundNamespaceGenerated = FindProp(nameof(asset.soundNamespaceGenerated));

            sounds = FindProp(nameof(asset.Sounds));
            soundCategories = FindProp(nameof(asset.soundCategories));
            soundCategoriesToList = FindProp("soundCategoriesToList");

            musicNamespaceGenerated = FindProp(nameof(asset.musicNamespaceGenerated));

            music = FindProp(nameof(asset.Music));
            musicCategories = FindProp(nameof(asset.musicCategories));
            musicCategoriesToList = FindProp("musicCategoriesToList");

            HandleAssetDeletion();
            InitializeCategories();
            DocumentAudioFiles();

            Window.Repaint();
        }

        void DocumentAudioFiles()
        {
            {
                List<string> registeredNames = new List<string>();
                string generatedSoundName = soundEnumGenerated.stringValue;
                if (!soundNamespaceGenerated.stringValue.IsNullEmptyOrWhiteSpace())
                {
                    generatedSoundName = soundNamespaceGenerated.stringValue + "." + generatedSoundName;
                }

                var type = AudioLibrary.GetEnumType(generatedSoundName);
                registeredNames.AddRange(new List<string>(Enum.GetNames(type)));

                List<string> libraryNames = new List<string>();
                for (int i = 0; i < asset.Sounds.Count; i++)
                {
                    libraryNames.Add(asset.Sounds[i].SafeName);
                }
                // Get all sound names that are registered but not in the library
                missingSoundNames = new List<string>(registeredNames.Except(libraryNames));
                // Get all sound names that are in the library but aren't registered
                newSoundNames = new List<string>(libraryNames.Except(registeredNames));

                if (missingSoundNames == null) missingSoundNames = new List<string>();
                if (newSoundNames == null) newSoundNames = new List<string>();
            }

            {
                List<string> registeredNames = new List<string>();

                string generatedMusicName = musicEnumGenerated.stringValue;
                if (!musicNamespaceGenerated.stringValue.IsNullEmptyOrWhiteSpace())
                {
                    generatedMusicName = musicNamespaceGenerated.stringValue + "." + generatedMusicName;
                }
                
                var type = AudioLibrary.GetEnumType(generatedMusicName);
                registeredNames.AddRange(new List<string>(Enum.GetNames(type)));

                List<string> libraryNames = new List<string>();
                for (int i = 0; i < asset.Music.Count; i++)
                {
                    libraryNames.Add(asset.Music[i].SafeName);
                }
                // Get all sound names that are registered but not in the library
                missingMusicNames = new List<string>(registeredNames.Except(libraryNames));
                // Get all sound names that are in the library but aren't registered
                newMusicNames = new List<string>(libraryNames.Except(registeredNames));

                if (missingMusicNames == null) missingMusicNames = new List<string>();
                if (newMusicNames == null) newMusicNames = new List<string>();
            }
        }

        void InitializeCategories()
        {
            // Link existing Category-to-Structs to Categories
            {
                // Sound
                categoryToSoundStructs = new Dictionary<string, SerializedProperty>();
                for (int i = 0; i < soundCategoriesToList.arraySize; i++)
                {
                    var element = soundCategoriesToList.GetArrayElementAtIndex(i);
                    string name = element.FindPropertyRelative("name").stringValue;
                    categoryToSoundStructs.Add(name, element);
                }

                // Music
                categoryToMusicStructs = new Dictionary<string, SerializedProperty>();
                for (int i = 0; i < musicCategoriesToList.arraySize; i++)
                {
                    var element = musicCategoriesToList.GetArrayElementAtIndex(i);
                    string name = element.FindPropertyRelative("name").stringValue;
                    categoryToMusicStructs.Add(name, element);
                }
            }

            // Initialize reorderable sound lists
            {
                reorderableSoundLists = new Dictionary<string, AudioList>();
                reorderableMusicLists = new Dictionary<string, AudioList>();

                // Sounds
                var keys = new string[categoryToSoundStructs.Keys.Count];
                categoryToSoundStructs.Keys.CopyTo(keys, 0);
                for (int i = 0; i < keys.Length; i++)
                {
                    var files = categoryToSoundStructs[keys[i]].FindPropertyRelative("files");
                    var category = categoryToSoundStructs[keys[i]].FindPropertyRelative("name").stringValue;
                    AudioList newList = new AudioList(serializedObject, files, category, false);
                    reorderableSoundLists[category] = newList;
                }

                keys = new string[categoryToMusicStructs.Keys.Count];
                categoryToMusicStructs.Keys.CopyTo(keys, 0);
                for (int i = 0; i < keys.Length; i++)
                {
                    var files = categoryToMusicStructs[keys[i]].FindPropertyRelative("files");
                    var category = categoryToMusicStructs[keys[i]].FindPropertyRelative("name").stringValue;
                    AudioList newList = new AudioList(serializedObject, files, category, true);
                    reorderableMusicLists[category] = newList;
                }
            }
        }

        private void OnGUI()
        {
            if (markedForDeletion)
            {
                HandleAssetDeletion();
            }

            GUIContent blontent = new GUIContent();

            // Apply leftover rename operation
            if (renamingLibrary)
            {
                AssetDatabase.SaveAssets();
                renamingLibrary = false;
            }

            EditorGUILayout.BeginHorizontal();
            blontent = new GUIContent("Selected Library", "The library you're currently modifying, " +
                "use the drop-down menu to choose a different library");
            EditorGUI.BeginChangeCheck();
            selectedLibrary = EditorGUILayout.Popup(blontent, selectedLibrary, projectLibrariesNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                AssignAsset(projectLibraries[selectedLibrary]);
            }

            blontent = new GUIContent(" Create ", "Click to create a new Audio Library asset");
            if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
            {
                var window = JSAMUtilityWindow.Init("Enter new library name", true, true);
                window.AddField(GUIContent.none, "New Library", false);
                JSAMUtilityWindow.onSubmitField += OnCreateLibrary;
            }

            using (new EditorGUI.DisabledScope(serializedObject == null))
            {
                blontent = new GUIContent(" Rename ", "Click to rename this Audio Library asset");
                if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    var window = JSAMUtilityWindow.Init("Enter new library name", true, true);
                    window.AddField(GUIContent.none, projectLibrariesNames[selectedLibrary], false);
                    JSAMUtilityWindow.onSubmitField += OnRenameLibrary;
                }

                blontent = new GUIContent(" Duplicate ", "Click to make a copy of this Audio Library asset");
                if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    bool result = EditorUtility.DisplayDialog("Confirm Asset Duplication",
                        "Are you sure you want to create a duplicate of this Audio Library asset? " +
                        "This process cannot be undone.", "Yes", "No");
                    if (result)
                    {
                        string ogPath = AssetDatabase.GetAssetPath(projectLibraries[selectedLibrary]);
                        string path = ogPath;
                        path = path.Remove(path.IndexOf(".asset")) + " - Copy.asset";
                        AssetDatabase.CopyAsset(ogPath, path);
                        LoadLibraries(projectLibrariesNames[selectedLibrary] + " - Copy");
                    }
                    GUIUtility.ExitGUI();
                }

                JSAMEditorHelper.BeginColourChange(Color.red);
                blontent = new GUIContent(" Delete ", "Click to delete this library asset");
                if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                {
                    bool result = EditorUtility.DisplayDialog("Confirm Library Deletion", "Are you sure you want to delete this library? " +
                        "This process cannot be undone, but can be recovered from the system trash bin.", "Yes", "No");
                    if (result)
                    {
                        string path = JSAMSettings.Settings.LibraryPath + "\"" + projectLibrariesNames[selectedLibrary] + ".asset";
                        AssetDatabase.MoveAssetToTrash(path);
                        LoadLibraries();
                    }
                    GUIUtility.ExitGUI();
                }
                JSAMEditorHelper.EndColourChange();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (serializedObject == null)
            {
                if (projectLibraries.Count > 0)
                {
                    EditorGUILayout.LabelField("Select an Audio Library asset to get started!");
                }
                else
                {
                    EditorGUILayout.LabelField("Create an Audio Library asset to get started!");
                }
            }
            else
            {
                serializedObject.UpdateIfRequiredOrScript();

                HandleMouseEnterLeave();

                if (missingSoundNames.Count > 0 || missingMusicNames.Count > 0)
                {
                    EditorGUILayout.HelpBox("Generated Audio Library files have been removed! " +
                        "Please re-generate your Audio Library to prevent errors during runtime!",
                        MessageType.Warning);
                }

                scrollProgress = EditorGUILayout.BeginScrollView(scrollProgress);

                EditorGUILayout.BeginHorizontal();

                if (!showMusic) JSAMEditorHelper.BeginColourChange(GUI.color.Subtract(new Color(0.2f, 0.2f, 0.2f, 0)));
                string topLabel = "Sound Library";
                if (newSoundNames.Count > 0 || missingSoundNames.Count > 0) topLabel += "*";
                if (GUILayout.Button(topLabel, EditorStyles.miniButtonLeft.SetTextColor(Color.white)))
                {
                    showMusic = false;
                }
                if (!showMusic) JSAMEditorHelper.EndColourChange();

                topLabel = "Music Library";
                if (showMusic) JSAMEditorHelper.BeginColourChange(GUI.color.Subtract(new Color(0.2f, 0.2f, 0.2f, 0)));
                if (newMusicNames.Count > 0 || missingMusicNames.Count > 0) topLabel += "*";
                if (GUILayout.Button(topLabel, EditorStyles.miniButtonRight))
                {
                    showMusic = true;
                }
                if (showMusic) JSAMEditorHelper.EndColourChange();

                EditorGUILayout.EndHorizontal();

                // Sound Library
                if (!showMusic)
                {
                    string title = string.Empty;
                    title += "New: " + newSoundNames.Count + " | ";
                    title += "Missing: " + missingSoundNames.Count + " | ";
                    title += "Total Files: " + asset.Sounds.Count + " | ";
                    title += "Categories: " + soundCategories.arraySize;
                    blontent = new GUIContent(title);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(blontent);
                    blontent = new GUIContent("Hide All", "Collapse all category foldouts in the Sound Library");
                    if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        for (int i = 0; i < asset.soundCategories.Count; i++)
                        {
                            SetSoundCategoryFoldout(asset.soundCategories[i], false);
                        }
                    }
                    blontent = new GUIContent("Show All", "Expand all category foldouts in the Sound Library");
                    if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        for (int i = 0; i < asset.soundCategories.Count; i++)
                        {
                            SetSoundCategoryFoldout(asset.soundCategories[i], true);
                        }
                    }
                    blontent = new GUIContent("Add New Category", "Add a new sound category");
                    if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        var utility = JSAMUtilityWindow.Init("Enter Category Name", true, true);
                        utility.AddField(new GUIContent("Category Name"), "New Category");
                        JSAMUtilityWindow.onSubmitField += AddNewSoundCategory;
                    }
                    EditorGUILayout.EndHorizontal();

                    // History of all instances of drags
                    List<int> dragHistory = new List<int>();
                    for (int i = 0; i < asset.soundCategories.Count; i++)
                    {
                        string category = asset.soundCategories[i];
                        string categoryName = category;
                        if (category.Equals(string.Empty)) categoryName = CATEGORY_NONE;
                        bool foldout = GetSoundCategoryFoldout(category);

                        Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUI.BeginChangeCheck();
                        foldout = EditorCompatability.SpecialFoldouts(foldout, categoryName);
                        
                        if (EditorGUI.EndChangeCheck())
                        {
                            SetSoundCategoryFoldout(category, foldout);
                        }
                        if (foldout)
                        {
                            reorderableSoundLists[category].Draw();
                        }

                        HandleDragAndDrop(rect, category, false);

                        EditorCompatability.EndSpecialFoldoutGroup();
                        EditorGUILayout.EndVertical();
                    }
                    // Un-mark drag-hovered lists
                    if (dragHistory.Count > 0)
                    {
                        bool blankHover = true;
                        for (int i = 0; i < dragHistory.Count; i++)
                        {
                            if (dragHistory[i] > -1)
                            {
                                blankHover = false;
                                break;
                            }
                        }
                        if (blankHover == true) dragSelectedIndex = -1;
                    }
                }
                else
                // Music Library
                {
                    string title = string.Empty;
                    title += "New: " + newMusicNames.Count + " | ";
                    title += "Missing: " + missingMusicNames.Count + " | ";
                    title += "Total Files: " + asset.Music.Count + " | ";
                    title += "Categories: " + musicCategories.arraySize;
                    blontent = new GUIContent(title);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(blontent);
                    blontent = new GUIContent("Hide All", "Collapse all category foldouts in the Music Library");

                    if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        for (int i = 0; i < asset.musicCategories.Count; i++)
                        {
                            SetMusicCategoryFoldout(asset.musicCategories[i], false);
                        }
                    }
                    blontent = new GUIContent("Show All", "Expand all category foldouts in the Music Library");
                    if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        for (int i = 0; i < asset.musicCategories.Count; i++)
                        {
                            SetMusicCategoryFoldout(asset.musicCategories[i], true);
                        }
                    }
                    blontent = new GUIContent("Add New Category", "Add a new sound category");
                    if (GUILayout.Button(blontent, new GUILayoutOption[] { GUILayout.ExpandWidth(false) }))
                    {
                        var utility = JSAMUtilityWindow.Init("Enter Category Name", true, true);
                        utility.AddField(new GUIContent("Category Name"), "New Category");
                        JSAMUtilityWindow.onSubmitField += AddNewMusicCategory;
                    }
                    EditorGUILayout.EndHorizontal();

                    List<int> dragHistory = new List<int>();
                    for (int i = 0; i < asset.musicCategories.Count; i++)
                    {
                        string category = asset.musicCategories[i];
                        string categoryName = category;
                        if (category.Equals(string.Empty)) categoryName = CATEGORY_NONE;
                        bool foldout = GetMusicCategoryFoldout(category);

                        Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUI.BeginChangeCheck();
                        foldout = EditorCompatability.SpecialFoldouts(foldout, categoryName);

                        if (EditorGUI.EndChangeCheck())
                        {
                            SetMusicCategoryFoldout(category, foldout);
                        }
                        if (foldout)
                        {
                            reorderableMusicLists[category].Draw();
                        }

                        HandleDragAndDrop(rect, category, true);

                        EditorCompatability.EndSpecialFoldoutGroup();
                        EditorGUILayout.EndVertical();
                    }
                    // Un-mark drag-hovered lists
                    if (dragHistory.Count > 0)
                    {
                        bool blankHover = true;
                        for (int i = 0; i < dragHistory.Count; i++)
                        {
                            if (dragHistory[i] > -1)
                            {
                                blankHover = false;
                                break;
                            }
                        }
                        if (blankHover == true) dragSelectedIndex = -1;
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(useCustomNames);
                showCustomNames = EditorCompatability.SpecialFoldouts(showCustomNames, "Custom Names");
                if (showCustomNames)
                {
                    using (new EditorGUI.DisabledScope(!useCustomNames.boolValue))
                    {
                        blontent = new GUIContent("Sound Enum", "Change the name enum name used to refer to your sounds. Generated enums will appear as <Sound Namespace>.<Sound Enum>.<Sound Name>.");
                        RenderCodeField(soundEnum, blontent, false, "Change Sound Enum Name", asset.defaultSoundEnum);

                        blontent = new GUIContent("Sound Namespace", "Adds a custom namespace to generated sound enums. No namespace will be used if this field is empty.");
                        RenderCodeField(soundNamespace, blontent, true, "Change Sound Namespace Name", "");

                        blontent = new GUIContent("Music Enum", "Change the name enum name used to refer to your music. Generated enums will appear as <Music Namespace>.<Music Enum>.<Music Name>.");
                        RenderCodeField(musicEnum, blontent, false, "Change Music Namespace Name", asset.defaultMusicEnum);

                        blontent = new GUIContent("Music Namespace", "Adds a custom namespace to generated music enums. No namespace will be used if this field is empty.");
                        RenderCodeField(musicNamespace, blontent, true, "Change Music Namespace Name", "");
                    }
                }
                EditorCompatability.EndSpecialFoldoutGroup();

                blontent = new GUIContent("Re-Generate Audio Library", "Click to generate your Audio enums and make the files in your Audio Library usable. " +
                    "Do this every time you Add/Remove Audio File objects in your library.");
                if (GUILayout.Button(blontent))
                {
                    GenerateEnumFile(JSAMSettings.Settings.GeneratedEnumsPath);
                    GUIUtility.ExitGUI();
                }

                if (serializedObject.hasModifiedProperties)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            string[] howToText = new string[] {
                    "Overview",
                    "Your Audio Library functions as a bank of Audio Files that your AudioManager will pull from when " +
                    "playing audio.",
                    "Do click the button to regenerate your Audio Library enum script every time you add/remove an Audio File Object.",
                    "Tips",
                    "You can batch import multiple Audio File objects by multi-selecting them in the project window and " +
                    "dragging them in at once, or by dragging and dropping an entire folder.",
                    "Click the Copy Enum button to quickly get the entire enum name of your Audio File object!",
                };
            showHowTo = JSAMEditorHelper.RenderQuickReferenceGuide(showHowTo, howToText);
        }

        void ApplyChanges()
        {
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                InitializeCategories();
                DocumentAudioFiles();
                Window.Repaint();
            }
        }

        void ApplyChangesHard()
        {
            serializedObject.ApplyModifiedProperties();
            ReinitializeSerializedObject();
        }

        void OnUndoRedoPerformed()
        {
            if (serializedObject != null)
            {
                ReinitializeSerializedObject();
                window.Repaint();
            }
        }

        void OnCreateLibrary(string[] input)
        {
            if (input[0].IsNullEmptyOrWhiteSpace())
            {
                EditorUtility.DisplayDialog("Create Library Error",
                    "Your Audio Library name cannot be left blank", "OK");
            }
            else if (projectLibrariesNames.Contains(input[0]))
            {
                EditorUtility.DisplayDialog("Create Library Error",
                    "An Audio Library with this name already exists! Please use a different name.", "OK");
            }
            var newLibrary = CreateInstance<AudioLibrary>();
            newLibrary.name = input[0];
            newLibrary.InitializeValues();
            string path = JSAMSettings.Settings.LibraryPath + "\"" + input[0] + ".asset";
            if (JSAMEditorHelper.CreateAssetSafe(newLibrary, path))
            {
                LoadLibraries(newLibrary.name);
            }
        }

        void OnRenameLibrary(string[] input)
        {
            if (input[0].IsNullEmptyOrWhiteSpace())
            {
                EditorUtility.DisplayDialog("Create Library Error",
                    "Your Audio Library name cannot be left blank", "OK");
            }
            else if (projectLibrariesNames.Contains(input[0]))
            {
                EditorUtility.DisplayDialog("Create Library Error",
                    "An Audio Library with this name already exists! Please use a different name.", "OK");
            }
            var targetLibrary = projectLibraries[selectedLibrary];
            string path = AssetDatabase.GetAssetPath(targetLibrary);
            AssetDatabase.RenameAsset(path, input[0]);
            LoadLibraries(targetLibrary.name);
            renamingLibrary = true;
        }

        void HandleMouseEnterLeave()
        {
            if (Event.current.type == EventType.MouseEnterWindow)
            {
                InitializeCategories();
                window.Repaint();
            }
            else if (Event.current.type == EventType.MouseLeaveWindow)
            {
            }
            else if (Event.current.type == EventType.DragExited)
            {
                dragSelectedIndex = -1;
                window.Repaint();
            }
        }

        /// <summary>
        /// https://answers.unity.com/questions/1548292/gui-editor-drag-and-drop-inspector.html
        /// </summary>
        /// <param name="dragRect"></param>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        void HandleDragAndDrop(Rect dragRect, string categoryName, bool isMusic)
        {
            if (JSAMEditorHelper.DragAndDropRegion(dragRect, "", "Release to Drop Audio Files"))
            {
                List<BaseAudioFileObject> duplicates = new List<BaseAudioFileObject>();

                if (isMusic)
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        string filePath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[i]);
                        var mimport = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<JSAMMusicFileObject>(filePath);

                        for (int j = 0; j < mimport.Count; j++)
                        {
                            if (asset.Music.Contains(mimport[j]))
                            {
                                duplicates.Add(mimport[j]);
                                continue;
                            }

                            AddMusicFile(mimport[j], categoryName);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        string filePath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[i]);
                        var simport = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<JSAMSoundFileObject>(filePath);

                        for (int j = 0; j < simport.Count; j++)
                        {
                            if (asset.Sounds.Contains(simport[j]))
                            {
                                duplicates.Add(simport[j]);
                                continue;
                            }

                            AddSoundFile(simport[j], categoryName);
                        }
                    }
                }

                if (duplicates.Count > 0)
                {
                    string multiLine = string.Empty;
                    for (int i = 0; i < duplicates.Count; i++)
                    {
                        multiLine = duplicates[i].name + "/n";
                    }
                    EditorUtility.DisplayDialog("Duplicate Audio Files!",
                        "The following Audio File Objects are already present in the Audio Library! They have been skipped./n" + multiLine,
                        "OK");
                }

                dragSelectedIndex = -1;
                ApplyChangesHard();
            }
        }

        void AddSoundFile(JSAMSoundFileObject newSound, string category)
        {
            sounds.AddNewArrayElement().objectReferenceValue = newSound;
            categoryToSoundStructs[category].FindPropertyRelative("files").AddNewArrayElement().objectReferenceValue = newSound;
            serializedObject.ApplyModifiedProperties();
        }

        void AddMusicFile(JSAMMusicFileObject newMusic, string category)
        {
            music.AddNewArrayElement().objectReferenceValue = newMusic;
            categoryToMusicStructs[category].FindPropertyRelative("files").AddNewArrayElement().objectReferenceValue = newMusic;
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Returns true if this sound is registered in the library
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool GetSoundEnumName(BaseAudioFileObject file, out string enumName)
        {
            enumName = soundEnumGenerated.stringValue + "." + file.SafeName;
            if (soundNamespaceGenerated.stringValue.Length > 0)
            {
                enumName = soundNamespaceGenerated.stringValue + "." + enumName;
            }
            return !newSoundNames.Contains(file.SafeName);
        }

        /// <summary>
        /// Returns true if this sound is registered in the library
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool GetMusicEnumName(BaseAudioFileObject file, out string enumName)
        {
            enumName = musicEnumGenerated.stringValue + "." + file.SafeName;
            if (musicNamespaceGenerated.stringValue.Length > 0)
            {
                enumName = musicNamespaceGenerated.stringValue + "." + enumName;
            }
            return !newMusicNames.Contains(file.SafeName);
        }

        public void RemoveAudioFile(BaseAudioFileObject file, bool isMusic)
        {
            SerializedProperty array = null;
            SerializedProperty categoriesToList = null;
            int index = 0;

            if (isMusic)
            {
                array = music;
                categoriesToList = musicCategoriesToList;
                index = asset.Music.IndexOf(file as JSAMMusicFileObject);
            }
            else
            {
                array = sounds;
                categoriesToList = soundCategoriesToList;
                index = asset.Sounds.IndexOf(file as JSAMSoundFileObject);
            }

            array.GetArrayElementAtIndex(index).objectReferenceValue = null;
            array.DeleteArrayElementAtIndex(index);

            for (int i = 0; i < categoriesToList.arraySize; i++)
            {
                var filesArray = categoriesToList.GetArrayElementAtIndex(i).FindPropertyRelative("files");
                for (int j = 0; j < filesArray.arraySize; j++)
                {
                    var element = filesArray.GetArrayElementAtIndex(j).objectReferenceValue as BaseAudioFileObject;
                    if (element.Equals(file))
                    {
                        filesArray.GetArrayElementAtIndex(j).objectReferenceValue = null;
                        filesArray.DeleteArrayElementAtIndex(j);
                        break;
                    }
                }
            }

            ApplyChanges();
            GUIUtility.ExitGUI();
        }

        #region Category Operations

        bool GetSoundCategoryFoldout(string category)
        {
            return categoryToSoundStructs[category].FindPropertyRelative("foldout").boolValue;
        }

        void SetSoundCategoryFoldout(string category, bool state)
        {
            categoryToSoundStructs[category].FindPropertyRelative("foldout").boolValue = state;
        }

        bool GetMusicCategoryFoldout(string category)
        {
            return categoryToMusicStructs[category].FindPropertyRelative("foldout").boolValue;
        }

        void SetMusicCategoryFoldout(string category, bool state)
        {
            categoryToMusicStructs[category].FindPropertyRelative("foldout").boolValue = state;
        }

        void AddNewSoundCategory(string[] input)
        {
            string category = input[0];
            if (category.IsNullEmptyOrWhiteSpace())
            {
                EditorUtility.DisplayDialog("Category Name Error",
                    "Category name field was left blank, please enter a valid name.", "OK");
                GUIUtility.ExitGUI();
            }
            else if (asset.soundCategories.Contains(category))
            {
                EditorUtility.DisplayDialog("Duplicate Category!",
                    "A category with this name already exists in the sound library!", "OK");
                GUIUtility.ExitGUI();
                return;
            }

            soundCategories.AddNewArrayElement().stringValue = category;

            var element = soundCategoriesToList.AddNewArrayElement();
            element.FindPropertyRelative("name").stringValue = category;
            element.FindPropertyRelative("foldout").boolValue = true;
            element.FindPropertyRelative("files").ClearArray();

            ApplyChanges();
        }

        void AddNewMusicCategory(string[] input)
        {
            string category = input[0];
            if (category.IsNullEmptyOrWhiteSpace())
            {
                EditorUtility.DisplayDialog("Category Name Error",
                    "Category name field was left blank, please enter a valid name.", "OK");
                GUIUtility.ExitGUI();
            }
            else if (asset.musicCategories.Contains(category))
            {
                EditorUtility.DisplayDialog("Duplicate Category!",
                    "A category with this name already exists in the music library!", "OK");
                GUIUtility.ExitGUI();
                return;
            }

            musicCategories.AddNewArrayElement().stringValue = category;

            var element = musicCategoriesToList.AddNewArrayElement();
            element.FindPropertyRelative("name").stringValue = category;
            element.FindPropertyRelative("foldout").boolValue = true;
            element.FindPropertyRelative("files").ClearArray();

            ApplyChanges();
        }

        public void RenameCategory(string prevName, string newName, bool isMusic)
        {
            if (prevName.Equals(newName))
            {
                EditorUtility.DisplayDialog("Category Rename Error",
                    "Category name is identical to previous name!", "OK");
                FocusWindowIfItsOpen<JSAMUtilityWindow>();
                GUIUtility.ExitGUI();
            }
            else if (newName.Equals(CATEGORY_NONE))
            {
                newName = string.Empty;
            }

            if (prevName.Equals(CATEGORY_NONE))
            {
                prevName = string.Empty;
            }

            int index = 0;
            SerializedProperty categories = null;
            Dictionary<string, SerializedProperty> categoryToStructs = null;
            if (isMusic)
            {
                categories = musicCategories;
                categoryToStructs = categoryToMusicStructs;

                index = asset.musicCategories.IndexOf(prevName);
            }
            else
            {
                categories = soundCategories;
                categoryToStructs = categoryToSoundStructs;

                index = asset.soundCategories.IndexOf(prevName);
            }

            // Unique category name? Easiest rename of my life
            if (!categoryToStructs.ContainsKey(newName))
            {
                categories.GetArrayElementAtIndex(index).stringValue = newName;
                categoryToStructs[prevName].FindPropertyRelative("name").stringValue = newName;
            }
            // Category with this name exists, time to get dirty
            else
            {
                // Move files over to new category
                var array = categoryToStructs[prevName].FindPropertyRelative("files");
                var newArray = categoryToStructs[newName].FindPropertyRelative("files");
                for (int i = 0; i < array.arraySize; i++)
                {
                    newArray.AddNewArrayElement().objectReferenceValue = array.GetArrayElementAtIndex(i).objectReferenceValue;
                }

                // Don't actually delete if you're the important category
                if (!prevName.Equals(string.Empty) || !prevName.Equals(CATEGORY_NONE))
                {
                    categoryToStructs[prevName].DeleteCommand();
                    categories.DeleteArrayElementAtIndex(index);
                }
            }

            ApplyChanges();
        }

        public void ChangeAudioFileCategory(BaseAudioFileObject file, string newCategory, bool isMusic)
        {
            Dictionary<string, SerializedProperty> categoryToStructs = null;
            SerializedProperty categoriesToList = null;

            if (isMusic)
            {
                categoryToStructs = categoryToMusicStructs;
                categoriesToList = musicCategoriesToList;
            }
            else
            {
                categoryToStructs = categoryToSoundStructs;
                categoriesToList = soundCategoriesToList;
            }

            bool found = false;
            for (int i = 0; i < categoriesToList.arraySize; i++)
            {
                var files = categoriesToList.GetArrayElementAtIndex(i).FindPropertyRelative("files");
                for (int j = 0; j < files.arraySize; j++)
                {
                    var element = files.GetArrayElementAtIndex(j).objectReferenceValue as BaseAudioFileObject;
                    if (element == file)
                    {
                        files.GetArrayElementAtIndex(j).objectReferenceValue = null;
                        files.DeleteArrayElementAtIndex(j);
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            var array = categoryToStructs[newCategory].FindPropertyRelative("files");
            array.AddNewArrayElement().objectReferenceValue = file;

            ApplyChanges();
        }

        public bool CanCategoryMove(string category, bool isUp, bool isMusic)
        {
            List<string> categoryList = null;
            if (isMusic)
            {
                categoryList = asset.musicCategories;
            }
            else
            {
                categoryList = asset.soundCategories;
            }

            if (isUp)
            {
                return categoryList.IndexOf(category) == 0;
            }
            else
            {
                return categoryList.IndexOf(category) == categoryList.Count - 1;
            }
        }

        /// <summary>
        /// Moving up at the top and down at the bottom disabled 
        /// Out of scope for the time being
        /// Try using MoveArrayElement
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="isUp"></param>
        /// <param name="isMusic"></param>
        public void MoveCategory(string categoryName, bool isUp, bool isMusic)
        {
            // Initialize variables
            int startIndex = -1;
            SerializedProperty propCategories = null;
            SerializedProperty propCategoriesToList = null;
            int ogSize = -1;
            int startOffset = -1;
            int destinationIndex = -1;
            int destinationOffset = -1;
            int deletionTarget = -1;
            string destinationCategory = string.Empty;

            if (isMusic)
            {
                // Search for start position of category
                startIndex = asset.musicCategories.IndexOf(categoryName);

                // Cache initial size of category array
                ogSize = musicCategories.arraySize;

                propCategories = musicCategories;
                propCategoriesToList = musicCategoriesToList;
            }
            else
            {
                // Search for start position of category
                startIndex = asset.soundCategories.IndexOf(categoryName);

                // Cache initial size of category array
                ogSize = soundCategories.arraySize;

                propCategories = soundCategories;
                propCategoriesToList = soundCategoriesToList;
            }

            // Insert a helper element into array
            propCategories.InsertArrayElementAtIndex(startIndex);
            propCategoriesToList.InsertArrayElementAtIndex(startIndex);

            if (isUp)
            {
                startOffset = startIndex + 1;
                // Moving up while at the top
                if (startIndex == 0)
                {
                    destinationIndex = ogSize;
                    destinationCategory = propCategories.GetArrayElementAtIndex(destinationIndex).stringValue;
                }
                else
                {
                    destinationIndex = startIndex - 1;
                    destinationCategory = propCategories.GetArrayElementAtIndex(destinationIndex).stringValue;
                }
                destinationOffset = destinationIndex;

                deletionTarget = startOffset;
            }
            else
            {
                startOffset = startIndex + 1;
                // Moving down while at the bottom
                if (startIndex == ogSize - 1)
                {
                    destinationIndex = 0;
                    destinationOffset = destinationIndex + 1;
                    destinationCategory = propCategories.GetArrayElementAtIndex(destinationIndex).stringValue;
                    deletionTarget = ogSize;
                }
                else
                {
                    destinationIndex = startOffset;
                    destinationOffset = destinationIndex + 1;
                    destinationCategory = propCategories.GetArrayElementAtIndex(destinationOffset).stringValue;
                    deletionTarget = destinationOffset;
                }
            }

            // Move destination to original position
            propCategories.GetArrayElementAtIndex(startIndex).stringValue = destinationCategory;
            propCategoriesToList.GetArrayElementAtIndex(startIndex).FindPropertyRelative("name").stringValue = destinationCategory;
            var startArray = propCategoriesToList.GetArrayElementAtIndex(startIndex).FindPropertyRelative("files");
            var destArray = propCategoriesToList.GetArrayElementAtIndex(destinationOffset).FindPropertyRelative("files");
            startArray.ClearArray();
            for (int i = 0; i < destArray.arraySize; i++)
            {
                var element = startArray.AddNewArrayElement();
                element.objectReferenceValue = destArray.GetArrayElementAtIndex(i).objectReferenceValue;
            }

            // Move original to destination
            propCategories.GetArrayElementAtIndex(destinationIndex).stringValue = categoryName;
            propCategoriesToList.GetArrayElementAtIndex(destinationIndex).FindPropertyRelative("name").stringValue = categoryName;
            var startOffsetArray = propCategoriesToList.GetArrayElementAtIndex(startOffset).FindPropertyRelative("files");
            destArray.ClearArray();
            for (int i = 0; i < startOffsetArray.arraySize; i++)
            {
                var element = destArray.AddNewArrayElement();
                element.objectReferenceValue = startOffsetArray.GetArrayElementAtIndex(i).objectReferenceValue;
            }

            // Delete the outlying index
            propCategories.DeleteArrayElementAtIndex(deletionTarget);
            propCategoriesToList.DeleteArrayElementAtIndex(deletionTarget);

            ApplyChanges();
        }

        public void DeleteCategory(string categoryName, bool isMusic)
        {
            Dictionary<string, SerializedProperty> categoryToStruct = null;
            SerializedProperty categoriesToList = null;
            SerializedProperty categories = null;
            SerializedProperty audioProp = null;
            int index = -1;

            if (isMusic)
            {
                categoryToStruct = categoryToMusicStructs;
                categoriesToList = musicCategoriesToList;
                index = asset.musicCategories.IndexOf(categoryName);
                categories = musicCategories;
                audioProp = music;
            }
            else
            {
                categoryToStruct = categoryToSoundStructs;
                categoriesToList = soundCategoriesToList;
                index = asset.soundCategories.IndexOf(categoryName);
                categories = soundCategories;
                audioProp = sounds;
            }

            int categorySize = categoryToStruct[categoryName].FindPropertyRelative("files").arraySize;
            if (categorySize > 0)
            {
                bool cancel = EditorUtility.DisplayDialog("Delete Category?", "This category holds " + categorySize + " Audio File object(s). " +
                    "Are you sure you want to remove the category as well all its Audio Files from this library?/n" +
                    "(Note: This process can be undone with Edit -> Undo)", "Yes", "No");
                if (!cancel)
                {
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            List<BaseAudioFileObject> filesToDelete = new List<BaseAudioFileObject>();
            for (int i = 0; i < categoriesToList.arraySize; i++)
            {
                if (categoriesToList.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == categoryName)
                {
                    var files = categoriesToList.GetArrayElementAtIndex(i).FindPropertyRelative("files");
                    for (int j = 0; j < files.arraySize; j++)
                    {
                        filesToDelete.Add(files.GetArrayElementAtIndex(j).objectReferenceValue as BaseAudioFileObject);
                    }
                    break;
                }
            }

            categories.DeleteArrayElementAtIndex(index);
            for (int i = 0; i < audioProp.arraySize && filesToDelete.Count > 0; i++)
            {
                var audio = audioProp.GetArrayElementAtIndex(i).objectReferenceValue as BaseAudioFileObject;
                if (filesToDelete.Contains(audio))
                {
                    // A dirty hack, but Unity serialization is real messy
                    // https://answers.unity.com/questions/555724/serializedpropertydeletearrayelementatindex-leaves.html
                    audioProp.DeleteArrayElementAtIndex(i);
                    audioProp.DeleteArrayElementAtIndex(i);
                    i--; // Decrement current index so we can keep iterating through the array scot-free

                    filesToDelete.Remove(audio);
                }
            }

            categoryToStruct[categoryName].DeleteCommand();

            ApplyChanges();
            GUIUtility.ExitGUI();
        }

        #endregion

        /// <summary>
        /// With help from Daniel Robledo
        /// https://support.unity3d.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-
        /// </summary>
        public bool GenerateEnumFile(string filePath)
        {
            // Evaluate potential enum names before writing to file
            List<JSAMSoundFileObject> soundLibrary = asset.Sounds;

            List<string> soundNames = new List<string>();
            for (int i = 0; i < soundLibrary.Count; i++)
            {
                string newName = soundLibrary[i].SafeName;

                if (!soundNames.Contains(newName))
                {
                    soundNames.Add(newName);
                }
                else
                {
                    string problemString = soundLibrary[soundNames.IndexOf(newName)].name;
                    EditorUtility.DisplayDialog("Audio Library Generation Error!", "\"" + soundLibrary[i].name + "\" shares the same name with \"" + problemString + "\"! " +
                        "When converting to Audio Enums, AudioManager strips away non Alphanumeric characters to ensure proper C# syntax and " +
                        "in the process they've ended with the same name! Please make \"" + soundLibrary[i].name + "\" more different and try again!", "OK");
                    return false;
                }
            }

            List<JSAMMusicFileObject> musicLibrary = asset.Music;

            List<string> musicNames = new List<string>();
            for (int i = 0; i < musicLibrary.Count; i++)
            {
                string newName = musicLibrary[i].SafeName;

                if (!musicNames.Contains(newName))
                {
                    musicNames.Add(newName);
                }
                else
                {
                    string problemString = musicLibrary[musicNames.IndexOf(newName)].name;
                    EditorUtility.DisplayDialog("Audio Library Generation Error!", "\"" + musicLibrary[i].name + "\" shares the same name with \"" + problemString + "\"! " +
                        "When converting to Audio Enums, AudioManager strips away non Alphanumeric characters to ensure proper C# syntax and " +
                        "in the process they've ended with the same name! Please make \"" + musicLibrary[i].name + "\" more different and try again!", "OK");
                    return false;
                }
            }

            string fileName = "//AudioEnums - " + asset.SafeName + ".cs";
            string prevName = "//AudioEnums - " + asset.generatedName + ".cs";

            if (!JSAMEditorHelper.GenerateFolderStructureAt(filePath))
            {
                return false;
            }

            bool overwriting = false;
            // Looking for previous AudioEnums
            string[] GUIDs = AssetDatabase.FindAssets("AudioEnums", new[] { filePath });
            for (int i = 0; i < GUIDs.Length; i++)
            {
                var p = GUIDs[i];
                // Make the detected file match the format of expected filenames up above
                string assetPath = AssetDatabase.GUIDToAssetPath(p);
                string assetName = "\\" + assetPath.Remove(0, assetPath.LastIndexOf('/') + 1);
                if (assetName.Equals(fileName))
                {
                    overwriting = true;
                    continue; // We're overwriting this anyway
                }
                else if (assetName.Equals(prevName))
                {
                    if (EditorUtility.DisplayDialog("Old AudioEnums file Found!", "An AudioEnums file whose name matches this library's " +
                        "previous name has been found. It is likely a remnant left behind after this library was renamed. " +
                        "Would you like to delete this Audio Enums file?", "Yes", "No"))
                    {
                        AssetDatabase.MoveAssetToTrash(assetPath);
                    }
                }
            }

            string soundEnumName = useCustomNames.boolValue ? soundEnum.stringValue : asset.defaultSoundEnum;
            string musicEnumName = useCustomNames.boolValue ? musicEnum.stringValue : asset.defaultMusicEnum;
            string soundTypeName = soundNamespace.stringValue + "." + soundEnumName;
            string musicTypeName = musicNamespace.stringValue + "." + musicEnumName;

            // Check for existing Enums of the same name
            if (!overwriting)
            {
                if (AudioLibrary.GetEnumType(soundTypeName) != null)
                {
                    if (EditorUtility.DisplayDialog("Duplicate Enum Type Found!", "An existing script in the project already " +
                        "contains an Enum called \"" + soundTypeName + "\"! AudioManager cannot regenerate the library " +
                        "until the scene name is changed to something different or the existing enum name is modified!", "OK"))
                    {
                        return false;
                    }
                }
                else if (AudioLibrary.GetEnumType(musicTypeName) != null)
                {
                    if (EditorUtility.DisplayDialog("Duplicate Enum Type Found!", "An existing script in the project already " +
                        "contains an Enum called \"" + musicTypeName + "\"! AudioManager cannot regenerate the library " +
                        "until the scene name is changed to something different or the existing enum name is modified!", "OK"))
                    {
                        return false;
                    }
                }
            }

            EditorUtility.DisplayProgressBar("Re-generating Audio Library", "Generating enum code...", 0.15f);

            // May want to use filePath = "/\" + fileName instead?
            filePath += fileName;
            File.WriteAllText(filePath, string.Empty);
            StreamWriter writer = new StreamWriter(filePath, true);
            if (soundNamespace.stringValue.Length > 0 && useCustomNames.boolValue)
            {
                writer.WriteLine("namespace " + soundNamespace.stringValue + " {");
            }

            writer.WriteLine("    public enum " + soundEnumName + " {");

            if (soundLibrary != null)
            {
                if (soundLibrary.Count > 0)
                {
                    for (int i = 0; i < soundLibrary.Count - 1; i++)
                    {
                        writer.WriteLine("        " + soundLibrary[i].SafeName + ",");
                    }
                    writer.WriteLine("        " + soundLibrary[soundLibrary.Count - 1].SafeName);
                }
            }

            writer.WriteLine("    }");

            if (soundNamespace.stringValue.Length > 0 && useCustomNames.boolValue)
            {
                writer.WriteLine("}");
            }

            if (musicNamespace.stringValue.Length > 0 && useCustomNames.boolValue)
            {
                writer.WriteLine("namespace " + musicNamespace.stringValue + " {");
            }

            writer.WriteLine("    public enum " + musicEnumName + " {");

            if (musicLibrary != null)
            {
                if (musicLibrary.Count > 0)
                {
                    for (int i = 0; i < musicLibrary.Count - 1; i++)
                    {
                        writer.WriteLine("        " + musicLibrary[i].SafeName + ",");
                    }
                    writer.WriteLine("        " + musicLibrary[musicLibrary.Count - 1].SafeName);
                }
            }

            writer.WriteLine("    }");

            if (musicNamespace.stringValue.Length > 0 && useCustomNames.boolValue)
            {
                writer.WriteLine("}");
            }
            writer.Close();

            EditorUtility.DisplayProgressBar("Re-generating Audio Library", "Done! Importing and compiling...", 0.9f);

            //Re-import the file to update the reference in the editor
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh();

            //Cache and embed names used during generation for easy lookup
            musicEnumGenerated.stringValue = musicEnumName;
            musicNamespaceGenerated.stringValue = useCustomNames.boolValue ? musicNamespace.stringValue : "";

            soundEnumGenerated.stringValue = soundEnumName;
            soundNamespaceGenerated.stringValue = useCustomNames.boolValue ? soundNamespace.stringValue : "";

            generatedName.stringValue = asset.SafeName;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.ClearProgressBar();

            return true;
        }

        bool nextAllowPeriods;
        SerializedProperty nextProperty;
        void RenderCodeField(SerializedProperty property, GUIContent content, bool allowPeriods, string title, string defaultName)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            if (asset.useCustomNames)
            {
                EditorGUILayout.LabelField(content, new GUIContent(property.stringValue));
            }
            else
            {
                EditorGUILayout.LabelField(content, new GUIContent(defaultName));
            }

            if (JSAMEditorHelper.CondensedButton("Edit"))
            {
                var utility = JSAMUtilityWindow.Init(title, true, true);
                utility.AddField(content, property.stringValue);
                JSAMUtilityWindow.onSubmitField += ChangeCodeNames;
                nextAllowPeriods = allowPeriods;
                nextProperty = property;
            }
            if (JSAMEditorHelper.CondensedButton("Set to Default"))
            {
                property.stringValue = defaultName;
            }
            EditorGUILayout.EndHorizontal();
        }

        void ChangeCodeNames(string[] fields)
        {
            string fixedString = fields[0].ConvertToAlphanumeric(nextAllowPeriods);
            if (fixedString != fields[0])
            {
                EditorUtility.DisplayDialog("Warning",
                    "Your entry contained elements that wouldn't work once converted into code! " +
                    "The name has been fixed for you, but remember to avoid the following: " +
                    "/n - Spaces " +
                    "/n - Non-alphanumeric characters " +
                    "/n - Having a number in the very front ",
                    "Got it.");
                nextProperty.stringValue = fixedString;
            }
            nextProperty.stringValue = fixedString;
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}