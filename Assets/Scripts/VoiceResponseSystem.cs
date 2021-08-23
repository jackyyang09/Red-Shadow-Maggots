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
        AudioManager.PlaySound(BattleSceneSounds.HitEffective);
    }

    void BlockEffective()
    {
        AudioManager.PlaySound(BattleSceneSounds.BlockSuccess);
    }

    void UIClick(BaseCharacter obj)
    {
        AudioManager.PlaySound(BattleSceneSounds.UIClick);
    }

    public void Entry()
    {
        var audio = BattleSystem.Instance.RandomPlayerCharacter.Reference.voiceEntry;
        AudioManager.PlaySound(audio);
    }

    public void EnterFinal()
    {
        AudioManager.PlayMusic(BattleSceneMusic.BossTheme);
    }

    public void WaveClear()
    {
        var audio = BattleSystem.Instance.RandomPlayerCharacter.Reference.voiceVictory;
        AudioManager.PlaySound(audio);
    }

    public void GameClear()
    {
        var audio = BattleSystem.Instance.RandomPlayerCharacter.Reference.voiceVictory;
        AudioManager.PlaySound(BattleSceneSounds.PlayerVictory);
        //AudioManager.StopMusic();
        var source = AudioManager.PlaySound(audio);
        source.Stop();
        source.AudioSource.PlayDelayed(2);
    }

    public void Attack(BaseCharacter character)
    {
        var audio = character.Reference.voiceAttack;
        AudioManager.PlaySound(audio);
    }

    public void PlayerAttacked(BaseCharacter character)
    {
        var audio = character.Reference.voiceHurt;
        AudioManager.PlaySound(audio);
    }

    private void UsedSkill(BaseCharacter character, GameSkill skill)
    {
        AudioManager.PlaySound(BattleSceneSounds.SkillGeneral);
    }

    public void Select(PlayerCharacter character)
    {
        var audio = character.Reference.voiceSelected;
        AudioManager.PlaySound(audio);
    }

    private void WeaponHit(BaseCharacter character, DamageStruct damage)
    {
        var audio = character.Reference.weaponSound;
        AudioManager.PlaySound(audio);
        if (damage.isCritical) AudioManager.PlaySound(BattleSceneSounds.HitCritical);
    }

    private void CharacterDeath(BaseCharacter character)
    {
        var audio = character.Reference.voiceDeath;
        AudioManager.PlaySound(audio);
    }

    void PlayerLose()
    {
        //AudioManager.StopMusic();
        AudioManager.PlaySound(BattleSceneSounds.PlayerDefeat);
    }
}
