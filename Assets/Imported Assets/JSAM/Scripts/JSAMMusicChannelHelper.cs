using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [AddComponentMenu("")]
    public class JSAMMusicChannelHelper : BaseAudioChannelHelper<JSAMMusicFileObject>
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            if (audioFile)
            {
                AudioManager.OnMusicVolumeChanged += OnUpdateVolume;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            AudioManager.OnMusicVolumeChanged -= OnUpdateVolume;
        }

        public override AudioSource Play(JSAMMusicFileObject file)
        {
            ClearProperties();

            if (file == null)
            {
                AudioManager.Instance.DebugWarning("Attempted to play a non-existent JSAM Music File Object!");
                return AudioSource;
            }

            audioFile = file;

            AudioSource.pitch = 1;
            OnUpdateVolume(AudioManager.MusicVolume);

            if (file.loopMode == LoopMode.NoLooping)
            {
                AudioSource.loop = false;
            }
            else
            {
                AudioSource.loop = true;
            }

            switch (file.TransitionMode)
            {
                case JSAMMusicFileObject.TransitionModes.CrossfadeInAndOut:
                    StartCoroutine(FadeIn(audioFile.TransitionInTime));
                    StartCoroutine(FadeOut(audioFile.TransitionOutTime));
                    break;
            }

            return base.Play(file);
        }

        public override void Stop(bool stopInstantly = true)
        {
            StopAllCoroutines();
            if (stopInstantly)
            {
                AudioSource.Stop();
            }
            else
            {
                switch (audioFile.TransitionMode)
                {
                    case JSAMMusicFileObject.TransitionModes.CrossfadeInAndOut:
                        StartCoroutine(FadeOut(audioFile.TransitionOutTime));
                        break;
                }
            }
        }

        protected override void OnUpdateVolume(float volume)
        {
            AudioSource.volume = AudioManager.InternalInstance.ModifiedMusicVolume;
            if (audioFile) AudioSource.volume *= audioFile.relativeVolume;
        }

#region Fade Logic
        /// <summary>
        /// </summary>
        /// <param name="fadeTime">Fade-in time in seconds</param>
        /// <returns></returns>
        protected override IEnumerator FadeIn(float fadeTime)
        {
            // Check if FadeTime isn't actually just 0
            if (fadeTime != 0) // To prevent a division by zero
            {
                float timer = 0;
                while (timer < fadeTime)
                {
                    if (audioFile.ignoreTimeScale) timer += Time.unscaledDeltaTime;
                    else timer += Time.deltaTime;

                    float volume = audioFile.relativeVolume * AudioManager.MusicVolume;
                    AudioSource.volume = Mathf.Lerp(0, volume, timer / fadeTime);
                    yield return null;
                }
            }
        }
#endregion

#if UNITY_EDITOR
        public void PlayDebug(JSAMMusicFileObject file, bool dontReset)
        {
            if (!dontReset)
            {
                AudioSource.Stop();
            }
            audioFile = file;
            AudioSource.clip = file.Files[0];
            AudioSource.timeSamples = (int)Mathf.Clamp((float)AudioSource.timeSamples, 0, (float)AudioSource.clip.samples - 1);
            AudioSource.pitch = 1;
            AudioSource.volume = file.relativeVolume;

            base.PlayDebug(dontReset);
        }
#endif
    }
}