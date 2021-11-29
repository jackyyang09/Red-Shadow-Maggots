using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;

namespace JSAM.JSAMEditor
{
    public class SoundEditorFader : System.IDisposable
    {
        BaseAudioFileObject asset;

        AudioClip playingClip;
        float fadeInTime, fadeOutTime;

        public SoundEditorFader(BaseAudioFileObject _asset)
        {
            EditorApplication.update += Update;
            asset = _asset;
        }

        public void Dispose()
        {
            EditorApplication.update -= Update;
        }

        /// <summary>
        /// Can't use co-routines, so this is the alternative
        /// </summary>
        /// <param name="asset"></param>
        void Update()
        {
            if (!asset.fadeInOut)
            {
                AudioPlaybackToolEditor.helperSource.volume = asset.relativeVolume;
                return;
            }

            var helperSource = AudioPlaybackToolEditor.helperSource;
            if (helperSource.isPlaying)
            {
                if (helperSource.time < playingClip.length - fadeOutTime)
                {
                    if (fadeInTime == float.Epsilon)
                    {
                        helperSource.volume = asset.relativeVolume;
                    }
                    else
                    {
                        helperSource.volume = Mathf.Lerp(0, asset.relativeVolume, helperSource.time / fadeInTime);
                    }
                }
                else
                {
                    if (fadeOutTime == float.Epsilon)
                    {
                        helperSource.volume = asset.relativeVolume;
                    }
                    else
                    {
                        helperSource.volume = Mathf.Lerp(0, asset.relativeVolume, (playingClip.length - helperSource.time) / fadeOutTime);
                    }
                }
            }
        }

        public void StartFading(AudioClip audioClip)
        {
            playingClip = audioClip;
            AudioPlaybackToolEditor.helperSource.clip = playingClip;

            fadeInTime = asset.fadeInDuration * AudioPlaybackToolEditor.helperSource.clip.length;
            fadeOutTime = asset.fadeOutDuration * AudioPlaybackToolEditor.helperSource.clip.length;
            // To prevent divisions by 0
            if (fadeInTime == 0) fadeInTime = float.Epsilon;
            if (fadeOutTime == 0) fadeOutTime = float.Epsilon;

            AudioPlaybackToolEditor.soundHelper.PlayDebug((JSAMSoundFileObject)asset, false);
        }
    }

    [CustomEditor(typeof(JSAMSoundFileObject))]
    [CanEditMultipleObjects]
    public class SoundFileObjectEditor : BaseAudioFileObjectEditor<SoundFileObjectEditor>
    {
        Color buttonPressedColor = new Color(0.475f, 0.475f, 0.475f);
        Vector2 scroll;

        AudioClip playingClip;

        bool clipPlaying
        {
            get
            {
                return playingClip != null && AudioPlaybackToolEditor.helperSource.isPlaying;
            }
        }
        bool playingRandom;

        Texture2D cachedTex;
        AudioClip cachedClip;

        GUIContent openIcon;

        SoundEditorFader editorFader;

        const string SHOW_FADETOOL = "JSAM_SFO_SHOWFADETOOL";
        static bool showFadeTool
        {
            get
            {
                if (!EditorPrefs.HasKey(SHOW_FADETOOL))
                {
                    EditorPrefs.SetBool(SHOW_FADETOOL, false);
                }
                return EditorPrefs.GetBool(SHOW_FADETOOL);
            }
            set
            {
                EditorPrefs.SetBool(SHOW_FADETOOL, value);
            }
        }

        const string SHOW_LIBRARY = "JSAM_SFO_SHOWLIBRARY";
        static bool showLibrary
        {
            get
            {
                if (!EditorPrefs.HasKey(SHOW_LIBRARY))
                {
                    EditorPrefs.SetBool(SHOW_LIBRARY, false);
                }
                return EditorPrefs.GetBool(SHOW_LIBRARY);
            }
            set
            {
                EditorPrefs.SetBool(SHOW_LIBRARY, value);
            }
        }

        SerializedProperty neverRepeat;
        SerializedProperty fadeInOut;
        SerializedProperty fadeInDuration;
        SerializedProperty fadeOutDuration;
        SerializedProperty useLibrary;

