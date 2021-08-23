using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [AddComponentMenu("")]
    [DefaultExecutionOrder(2)]
    public class JSAMSoundChannelHelper : BaseAudioChannelHelper<JSAMSoundFileObject>
    {
        float prevPlaybackTime;

        protected override void Update()
        {
            if (AudioSource.loop)
            {
                // Check if the AudioSource is beginning to loop
                if (prevPlaybackTime > AudioSource.time)
                {
                    if (audioFile.UsingLibrary)
                    {
                        AudioClip[] library = audioFile.Files.ToArray();
                        do
                        {
                            AudioSource.clip = library[Random.Range(0, library.Length)];
                        } while (AudioSource.clip == null); // If the user is a dingus and left a few null references in the library
                        AudioSource.Play();
                    }
                    AudioSource.pitch = JSAMSoundFileObject.GetRandomPitch(audioFile);
                }
                prevPlaybackTime = AudioSource.time;
            }

            base.Update();
        }

        public override AudioSource Play(JSAMSoundFileObject file)
        {
            ClearProperties();

            if (file == null)
            {
                AudioManager.Instance.DebugWarning("Attempted to play a non-existent JSAM Sound File Object!");
                return AudioSource;
            }

            AudioSource.pitch = JSAMSoundFileObject.GetRandomPitch(file);

            switch (file.loopMode)
            {
                case LoopMode.NoLooping:
                    AudioSource.loop = false;
                    break;
                case LoopMode.Looping:
                case LoopMode.LoopWithLoopPoints:
                case LoopMode.ClampedLoopPoints:
                    AudioSource.loop = true;
                    break;
            }

            switch (file.fadeMode)
            {
                case FadeMode.FadeIn:
                    StartCoroutine(FadeIn(audioFile.fadeInDuration * AudioSource.clip.length));
                    break;
                case FadeMode.FadeInAndOut:
                    StartCoroutine(FadeIn(audioFile.fadeInDuration * AudioSource.clip.length));
                    break;
                case FadeMode.FadeOut:
                    StartCoroutine(FadeOut(audioFile.fadeOutDuration * AudioSource.clip.length));
                    break;
            }

            return base.Play(file);
        }

        public override void Stop(bool stopInstantly = true)
        {
            base.Stop(stopInstantly);
            prevPlaybackTime = -1;
        }

#if UNITY_EDITOR
        public void PlayDebug(JSAMSoundFileObject file, bool dontReset)
        {
            if (!dontReset)
            {
                AudioSource.Stop();
            }
            AudioSource.timeSamples = (int)Mathf.Clamp((float)AudioSource.timeSamples, 0, (float)AudioSource.clip.samples - 1);
            AudioSource.pitch = JSAMSoundFileObject.GetRandomPitch(file);

            base.Play(file);
        }
#endif
    }
}