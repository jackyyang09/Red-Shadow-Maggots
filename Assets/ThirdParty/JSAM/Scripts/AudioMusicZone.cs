﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [AddComponentMenu("AudioManager/Audio Music Zone")]
    public class AudioMusicZone : BaseAudioMusicFeedback
    {
        [System.Serializable]
        public class MusicZone
        {
            public Vector3 Position;
            public float MaxDistance;
            public float MinDistance;
        }

        public bool keepPlayingWhenAway;

        public List<MusicZone> MusicZones = new List<MusicZone>();

        Transform Listener => AudioManager.AudioListener.transform;

        JSAMMusicChannelHelper helper;

        private void Start()
        {
            if (keepPlayingWhenAway)
            {
                helper = AudioManager.PlayMusic(music, null, helper);
            }
        }

        // Update is called once per frame
        void Update()
        {
            float loudest = 0;
            for (int i = 0; i < MusicZones.Count; i++)
            {
                var z = MusicZones[i];
                float dist = Vector3.Distance(Listener.position, z.Position);
                if (dist <= z.MaxDistance)
                {
                    if (!helper) helper = AudioManager.PlayMusic(music, null, helper);

                    if (dist <= z.MinDistance)
                    {
                        // Set to the max volume
                        helper.AudioSource.volume = AudioManager.MusicVolume * music.relativeVolume;
                        return; // Can't be beat
                    }
                    else
                    {
                        float distanceFactor = Mathf.InverseLerp(z.MaxDistance, z.MinDistance, dist);
                        float newVol = AudioManager.MusicVolume * music.relativeVolume * distanceFactor;
                        if (newVol > loudest) loudest = newVol;
                    }
                }
            }
            if (helper) helper.AudioSource.volume = loudest;
        }
    }
}