        new protected void OnEnable()
        {
            base.OnEnable();

            EditorApplication.update += Update;
            Undo.undoRedoPerformed += OnUndoRedo;
            Undo.postprocessModifications += ApplyHelperEffects;

            if (target.name.Length > 0) // Creating from right-click dialog throws error here because name is invalid when first selected
            {
                //safeName.stringValue = JSAMEditorHelper.ConvertToAlphanumeric(target.name);
            }
            DesignateSerializedProperties();

            openIcon = EditorGUIUtility.TrIconContent("d_ScaleTool", "Click to open Playback Preview in a standalone window");

            AudioPlaybackToolEditor.CreateAudioHelper(asset.FirstAvailableFile);

            editorFader = new SoundEditorFader(asset);
            list = new AudioClipList(serializedObject, files);
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
            Undo.undoRedoPerformed -= OnUndoRedo;
            if (!AudioPlaybackToolEditor.WindowOpen)
            {
                AudioPlaybackToolEditor.DestroyAudioHelper();
            }
            Undo.postprocessModifications -= ApplyHelperEffects;

            if (editorFader != null)
            {
                editorFader.Dispose();
                editorFader = null;
            }
        }

        protected override void DesignateSerializedProperties()
        {
            base.DesignateSerializedProperties();

            neverRepeat = FindProp("neverRepeat");

            fadeInOut = FindProp(nameof(fadeInOut));
            excludedProperties.Add(nameof(fadeInOut));

            fadeInDuration = FindProp("fadeInDuration");
            fadeOutDuration = FindProp("fadeOutDuration");
            useLibrary = FindProp("useLibrary");
        }

