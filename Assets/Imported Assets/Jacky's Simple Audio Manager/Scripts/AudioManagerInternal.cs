using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [AddComponentMenu("")]
    public class AudioManagerInternal : MonoBehaviour
    {
        /// <summary>
        /// Sources dedicated to playing sound
        /// </summary>
        List<AudioSource> sources = new List<AudioSource>();
        List<JSAMSoundChannelHelper> soundHelpers = new List<JSAMSoundChannelHelper>();

        /// <summary>
        /// Sources dedicated to playing music
        /// </summary>
        List<JSAMMusicChannelHelper> musicHelpers = new List<JSAMMusicChannelHelper>();
        public JSAMMusicChannelHelper mainMusic { get; private set; }

        /// <summary>
        /// This object holds all AudioChannels
        /// </summary>
        Transform sourceHolder;

        AudioManager audioManager;

        AudioManagerSettings Settings { get { return audioManager.Settings; } }

        [SerializeField] GameObject sourcePrefab;

        float prevTimeScale = 1;

        /// <summary>
        /// A bit like float Epsilon, but large enough for the purpose of pushing the playback position of AudioSources just far enough to not throw an error
        /// </summary>
        public static float EPSILON = 0.000001f;

        /// <summary>
        /// Notifies Audio Channels to follow their target. 
        /// Only invoked when Spatialize is set to true
        /// </summary>
        public static Action OnSpatializeUpdate;
        /// <summary>
        /// Notifies Audio Channels to follow their target on LateUpdate 
        /// Only invoked when Spatialize is set to true
        /// </summary>
        public static Action OnSpatializeLateUpdate;
        /// <summary>
        /// Notifies Audio Channels to follow their target on FixedUpdate
        /// Only invoked when Spatialize is set to true
        /// </summary>
        public static Action OnSpatializeFixedUpdate;

        /// <summary>
        /// <para>float previousTimeScale</para>
        /// Invoked when the user changes the TimeScale
        /// Notifies Audio Channels to adjust pitch accordingly. 
        /// </summary>
        public static Action<float> OnTimeScaleChanged;

        void Awake()
        {
            audioManager = GetComponent<AudioManager>();

            sourceHolder = new GameObject("Sources").transform;
            sourceHolder.SetParent(transform);
            for (int i = 0; i < Settings.StartingSoundChannels; i++)
            {
                soundHelpers.Add(CreateSoundChannel());
            }
        }

        // Update is called once per frame
        void Update()
        {
            OnSpatializeUpdate?.Invoke();

            if (Mathf.Abs(Time.timeScale - prevTimeScale) > 0)
            {
                OnTimeScaleChanged?.Invoke(prevTimeScale);
            }
            prevTimeScale = Time.timeScale;
        }

        void FixedUpdate()
        {
            OnSpatializeFixedUpdate?.Invoke();
        }

        void LateUpdate()
        {
            OnSpatializeLateUpdate?.Invoke();
        }

        /// <summary>
        /// Set whether or not sounds are 2D or 3D (spatial)
        /// </summary>
        /// <param name="b">Enable spatial sound if true</param>
        public void SetSpatialSound(bool b)
        {

        }

        #region PlayMusic
        public JSAMMusicChannelHelper PlayMusicInternal(JSAMMusicFileObject music, bool isMain)
        {
            if (!Application.isPlaying) return null;

            var helper = PlayMusicInternal(music);
            if (isMain)
            {
                mainMusic = helper;
            }
            return mainMusic;
        }

        public JSAMMusicChannelHelper PlayMusicInternal(JSAMMusicFileObject music, Transform newTransform = null, JSAMMusicChannelHelper helper = null)
        {
            if (!Application.isPlaying) return null;
            if (helper == null) helper = musicHelpers[GetFreeMusicChannel()];
            helper.Play(music);
            helper.SetSpatializationTarget(newTransform);

            return helper;
        }

        public JSAMMusicChannelHelper PlayMusicInternal(JSAMMusicFileObject music, Vector3 position, JSAMMusicChannelHelper helper = null)
        {
            if (!Application.isPlaying) return null;
            if (helper == null) helper = musicHelpers[GetFreeMusicChannel()];
            helper.Play(music);
            helper.SetSpatializationTarget(position);

            return helper;
        }
        #endregion

        #region StopMusic
        public JSAMMusicChannelHelper StopMusicInternal(JSAMMusicFileObject music, Transform t, bool stopInstantly)
        {
            if (!Application.isPlaying) return null;
            for (int i = 0; i < musicHelpers.Count; i++)
            {
                if (music.ContainsAudioClip(musicHelpers[i].AudioSource.clip))
                {
                    if (t != null && music.spatialize)
                    {
                        if (musicHelpers[i].SpatializationTarget != t) continue;
                    }
                    musicHelpers[i].Stop(stopInstantly);
                    return musicHelpers[i];
                }
            }
            return null;
        }

        public JSAMMusicChannelHelper StopMusicInternal(JSAMMusicFileObject s, Vector3 pos, bool stopInstantly)
        {
            if (!Application.isPlaying) return null;
            for (int i = 0; i < musicHelpers.Count; i++)
            {
                if (s.ContainsAudioClip(musicHelpers[i].AudioSource.clip))
                {
                    if (musicHelpers[i].SpatializationPosition != pos && s.spatialize) continue;
                    musicHelpers[i].Stop(stopInstantly);
                    return musicHelpers[i];
                }
            }
            return null;
        }

        public bool StopMusicIfPlayingInternal(JSAMMusicFileObject music, Transform trans = null, bool stopInstantly = true)
        {
            if (!IsMusicPlayingInternal(music, trans)) return false;
            StopMusicInternal(music, trans, stopInstantly);
            return true;
        }

        public bool StopMusicIfPlayingInternal(JSAMMusicFileObject music, Vector3 pos, bool stopInstantly = true)
        {
            if (!IsMusicPlayingInternal(music, pos)) return false;
            StopMusicInternal(music, pos, stopInstantly);
            return true;
        }
        #endregion

        #region PlaySound
        public JSAMSoundChannelHelper PlaySoundInternal(JSAMSoundFileObject sound, Transform newTransform = null, JSAMSoundChannelHelper helper = null)
        {
            if (!Application.isPlaying) return null;

            if (helper == null) helper = soundHelpers[GetFreeSoundChannel()];
            helper.Play(sound);
            helper.SetSpatializationTarget(newTransform);

            return helper;
        }

        public JSAMSoundChannelHelper PlaySoundInternal(JSAMSoundFileObject sound, Vector3 position, JSAMSoundChannelHelper helper = null)
        {
            if (!Application.isPlaying) return null;

            if (helper == null) helper = soundHelpers[GetFreeSoundChannel()];
            helper.Play(sound);
            helper.SetSpatializationTarget(position);

            return helper;
        }
        #endregion

        #region StopSound
        public void StopAllSoundsInternal()
        {
            for (int i = 0; i < soundHelpers.Count; i++)
            {
                if (soundHelpers[i].AudioSource.isPlaying)
                {
                    soundHelpers[i].Stop();
                }
            }
        }

        public void StopSoundInternal(JSAMSoundFileObject s, Transform t = null)
        {
            for (int i = 0; i < soundHelpers.Count; i++)
            {
                if (s.ContainsAudioClip(soundHelpers[i].AudioSource.clip))
                {
                    if (t != null && s.spatialize)
                    {
                        if (soundHelpers[i].SpatializationTarget != t) continue;
                    }
                    soundHelpers[i].Stop(true);
                    return;
                }
            }
        }

        public void StopSoundInternal(JSAMSoundFileObject s, Vector3 pos)
        {
            for (int i = 0; i < soundHelpers.Count; i++)
            {
                if (s.ContainsAudioClip(soundHelpers[i].AudioSource.clip))
                {
                    if (soundHelpers[i].SpatializationPosition != pos && s.spatialize) continue;
                    soundHelpers[i].Stop(true);
                    return;
                }
            }
        }

        public bool StopSoundIfPlayingInternal(JSAMSoundFileObject sound, Transform trans = null)
        {
            if (!IsSoundPlayingInternal(sound, trans)) return false;
            StopSoundInternal(sound, trans);
            return true;
        }

        public bool StopSoundIfPlayingInternal(JSAMSoundFileObject sound, Vector3 pos)
        {
            if (!IsSoundPlayingInternal(sound, pos)) return false;
            StopSoundInternal(sound, pos);
            return true;
        }
        #endregion

        /// <returns>The index of the next free sound channel</returns>
        int GetFreeMusicChannel()
        {
            for (int i = 0; i < musicHelpers.Count; i++)
            {
                var helper = musicHelpers[i];
                if (helper.IsFree)
                {
                    return i;
                }
            }

            if (audioManager.Settings.DynamicSourceAllocation)
            {
                musicHelpers.Add(CreateMusicChannel());
                return musicHelpers.Count - 1;
            }
            else
            {
                Debug.LogError(
                    "AudioManager Error: Ran out of Music Sources! " +
                    "Please enable Dynamic Source Allocation in the AudioManager's settings or " +
                    "increase the number of Music Channels created on startup. " +
                    "You might be playing too many sounds at once.");
            }
            return -1;
        }

        /// <returns>The index of the next free sound channel</returns>
        int GetFreeSoundChannel()
        {
            for (int i = 0; i < soundHelpers.Count; i++)
            {
                var helper = soundHelpers[i];
                if (helper.IsFree)
                {
                    return i;
                }
            }

            if (audioManager.Settings.DynamicSourceAllocation)
            {
                soundHelpers.Add(CreateSoundChannel());
                return soundHelpers.Count - 1;
            }
            else
            {
                Debug.LogError(
                    "AudioManager Error: Ran out of Sound Sources! " +
                    "Please enable Dynamic Source Allocation in the AudioManager's settings or " +
                    "increase the number of Sound Channels created on startup. " +
                    "You might be playing too many sounds at once.");
            }
            return -1;
        }

        /// <summary>
        /// Deprecated
        /// Returns -1 if all sources are used
        /// </summary>
        /// <returns></returns>
        int GetAvailableSource()
        {
            for (int i = 0; i < sources.Count; i++)
            {
                if (!sources[i].isPlaying/* && !loopingSources.Contains(sources[i])*/)
                {
                    return i;
                }
            }

            if (audioManager.Settings.DynamicSourceAllocation)
            {
                AudioSource newSource = Instantiate(sourcePrefab, sourceHolder.transform).GetComponent<AudioSource>();
                JSAMSoundChannelHelper newHelper = newSource.gameObject.AddComponent<JSAMSoundChannelHelper>();
                newSource.name = "AudioSource " + sources.Count;
                //newHelper.Init();
                sources.Add(newSource);
                soundHelpers.Add(newHelper);
                return sources.Count - 1;
            }
            else
            {
                Debug.LogError("AudioManager Error: Ran out of Audio Sources!");
            }
            return -1;
        }

        #region IsPlaying
        public bool IsSoundPlayingInternal(JSAMSoundFileObject s, Transform trans)
        {
            for (int i = 0; i < soundHelpers.Count; i++)
            {
                if (soundHelpers[i].AudioFile == s && soundHelpers[i].AudioSource.isPlaying)
                {
                    if (trans != null && s.spatialize)
                    {
                        if (soundHelpers[i].SpatializationTarget != trans) continue;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool IsSoundPlayingInternal(JSAMSoundFileObject s, Vector3 pos)
        {
            for (int i = 0; i < soundHelpers.Count; i++)
            {
                if (soundHelpers[i].AudioFile == s && soundHelpers[i].AudioSource.isPlaying)
                {
                    if (soundHelpers[i].SpatializationPosition != pos && s.spatialize) continue;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetPlayingSound(JSAMSoundFileObject s, out JSAMSoundChannelHelper helper)
        {
            for (int i = 0; i < soundHelpers.Count; i++)
            {
                if (soundHelpers[i].AudioFile == s && soundHelpers[i].AudioSource.isPlaying)
                {
                    helper = soundHelpers[i];
                    return true;
                }
            }
            helper = null;
            return false;
        }

        public bool IsMusicPlayingInternal(JSAMMusicFileObject a, Transform trans = null)
        {
            for (int i = 0; i < musicHelpers.Count; i++)
            {
                if (musicHelpers[i].AudioFile == a && musicHelpers[i].AudioSource.isPlaying)
                {
                    if (trans != null && a.spatialize)
                    {
                        if (musicHelpers[i].SpatializationTarget != trans) continue;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool IsMusicPlayingInternal(JSAMMusicFileObject s, Vector3 pos)
        {
            for (int i = 0; i < musicHelpers.Count; i++)
            {
                if (musicHelpers[i].AudioFile == s && musicHelpers[i].AudioSource.isPlaying)
                {
                    if (musicHelpers[i].SpatializationPosition != pos && s.spatialize) continue;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetPlayingMusic(JSAMMusicFileObject a, out JSAMMusicChannelHelper helper)
        {
            for (int i = 0; i < musicHelpers.Count; i++)
            {
                if (musicHelpers[i].AudioFile == a && musicHelpers[i].AudioSource.isPlaying)
                {
                    helper = musicHelpers[i];
                    return true;
                }
            }
            helper = null;
            return false;
        }
        #endregion

        #region Channel Creation
        /// <summary>
        /// Creates a new GameObject and sets the parent to sourceHolder
        /// </summary>
        JSAMMusicChannelHelper CreateMusicChannel()
        {
            var newChannel = new GameObject("AudioChannel");
            newChannel.transform.SetParent(sourceHolder);
            newChannel.AddComponent<AudioSource>();
            var newHelper = newChannel.AddComponent<JSAMMusicChannelHelper>();
            newHelper.Init(Settings.MusicGroup);
            return newHelper;
        }

        /// <summary>
        /// Creates a new GameObject and sets the parent to sourceHolder
        /// </summary>
        JSAMSoundChannelHelper CreateSoundChannel()
        {
            var newChannel = new GameObject("AudioChannel");
            newChannel.transform.SetParent(sourceHolder);
            newChannel.AddComponent<AudioSource>();
            var newHelper = newChannel.AddComponent<JSAMSoundChannelHelper>();
            newHelper.Init(Settings.SoundGroup);
            return newHelper;
        }
        #endregion

        #region Volume Logic
        public float MasterVolume
        {
            get
            {
                float volume;
                Settings.Mixer.GetFloat(Settings.MasterVolumePararm, out volume);
                return volume;
            }
        }

        public float MusicVolume
        {
            get
            {
                float volume;
                Settings.Mixer.GetFloat(Settings.MusicVolumePararm, out volume);
                return volume;
            }
        }

        public float SoundVolume
        {
            get
            {
                float volume;
                Settings.Mixer.GetFloat(Settings.SoundVolumePararm, out volume);
                return volume;
            }
        }

        public void SetMasterVolume(float newVolume) => Settings.Mixer.SetFloat(Settings.MasterVolumePararm, newVolume);
        public void SetMusicVolume(float newVolume) => Settings.Mixer.SetFloat(Settings.MusicVolumePararm, newVolume);
        public void SetSoundVolume(float newVolume) => Settings.Mixer.SetFloat(Settings.SoundVolumePararm, newVolume);
        #endregion
    }
}