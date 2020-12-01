using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using System;

public class VoiceResponseSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        GlobalEvents.onEnterWave += Entry;
        GlobalEvents.onEnterFinalWave += EnterFinal;
        GlobalEvents.onWaveClear += WaveClear;
        GlobalEvents.onFinalWaveClear += GameClear;

        GlobalEvents.onCharacterStartAttack += Attack;
        GlobalEvents.onCharacterAttacked += PlayerAttacked;
        GlobalEvents.onCharacterExecuteAttack += WeaponHit;
        GlobalEvents.onCharacterActivateSkill += UsedSkill;
        GlobalEvents.onCharacterDeath += CharacterDeath;
        GlobalEvents.onPlayerDefeat += PlayerLose;
        GlobalEvents.onPlayerQuickTimeAttackSuccess += HitEffective;
        GlobalEvents.onPlayerQuickTimeBlockSuccess += BlockEffective;

        PlayerCharacter.onSelectedPlayerCharacterChange += UIClick;
        EnemyCharacter.onSelectedEnemyCharacterChange += UIClick;

        PlayerCharacter.onSelectedPlayerCharacterChange += Select;
    }

    private void OnDisable()
    {
        GlobalEvents.onEnterWave -= Entry;
        GlobalEvents.onEnterFinalWave -= EnterFinal;
        GlobalEvents.onWaveClear -= WaveClear;
        GlobalEvents.onFinalWaveClear -= GameClear;

        GlobalEvents.onCharacterStartAttack -= Attack;
        GlobalEvents.onCharacterAttacked -= PlayerAttacked;
        GlobalEvents.onCharacterExecuteAttack -= WeaponHit;
        GlobalEvents.onCharacterActivateSkill -= UsedSkill;
        GlobalEvents.onCharacterDeath -= CharacterDeath;
        GlobalEvents.onPlayerDefeat -= PlayerLose;
        GlobalEvents.onPlayerQuickTimeAttackSuccess -= HitEffective;
        GlobalEvents.onPlayerQuickTimeBlockSuccess -= BlockEffective;

        PlayerCharacter.onSelectedPlayerCharacterChange -= UIClick;
        EnemyCharacter.onSelectedEnemyCharacterChange -= UIClick;

        PlayerCharacter.onSelectedPlayerCharacterChange -= Select;
    }

    void HitEffective()
    {
        AudioManager.PlaySound(Sounds.Hit_Effective);
    }

    void BlockEffective()
    {
        AudioManager.PlaySound(Sounds.Block_Success);
    }

    void UIClick(BaseCharacter obj)
    {
        AudioManager.PlaySound(Sounds.UIClick);
    }

    public void Entry()
    {
        var audio = BattleSystem.instance.RandomPlayerCharacter.Reference.voiceEntry;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    public void EnterFinal()
    {
        AudioManager.PlayMusic(Music.BossTheme);
    }

    public void WaveClear()
    {
        var audio = BattleSystem.instance.RandomPlayerCharacter.Reference.voiceVictory;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    public void GameClear()
    {
        var audio = BattleSystem.instance.RandomPlayerCharacter.Reference.voiceVictory;
        AudioManager.PlaySound(Sounds.PlayerVictory);
        AudioManager.StopMusic();
        var source = AudioManager.instance.PlaySoundInternal(audio);
        source.Stop();
        source.PlayDelayed(2);
    }

    public void Attack(BaseCharacter character)
    {
        var audio = character.Reference.voiceAttack;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    public void PlayerAttacked(BaseCharacter character)
    {
        var audio = character.Reference.voiceHurt;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    private void UsedSkill(BaseCharacter character)
    {
        AudioManager.PlaySound(Sounds.SkillGeneral);
    }

    public void Select(PlayerCharacter character)
    {
        var audio = character.Reference.voiceSelected;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    private void WeaponHit(BaseCharacter character, DamageStruct damage)
    {
        var audio = character.Reference.weaponSound;
        AudioManager.instance.PlaySoundInternal(audio);
        if (damage.isCritical) AudioManager.PlaySound(Sounds.Hit_Critical);
    }

    private void CharacterDeath(BaseCharacter character)
    {
        var audio = character.Reference.voiceDeath;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    void PlayerLose()
    {
        AudioManager.StopMusic();
        AudioManager.PlaySound(Sounds.PlayerDefeat);
    }
}
