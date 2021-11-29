﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

namespace JSAM
{
    /// <summary>
    /// AudioManager singleton that manages all audio in the game
    /// </summary>
    [DefaultExecutionOrder(1)]
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        [Header("General Settings")]

        static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                bool missing = false;
                if (instance == null) missing = true;
                else if (instance.gameObject.scene == null) missing = true;
                if (missing) instance = FindObjectOfType<AudioManager>();
                return instance;
            }
        }

        [Tooltip("The settings used for this AudioManager")]
        [SerializeField, HideInInspector] AudioManagerSettings settings = null;
        public AudioManagerSettings Settings { get { return settings; } }

        [Tooltip("The Audio Library that this AudioManager should use")]
        [SerializeField, HideInInspector] AudioLibrary library = null;
        public AudioLibrary Library { get { return library; } }

        [Header("Scene AudioListener Reference (Optional)")]

        /// <summary>
        /// The Audio Listener in your scene, will try to automatically set itself on start by looking at the object tagged as \"Main Camera\"
        /// </summary>
        [Tooltip("The Audio Listener in your scene, will try to automatically set itself on Start by looking in the object tagged as \"Main Camera\"")]
        [SerializeField] AudioListener listener = null;
        public static AudioListener AudioListener { get { return Instance.listener; } }

        [Header("AudioSource Reference Prefab (MANDATORY)")]

        [SerializeField] AudioSource sourcePrefab = null;

        bool doneLoading;

        bool initialized = false;

        public static JSAMMusicChannelHelper MainMusicHelper { get { return InternalInstance.mainMusic; } }

        static AudioManagerInternal internalInstance;
        public static AudioManagerInternal InternalInstance
        {
            get
            {
                if (internalInstance == null)
                {
                    if (Instance != null)
                    {
                        internalInstance = Instance.gameObject.AddComponent<AudioManagerInternal>();
                    }
                }
                return internalInstance;
            }
        }

        public static Action OnAudioManagerInitialized;

        // Use this for initialization
        void Awake()
        {
            // AudioManager is important, keep it between scenes
            if (settings.DontDestroyOnLoad)
            {
                gameObject.transform.SetParent(null, true); 
                DontDestroyOnLoad(gameObject);
            }

            EstablishSingletonDominance();

            if (!initialized)
            {
                //Set sources properties based on current settings
                InternalInstance.SetSpatialSound(Settings.Spatialize);

                // Find the listener if not manually set
                FindNewListener();

                doneLoading = true;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            for (int i = 0; i < library.Sounds.Count; i++)
            {
                library.Sounds[i].Initialize();
            }

            initialized = true;
        }

        public bool Initialized()
        {
            return initialized;
        }

        void FindNewListener()
        {
            if (listener == null)
            {
                if (Camera.main != null)
                {
                    listener = Camera.main.GetComponent<AudioListener>();
                }
                if (listener != null)
                {
                    DebugLog("AudioManager located an AudioListener successfully!");
                }
                else if (listener == null) // Try to find one ourselves
                {
                    listener = FindObjectOfType<AudioListener>();
                    DebugLog("AudioManager located an AudioListener successfully!");
                }
                if (listener == null) // In the case that there still isn't an AudioListener
                {
                    Debug.LogWarning("AudioManager Warning: Scene is missing an AudioListener! Mark the listener with the \"Main Camera\" tag or set it manually!");
                }
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindNewListener();
            if (Settings.StopSoundsOnSceneLoad)
            {
                StopAllSounds();
            }
        }

        #region PlaySound
        /// <summary>
        /// Plays the specified sound using the settings provided in the Sound File Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The enum correlating with the audio file you wish to play</param>
        /// <param name="transform">Optional: The transform of the sound's source</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only emit one sound at any time</para></param>
        /// <returns>The Sound Channel Helper playing the sound</returns>
        public static JSAMSoundChannelHelper PlaySound<T>(T sound, Transform transform = null, JSAMSoundChannelHelper helper = null) where T : Enum
        {
            return InternalInstance.PlaySoundInternal(Instance.Library.Sounds[Convert.ToInt32(sound)], transform, helper);
        }

        /// <summary>
        /// <inheritdoc cref="PlaySound{T}(T, Transform, JSAMSoundChannelHelper)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The enum correlating with the audio file you wish to play</param>
        /// <param name="position">Optional: The world position you want the sound to play from</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only emit one sound at any time</para></param>
        /// <returns><inheritdoc cref="PlaySound{T}(T, Transform, JSAMSoundChannelHelper)" path="/returns"/></returns>
        public static JSAMSoundChannelHelper PlaySound<T>(T sound, Vector3 position, JSAMSoundChannelHelper helper = null) where T : Enum
        {
            return InternalInstance.PlaySoundInternal(Instance.Library.Sounds[Convert.ToInt32(sound)], position, helper);
        }

        /// <summary>
        /// <inheritdoc cref="PlaySound{T}(T, Transform, JSAMSoundChannelHelper)"/>
        /// </summary>
        /// <param name="sound">A reference to the Sound File asset to play directly</param>
        /// <param name="transform">Optional: The transform of the sound's source</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only emit one sound at any time</para></param>
        /// <returns><inheritdoc cref="PlaySound{T}(T, Transform, JSAMSoundChannelHelper)" path="/returns"/></returns>
        public static JSAMSoundChannelHelper PlaySound(JSAMSoundFileObject sound, Transform transform = null, JSAMSoundChannelHelper helper = null) => InternalInstance.PlaySoundInternal(sound, transform, helper);

        /// <summary>
        /// <inheritdoc cref="PlaySound{T}(T, Transform, JSAMSoundChannelHelper)"/>
        /// </summary>
        /// <param name="sound">A reference to the Sound File asset to play directly</param>
        /// <param name="position">The position you want to play the sound at</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only emit one sound at any time</para></param>
        /// <returns><inheritdoc cref="PlaySound{T}(T, Transform, JSAMSoundChannelHelper)" path="/returns"/></returns>
        public static JSAMSoundChannelHelper PlaySound(JSAMSoundFileObject sound, Vector3 position, JSAMSoundChannelHelper helper = null) => InternalInstance.PlaySoundInternal(sound, position, helper);
        #endregion

        #region StopSound
        /// <summary>
        /// Stops the first playing instance of the given sound immediately
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The sound to be stopped</param>
        /// <param name="transform">Optional: If the sound was initially passed a reference to 
        /// a transform in PlaySound, passing the same Transform reference will stop that specific playing instance</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        public static void StopSound<T>(T sound, Transform transform = null, bool stopInstantly = true) where T : Enum =>
            InternalInstance.StopSoundInternal(Instance.Library.Sounds[Convert.ToInt32(sound)], transform, stopInstantly);

        /// <summary>
        /// <inheritdoc cref="StopSound{T}(T, Transform)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The sound to be stopped</param>
        /// <param name="position">The sound's playback position in world space. 
        /// Passing this property will limit playback stopping 
        /// to only the sound playing at this specific position</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        public static void StopSound<T>(T sound, Vector3 position, bool stopInstantly = true) => 
            InternalInstance.StopSoundInternal(Instance.Library.Sounds[Convert.ToInt32(sound)], position, stopInstantly);

        /// <summary>
        /// </summary>
        /// <param name="sound">A reference to the Sound File asset to play directly</param>
        /// <param name="transform">Optional: If the sound was initially passed a reference to 
        /// a transform in PlaySound, passing the same Transform reference will stop that specific playing instance</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        public static void StopSound(JSAMSoundFileObject sound, Transform transform = null, bool stopInstantly = true) =>
            InternalInstance.StopSoundInternal(sound, transform, stopInstantly);

        /// <summary>
        /// <inheritdoc cref="StopSound{T}(T, Transform)"/>
        /// </summary>
        /// <param name="sound">A reference to the Sound File asset to play directly</param>
        /// <param name="position">The sound's playback position in world space. 
        /// Passing this property will limit playback stopping 
        /// to only the sound playing at this specific position</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        public static void StopSound(JSAMSoundFileObject sound, Vector3 position, bool stopInstantly = true) => 
            InternalInstance.StopSoundInternal(sound, position, stopInstantly);

        /// <summary>
        /// Stops all playing sounds maintained by AudioManager
        /// <param name="stopInstantly">Optional: If true, stop all sounds immediately</param>
        /// </summary>
        public static void StopAllSounds(bool stopInstantly = true) =>
            InternalInstance.StopAllSoundsInternal(stopInstantly);

        /// <summary>
        /// A shorthand for wrapping StopSound in an IsSoundPlaying if-statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The sound to be stopped</param>
        /// <param name="transform">Optional: If the sound was initially passed a reference to 
        /// a transform in PlaySound, passing the same Transform reference will stop that specific playing instance</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        /// <returns>True if sound was stopped successfully, false if sound wasn't playing</returns>
        public static bool StopSoundIfPlaying<T>(T sound, Transform transform = null, bool stopInstantly = true) where T : Enum =>
            InternalInstance.StopSoundIfPlayingInternal(Instance.library.Sounds[Convert.ToInt32(sound)], transform, stopInstantly);

        /// <summary>
        /// <inheritdoc cref="StopSoundIfPlaying{T}(T, Transform)"/>
        /// </summary>
        /// <param name="sound">The sound to be stopped</param>
        /// <param name="position">The sound's playback position in world space. 
        /// Passing this property will limit playback stopping 
        /// to only the sound playing at this specific position</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        /// <returns>True if sound was stopped successfully, false if sound wasn't playing</returns>
        public static bool StopSoundIfPlaying<T>(T sound, Vector3 position, bool stopInstantly = true) where T : Enum =>
            InternalInstance.StopSoundIfPlayingInternal(Instance.library.Sounds[Convert.ToInt32(sound)], position, stopInstantly);

        /// <summary>
        /// <inheritdoc cref="StopSoundIfPlaying{T}(T, Transform)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">A reference to the Sound File asset to stop directly</param>
        /// <param name="transform">Optional: If the sound was initially passed a reference to 
        /// a transform in PlaySound, passing the same Transform reference will stop that specific playing instance</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        /// <returns>True if sound was stopped successfully, false if sound wasn't playing</returns>
        public static bool StopSoundIfPlaying(JSAMSoundFileObject sound, Transform transform = null, bool stopInstantly = true) =>
            InternalInstance.StopSoundIfPlayingInternal(sound, transform, stopInstantly);

        /// <summary>
        /// <inheritdoc cref="StopSoundIfPlaying{T}(T, Transform)"/>
        /// </summary>
        /// <param name="sound">A reference to the Sound File asset to stop directly</param>
        /// <param name="position">The sound's playback position in world space. 
        /// Passing this property will limit playback stopping 
        /// to only the sound playing at this specific position</param>
        /// <param name="stopInstantly">Optional: If true, stop the sound immediately, you may want to leave this false for looping sounds</param>
        /// <returns>True if sound was stopped successfully, false if sound wasn't playing</returns>
        public static bool StopSoundIfPlaying(JSAMSoundFileObject sound, Vector3 position, bool stopInstantly = true) =>
            InternalInstance.StopSoundIfPlayingInternal(sound, position, stopInstantly);
        #endregion

        #region IsSoundPlaying
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The enum value for the sound in question. Check AudioManager to see what Enum you should use</param>
        /// <param name="transform">Optional: Only return true if the sound is playing from this transform</param>
        /// <returns>True if a sound that was played using PlaySound is currently playing</returns>
        public static bool IsSoundPlaying<T>(T sound, Transform transform = null) where T : Enum =>
            InternalInstance.IsSoundPlayingInternal(Instance.Library.Sounds[Convert.ToInt32(sound)], transform);

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The enum value for the sound in question. Check AudioManager to see what Enum you should use</param>
        /// <param name="position">Only return true if the sound is played at this position</param>
        /// <returns><inheritdoc cref="IsSoundPlaying{T}(T, Transform)" path="/returns"/></returns>
        public static bool IsSoundPlaying<T>(T sound, Vector3 position) where T : Enum =>
            InternalInstance.IsSoundPlayingInternal(Instance.Library.Sounds[Convert.ToInt32(sound)], position);

        /// <summary>
        /// </summary>
        /// <param name="sound">The enum value for the sound in question. Check AudioManager to see what Enum you should use</param>
        /// <param name="transform">Optional: Only return true if the sound is playing from this transform</param>
        /// <returns><inheritdoc cref="IsSoundPlaying{T}(T, Transform)" path="/returns"/></returns>
        public static bool IsSoundPlaying(JSAMSoundFileObject sound, Transform transform = null) =>
            InternalInstance.IsSoundPlayingInternal(sound, transform);

        /// <summary>
        /// </summary>
        /// <param name="sound">The enum value for the sound in question. Check AudioManager to see what Enum you should use</param>
        /// <param name="position">Only return true if the sound is played at this position</param>
        /// <returns><inheritdoc cref="IsSoundPlaying{T}(T, Transform)" path="/returns"/></returns>
        public static bool IsSoundPlaying(JSAMSoundFileObject sound, Vector3 position) =>
            InternalInstance.IsSoundPlayingInternal(sound, position);

        /// <summary>
        /// Very similar use case as TryGetComponent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sound">The enum of the music in question, check AudioManager to see what enums you can use</param>
        /// <param name="helper">This helper reference will be given a value if the method returns true</param>
        /// <returns>The first Sound Helper that's currently playing the specified music</returns>
        public static bool TryGetPlayingSound<T>(T sound, out JSAMSoundChannelHelper helper) where T : Enum =>
            InternalInstance.TryGetPlayingSound(Instance.library.Sounds[Convert.ToInt32(sound)], out helper);

        /// <summary>
        /// Very similar use case as TryGetComponent
        /// </summary>
        /// <param name="sound">The enum of the music in question, check AudioManager to see what enums you can use</param>
        /// <param name="helper">This helper reference will be given a value if the method returns true</param>
        /// <returns>The first Sound Helper that's currently playing the specified music</returns>
        public static bool TryGetPlayingSound(JSAMSoundFileObject sound, out JSAMSoundChannelHelper helper) =>
            InternalInstance.TryGetPlayingSound(sound, out helper);

        #endregion

        #region PlayMusic
        /// <summary>
        /// Play Music globally without spatialization
        /// Supports built-in track transition operations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">Enum value for the music to be played. You can find this in the AudioLibrary</param>
        /// <param name="isMainTrack">If true, allow track to be transitioned to another track easily</param>
        /// <returns>The Music Channel helper playing the sound, useful for transitions, like copying the playback position to the next track</returns>
        public static JSAMMusicChannelHelper PlayMusic<T>(T track, bool isMainTrack) where T : Enum
        {
            return InternalInstance.PlayMusicInternal(Instance.Library.Music[Convert.ToInt32(track)], isMainTrack);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="isMainTrack"></param>
        /// <returns><inheritdoc cref="PlayMusic{T}(T, bool)" path="/returns"/></returns>
        public static JSAMMusicChannelHelper PlayMusic(JSAMMusicFileObject track, bool isMainTrack)
        {
            return InternalInstance.PlayMusicInternal(track, isMainTrack);
        }

        /// <summary>
        /// Plays the specified music using the settings provided in the Music File Object. 
        /// Supports spatialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">Enum value for the music to be played. You can find this in the AudioLibrary</param>
        /// <param name="transform">Optional: The transform of the music's source</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only play a single track at any time</para></param>
        /// <returns><inheritdoc cref="PlayMusic{T}(T, bool)" path="/returns"/></returns>
        public static JSAMMusicChannelHelper PlayMusic<T>(T track, Transform transform = null, JSAMMusicChannelHelper helper = null) where T : Enum
        {
            return InternalInstance.PlayMusicInternal(Instance.Library.Music[Convert.ToInt32(track)], transform, helper);
        }

        /// <summary>
        /// <inheritdoc cref="PlayMusic{T}(T, Transform, JSAMMusicChannelHelper)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">Enum value for the music to be played. You can find this in the AudioLibrary</param>
        /// <param name="position">The world position you want the music to play from</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only play a single track at any time</para></param>
        /// <returns><inheritdoc cref="PlayMusic{T}(T, bool)" path="/returns"/></returns>
        public static JSAMMusicChannelHelper PlayMusic<T>(T track, Vector3 position, JSAMMusicChannelHelper helper = null) where T : Enum
        {
            return InternalInstance.PlayMusicInternal(Instance.Library.Music[Convert.ToInt32(track)], position, helper);
        }

        /// <summary>
        /// <inheritdoc cref="PlayMusic{T}(T, Transform, JSAMMusicChannelHelper)"/>
        /// </summary>
        /// <param name="track">A reference to the Music File asset to play directly</param>
        /// <param name="transform">Optional: The transform of the music's source</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only play a single track at any time</para></param>
        /// <returns><inheritdoc cref="PlayMusic{T}(T, bool)" path="/returns"/></returns>
        public static JSAMMusicChannelHelper PlayMusic(JSAMMusicFileObject track, Transform transform = null, JSAMMusicChannelHelper helper = null) => InternalInstance.PlayMusicInternal(track, transform, helper);

        /// <summary>
        /// <inheritdoc cref="PlayMusic{T}(T, Transform, JSAMMusicChannelHelper)"/>
        /// </summary>
        /// <param name="track">A reference to the Music File asset to play directly</param>
        /// <param name="position">The world position you want the music to play from</param>
        /// <param name="helper">Optional: The specific channel you want to play the sound from. 
        /// <para>Good if you want an entity to only play a single track at any time</para></param>
        /// <returns><inheritdoc cref="PlayMusic{T}(T, bool)" path="/returns"/></returns>
        public static JSAMMusicChannelHelper PlayMusic(JSAMMusicFileObject track, Vector3 position, JSAMMusicChannelHelper helper = null) => InternalInstance.PlayMusicInternal(track, position, helper);
        #endregion

        #region IsMusicPlaying
        /// <summary>
        /// </summary>
        /// <param name="music">The enum of the music in question, check AudioManager to see what enums you can use</param>
        /// <returns>True if music that was played through PlayMusic is currently playing</returns>
        public static bool IsMusicPlaying<T>(T music) where T : Enum =>
            InternalInstance.IsMusicPlayingInternal(Instance.library.Music[Convert.ToInt32(music)]);

        /// <summary>
        /// </summary>
        /// <param name="music">The enum of the music in question, check AudioManager to see what enums you can use</param>
        /// <returns></returns>
        public static bool IsMusicPlaying(JSAMMusicFileObject music) => InternalInstance.IsMusicPlayingInternal(music);

        /// <summary>
        /// Very similar use case as TryGetComponent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="music">The enum of the music in question, check AudioManager to see what enums you can use</param>
        /// <param name="helper">This helper reference will be given a value if the method returns true</param>
        /// <returns>The first Music Helper that's currently playing the specified music</returns>
        public static bool TryGetPlayingMusic<T>(T music, out JSAMMusicChannelHelper helper) where T : Enum =>
            InternalInstance.TryGetPlayingMusic(Instance.library.Music[Convert.ToInt32(music)], out helper);

        /// <summary>
        /// Very similar use case as TryGetComponent
        /// </summary>
        /// <param name="music">The enum of the music in question, check AudioManager to see what enums you can use</param>
        /// <param name="helper">This helper reference will be given a value if the method returns true</param>
        /// <returns>The first Music Helper that's currently playing the specified music</returns>
        public static bool TryGetPlayingMusic(JSAMMusicFileObject music, out JSAMMusicChannelHelper helper) =>
            InternalInstance.TryGetPlayingMusic(music, out helper);
        #endregion

        #region StopMusic
        /// <summary>
        /// Instantly stops the playback of the specified playing music track
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="transform">Optional: The transform of the music's source</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns>The Music Channel helper playing the sound, useful for transitions, like copying the playback position to the next track</returns>
        public static JSAMMusicChannelHelper StopMusic<T>(T track, Transform transform = null, bool stopInstantly = true) where T : Enum
        {
            return InternalInstance.StopMusicInternal(Instance.Library.Music[Convert.ToInt32(track)], transform, stopInstantly);
        }

        /// <summary>
        /// <inheritdoc cref="StopMusic(JSAMMusicFileObject, Transform){T}(T, Transform, JSAMMusicChannelHelper)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="position">The world position the music is playing from</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns><inheritdoc cref="StopMusic{T}(T, Transform, bool)"/></returns>
        public static JSAMMusicChannelHelper StopMusic<T>(T track, Vector3 position, bool stopInstantly = true) where T : Enum
        {
            return InternalInstance.StopMusicInternal(Instance.Library.Music[Convert.ToInt32(track)], position, stopInstantly);
        }

        /// <summary>
        /// <inheritdoc cref="StopMusic(JSAMMusicFileObject, Transform){T}(T, Transform, JSAMMusicChannelHelper)"/>
        /// </summary>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="transform">Optional: The transform of the music's source</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns><inheritdoc cref="StopMusic{T}(T, Transform, bool)"/></returns>
        public static JSAMMusicChannelHelper StopMusic(JSAMMusicFileObject track, Transform transform = null, bool stopInstantly = true)
        {
            return InternalInstance.StopMusicInternal(track, transform, stopInstantly);
        }

        /// <summary>
        /// <inheritdoc cref="StopMusic(JSAMMusicFileObject, Transform){T}(T, Transform, JSAMMusicChannelHelper)"/>
        /// </summary>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="position">The world position the music is playing from</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns><inheritdoc cref="StopMusic{T}(T, Transform, bool)"/></returns>
        public static JSAMMusicChannelHelper StopMusic(JSAMMusicFileObject track, Vector3 position, bool stopInstantly = true)
        {
            return InternalInstance.StopMusicInternal(track, position, stopInstantly);
        }

        /// <summary>
        /// A shorthand for wrapping StopMusic in an IsMusicPlaying if-statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="transform">Optional: The transform of the music's source</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns>True if music was stopped successfully, false if music wasn't playing</returns>
        public static bool StopMusicIfPlaying<T>(T track, Transform transform = null, bool stopInstantly = true) where T : Enum
        {
            return InternalInstance.StopMusicIfPlayingInternal(Instance.Library.Music[Convert.ToInt32(track)], transform, stopInstantly);
        }

        /// <summary>
        /// <inheritdoc cref="StopMusicIfPlaying{T}(T, Transform, bool)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="position">The world position the music is playing from</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns><inheritdoc cref="StopMusicIfPlaying{T}(T, Transform, bool)"/></returns>
        public static bool StopMusicIfPlaying<T>(T track, Vector3 position, bool stopInstantly = true) where T : Enum =>
            InternalInstance.StopMusicIfPlayingInternal(Instance.Library.Music[Convert.ToInt32(track)], position, stopInstantly);

        /// <summary>
        /// <inheritdoc cref="StopMusicIfPlaying{T}(T, Transform, bool)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="transform">Optional: The transform of the music's source</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns><inheritdoc cref="StopMusicIfPlaying{T}(T, Transform, bool)"/></returns>
        public static bool StopMusicIfPlaying(JSAMMusicFileObject track, Transform transform = null, bool stopInstantly = true) =>
            InternalInstance.StopMusicIfPlayingInternal(track, transform, stopInstantly);

        /// <summary>
        /// <inheritdoc cref="StopMusicIfPlaying{T}(T, Transform, bool)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="track">The enum corresponding to the music track</param>
        /// <param name="position">The world position the music is playing from</param>
        /// <param name="stopInstantly">Optional: If false, will allow music to transition out using it's transition settings. 
        /// Otherwise, will immediately end playback</param>
        /// <returns><inheritdoc cref="StopMusicIfPlaying{T}(T, Transform, bool)"/></returns>
        public static bool StopMusicIfPlaying(JSAMMusicFileObject track, Vector3 position, bool stopInstantly = true) =>
            InternalInstance.StopMusicIfPlayingInternal(track, position, stopInstantly);
        #endregion

        #region Volume
        public static float MasterVolume { get { return InternalInstance.MasterVolume; } }
        public static float MusicVolume { get { return InternalInstance.MusicVolume; } }
        public static float SoundVolume { get { return InternalInstance.SoundVolume; } }

        public static void SetMasterVolume(float newVolume) => InternalInstance.SetMasterVolume(newVolume);
        public static void SetMusicVolume(float newVolume) => InternalInstance.SetMusicVolume(newVolume);
        public static void SetSoundVolume(float newVolume) => InternalInstance.SetSoundVolume(newVolume);
        #endregion

        /// <summary>
        /// TODO: Make this more stable
        /// Ensures that the AudioManager you think you're referring to actually exists in this scene
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public void EstablishSingletonDominance()
        {
            if (Instance != this && Instance != null)
            {
                // A unique case where the Singleton exists but not in this scene
                if (Instance.gameObject.scene.name != gameObject.scene.name)
                {
                    if (Instance.gameObject.scene.name == "DontDestroyOnLoad" || gameObject.scene == null) // Previous is still here and active
                    {
                        enabled = false;
                    }
                    else
                    {
                        instance = this;
                    }
                }
                else if (!Instance.gameObject.activeInHierarchy)
                {
                    instance = this;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) instance = null;
        }

        /// <summary>
        /// Called internally by AudioManager to output non-error console messages
        /// </summary>
        /// <param name="consoleOutput"></param>
        public void DebugLog(string consoleOutput)
        {
            if (Settings.DisableConsoleLogs) return;
            Debug.Log(consoleOutput);
        }

        public void DebugWarning(string consoleOutput)
        {
            Debug.LogWarning("JSAM Warning! " +
                consoleOutput);
        }

        /// <summary>
        /// Given an enum, returns the corresponding AudioFileObject
        /// </summary>
        /// <typeparam name="T">The audio enum type corresponding to your scene. In most cases, this is just JSAM.Music</typeparam>
        /// <param name="track"></param>
        /// <returns></returns>
        public static JSAMMusicFileObject GetMusic<T>(T track) where T : Enum
        {
            int a = Convert.ToInt32(track);
            return Instance.library.Music[a];
        }

        /// <summary>
        /// Given an enum, returns the corresponding AudioFileObject
        /// </summary>
        /// <typeparam name="T">The audio enum type corresponding to your scene. In most cases, this is just JSAM.Sound</typeparam>
        /// <param name="sound"></param>
        /// <returns></returns>
        public static JSAMSoundFileObject GetSound<T>(T sound) where T : Enum
        {
            int a = Convert.ToInt32(sound);
            return Instance.library.Sounds[a];
        }

#if UNITY_EDITOR

        /// <summary>
        /// A MonoBehaviour function called when the script is loaded or a value is changed in the inspector (Called in the editor only).
        /// </summary>
        private void OnValidate()
        {
            EstablishSingletonDominance();
            //if (GetListener() == null) FindNewListener();
            ValidateSourcePrefab();

            if (!doneLoading) return;
        }

        public bool SourcePrefabExists()
        {
            return sourcePrefab != null;
        }

        void ValidateSourcePrefab()
        {
            if (!SourcePrefabExists()) return;
        }
#endif
    }
}