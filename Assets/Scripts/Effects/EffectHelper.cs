using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectHelper : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particles;

    public void PlayEffect()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Play();
        }
    }
}