        protected override void OnCreatePreset(string[] input)
        {
            presetDescription.stringValue = input[1];
            serializedObject.ApplyModifiedProperties();
            Preset newPreset = new Preset(asset as JSAMSoundFileObject);
            newPreset.excludedProperties = new string[] {
                "file", "files", "UsingLibrary", "category"
            };
            string path = JSAMSettings.Settings.PresetsPath + "/" + input[0] + ".preset";
            JSAMEditorHelper.CreateAssetSafe(newPreset, path);
        }

#if !UNITY_2019_3_OR_NEWER
        static bool filesFoldout;
#endif
        public override void OnInspectorGUI()
        {
            if (asset == null) return;

            serializedObject.UpdateIfRequiredOrScript();

            RenderPresetDescription();

            EditorGUILayout.Space();

            RenderGeneratePresetButton();

            Rect overlay = new Rect();
            showLibrary = EditorCompatability.SpecialFoldouts(showLibrary, "Library");
            if (showLibrary)
            {
                overlay = EditorGUILayout.BeginVertical(GUI.skin.box, new GUILayoutOption[] { GUILayout.MinHeight(150)/*, GUILayout.MaxHeight(150)*/ });

                scroll = EditorGUILayout.BeginScrollView(scroll);

                if (files.arraySize >= 0)
                {
                    list.Draw();
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorCompatability.EndSpecialFoldoutGroup();

            if (showLibrary)
            {
                string normalLabel = files.arraySize == 0 ? "Drag AudioClips here" : string.Empty;
                if (JSAMEditorHelper.DragAndDropRegion(overlay, normalLabel, "Release to add AudioClips"))
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        string filePath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[i]);
                        var clips = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<AudioClip>(filePath);

                        for (int j = 0; j < clips.Count; j++)
                        {
                            files.AddNewArrayElement().objectReferenceValue = clips[j];
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            
            blontent = new GUIContent("Use Library", "If true, the single AudioFile will be changed to a list of AudioFiles. AudioManager will choose a random AudioClip from this list when you play this sound");
            bool oldValue = asset.UsingLibrary;
            bool newValue = EditorGUILayout.Toggle(blontent, oldValue);
            if (newValue != oldValue) // If you clicked the toggle
            {
                // TODO: Convenience feature that quickly copies the existing file over to the library
                useLibrary.boolValue = newValue;
            }

            if (asset.UsingLibrary)
            {
                blontent = new GUIContent("Never Repeat", "Sometimes, AudioManager will allow the same sound from the Audio " +
                "library to play twice in a row, enabling this option will ensure that this audio file never plays the same " +
                "sound until after it plays a different sound.");
                EditorGUILayout.PropertyField(neverRepeat, blontent);
            }

            bool noFiles = asset.File == null && asset.IsLibraryEmpty;

            EditorGUILayout.PropertyField(relativeVolume);
            EditorGUILayout.PropertyField(spatialize);
            using (new EditorGUI.DisabledScope(!spatialize.boolValue))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(maxDistance);
                if (EditorGUI.EndChangeCheck())
                {
                    if (maxDistance.floatValue < 0)
                    {
                        maxDistance.floatValue = 0;
                    }
                }
            }

            DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());

            if (noFiles)
            {
                EditorGUILayout.HelpBox("Error! Add an audio file before running!", MessageType.Error);
            }
            if (asset.name.Contains("NEW AUDIO FILE") || asset.name.Equals("None") || asset.name.Equals("GameObject"))
            {
                EditorGUILayout.HelpBox("Warning! Change the name of the gameObject to something different or things will break!", MessageType.Warning);
            }

            if (playingClip == null)
            {
                DesignateActiveAudioClip(asset);
            }
            if (!noFiles && !AudioPlaybackToolEditor.WindowOpen) DrawPlaybackTool();

            DrawLoopPointTools(target as JSAMSoundFileObject);

            #region Fade Tools
            EditorGUILayout.PropertyField(fadeInOut);

            using (new EditorGUI.DisabledScope(!fadeInOut.boolValue))
            {
                if (!asset.IsLibraryEmpty)
                {
                    showFadeTool = EditorCompatability.SpecialFoldouts(showFadeTool, new GUIContent("Fade Tools", "Show/Hide the Audio Fade previewer"));
                    if (showFadeTool)
                    {
                        GUIContent fContent = new GUIContent();
                        GUIStyle rightJustified = new GUIStyle(EditorStyles.label);
                        rightJustified.alignment = TextAnchor.UpperRight;
                        rightJustified.padding = new RectOffset(0, 15, 0, 0);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent("Fade In Time:    " + (fadeInDuration.floatValue * playingClip.length).TimeToString(), "Fade in time for this AudioClip in seconds"));
                        EditorGUILayout.LabelField(new GUIContent("Sound Length: " + (playingClip.length).TimeToString(), "Length of the preview clip in seconds"), rightJustified);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.LabelField(new GUIContent("Fade Out Time: " + (fadeOutDuration.floatValue * playingClip.length).TimeToString(), "Fade out time for this AudioClip in seconds"));
                        float fid = fadeInDuration.floatValue;
                        float fod = fadeOutDuration.floatValue;
                        fContent = new GUIContent("Fade In Percentage", "The percentage of time the sound takes to fade-in relative to it's total length.");
                        fid = Mathf.Clamp(EditorGUILayout.Slider(fContent, fid, 0, 1), 0, 1 - fod);
                        fContent = new GUIContent("Fade Out Percentage", "The percentage of time the sound takes to fade-out relative to it's total length.");
                        fod = Mathf.Clamp(EditorGUILayout.Slider(fContent, fod, 0, 1), 0, 1 - fid);
                        fadeInDuration.floatValue = fid;
                        fadeOutDuration.floatValue = fod;
                        EditorGUILayout.HelpBox("Note: The sum of your Fade-In and Fade-Out durations cannot exceed 1 (the length of the sound).", MessageType.None);

                    }
                    EditorCompatability.EndSpecialFoldoutGroup();
                }
            }
#endregion

            if (!noFiles) DrawAudioEffectTools();

            if (serializedObject.hasModifiedProperties)
            {
                AudioPlaybackToolEditor.DoForceRepaint(true);
                serializedObject.ApplyModifiedProperties();

                // Manually fix variables
                if (asset.delay < 0)
                {
                    asset.delay = 0;
                    Undo.RecordObject(asset, "Fixed negative delay");
                }
            }

#region Quick Reference Guide
            string[] howToText = new string[]
            {
                "Overview",
                "Audio File Objects are containers that hold your sound files to be read by Audio Manager.",
                "No matter the filename or folder location, this Audio File will be referred to as it's name above",
                "Tips",
                "If your one sound has many different variations available, try enabling the \"Use Library\" option " +
                    "just below the name field. This let's AudioManager play a random different sound whenever you choose to play from this audio file object.",
                "Relative volume only helps to reduce how loud a sound is. To increase how loud an individual sound is, you'll have to " +
                    "edit it using a sound editor.",
                "You can always check what audio file objects you have loaded in AudioManager's library by selecting the AudioManager " +
                    "in the inspector and clicking on the drop-down near the bottom.",
                "If you want to better organize your audio file objects in AudioManager's library, you can assign a "+ 
                "category to this audio file object."
            };

            showHowTo = JSAMEditorHelper.RenderQuickReferenceGuide(showHowTo, howToText);
#endregion
        }

