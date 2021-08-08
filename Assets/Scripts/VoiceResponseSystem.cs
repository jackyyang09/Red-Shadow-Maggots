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
        GlobalEvents.OnEnterWave += Entry;
        GlobalEvents.OnEnterFinalWave += EnterFinal;
        GlobalEvents.OnWaveClear += WaveClear;
        GlobalEvents.OnFinalWaveClear += GameClear;

        GlobalEvents.OnCharacterStartAttack += Attack;
        GlobalEvents.OnCharacterAttacked += PlayerAttacked;
        GlobalEvents.OnCharacterExecuteAttack += WeaponHit;
        GlobalEvents.OnCharacterActivateSkill += UsedSkill;
        GlobalEvents.OnCharacterDeath += CharacterDeath;
        GlobalEvents.OnPlayerDefeat += PlayerLose;
        GlobalEvents.OnPlayerQuickTimeAttackSuccess += HitEffective;
        GlobalEvents.OnPlayerQuickTimeBlockSuccess += BlockEffective;

        PlayerCharacter.onSelectedPlayerCharacterChange += UIClick;
        EnemyCharacter.onSelectedEnemyCharacterChange += UIClick;

        PlayerCharacter.onSelectedPlayerCharacterChange += Select;
    }

    private void OnDisable()
    {
        GlobalEvents.OnEnterWave -= Entry;
        GlobalEvents.OnEnterFinalWave -= EnterFinal;
        GlobalEvents.OnWaveClear -= WaveClear;
        GlobalEvents.OnFinalWaveClear -= GameClear;

        GlobalEvents.OnCharacterStartAttack -= Attack;
        GlobalEvents.OnCharacterAttacked -= PlayerAttacked;
        GlobalEvents.OnCharacterExecuteAttack -= WeaponHit;
        GlobalEvents.OnCharacterActivateSkill -= UsedSkill;
        GlobalEvents.OnCharacterDeath -= CharacterDeath;
        GlobalEvents.OnPlayerDefeat -= PlayerLose;
        GlobalEvents.OnPlayerQuickTimeAttackSuccess -= HitEffective;
        GlobalEvents.OnPlayerQuickTimeBlockSuccess -= BlockEffective;

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
        var audio = BattleSystem.Instance.RandomPlayerCharacter.Reference.voiceEntry;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    public void EnterFinal()
    {
        AudioManager.PlayMusic(Music.BossTheme);
    }

    public void WaveClear()
    {
        var audio = BattleSystem.Instance.RandomPlayerCharacter.Reference.voiceVictory;
        AudioManager.instance.PlaySoundInternal(audio);
    }

    public void GameClear()
    {
        var audio = BattleSystem.Instance.RandomPlayerCharacter.Reference.voiceVictory;
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

    private void UsedSkill(BaseCharacter character, GameSkill skill)
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
