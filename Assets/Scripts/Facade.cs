static class Facade
{
    // Map Menu Systems
    public static TreasureNode treasureNode => TreasureNode.Instance;
    public static RestNode restNode => RestNode.Instance;
    public static CardListUI cardListUI => CardListUI.Instance;
    public static MaggotUpgradeUI maggotUpgradeUI => MaggotUpgradeUI.Instance;

    // Battle System
    public static BattleSystem battleSystem => BattleSystem.Instance;
    public static EnemyController enemyController => EnemyController.Instance;
    public static EnemyWaveManager waveManager => EnemyWaveManager.Instance;
    public static GameManager gameManager => GameManager.Instance;
    public static GlobalIndex index => GlobalIndex.Instance;
    public static SceneTweener sceneTweener => SceneTweener.Instance;
    public static UIManager ui => UIManager.Instance;
    public static CanteenSystem canteenSystem => CanteenSystem.Instance;
    public static PlayerControlManager playerControlManager => PlayerControlManager.Instance;

    public static ScreenEffects screenEffects => ScreenEffects.Instance;

    // System Objects
    public static GachaSystem gachaSystem => GachaSystem.Instance;
    public static GraphicsSettings graphicsSettings => GraphicsSettings.Instance;
    public static SceneLoader sceneLoader => SceneLoader.Instance;

    // Save Systems
    public static PlayerDataManager playerDataManager => PlayerDataManager.Instance as PlayerDataManager;
    public static BattleStateManager battleStateManager => BattleStateManager.Instance as BattleStateManager;
    public static GameEffectLoader gameEffectLoader => GameEffectLoader.Instance;

    public static SickDev.DevConsole.DevConsole devConsole => SickDev.CommandSystem.DevConsoley.Instance;
}