        protected override void DrawPlaybackTool()
        {
            blontent = new GUIContent("Audio Playback Preview", 
                "Allows you to preview how your AudioFileObject will sound during runtime right here in the inspector. " +
                "Some effects, like spatialization and delay, will not be available to preview");
            showPlaybackTool = EditorCompatability.SpecialFoldouts(showPlaybackTool, blontent);

            if (showPlaybackTool)
            {
                var helperSource = AudioPlaybackToolEditor.helperSource;
                ProgressBar(AudioPlaybackToolEditor.helperSource.time / playingClip.length, GetInfoString());

                EditorGUILayout.BeginHorizontal();
                Color colorbackup = GUI.backgroundColor;
                if (clipPlaying)
                {
                    GUI.backgroundColor = buttonPressedColor;
                    blontent = new GUIContent("Stop", "Stop playback");
                }
                else
                {
                    blontent = new GUIContent("Play", "Play a preview of the sound with it's current sound settings.");
                }
                if (GUILayout.Button(blontent))
                {
                    if (clipPlaying)
                    {
                        AudioPlaybackToolEditor.helperSource.Stop();
                        AudioPlaybackToolEditor.helperSource.time = 0;
                        if (playingRandom)
                        {
                            playingRandom = false;
                        }
                    }
                    else
                    {
                        if (playingClip != null)
                        {
                            editorFader.StartFading(playingClip);
                        }
                    }
                    AudioPlaybackToolEditor.DoForceRepaint(true);
                }
                GUI.backgroundColor = colorbackup;
                using (new EditorGUI.DisabledScope(files.arraySize < 2))
                {
                    if (GUILayout.Button(new GUIContent("Play Random", "Preview settings with a random track from your library. Only usable if this Audio File has \"Use Library\" enabled.")))
                    {
                        DesignateRandomAudioClip();
                        helperSource.Stop();
                        editorFader.StartFading(playingClip);
                    }
                }

                if (GUILayout.Button(openIcon, new GUILayoutOption[] { GUILayout.MaxHeight(19) }))
                {
                    AudioPlaybackToolEditor.Init();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorCompatability.EndSpecialFoldoutGroup();
        }

        public void DesignateActiveAudioClip(BaseAudioFileObject asset)
        {
            AudioClip theClip = null;
            if (!asset.IsLibraryEmpty)
            {
                theClip = asset.FirstAvailableFile;
            }
            if (theClip != null)
            {
                playingClip = theClip;
            }
        }

        public AudioClip DesignateRandomAudioClip()
        {
            AudioClip theClip = playingClip;
            if (!asset.IsLibraryEmpty)
            {
                List<AudioClip> files = asset.Files;
                while (theClip == null || theClip == playingClip)
                {
                    theClip = files[Random.Range(0, files.Count)];
                }
            }
            playingClip = theClip;
            playingRandom = true;
            AudioPlaybackToolEditor.DoForceRepaint(true);
            return playingClip;
        }

        void Update()
        {
            if (asset == null) return; // This can happen on the same frame it's deleted
            AudioClip clip = asset.FirstAvailableFile;
            if (playingClip != null && clip != null)
            {
                if (!AudioPlaybackToolEditor.WindowOpen)
                {
                    if (clip != cachedClip)
                    {
                        AudioPlaybackToolEditor.DoForceRepaint(true);
                        cachedClip = asset.FirstAvailableFile;
                        playingClip = cachedClip;
                    }

                    if (!clipPlaying && playingRandom)
                    {
                        DesignateActiveAudioClip(asset);
                    }
                }

                if (clipPlaying)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                }
            }
        }

        void OnUndoRedo()
        {
            AudioPlaybackToolEditor.DoForceRepaint(true);
        }

        public UndoPropertyModification[] ApplyHelperEffects(UndoPropertyModification[] modifications)
        {
            if (AudioPlaybackToolEditor.helperSource.isPlaying)
            {
                AudioPlaybackToolEditor.soundHelper.ApplyEffects();
            }
            return modifications;
        }

        GameObject helperObject;

        /// <summary>
        /// Conveniently draws a progress bar
        /// Referenced from the official Unity documentation
        /// https://docs.unity3d.com/ScriptReference/Editor.html
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        Rect ProgressBar(float value, string label)
        {
            // Get a rect for the progress bar using the same margins as a TextField:
            Rect rect = GUILayoutUtility.GetRect(64, 64, "TextField");

            AudioClip sound = playingClip;

            if ((cachedTex == null || AudioPlaybackToolEditor.forceRepaint) && Event.current.type == EventType.Repaint)
            {
                Texture2D waveformTexture = AudioPlaybackToolEditor.RenderStaticPreview(sound, rect, asset.relativeVolume);
                cachedTex = waveformTexture;
                if (waveformTexture != null)
                    GUI.DrawTexture(rect, waveformTexture);
                AudioPlaybackToolEditor.forceRepaint = false;
            }
            else
            {
                GUI.DrawTexture(rect, cachedTex);
            }

            if (playingClip != null)
            {
                Rect progressRect = new Rect(rect);
                progressRect.width *= value;
                progressRect.xMin = progressRect.xMax - 1;
                GUI.Box(progressRect, "", "SelectionRect");
            }

            EditorGUILayout.Space();

            return rect;
        }

        public static void DrawPropertyOverlay(JSAMSoundFileObject sound, int width, int height)
        {
            if (Event.current.type != EventType.Repaint) return;

            if (sound.fadeInOut)
            {
                Rect newRect = new Rect();

                // Draw Loop Start
                newRect.height = height;
                newRect.xMax = sound.fadeInDuration * width;
                float firstLabel = newRect.xMax;

                JSAMEditorHelper.BeginColourChange(Color.magenta);
                GUI.Box(newRect, "", "SelectionRect");
                newRect.xMin = newRect.xMax - 48;
                newRect.x += 48;
                JSAMEditorHelper.EndColourChange();
                GUI.Label(newRect, new GUIContent("Fade In"), EditorStyles.label.ApplyTextAnchor(TextAnchor.UpperRight));
                newRect.xMax = newRect.xMin + 2;
                JSAMEditorHelper.BeginColourChange(Color.black);
                GUI.Box(newRect, "", "SelectionRect");
                JSAMEditorHelper.EndColourChange();

                // Draw Loop End
                newRect.height = height;
                newRect.xMin = (1 - sound.fadeOutDuration) * width;
                float secondLabel = newRect.xMin;
                newRect.xMax = width;
                JSAMEditorHelper.BeginColourChange(Color.magenta);
                GUI.Box(newRect, "", "SelectionRect");
                JSAMEditorHelper.EndColourChange();
                newRect.height = 35;
                if (newRect.width < 60)
                {
                    newRect.width = 100;
                    newRect.x -= 60;
                }
                var style = EditorStyles.label.ApplyTextAnchor(TextAnchor.UpperLeft);
                newRect.x += 5;

                if (secondLabel - firstLabel < 70)
                {
                    style = EditorStyles.label.ApplyTextAnchor(TextAnchor.LowerLeft);
                }
                GUI.Label(newRect, new GUIContent("Fade Out"), style);

                newRect.x = 0;
                newRect.height = height;
                newRect.xMin = (1 - sound.fadeOutDuration) * width;
                newRect.xMax = newRect.xMin + 2;
                JSAMEditorHelper.BeginColourChange(Color.black);
                GUI.Box(newRect, "", "SelectionRect");
                JSAMEditorHelper.EndColourChange();
            }
        }
    }
}