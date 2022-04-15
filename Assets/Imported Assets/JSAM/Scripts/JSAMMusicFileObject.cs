using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [CreateAssetMenu(fileName = "New Audio Music File", menuName = "AudioManager/New Audio File Music Object", order = 1)]
    public class JSAMMusicFileObject : BaseAudioFileObject
    {
        /// <summary>
        /// Different from fades in that fades happen only at the very start/end of a track and transitions happen from anywhere Play() or Stop() is called
        /// </summary>
        public enum TransitionModes
        {
            None,
            CrossfadeInAndOut
        }

        [Tooltip("Adds a transition effect for playing this music. If not none, music will fade-in and out on Play and will fade-out on Stop")]
        [SerializeField] TransitionModes transitionMode = TransitionModes.None;
        public TransitionModes TransitionMode { get { return transitionMode; } }
        [SerializeField] float transitionInTime;
        public float TransitionInTime { get { return transitionInTime; } }
        [SerializeField] float transitionOutTime;
        public float TransitionOutTime { get { return transitionOutTime; } }
    }
}