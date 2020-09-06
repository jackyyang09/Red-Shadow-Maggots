using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public class VoiceResponseSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        GlobalEvents.onEnterWave += PlayEntrySound;
        GlobalEvents.onPlayerStartAttack += PlayAttackSound;
    }

    private void OnDisable()
    {
        GlobalEvents.onEnterWave -= PlayEntrySound;
        GlobalEvents.onPlayerStartAttack -= PlayAttackSound;
    }

    public void PlayEntrySound()
    {
        var audio = BattleSystem.instance.RandomPlayerCharacter.Reference.voiceEntry;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    public void PlayAttackSound(PlayerCharacter character)
    {
        var audio = character.Reference.voiceAttack;
        AudioManager.instance.PlaySoundInternal(audio);
    }
}
