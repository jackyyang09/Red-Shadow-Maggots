using System;

public class GlobalEvents
{
    public static Action OnAnyEnemyDeath;
    public static Action OnAnyPlayerDeath;
    public static Action OnPlayerQuickTimeAttackSuccess;
    public static Action OnPlayerQuickTimeBlockSuccess;

    public static Action OnModifyGameSpeed;

    public static Action OnEnterBattleCutscene;
    public static Action OnExitBattleCutscene;

    public static Action<BaseCharacter> OnCharacterStartAttack;
    public static Action<BaseCharacter> OnCharacterAttacked;

    public static Action<BaseCharacter, GameSkill> OnCharacterActivateSkill;
    public static Action<BaseCharacter> OnCharacterSuperCritical;
    public static Action<BaseCharacter> OnCharacterFinishSuperCritical;

    public static Action<BaseGameEffect> OnGameEffectApplied;

    public static Action<BaseCharacter> OnCharacterDeath;

    public static Action<BaseCharacter> OnSelectCharacter;
    public static Action<BaseCharacter, DamageStruct> OnCharacterExecuteAttack;
}