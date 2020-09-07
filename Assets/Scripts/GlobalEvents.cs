using System;

public class GlobalEvents
{
    public static Action onEnterWave;
    public static Action onAnyEnemyDeath;
    public static Action onWaveClear;
    public static Action onEnterFinalWave;
    public static Action onFinalWaveClear;
    public static Action onPlayerCrit;
    public static Action onPlayerDefeat;
    public static Action onAnyPlayerDeath;

    public static Action<BaseCharacter> onCharacterStartAttack;
    public static Action<BaseCharacter> onCharacterAttacked;
    public static Action<BaseCharacter> onCharacterDeath;

    public static Action<BaseCharacter> onSelectCharacter;
    public static Action<BaseCharacter> onCharacterExecuteAttack;
}