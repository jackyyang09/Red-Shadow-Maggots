using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public class VoiceResponseSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        EnemyWaveManager.OnEnterBossWave += EnterFinal;

        BattleSystem.OnWaveClear += WaveClear;
        BattleSystem.OnFinalWaveClear += GameClear;
        BattleSystem.OnStartPhase[BattlePhases.BattleLose.ToInt()] += PlayerLose;
        BattleSystem.OnTickEffect += OnTickEffect;

        BaseCharacter.OnCharacterStartAttack += Attack;
        BaseCharacter.OnCharacterAttackBlocked += AttackBlocked;
        BaseCharacter.OnCharacterExecuteAttack += WeaponHit;
        BaseCharacter.OnCharacterActivateSkill += UsedSkill;
        BaseCharacter.OnCharacterDeath += CharacterDeath;
        GlobalEvents.OnPlayerQuickTimeAttackSuccess += HitEffective;
        GlobalEvents.OnPlayerQuickTimeBlockSuccess += BlockEffective;
        GlobalEvents.OnGameEffectApplied += GameEffectApplied;

        PlayerCharacter.OnSelectedPlayerCharacterChange += UIClick;
        EnemyCharacter.OnSelectedEnemyCharacterChange += UIClick;

        PlayerCharacter.OnSelectedPlayerCharacterChange += Select;
    }

    private void OnDisable()
    {
        EnemyWaveManager.OnEnterBossWave -= EnterFinal;

        BattleSystem.OnWaveClear -= WaveClear;
        BattleSystem.OnFinalWaveClear -= GameClear;
        BattleSystem.OnStartPhase[BattlePhases.BattleLose.ToInt()] -= PlayerLose;
        BattleSystem.OnTickEffect -= OnTickEffect;

        BaseCharacter.OnCharacterStartAttack -= Attack;
        BaseCharacter.OnCharacterAttackBlocked -= AttackBlocked;
        BaseCharacter.OnCharacterExecuteAttack -= WeaponHit;
        BaseCharacter.OnCharacterActivateSkill -= UsedSkill;
        BaseCharacter.OnCharacterDeath -= CharacterDeath;
        GlobalEvents.OnPlayerQuickTimeAttackSuccess -= HitEffective;
        GlobalEvents.OnPlayerQuickTimeBlockSuccess -= BlockEffective;
        GlobalEvents.OnGameEffectApplied -= GameEffectApplied;

        PlayerCharacter.OnSelectedPlayerCharacterChange -= UIClick;
        EnemyCharacter.OnSelectedEnemyCharacterChange -= UIClick;

        PlayerCharacter.OnSelectedPlayerCharacterChange -= Select;
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
        AudioManager.StopMusicIfPlaying(AudioManager.MainMusic);
        var source = AudioManager.PlaySound(audio);
        source.Stop();
        source.AudioSource.PlayDelayed(2);
    }

    public void Attack(BaseCharacter character)
    {
        var audio = character.Reference.voiceAttack;
        AudioManager.PlaySound(audio);
    }

    public void AttackBlocked(BaseCharacter character, bool blocked)
    {
        if (blocked) return;
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

    private void GameEffectApplied(BaseGameEffect obj)
    {
        AudioManager.PlaySound(obj.activationSound);
    }

    private void OnTickEffect(BaseGameEffect obj)
    {
        AudioManager.PlaySound(obj.tickSound);
    }
}