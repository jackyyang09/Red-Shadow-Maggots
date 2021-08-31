using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [AddComponentMenu("")]
    public class JSAMMusicChannelHelper : BaseAudioChannelHelper<JSAMMusicFileObject>
    {
        public override AudioSource Play(JSAMMusicFileObject file)
        {
            ClearProperties();

            if (file == null)
            {
                AudioManager.Instance.DebugWarning("Attempted to play a non-existent JSAM Music File Object!");
                return AudioSource;
            }

            AudioSource.pitch = 1;

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

        public void StopPlayback(bool immediately)
        {
            StopAllCoroutines();
            if (immediately)
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

#if UNITY_EDITOR
        public void PlayDebug(JSAMMusicFileObject file, bool dontReset)
        {
            if (!dontReset)
            {
                AudioSource.Stop();
            }
            AudioSource.timeSamples = (int)Mathf.Clamp((float)AudioSource.timeSamples, 0, (float)AudioSource.clip.samples - 1);
            AudioSource.pitch = 1;

            base.Play(file);
        }
#endif
    }
}