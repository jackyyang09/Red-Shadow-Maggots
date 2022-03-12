static class Facade
{
    public static BattleSystem battleSystem => BattleSystem.Instance;
    public static EnemyController enemyController => EnemyController.Instance;
    public static EnemyWaveManager waveManager => EnemyWaveManager.Instance;
    public static GlobalIndex index => GlobalIndex.Instance;
    public static SceneTweener sceneTweener => SceneTweener.Instance;
    public static UIManager ui => UIManager.Instance;
    public static CanteenSystem canteenSystem => CanteenSystem.Instance;
}