﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [AddComponentMenu("")]
    public class JSAMMusicChannelHelper : BaseAudioChannelHelper<JSAMMusicFileObject>
    {
        protected override float Volume
        {
            get
            {
                var vol = AudioManager.InternalInstance.ModifiedMusicVolume;
                if (audioFile) vol *= audioFile.relativeVolume;
                return vol;
            }
        }

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

            return base.Play(file);
        }

        public override void Stop(bool stopInstantly = true)
        {
            StopAllCoroutines();
            if (stopInstantly)
            {
                AudioSource.Stop();
            }
        }

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