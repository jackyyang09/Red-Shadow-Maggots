using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JSAM;
using DG.Tweening;

public class VoiceResponseSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        BattleSystem.OnWaveClear += WaveClear;
        BattleSystem.OnFinalWaveClear += GameClear;
        BattleSystem.OnStartPhase[BattlePhases.BattleLose.ToInt()] += PlayerLose;

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
        BattleSystem.OnWaveClear -= WaveClear;
        BattleSystem.OnFinalWaveClear -= GameClear;
        BattleSystem.OnStartPhase[BattlePhases.BattleLose.ToInt()] -= PlayerLose;

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

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => BattleSystem.Initialized);

        if (EnemyController.Instance.EnemyList.Any(e => e.Reference.isBoss))
        {
            AudioManager.PlayMusic(BattleSceneMusic.BossTheme);
        }
        else
        {
            AudioManager.PlayMusic(BattleSceneMusic.BattleTheme);
        }
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
        DOVirtual.DelayedCall(2, new TweenCallback(() =>
        {
            var source = AudioManager.PlaySound(audio);
        }));
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

    private void WeaponHit(BaseCharacter character)
    {
        var audio = character.Reference.weaponSound;
        AudioManager.PlaySound(audio);
        if (BaseCharacter.IncomingDamage.IsCritical) AudioManager.PlaySound(BattleSceneSounds.HitCritical);
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
}