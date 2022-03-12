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

    public static Action<BaseCharacter> OnCharacterUseSuperCritical;
    public static Action<BaseCharacter> OnCharacterFinishSuperCritical;

    public static Action<BaseGameEffect> OnGameEffectApplied;
}