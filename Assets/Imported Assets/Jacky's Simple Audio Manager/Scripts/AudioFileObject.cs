﻿using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    #region Effect Structs
    [System.Serializable]
    public struct AudioChorusObj
    {
        public bool enabled;
        /// <summary>Clamped between 0 to 1</summary>
        public float dryMix;
        /// <summary>Clamped between 0 to 1</summary>
        public float wetMix1;
        /// <summary>Clamped between 0 to 1</summary>
        public float wetMix2;
        /// <summary>Clamped between 0 to 1</summary>
        public float wetMix3;
        /// <summary>Clamped between 0 to 100</summary>
        public float delay;
        /// <summary>Clamped between 0 to 20</summary>
        public float rate;
        /// <summary>Clamped between 0 to 1</summary>
        public float depth;
    }

    [System.Serializable]
    public struct AudioDistortionObj
    {
        public bool enabled;
        /// <summary>
        /// Ranges from 0 to 1
        /// </summary>
        public float distortionLevel;
    }

    [System.Serializable]
    public struct AudioEchoObj
    {
        public bool enabled;
        /// <summary>Clamped between 10 to 5000</summary>
        public float delay;
        /// <summary>Clamped between 0 to 1</summary>
        public float decayRatio;
        /// <summary>Clamped between 0 to 1</summary>
        public float wetMix;
        /// <summary>Clamped between 0 to 1</summary>
        public float dryMix;
    }

    [System.Serializable]
    public struct AudioHighPassObj
    {
        public bool enabled;
        /// <summary>
        /// Ranges from 10 to 22000
        /// </summary>
        public float cutoffFrequency;
        /// <summary>
        /// Ranges from 1 to 10
        /// </summary>
        public float highpassResonanceQ;
    }

    [System.Serializable]
    public struct AudioLowPassObj
    {
        public bool enabled;
        /// <summary>
        /// Ranges from 10 to 22000
        /// </summary>
        public float cutoffFrequency;
        /// <summary>
        /// Ranges from 1 to 10
        /// </summary>
        public float lowpassResonanceQ;
    }

    [System.Serializable]
    public struct AudioReverbObj
    {
        public bool enabled;
        public AudioReverbPreset reverbPreset;
        /// <summary>Clamped between –10000 to 0</summary>
        public float dryLevel;
        /// <summary>Clamped between –10000 to 0</summary>
        public float room;
        /// <summary>Clamped between –10000 to 0</summary>
        public float roomHF;
        /// <summary>Clamped between –10000 to 0</summary>
        public float roomLF;
        /// <summary>Clamped between 0.1 to 20</summary>
        public float decayTime;
        /// <summary>Clamped between 0.1 to 20</summary>
        public float decayHFRatio;
        /// <summary>Clamped between -10000 to 1000</summary>
        public float reflectionsLevel;
        /// <summary>Clamped between 0 to 0.3</summary>
        public float reflectionsDelay;
        /// <summary>Clamped between -10000 to 2000</summary>
        public float reverbLevel;
        /// <summary>Clamped between 0.0 to 0.1</summary>
        public float reverbDelay;
        /// <summary>Clamped between 1000 to 20000</summary>
        public float hFReference;
        /// <summary>Clamped between 1000 to 20000</summary>
        public float lFReference;
        /// <summary>Clamped between 0 to 100</summary>
        public float diffusion;
        /// <summary>Clamped between 0 to 100</summary>
        public float density;
    }

    #endregion

    [CreateAssetMenu(fileName = "New Audio File", menuName = "AudioManager/New Audio File Object", order = 1)]
    public class AudioFileObject : ScriptableObject, IComparer<AudioFileObject>
    {
        [Header("Attach audio file here to use")]
        [SerializeField]
        public AudioClip file;

        [Header("Attach audio files here to use")]
        [SerializeField]
        public List<AudioClip> files = new List<AudioClip>();

        [HideInInspector] public bool useLibrary;
        [HideInInspector] public bool neverRepeat;
        [HideInInspector] public int lastClipIndex = -1;

        [HideInInspector]
        public string category = "";
        public static Dictionary<string, int> projectCategories = new Dictionary<string, int>();
        public static Dictionary<string, int> projectMusicCategories = new Dictionary<string, int>();

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The volume of this Audio File relative to the volume levels defined in the main AudioManager. Leave at 1 to keep unchanged. The lower the value, the quieter it will be during playback.")]
        public float relativeVolume = 1;

        [SerializeField]
        [Tooltip("If true, playback will be affected based on distance and direction from listener. Otherwise, sounds will be played at the same volume at all times.")]
        public bool spatialize = false;

        [SerializeField]
        [Tooltip("If set above 0, sound can be heard from up to this distance before finally fading away. Acts as an override to the max distance value set in the AudioSource prefab. Good for ambient sounds. Only works if \"spatialize\" is set to true.")]
        public float maxDistance;

        [Tooltip("If there are several sounds playing at once, sounds with higher priority will be culled by Unity's sound system later than sounds with lower priority. \"Music\" has the absolute highest priority and \"Spam\" has the lowest.")]
        [SerializeField]
        public Priority priority = Priority.Default;

        [Tooltip("The frequency that the sound plays at by default. \"Pitch shift\" is added to this value additively to get the final pitch. Negative \"pitches\" result in the audio being played backwards.")]
        [Range(0, 3), SerializeField]
        public float startingPitch = 1;

        [Tooltip("Amount of random variance to the sound's frequency to be applied (both positive and negative) when this sound is played. Keep below 0.2 for best results.")]
        [SerializeField]
        [Range(0, 0.5f)]
        public float pitchShift = 0.05f;

        [Tooltip("If true, takes the pitch settings and applies them to the frequency as negative values, making the sound playback in reverse.")]
        [SerializeField]
        public bool playReversed = false;

        [Tooltip("Adds a delay in seconds before this sound is played. If the sound loops, delay is only added to when the sound is first played before the first loop.")]
        [SerializeField]
        public float delay = 0;

        [Tooltip("If true, will ignore the \"Time Scaled Sounds\" parameter in AudioManager and will keep playing the sound even when the Time Scale is set to 0")]
        [SerializeField]
        public bool ignoreTimeScale = false;

        [Tooltip("Add fade to your sound. Set the details of this fade using the FadeMode tools.")]
        [SerializeField]
        public FadeMode fadeMode;
        [Tooltip("The percentage of time the sound takes to fade-in relative to it's total length.")]
        [SerializeField, HideInInspector]
        public float fadeInDuration;

        [Tooltip("The percentage of time the sound takes to fade-out relative to it's total length.")]
        [SerializeField, HideInInspector]
        public float fadeOutDuration;

        [Tooltip("If true, this audio file ignore effects applied in the Audio Effects stack and any effects applied to the Audio Listener.")]
        [SerializeField, HideInInspector]
        public bool bypassEffects;

        [Tooltip("If true, this audio file will ignore any effects applied to the Audio Listener.")]
        [SerializeField, HideInInspector]
        public bool bypassListenerEffects;

        [Tooltip("If true, this audio file will ignore reverb effects created when the Audio Listener enters a reverb zone")]
        [SerializeField, HideInInspector]
        public bool bypassReverbZones;

        /// <summary>
        /// Don't touch this unless you're modifying AudioManager functionality
        /// </summary>
        public string safeName = "";

        [SerializeField, HideInInspector] public AudioChorusObj chorusFilter;
        [SerializeField, HideInInspector] public AudioDistortionObj distortionFilter;
        [SerializeField, HideInInspector] public AudioEchoObj echoFilter;
        [SerializeField, HideInInspector] public AudioLowPassObj lowPassFilter;
        [SerializeField, HideInInspector] public AudioHighPassObj highPassFilter;
        [SerializeField, HideInInspector] public AudioReverbObj reverbFilter;

        public void Initialize()
        {
            lastClipIndex = -1;
        }

        public AudioClip GetFile()
        {
            return file;
        }

        public List<AudioClip> GetFiles()
        {
            return files;
        }

        public AudioClip GetFirstAvailableFile()
        {
            if (useLibrary)
            {
                foreach (AudioClip f in files)
                {
                    if (f != null) return f;
                }
            }
            else
            {
                return file;
            }
            return null;
        }

        public int GetFileCount()
        {
            if (!useLibrary) return (file == null) ? 0 : 1;
            int count = 0;
            foreach (AudioClip a in files)
            {
                if (a != null)
                {
                    count++;
                }
            }
            return count;
        }

        public bool IsLibraryEmpty()
        {
            if (!useLibrary)
            {
                return file == null;
            }
            else
            {
                foreach (AudioClip a in files)
                {
                    if (a != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool HasAudioClip(AudioClip a)
        {
            return file == a || files.Contains(a);
        }

        public bool UsingLibrary()
        {
            return useLibrary;
        }

        public int Compare(AudioFileObject x, AudioFileObject y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return x.safeName.CompareTo(y.safeName);
        }

#if UNITY_EDITOR
        /// <summary>
        /// May need to be optimized
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCategories()
        {
            List<AudioFileObject> files = new List<AudioFileObject>();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:AudioFileObject"))
            {
                files.Add((AudioFileObject)UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid), typeof(AudioFileObject)));
            }
            // Reset count
            if (projectCategories.Keys.Count > 0) projectCategories.Clear();
            // Increment categories based on use
            foreach (AudioFileObject file in files)
            {
                if (file.category == "" || file.category == "Hidden") continue;
                else if (projectCategories.ContainsKey(file.category))
                {
                    //projectCategories[file.category]++;
                }
                else
                {
                    projectCategories.Add(file.category, 1);
                }
            }
            // Prune unused categories
            //foreach (string key in projectCategories.Keys)
            //{
            //    if (projectCategories[key] == 0)
            //    {
            //        projectCategories.Remove(key);
            //    }
            //}

            string[] newArray = new string[projectCategories.Count];
            projectCategories.Keys.CopyTo(newArray, 0);
            List<string> newList = new List<string>(newArray);
            newList.Sort();

            return newList;
        }

        //public void SetCategoriesDirty()
        //{
        //
        //}

        public static List<string> GetMusicCategories()
        {
            List<AudioFileObject> files = new List<AudioFileObject>();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:AudioFileMusicObject"))
            {
                files.Add((AudioFileObject)UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guid), typeof(AudioFileObject)));
            }
            // Reset count
            if (projectCategories.Keys.Count > 0) projectCategories.Clear();
            // Increment categories based on use
            foreach (AudioFileObject file in files)
            {
                if (file.category == "" || file.category == "Hidden") continue;
                else if (projectCategories.ContainsKey(file.category))
                {
                    projectCategories[file.category]++;
                }
                else
                {
                    projectCategories.Add(file.category, 1);
                }
            }
            // Prune unused categories
            foreach (string key in projectCategories.Keys)
            {
                if (projectCategories[key] == 0)
                {
                    projectCategories.Remove(key);
                }
            }

            string[] newArray = new string[projectCategories.Count];
            projectCategories.Keys.CopyTo(newArray, 0);
            List<string> newList = new List<string>(newArray);
            newList.Sort();

            return newList;
        }
#endif
    }
}