using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [AddComponentMenu("AudioManager/Audio Player Music")]
    public class AudioPlayerMusic : BaseAudioMusicFeedback
    {
        [Tooltip("Behaviour to trigger when the object this is attached to is created")]
        [SerializeField]
        protected AudioPlaybackBehaviour onStart = AudioPlaybackBehaviour.Play;

        [Tooltip("Behaviour to trigger when the object this is attached to is enabled or when the object is created")]
        [SerializeField]
        protected AudioPlaybackBehaviour onEnable = AudioPlaybackBehaviour.None;

        [Tooltip("Behaviour to trigger when the object this is attached to is destroyed or set to in-active")]
        [SerializeField]
        protected AudioPlaybackBehaviour onDisable = AudioPlaybackBehaviour.None;

        [Tooltip("Behaviour to trigger when the object this is attached to is destroyed")]
        [SerializeField]
        protected AudioPlaybackBehaviour onDestroy = AudioPlaybackBehaviour.Stop;

        JSAMMusicChannelHelper helper;
        public JSAMMusicChannelHelper MusicHelper { get { return helper; } }

        Coroutine playRoutine;

        // Start is called before the first frame update
        void Start()
        {
            switch (onStart)
            {
                case AudioPlaybackBehaviour.Play:
                    StartCoroutine(PlayDelayed());
                    break;
                case AudioPlaybackBehaviour.Stop:
                    Stop();
                    break;
            }
        }

        public void Play()
        {
            if (AudioManager.IsMusicPlaying(music) && !restartOnReplay) return;

            if (keepPlaybackPosition)
            {
                if (AudioManager.MainMusicHelper != null)
                {
                    if (AudioManager.MainMusicHelper.AudioSource.isPlaying)
                    {
                        float time = AudioManager.MainMusicHelper.AudioSource.time;
                        AudioManager.PlayMusic(music, transform).AudioSource.time = time;
                    }
                    else
                    {
                        AudioManager.PlayMusic(music, transform);
                    }
                }
            }
            else
            {
                AudioManager.StopMusicIfPlaying(music, transform);
                AudioManager.PlayMusic(music, transform);
            }
        }

        public void Stop()
        {
            AudioManager.StopMusic(music, transform, stopInstantly);
            helper = null;
        }

        private void OnEnable()
        {
            switch (onEnable)
            {
                case AudioPlaybackBehaviour.Play:
                    if (playRoutine != null) StopCoroutine(playRoutine);
                    playRoutine = StartCoroutine(PlayDelayed());
                    break;
                case AudioPlaybackBehaviour.Stop:
                    Stop();
                    break;
            }
        }

        IEnumerator PlayDelayed()
        {
            while (!AudioManager.Instance)
            {
                yield return new WaitForEndOfFrame();
            }
            while (!AudioManager.Instance.Initialized())
            {
                yield return new WaitForEndOfFrame();
            }

            playRoutine = null;
            Play();
        }

        private void OnDisable()
        {
            switch (onDisable)
            {
                case AudioPlaybackBehaviour.Play:
                    Play();
                    break;
                case AudioPlaybackBehaviour.Stop:
                    Stop();
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (onDestroy)
            {
                case AudioPlaybackBehaviour.Play:
                    Play();
                    break;
                case AudioPlaybackBehaviour.Stop:
                    Stop();
                    break;
            }
        }
    }
}