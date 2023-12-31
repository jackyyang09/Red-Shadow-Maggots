using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSidebar : BasicSingleton<CharacterSidebar>
{
    [SerializeField] OptimizedCanvas canvas;
    public OptimizedCanvas Canvas => canvas;

    [SerializeField] StatRenderer[] statRenderers;

    public void UpdateStats(PlayerSave.MaggotState state, CharacterObject character)
    {
        for (int i = 0; i < statRenderers.Length; i++)
        {
            statRenderers[i].UpdateStat(state, character, false);
        }
    }
}