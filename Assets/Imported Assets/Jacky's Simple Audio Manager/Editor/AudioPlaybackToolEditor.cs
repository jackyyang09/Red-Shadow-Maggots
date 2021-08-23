using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace JSAM.JSAMEditor
{
    /// <summary>
    /// Handles the Playback Tool Editor Window
    /// Can play AudioFileObjects, AudioFileMusicObjects and generic AudioClips
    /// Double click on the former to automatically open the window
    /// </summary>
    public class AudioPlaybackToolEditor : EditorWindow
    {
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
        public static JSAMSoundChannelHelper soundHelper;
        public static JSAMMusicChannelHelper musicHelper;

        static Color buttonPressedColor = new Color(0.475f, 0.475f, 0.475f);
        static Color buttonPressedColorLighter = new Color(0.75f, 0.75f, 0.75f);

        static float scrollbarProgress = 0;
        static float trueScrollProgress = 0;
        public const float MAX_SCROLL_ZOOM = 50;
        public static float scrollZoom = MAX_SCROLL_ZOOM;

        static Vector2 lastWindowSize = Vector2.zero;
        static bool resized = false;

        static bool showHowTo;
        static Vector2 guideScrollProgress = Vector2.zero;
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
            SetIcons();
        }

        private void OnDisable()
        {
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

        private void OnGUI()
        {
            if (selectedMusic || selectedClip || selectedSound)
            {
                DrawPlaybackTool(selectedClip, selectedSound, selectedMusic);
                EditorGUILayout.LabelField("Now Playing - " + helperSource.clip.name);

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
                    if (selectedSound.UsingLibrary && selectedSound.FileCount > 1)
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
                                    SoundFileObjectEditor.instance.StartFading(helperSource.clip);
                                    clipPlaying = true;
                                }
                                //EditorGUILayout.EndHorizontal();
                                GUI.backgroundColor = colorbackup;
                            }
                        }
                        EditorCompatability.EndSpecialFoldoutGroup();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No JSAM Audio File selected, select one in the Project window to preview it!"
                    , MessageType.Info);
                EditorGUILayout.Space();
            }

            #region Quick Reference Guide
            showHowTo = EditorCompatability.SpecialFoldouts(showHowTo, "Quick Reference Guide (Expand window before opening)");
            if (showHowTo)
            {
                Window.minSize = new Vector2(Window.minSize.x, Mathf.Clamp(Window.minSize.x, playbackPreviewClamped, 4000));
                guideScrollProgress = EditorGUILayout.BeginScrollView(guideScrollProgress, new GUILayoutOption[] { GUILayout.ExpandHeight(true) });

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "This EditorWindow serves as a high-fidelity alternative to the small playback preview in the inspector window used when inspecting Audio File Objects."
                    , MessageType.None);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Tips", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "The active playing clip can be changed by selecting different assets in the Project window."
                    , MessageType.None);
                EditorGUILayout.HelpBox(
                    "You can open the JSAM Playback Tool by double-clicking on Audio File assets in the Project window."
                    , MessageType.None);
                EditorGUILayout.HelpBox(
                    "The JSAM Playback Tool can also play standard AudioClips!"
                    , MessageType.None);

                EditorGUILayout.EndScrollView();
            }
            EditorCompatability.EndSpecialFoldoutGroup();
            #endregion
        }

        AudioClip selectedClip;
        JSAMSoundFileObject selectedSound;
        JSAMMusicFileObject selectedMusic;
        private void OnSelectionChange()
        {
            if (Selection.activeObject == null) return;
            System.Type activeType = Selection.activeObject.GetType();

            selectedClip = null;
            selectedSound = null;
            selectedMusic = null;

            if (activeType.Equals(typeof(AudioClip)))
            {
                selectedClip = (AudioClip)Selection.activeObject;
                CreateAudioHelper(selectedClip);
            }
            else if (activeType.Equals(typeof(JSAMSoundFileObject)))
            {
                selectedSound = ((JSAMSoundFileObject)Selection.activeObject);
                selectedClip = selectedSound.File;
                CreateAudioHelper(selectedClip);
            }
            else if (activeType.Equals(typeof(JSAMMusicFileObject)))
            {
                selectedMusic = ((JSAMMusicFileObject)Selection.activeObject);
                selectedClip = selectedMusic.File;
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
            Rect progressRect = ProgressBar((float)helperSource.timeSamples / (float)helperSource.clip.samples, selectedClip, selectedSound, selectedMusic);
            EditorGUI.BeginChangeCheck();
            scrollbarProgress = GUILayout.HorizontalScrollbar(scrollbarProgress, scrollZoom, 0, MAX_SCROLL_ZOOM);
            if (EditorGUI.EndChangeCheck())
            {
                DoForceRepaint(true);
            }

            EditorGUILayout.BeginHorizontal();

            PollMouseEvents(progressRect);

            if (GUILayout.Button(s_BackIcon, new GUILayoutOption[] { GUILayout.MaxHeight(20) }))
            {
                if (selectedMusic)
                {
                    if (selectedMusic.loopMode == LoopMode.LoopWithLoopPoints && selectedMusic.clampToLoopPoints)
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
            Color colorbackup = GUI.backgroundColor;
            GUIContent buttonIcon = (clipPlaying) ? s_PlayIcons[1] : s_PlayIcons[0];
            if (clipPlaying) GUI.backgroundColor = buttonPressedColor;
            if (GUILayout.Button(buttonIcon, new GUILayoutOption[] { GUILayout.MaxHeight(20) }))
            {
                clipPlaying = !clipPlaying;
                if (clipPlaying)
                {
                    // Note: For some reason, reading from helperSource.time returns 0 even if timeSamples is not 0
                    // However, writing a value to helperSource.time changes timeSamples to the appropriate value just fine
                    if (selectedSound)
                    {
                        SoundFileObjectEditor.instance.StartFading(helperSource.clip);
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

            // Pause button
            GUI.backgroundColor = colorbackup;
            GUIContent theText = (clipPaused) ? s_PauseIcons[1] : s_PauseIcons[0];
            if (clipPaused) GUI.backgroundColor = buttonPressedColor;
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

            // Loop button
            GUI.backgroundColor = colorbackup;
            buttonIcon = (loopClip) ? s_LoopIcons[1] : s_LoopIcons[0];
            if (loopClip) GUI.backgroundColor = buttonPressedColor;
            if (GUILayout.Button(buttonIcon, new GUILayoutOption[] { GUILayout.MaxHeight(20) }))
            {
                loopClip = !loopClip;
                // helperSource.loop = true;
            }
            GUI.backgroundColor = colorbackup;

            if (selectedSound)
            {
                using (new EditorGUI.DisabledScope(selectedSound.FileCount < 2))
                {
                    if (GUILayout.Button(new GUIContent("Play Random", "Preview settings with a random track from your library. Only usable if this Audio File has \"Use Library\" enabled.")))
                    {
                        selectedClip = SoundFileObjectEditor.instance.DesignateRandomAudioClip();
                        clipPlaying = true;
                        helperSource.Stop();
                        SoundFileObjectEditor.instance.StartFading(selectedClip);
                    }
                }
            }

            int loopPointInputMode = 0;
            // Reset loop point input mode if not using loop points so the duration shows up as time by default
            if (selectedMusic)
            {
                if (selectedMusic.loopMode != LoopMode.LoopWithLoopPoints) loopPointInputMode = 0;
            }

            GUIContent blontent = new GUIContent();
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
            GUIStyle rightJustified = new GUIStyle(EditorStyles.label);
            rightJustified.alignment = TextAnchor.UpperRight;
            EditorGUILayout.LabelField(blontent, rightJustified);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        void PollMouseEvents(Rect progressRect)
        {
            Event evt = Event.current;

            if (evt.isScrollWheel)
            {
                if (evt.mousePosition.y > progressRect.yMin && evt.mousePosition.y < progressRect.yMax)
                {
                    float destinedProgress = Mathf.InverseLerp(progressRect.xMin, progressRect.xMax, evt.mousePosition.x);

                    destinedProgress = Mathf.Lerp(CalculateZoomedLeftValue(), CalculateZoomedRightValue(), destinedProgress);

                    // Center the scrollbar because scrollbars have their pivot set to the left
                    scrollbarProgress = destinedProgress * MAX_SCROLL_ZOOM - scrollZoom / 2;

                    scrollZoom = Mathf.Clamp(scrollZoom + evt.delta.y / 3, 0, MAX_SCROLL_ZOOM);

                    // Because scrollbar progress isn't real progress
                    trueScrollProgress = destinedProgress;
                    DoForceRepaint(true);
                }
            }
            else if (evt.isMouse)
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
                            newProgress = Mathf.Lerp(CalculateZoomedLeftValue(), CalculateZoomedRightValue(), newProgress);
                            helperSource.time = Mathf.Clamp((newProgress * helperSource.clip.length), 0, helperSource.clip.length - AudioManagerInternal.EPSILON);
                            if (selectedMusic)
                            {
                                if (selectedMusic.loopMode == LoopMode.LoopWithLoopPoints && selectedMusic.clampToLoopPoints)
                                {
                                    float start = selectedMusic.loopStart * selectedMusic.File.frequency;
                                    float end = selectedMusic.loopEnd * selectedMusic.File.frequency;
                                    helperSource.timeSamples = (int)Mathf.Clamp(helperSource.timeSamples, start, end - AudioManagerInternal.EPSILON);
                                }
                            }
                        }
                        else if (evt.button == 2) // Middle mouse events
                        {
                            if (!mouseGrabbed)
                            {
                                if (evt.mousePosition.y > progressRect.yMin && evt.mousePosition.y < progressRect.yMax)
                                {
                                    mouseGrabbed = true;
                                    dragStartPos = evt.mousePosition;
                                }
                            }
                            if (mouseGrabbed)
                            {
                                float delta = Mathf.InverseLerp(progressRect.xMin, progressRect.xMax, Mathf.Abs(evt.delta.x));

                                if (evt.delta.x < 0) delta *= -1;
                                
                                scrollbarProgress -= delta * MAX_SCROLL_ZOOM;

                                DoForceRepaint(true);
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
            // Get a rect for the progress bar using the same margins as a text field
            // TODO: Make this dynamic, locks its previous size before showing the guide
            float maxHeight = (showHowTo) ? playbackPreviewClamped : 4000;
            float minHeight = (showHowTo) ? playbackPreviewClamped : 64;
            Rect rect = GUILayoutUtility.GetRect(64, 4000, minHeight, maxHeight);

            if ((cachedTex == null || forceRepaint) && Event.current.type == EventType.Repaint)
            {
                Texture2D waveformTexture;
                if (selectedSound)
                {
                    waveformTexture = SoundFileObjectEditor.instance.PaintWaveformSpectrum(helperSource.clip, (int)rect.width, (int)rect.height, new Color(1, 0.5f, 0));
                }
                else if (selectedMusic)
                {
                    waveformTexture = MusicFileObjectEditor.instance.PaintWaveformSpectrum(selectedClip, (int)rect.width, (int)rect.height, new Color(1, 0.5f, 0));
                }
                else
                {
                    waveformTexture = RenderStaticPreview(selectedClip, (int)rect.width, (int)rect.height);
                }
                cachedTex = waveformTexture;

                if (waveformTexture != null)
                    GUI.DrawTexture(rect, waveformTexture);
                forceRepaint = false;
            }
            else
            {
                GUI.DrawTexture(rect, cachedTex);
            }

            float left = CalculateZoomedLeftValue();
            float right = CalculateZoomedRightValue();
            if (value >= left && value <= right)
            {
                Rect progressRect = new Rect(rect);
                progressRect.width *= Mathf.InverseLerp(left, right, value);
                progressRect.xMin = progressRect.xMax - 1;
                GUI.Box(progressRect, "", "SelectionRect");
            }

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
                    if (selectedSound)
                    {
                        if (selectedSound.fadeMode != FadeMode.None)
                        {
                            SoundFileObjectEditor.instance.HandleFading(selectedSound);
                        }
                        else
                        {
                            helperSource.volume = selectedSound.relativeVolume;
                        }
                    }
                    
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
                        EditorApplication.QueuePlayerLoopUpdate();
                        if (selectedMusic.loopMode == LoopMode.LoopWithLoopPoints)
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
                            else if (selectedMusic.clampToLoopPoints || !MusicFileObjectEditor.firstPlayback)
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
                    else if (!loopClip && selectedMusic.loopMode == LoopMode.LoopWithLoopPoints)
                    {
                        if ((!helperSource.isPlaying && !clipPaused) || clipPos > selectedMusic.loopEnd)
                        {
                            clipPlaying = false;
                            helperSource.Stop();
                        }
                        else if (selectedMusic.clampToLoopPoints && clipPos < selectedMusic.loopStart)
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

        #region Deprecated Waveform Rendering
        /// <summary>
        /// Code from these gents
        /// https://answers.unity.com/questions/189886/displaying-an-audio-waveform-in-the-editor.html
        /// </summary>
        public static Texture2D PaintWaveformSpectrum(AudioClip audio, int width, int height, Color col)
        {
            if (Event.current.type != EventType.Repaint) return null;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float[] samples = new float[audio.samples * audio.channels];
            // Copy sample data to array
            audio.GetData(samples, 0);

            float leftValue = CalculateZoomedLeftValue();
            float rightValue = CalculateZoomedRightValue();

            int leftSide = Mathf.RoundToInt(leftValue * samples.Length);
            int rightSide = Mathf.RoundToInt(rightValue * samples.Length);

            float zoomLevel = scrollZoom / MAX_SCROLL_ZOOM;
            int packSize = Mathf.RoundToInt((int)samples.Length / (int)width * (float)zoomLevel) + 1;

            int s = 0;
            int limit = Mathf.Min(rightSide, samples.Length);

            // Build waveform data
            float[] waveform = new float[limit];
            for (int i = leftSide; i < limit; i += packSize)
            {
                waveform[s] = Mathf.Abs(samples[i]);
                s++;
            }

            Color lightShade = new Color(0.3f, 0.3f, 0.3f);
            int halfHeight = height / 2;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, lightShade);
                }
            }

            for (int x = 0; x < Mathf.Clamp(rightSide, 0, width); x++)
            {
                // Scale the wave vertically relative to half the rect height and the relative volume
                float heightLimit = waveform[x] * halfHeight;

                for (int y = (int)heightLimit; y >= 0; y--)
                {
                    Color currentPixelColour = tex.GetPixel(x, halfHeight + y);
                    if (currentPixelColour == Color.black) continue;

                    tex.SetPixel(x, halfHeight + y, lightShade + col * 0.75f);

                    // Get data from upper half offset by 1 unit due to int truncation
                    tex.SetPixel(x, halfHeight - (y + 1), lightShade + col * 0.75f);
                }
            }

            tex.Apply();

            return tex;
        }
        #endregion

        #region Unity's Asset Preview render code
        /// <summary>
        /// Borrowed from Unity
        /// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/AudioClipInspector.cs
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="subAssets"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Texture2D RenderStaticPreview(AudioClip clip, int width, int height)
        {
            AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
            AudioImporter audioImporter = importer as AudioImporter;

            if (audioImporter == null || !ShaderUtil.hardwareSupportsRectRenderTexture)
                return null;

            if (m_PreviewUtility == null)
                m_PreviewUtility = new PreviewRenderUtility();

            m_PreviewUtility.BeginStaticPreview(new Rect(0, 0, width, height));

            // We're drawing into an offscreen here which will have a resolution defined by EditorGUIUtility.pixelsPerPoint. This is different from the DoRenderPreview call below where we draw directly to the screen, so we need to take
            // the higher resolution into account when drawing into the offscreen, otherwise only the upper-left quarter of the preview texture will be drawn.
            DoRenderPreview(clip, audioImporter, new Rect(0, 0, width * EditorGUIUtility.pixelsPerPoint, height * EditorGUIUtility.pixelsPerPoint), 1.0f);

            return m_PreviewUtility.EndStaticPreview();
        }

        private static void DoRenderPreview(AudioClip clip, AudioImporter audioImporter, Rect wantedRect, float scaleFactor)
        {
            scaleFactor *= 0.95f; // Reduce amplitude slightly to make highly compressed signals fit.
            float[] minMaxData = (audioImporter == null) ? null : AudioUtil.GetMinMaxData(audioImporter);
            int numChannels = clip.channels;
            int numSamples = (minMaxData == null) ? 0 : (minMaxData.Length / (2 * numChannels));
            float h = (float)wantedRect.height / (float)numChannels;
            for (int channel = 0; channel < numChannels; channel++)
            {
                Rect channelRect = new Rect(wantedRect.x, wantedRect.y + h * channel, wantedRect.width, h);
                Color curveColor = new Color(1.0f, 140.0f / 255.0f, 0.0f, 1.0f);

                AudioCurveRendering.AudioMinMaxCurveAndColorEvaluator dlg = delegate (float x, out Color col, out float minValue, out float maxValue)
                {
                    col = curveColor;
                    if (numSamples <= 0)
                    {
                        minValue = 0.0f;
                        maxValue = 0.0f;
                    }
                    else
                    {
                        float p = Mathf.Clamp(x * (numSamples - 2), 0.0f, numSamples - 2);
                        int i = (int)Mathf.Floor(p);
                        int offset1 = (i * numChannels + channel) * 2;
                        int offset2 = offset1 + numChannels * 2;
                        minValue = Mathf.Min(minMaxData[offset1 + 1], minMaxData[offset2 + 1]) * scaleFactor;
                        maxValue = Mathf.Max(minMaxData[offset1 + 0], minMaxData[offset2 + 0]) * scaleFactor;
                        if (minValue > maxValue) { float tmp = minValue; minValue = maxValue; maxValue = tmp; }
                    }
                };

                AudioCurveRendering.DrawMinMaxFilledCurve(channelRect, dlg);
            }
        }
        #endregion

        static GUIContent s_BackIcon = null;
        static GUIContent[] s_PlayIcons = { null, null };
        static GUIContent[] s_PauseIcons = { null, null };
        static GUIContent[] s_LoopIcons = { null, null };

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

        public static float CalculateZoomedLeftValue()
        {
            return scrollbarProgress / MAX_SCROLL_ZOOM;
        }

        public static float CalculateZoomedRightValue()
        {
            return Mathf.Clamp01((scrollbarProgress + scrollZoom) / MAX_SCROLL_ZOOM);
        }
    }
}