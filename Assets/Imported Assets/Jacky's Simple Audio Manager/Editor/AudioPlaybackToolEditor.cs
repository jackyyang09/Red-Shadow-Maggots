using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace JSAM.JSAMEditor
{
    /// <summary>
    /// Handles the Playback Tool Editor Window
    /// Can play AudioFileObjects, AudioFileMusicObjects and generic AudioClips
    /// Double click on the former to automatically open the window
    /// </summary>
    public class AudioPlaybackToolEditor : EditorWindow
    {
        SoundEditorFader soundFader;
        SoundEditorFader SoundFader
        {
            get
            {
                if (soundFader == null)
                {
                    soundFader = new SoundEditorFader(selectedSound);
                }
                return soundFader;
            }
        }

        static Texture2D cachedTex;
        public static bool forceRepaint;

        static Vector2 dragStartPos = Vector2.zero;
        static bool mouseGrabbed = false;
        static bool mouseDragging = false;
        static bool mouseScrubbed = false;
        static bool loopClip = false;
        static bool clipPlaying = false;
        static bool clipPaused = false;

        public static GameObject helperObject;
        public static AudioSource helperSource;
        public static bool HelperSourceActive 
        { 
            get
            {
                if (!helperSource) return false;
                if (!AudioManager.Instance) return false;
                if (!helperSource.clip) return false;
                return true;
            } 
        }
        public static JSAMSoundChannelHelper soundHelper;
        public static JSAMMusicChannelHelper musicHelper;

        static Color buttonPressedColor = new Color(0.475f, 0.475f, 0.475f);
        static Color buttonPressedColorLighter = new Color(0.75f, 0.75f, 0.75f);

        static Vector2 lastWindowSize = Vector2.zero;
        static bool resized = false;

        static bool showHowTo;
        static float playbackPreviewClamped = 300;
        static bool showLibraryView = false;

        static PreviewRenderUtility m_PreviewUtility;

        static AudioPlaybackToolEditor window;
        public static AudioPlaybackToolEditor Window
        {
            get
            {
                if (window == null) window = GetWindow<AudioPlaybackToolEditor>();
                return window;
            }
        }

        public static bool WindowOpen
        {
            get
            {
#if UNITY_2019_4_OR_NEWER
                return HasOpenInstances<AudioPlaybackToolEditor>();
#else
                return window != null;
#endif
            }
        }

        SoundFileObjectEditor soundFileEditorInstance 
        { 
            get
            {
                if (SoundFileObjectEditor.instance == null)
                {
                    if (selectedSound)
                    {
                        var lastSelection = Selection.activeObject;
                        Selection.activeObject = selectedSound;
                    }
                }
                return SoundFileObjectEditor.instance;
            } 
        }

        static System.Action OnToggleHowTo;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/JSAM/JSAM Playback Tool")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = GetWindow<AudioPlaybackToolEditor>();
            window.Show();
            window.Focus();
            window.titleContent.text = "JSAM Playback Tool";
            // Refresh window contents
            window.OnSelectionChange();
            lastWindowSize = Window.position.size;
        }

        [OnOpenAsset]
        public static bool OnDoubleClickAssets(int instanceID, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            JSAMSoundFileObject audioFile = AssetDatabase.LoadAssetAtPath<JSAMSoundFileObject>(assetPath);
            if (audioFile)
            {
                Init();
                // Force a repaint
                Selection.activeObject = null;
                EditorApplication.delayCall += () => Selection.activeObject = audioFile;
                return true;
            }
            JSAMMusicFileObject audioFileMusic = AssetDatabase.LoadAssetAtPath<JSAMMusicFileObject>(assetPath);
            if (audioFileMusic)
            {
                Init();
                // Force a repaint
                Selection.activeObject = null;
                EditorApplication.delayCall += () => Selection.activeObject = audioFileMusic;
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            OnToggleHowTo += AdjustWindowSize;
            SetIcons();
        }

        private void OnDisable()
        {
            if (SoundFader != null)
            {
                SoundFader.Dispose();
                soundFader = null;
            }

            OnToggleHowTo -= AdjustWindowSize;

            window = null;

            if (selectedSound)
            {
                Selection.activeObject = null;
                EditorApplication.delayCall += () => Selection.activeObject = selectedSound;
            }
            else if (selectedMusic)
            {
                Selection.activeObject = null;
                EditorApplication.delayCall += () => Selection.activeObject = selectedMusic;
            }

            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }

            DestroyAudioHelper();
        }

        static float preHowToToggleWindowSize = 0;
        private void AdjustWindowSize()
        {
            Rect newRect = new Rect(Window.position);

            if (showHowTo)
            {
                preHowToToggleWindowSize = Window.position.height;
                float heightIncrease = JSAMEditorHelper.lastGuideSize.y;
                newRect.height += heightIncrease;
            }
            else
            {
                if (preHowToToggleWindowSize > 0)
                {
                    newRect.height = preHowToToggleWindowSize;
                }
            }

            Window.position = newRect;
        }

        private void OnGUI()
        {
            DrawPlaybackTool(selectedClip, selectedSound, selectedMusic);
            EditorGUILayout.BeginHorizontal();
            if (HelperSourceActive)
            {
                EditorGUILayout.LabelField("Now Playing - " + helperSource.clip.name);
            }
            else
            {
                EditorGUILayout.LabelField("-");
            }
            EditorGUILayout.EndHorizontal();

            if (!resized)
            {
                if (lastWindowSize != Window.position.size)
                {
                    lastWindowSize = Window.position.size;
                    resized = true;
                }
            }
            if (selectedSound)
            {
                if (selectedSound.Files.Count > 1)
                {
                    showLibraryView = EditorCompatability.SpecialFoldouts(showLibraryView, "Show Audio File Object Library");
                    if (showLibraryView)
                    {
                        for (int i = 0; i < selectedSound.Files.Count; i++)
                        {
                            AudioClip sound = selectedSound.Files[i];
                            Color colorbackup = GUI.backgroundColor;
                            //EditorGUILayout.BeginHorizontal();
                            if (helperSource.clip == sound) GUI.backgroundColor = buttonPressedColor;
                            GUIContent bContent = new GUIContent(sound.name, "Click to change AudioClip being played back to " + sound.name);
                            if (GUILayout.Button(bContent))
                            {
                                // Play the sound
                                selectedClip = sound;
                                helperSource.clip = selectedClip;
                                SoundFader.StartFading(helperSource.clip, selectedSound);
                                clipPlaying = true;
                            }
                            //EditorGUILayout.EndHorizontal();
                            GUI.backgroundColor = colorbackup;
                        }
                    }
                    EditorCompatability.EndSpecialFoldoutGroup();
                }
            }

            #region Quick Reference Guide
            EditorGUI.BeginChangeCheck();
            JSAMEditorHelper.StartMeasureLastGuideSize();
            showHowTo = JSAMEditorHelper.RenderQuickReferenceGuide(showHowTo, new string[]
            {
                "Overview",
                "This EditorWindow serves as a high-fidelity alternative to the small playback preview in the" +
                "inspector window used when inspecting Audio File Objects.",
                "Tips",
                "The active playing clip can be changed by selecting different assets in the Project window.",
                "You can open the JSAM Playback Tool by double-clicking on Audio File assets in the Project window.",
                "The JSAM Playback Tool can also play standard AudioClips!"
            });
            if (EditorGUI.EndChangeCheck())
            {
                OnToggleHowTo?.Invoke();
            }
            if (!showHowTo)
            {
                // Dirty hack
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            #endregion
        }

        AudioClip selectedClip;
        JSAMSoundFileObject selectedSound;
        JSAMMusicFileObject selectedMusic;
        private void OnSelectionChange()
        {
            if (Selection.activeObject == null) return;
            System.Type activeType = Selection.activeObject.GetType();

            if (activeType.Equals(typeof(AudioClip)))
            {
                selectedSound = null;
                selectedMusic = null;

                selectedClip = (AudioClip)Selection.activeObject;
                CreateAudioHelper(selectedClip);
            }
            else if (activeType.Equals(typeof(JSAMSoundFileObject)))
            {
                selectedMusic = null;

                selectedSound = ((JSAMSoundFileObject)Selection.activeObject);
                selectedClip = selectedSound.Files[0];
                CreateAudioHelper(selectedClip);
            }
            else if (activeType.Equals(typeof(JSAMMusicFileObject)))
            {
                selectedSound = null;

                selectedMusic = ((JSAMMusicFileObject)Selection.activeObject);
                selectedClip = selectedMusic.Files[0];
                CreateAudioHelper(selectedClip);
            }
            else
            {
                DoForceRepaint(true);
                return;
            }
            helperSource.clip = selectedClip;

            DoForceRepaint(true);
        }

        /// <summary>
        /// Draws a playback 
        /// </summary>
        /// <param name="music"></param>
        public void DrawPlaybackTool(AudioClip selectedClip, JSAMSoundFileObject selectedSound = null, JSAMMusicFileObject selectedMusic = null)
        {
            float progress = 0;
            if (HelperSourceActive)
            {
                progress = (float)helperSource.timeSamples / (float)helperSource.clip.samples;
            }
            Rect progressRect = ProgressBar(progress, selectedClip, selectedSound, selectedMusic);
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
            {
                DoForceRepaint(true);
            }

            EditorGUILayout.BeginHorizontal();

            PollMouseEvents(progressRect);

            using (new EditorGUI.DisabledScope(!HelperSourceActive))
            {
                if (GUILayout.Button(s_BackIcon, new GUILayoutOption[] { GUILayout.MaxHeight(20) }))
                {
                    if (selectedMusic)
                    {
                        if (selectedMusic.loopMode == LoopMode.ClampedLoopPoints)
                        {
                            helperSource.timeSamples = Mathf.CeilToInt((selectedMusic.loopStart * selectedClip.frequency));
                        }
                    }
                    else
                    {
                        helperSource.timeSamples = 0;
                    }
                    helperSource.Stop();
                    mouseScrubbed = false;
                    clipPaused = false;
                    clipPlaying = false;
                }

                // Draw Play Button
                GUIContent buttonIcon = (clipPlaying) ? s_PlayIcons[1] : s_PlayIcons[0];
                if (clipPlaying) JSAMEditorHelper.BeginBackgroundColourChange(buttonPressedColor);
                if (GUILayout.Button(buttonIcon, new GUILayoutOption[] { GUILayout.MaxHeight(20) }))
                {
                    clipPlaying = !clipPlaying;
                    if (clipPlaying)
                    {
                        // Note: For some reason, reading from helperSource.time returns 0 even if timeSamples is not 0
                        // However, writing a value to helperSource.time changes timeSamples to the appropriate value just fine
                        if (selectedSound)
                        {
                            SoundFader.StartFading(helperSource.clip, selectedSound);
                        }
                        else if (selectedMusic)
                        {
                            musicHelper.PlayDebug(selectedMusic, mouseScrubbed);
                            MusicFileObjectEditor.firstPlayback = true;
                            MusicFileObjectEditor.freePlay = false;
                        }
                        else if (selectedClip)
                        {
                            soundHelper.PlayDebug(mouseScrubbed);
                        }
                        if (clipPaused) helperSource.Pause();
                    }
                    else
                    {
                        helperSource.Stop();
                        if (!mouseScrubbed)
                        {
                            helperSource.time = 0;
                        }
                        clipPaused = false;
                    }
                }
                if (clipPlaying) JSAMEditorHelper.EndBackgroundColourChange();

                // Pause button
                GUIContent theText = (clipPaused) ? s_PauseIcons[1] : s_PauseIcons[0];
                if (clipPaused) JSAMEditorHelper.BeginBackgroundColourChange(buttonPressedColor);
                if (GUILayout.Button(theText, new GUILayoutOption[] { GUILayout.MaxHeight(20) }))
                {
                    clipPaused = !clipPaused;
                    if (clipPaused)
                    {
                        helperSource.Pause();
                    }
                    else
                    {
                        helperSource.UnPause();
                    }
                }
                if (clipPaused) JSAMEditorHelper.EndBackgroundColourChange();

                // Loop button
                buttonIcon = (loopClip) ? s_LoopIcons[1] : s_LoopIcons[0];
                if (loopClip) JSAMEditorHelper.BeginBackgroundColourChange(buttonPressedColor);
                if (GUILayout.Button(buttonIcon, new GUILayoutOption[] { GUILayout.MaxHeight(20) }))
                {
                    loopClip = !loopClip;
                    // helperSource.loop = true;
                }
                if (loopClip) JSAMEditorHelper.EndBackgroundColourChange();

                if (selectedSound)
                {
                    using (new EditorGUI.DisabledScope(selectedSound.Files.Count < 2))
                    {
                        if (GUILayout.Button(new GUIContent("Play Random", "Preview settings with a random track from your library. Only usable if this Audio File has \"Use Library\" enabled.")))
                        {
                            selectedClip = soundFileEditorInstance.DesignateRandomAudioClip();
                            clipPlaying = true;
                            helperSource.Stop();
                            SoundFader.StartFading(selectedClip, selectedSound);
                        }
                    }
                }

                if (GUILayout.Button(lockIcon, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxHeight(18) }))
                {

                }


                int loopPointInputMode = 0;
                // Reset loop point input mode if not using loop points so the duration shows up as time by default
                if (selectedMusic)
                {
                    if (selectedMusic.loopMode != LoopMode.LoopWithLoopPoints) loopPointInputMode = 0;
                }

                GUIContent blontent = new GUIContent();
                if (HelperSourceActive)
                {
                    switch ((MusicFileObjectEditor.LoopPointTool)loopPointInputMode)
                    {
                        case MusicFileObjectEditor.LoopPointTool.Slider:
                        case MusicFileObjectEditor.LoopPointTool.TimeInput:
                            blontent = new GUIContent(TimeToString((float)helperSource.timeSamples / helperSource.clip.frequency) + " / " + (TimeToString(helperSource.clip.length)),
                                "The playback time in seconds");
                            break;
                        case MusicFileObjectEditor.LoopPointTool.TimeSamplesInput:
                            blontent = new GUIContent(helperSource.timeSamples + " / " + helperSource.clip.samples, "The playback time in samples");
                            break;
                        case MusicFileObjectEditor.LoopPointTool.BPMInput:
                            blontent = new GUIContent(string.Format("{0:0}", helperSource.time / (60f / selectedMusic.bpm)) + " / " + helperSource.clip.length / (60f / selectedMusic.bpm),
                                "The playback time in beats");
                            break;
                    }
                }
                else
                {
                    blontent = new GUIContent("00:00:00 / 00:00:00");
                }

                GUIStyle rightJustified = new GUIStyle(EditorStyles.label);
                rightJustified.alignment = TextAnchor.UpperRight;
                EditorGUILayout.LabelField(blontent, rightJustified);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        void PollMouseEvents(Rect progressRect)
        {
            Event evt = Event.current;

            if (evt.isMouse)
            {
                switch (evt.type)
                {
                    case EventType.MouseUp:
                        switch (evt.button)
                        {
                            case 0:
                                mouseDragging = false;
                                break;
                            case 2:
                                mouseGrabbed = false;
                                break;
                        }
                        break;
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        if (!HelperSourceActive) return;
                        if (evt.button == 0) // Left Mouse Events
                        {
                            if (evt.type == EventType.MouseDown)
                            {
                                // Only begin dragging if mouse is in the waveform window
                                if (evt.mousePosition.y > progressRect.yMin && evt.mousePosition.y < progressRect.yMax)
                                {
                                    mouseDragging = true;
                                    mouseScrubbed = true;
                                }
                                else mouseDragging = false;
                            }
                            if (!mouseDragging) break;
                            float newProgress = Mathf.InverseLerp(progressRect.xMin, progressRect.xMax, evt.mousePosition.x);
                            helperSource.time = Mathf.Clamp((newProgress * helperSource.clip.length), 0, helperSource.clip.length - AudioManagerInternal.EPSILON);

                            if (selectedMusic)
                            {
                                if (selectedMusic.loopMode == LoopMode.ClampedLoopPoints)
                                {
                                    float start = selectedMusic.loopStart * selectedMusic.Files[0].frequency;
                                    float end = selectedMusic.loopEnd * selectedMusic.Files[0].frequency;
                                    helperSource.timeSamples = (int)Mathf.Clamp(helperSource.timeSamples, start, end - AudioManagerInternal.EPSILON);
                                }
                            }
                        }
                        break;
                }
            }
        }

        public static void CreateAudioHelper(AudioClip selectedClip)
        {
            if (helperObject == null)
            {
                helperObject = GameObject.Find("JSAM Audio Helper");
                if (helperObject == null)
                {
                    helperObject = new GameObject("JSAM Audio Helper");
                    helperSource = helperObject.AddComponent<AudioSource>();
                    helperSource.playOnAwake = false;
                    helperSource.clip = selectedClip;

                    soundHelper = helperObject.AddComponent<JSAMSoundChannelHelper>();
                    soundHelper.Init(AudioManager.Instance.Settings.SoundGroup);
                    musicHelper = helperObject.AddComponent<JSAMMusicChannelHelper>();
                    musicHelper.Init(AudioManager.Instance.Settings.MusicGroup);
                }
                else
                {
                    soundHelper = helperObject.GetComponent<JSAMSoundChannelHelper>();
                    musicHelper = helperObject.GetComponent<JSAMMusicChannelHelper>();
                }
                helperObject.hideFlags = HideFlags.HideAndDontSave;
            }

            if (helperSource == null)
            {
                helperSource = helperObject.GetComponent<AudioSource>();
                helperSource.clip = selectedClip;
            }
        }

        public static void DestroyAudioHelper()
        {
            if (helperSource)
            {
                helperSource.Stop();
            }
            if (!WindowOpen && !SoundFileObjectEditor.instance && !MusicFileObjectEditor.instance)
            {
                DestroyImmediate(helperObject);
            }
        }

        /// <summary>
        /// Conveniently draws a progress bar
        /// Referenced from the official Unity documentation
        /// https://docs.unity3d.com/ScriptReference/Editor.html
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static Rect ProgressBar(float value, AudioClip selectedClip, JSAMSoundFileObject selectedSound = null, JSAMMusicFileObject selectedMusic = null)
        {
            //float maxHeight = (showHowTo) ? playbackPreviewClamped : 4000;
            //float minHeight = (showHowTo) ? playbackPreviewClamped : 64;
            Rect rect = GUILayoutUtility.GetRect(64, 4000, 64, 4000);

            Texture2D waveformTexture;
            if (selectedClip != null)
            {
                if (selectedSound)
                {
                    waveformTexture = RenderStaticPreview(selectedClip, rect, selectedSound.relativeVolume);
                }
                else if (selectedMusic)
                {
                    waveformTexture = RenderStaticPreview(selectedClip, rect, selectedMusic.relativeVolume);
                }
                else
                {
                    waveformTexture = RenderStaticPreview(selectedClip, rect, 1);
                }

                cachedTex = waveformTexture;

                if (waveformTexture != null)
                {
                    GUI.DrawTexture(rect, waveformTexture);
                }
            }
            else
            {
                GUIStyle style = GUI.skin.box.ApplyTextAnchor(TextAnchor.MiddleCenter)
                                .SetFontSize(30)
                                .ApplyBoldText();
                GUI.Box(rect, "Select an Audio File to preview it here", style);
            }
            
            forceRepaint = false;

            if (selectedSound) SoundFileObjectEditor.DrawPropertyOverlay(selectedSound, (int)rect.width, (int)rect.height);
            else if (selectedMusic) MusicFileObjectEditor.DrawPropertyOverlay(selectedMusic, (int)rect.width, (int)rect.height);

            Rect progressRect = new Rect(rect);
            progressRect.width = value * rect.width;
            progressRect.xMin = progressRect.xMax - 1;
            GUI.Box(progressRect, "", "SelectionRect");

            EditorGUILayout.Space();

            return rect;
        }

        void Update()
        {
            if (selectedClip == null) return;
            if (helperSource == null) CreateAudioHelper(selectedClip);

            if (!helperSource.isPlaying && mouseDragging || resized)
            {
                DoForceRepaint(resized);
            }

            #region Sound Update
            if (selectedSound || (selectedClip && !selectedMusic))
            {
                if ((clipPlaying && !clipPaused) || (mouseDragging && clipPlaying))
                {
                    float clipPos = helperSource.timeSamples / (float)selectedClip.frequency;
                    
                    Repaint();

                    if (!helperSource.isPlaying && !clipPaused && clipPlaying)
                    {
                        helperSource.time = 0;
                        if (loopClip)
                        {
                            helperSource.Play();
                        }
                        else
                        {
                            clipPlaying = false;
                        }
                    }
                }
            }
            #endregion
            #region Music Update
            if (selectedMusic)
            {
                if ((clipPlaying && !clipPaused) || (mouseDragging && clipPlaying))
                {
                    float clipPos = helperSource.timeSamples / (float)selectedClip.frequency;
                    helperSource.volume = selectedMusic.relativeVolume;
                    helperSource.pitch = selectedMusic.startingPitch;

                    Repaint();

                    if (loopClip)
                    {
                        if (selectedMusic.loopMode == LoopMode.LoopWithLoopPoints || selectedMusic.loopMode == LoopMode.ClampedLoopPoints)
                        {
                            if (!helperSource.isPlaying && clipPlaying && !clipPaused)
                            {
                                if (MusicFileObjectEditor.freePlay)
                                {
                                    helperSource.Play();
                                }
                                else
                                {
                                    helperSource.Play();
                                    helperSource.timeSamples = Mathf.CeilToInt(selectedMusic.loopStart * selectedClip.frequency);
                                }
                                MusicFileObjectEditor.freePlay = false;
                            }
                            else if (selectedMusic.loopMode == LoopMode.ClampedLoopPoints || !MusicFileObjectEditor.firstPlayback)
                            {
                                if (clipPos < selectedMusic.loopStart || clipPos > selectedMusic.loopEnd)
                                {
                                    // CeilToInt to guarantee clip position stays within loop bounds
                                    helperSource.timeSamples = Mathf.CeilToInt(selectedMusic.loopStart * selectedClip.frequency);
                                    MusicFileObjectEditor.firstPlayback = false;
                                }
                            }
                            else if (clipPos >= selectedMusic.loopEnd)
                            {
                                helperSource.timeSamples = Mathf.CeilToInt(selectedMusic.loopStart * selectedClip.frequency);
                                MusicFileObjectEditor.firstPlayback = false;
                            }
                        }
                    }
                    else if (!loopClip)
                    {
                        if (selectedMusic.loopMode == LoopMode.LoopWithLoopPoints)
                        {
                            if ((!helperSource.isPlaying && !clipPaused) || clipPos > selectedMusic.loopEnd)
                            {
                                clipPlaying = false;
                                helperSource.Stop();
                            }
                        }
                        else if (selectedMusic.loopMode == LoopMode.ClampedLoopPoints && clipPos < selectedMusic.loopStart)
                        {
                            helperSource.timeSamples = Mathf.CeilToInt(selectedMusic.loopStart * selectedClip.frequency);
                        }
                    }
                }

                if (selectedMusic.loopMode != LoopMode.LoopWithLoopPoints)
                {
                    if (!helperSource.isPlaying && !clipPaused && clipPlaying)
                    {
                        helperSource.time = 0;
                        if (loopClip)
                        {
                            helperSource.Play();
                        }
                        else
                        {
                            clipPlaying = false;
                        }
                    }
                }
            }
            #endregion
        }

        #region Unity's Asset Preview render code
        /// <summary>
        /// Borrowed from Unity
        /// <para>https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/AudioClipInspector.cs</para>
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Texture2D RenderStaticPreview(AudioClip clip, Rect rect, float relativeVolume)
        {
            GUI.Box(rect, "");
            float[] samples = new float[clip.samples];
            float[] waveform = new float[(int)rect.width];
            clip.GetData(samples, 0);
            int packSize = clip.samples / (int)rect.width + 1;
            int s = 0;

            for (int i = 0; i < clip.samples; i += packSize)
            {
                waveform[s] = samples[i];
                s++;
            }

            float halfHeight = rect.height / 2f;

            List<Vector3> points = new List<Vector3>();

            Handles.color = new Color(1.0f, 140.0f / 255.0f, 0.0f, 1.0f);
            for (int x = 0; x < waveform.Length; x++)
            {
                Vector2 currentPoint = new Vector2(x, (waveform[x] * rect.height * 0.5f * relativeVolume) + halfHeight);
                currentPoint += rect.position;
                points.Add(currentPoint);
            }

            Handles.DrawPolyLine(points.ToArray());
            return null;
        }
        #endregion

        static GUIContent s_BackIcon = null;
        static GUIContent[] s_PlayIcons = { null, null };
        static GUIContent[] s_PauseIcons = { null, null };
        static GUIContent[] s_LoopIcons = { null, null };
        static GUIContent lockIcon = null;

        /// <summary>
        /// Why does Unity keep all this stuff secret?
        /// https://unitylist.com/p/5c3/Unity-editor-icons
        /// </summary>
        static void SetIcons()
        {
            s_BackIcon = EditorGUIUtility.TrIconContent("beginButton", "Click to Reset Playback Position");
#if UNITY_2019_4_OR_NEWER
            s_PlayIcons[0] = EditorGUIUtility.TrIconContent("d_PlayButton", "Click to Play");
            s_PlayIcons[1] = EditorGUIUtility.TrIconContent("d_PlayButton On", "Click to Stop");
#else
            s_PlayIcons[0] = EditorGUIUtility.TrIconContent("preAudioPlayOff", "Click to Play");
            s_PlayIcons[1] = EditorGUIUtility.TrIconContent("preAudioPlayOn", "Click to Stop");
#endif
            s_PauseIcons[0] = EditorGUIUtility.TrIconContent("PauseButton", "Click to Pause");
            s_PauseIcons[1] = EditorGUIUtility.TrIconContent("PauseButton On", "Click to Unpause");
#if UNITY_2019_4_OR_NEWER
            s_LoopIcons[0] = EditorGUIUtility.TrIconContent("d_preAudioLoopOff", "Click to enable looping");
            s_LoopIcons[1] = EditorGUIUtility.TrIconContent("preAudioLoopOff", "Click to disable looping");
#else
            s_LoopIcons[0] = EditorGUIUtility.TrIconContent("playLoopOff", "Click to enable looping");
            s_LoopIcons[1] = EditorGUIUtility.TrIconContent("playLoopOn", "Click to disable looping");
#endif
            lockIcon = EditorGUIUtility.TrIconContent("InspectorLock", "Toggles the changing of audio when selecting inspector objects");
        }

        public static string TimeToString(float time)
        {
            time *= 1000;
            int minutes = (int)time / 60000;
            int seconds = (int)time / 1000 - 60 * minutes;
            int milliseconds = (int)time - minutes * 60000 - 1000 * seconds;
            return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        public static void DoForceRepaint(bool fullRepaint = false)
        {
            forceRepaint = fullRepaint;
            if (WindowOpen)
            {
                resized = false;
                Window.Repaint();
            }
        }
    }
}