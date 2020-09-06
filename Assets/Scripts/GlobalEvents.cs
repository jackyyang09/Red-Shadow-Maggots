using System;

public static class GlobalEvents
{
    public static Action onEnterWave;
    public static Action onPlayerDeath;
    public static Action onEnemyDeath;
    public static Action onWaveClear;
    public static Action onPlayerDefeat;

    public static Action<PlayerCharacter> onPlayerStartAttack;
}