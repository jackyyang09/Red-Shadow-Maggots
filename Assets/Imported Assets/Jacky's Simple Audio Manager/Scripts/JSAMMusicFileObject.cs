using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [CreateAssetMenu(fileName = "New Audio Music File", menuName = "AudioManager/New Audio File Music Object", order = 1)]
    public class JSAMMusicFileObject : BaseAudioFileObject
    {
        public enum TransitionModes
        {
            None,
            CrossfadeIn,
            CrossfadeOut,
            CrossfadeInAndOut
        }

        [Tooltip("Adds a transition effect for playing this music. If not none, music will fade-in and out on Play and will fade-out on Stop")]
        [SerializeField] TransitionModes transitionMode = TransitionModes.None;
        public TransitionModes TransitionMode { get { return transitionMode; } }
        [SerializeField] float transitionInTime;
        public float TransitionInTime { get { return transitionInTime; } }
        [SerializeField] float transitionOutTime;
        public float TransitionOutTime { get { return transitionOutTime; } }

        [HideInInspector]
        [Tooltip("If true, music will always start and end between loop points")]
        public bool clampToLoopPoints = false;

        /// <summary>
        /// Starting loop point, stored as time for accuracy sake, converted to samples in back-end
        /// </summary>
        [HideInInspector] public float loopStart;
        /// <summary>
        /// Ending loop point, stored as time for accuracy sake, converted to samples in back-end
        /// </summary>
        [HideInInspector] public float loopEnd;

        [HideInInspector] public int bpm = 120;

        AudioClip cachedFile;

#if UNITY_EDITOR

        string fileExtension = "";

        /// <summary>
        /// Returns true if this AudioFile houses a .WAV
        /// </summary>
        /// <returns></returns>
        public bool IsWavFile()
        {
            if (cachedFile != file)
            {
                string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets(file.name)[0]);
                string trueFilePath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/") + 1) + filePath;
                fileExtension = trueFilePath.Substring(trueFilePath.Length - 4);
                cachedFile = file;
            }
            return fileExtension == ".wav";
        }
#endif
    }
}