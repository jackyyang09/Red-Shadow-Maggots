using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

public class GameManager : BasicSingleton<GameManager>
{
    [SerializeField] int baseExpGain = 100;

    public int ExpGained
    {
        get
        {
            return (PlayerData.NodesTravelled + 1);
        }
    }

    int turnCount = 0;
    public int TurnCount
    {
        get { return turnCount; }
        set
        {
            turnCount = value;
            OnTurnCountChanged?.Invoke();
        }
    }
    private void IncrementTurnCount() => TurnCount++;

    [SerializeField] DevLocker.Utils.SceneReference mapScene;

    public static System.Action OnTurnCountChanged;

    private void OnEnable()
    {
        BattleSystem.OnEndTurn += IncrementTurnCount;
    }

    private void OnDisable()
    {
        BattleSystem.OnEndTurn -= IncrementTurnCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        DiscordWrapper.Instance.UpdateActivity(
            state: "In-Battle",
            largeImageKey: "morshu",
            partySize: waveManager.WaveCount + 1,
            partyMax: waveManager.TotalWaves,
            startTime: DiscordWrapper.CurrentTime
            );
    }

    public void SaveBattleState()
    {
        if (gachaSystem.LegacyMode)
        {
            Debug.Log("Gacha System is in Legacy Mode, not saving");
            return;
        }

        var partyData = new List<BattleState.PlayerState>();
        var waveData = new List<BattleState.EnemyState>();

        var seed = (int)System.DateTime.Now.Ticks;
        BattleData.SavedSeed = seed;

        for (int i = 0; i < battleSystem.PlayerCharacters.Length; i++)
        {
            BattleState.PlayerState p = null;
            var player = battleSystem.PlayerCharacters[i];
            if (player)
            {
                p = new BattleState.PlayerState();
                p.Index = BattleData.PlayerStates[i].Index;
                p.Health = player.CurrentHealth;
                p.Effects = new List<BattleState.SerializedEffect>();

                BaseGameEffect[] keys = new BaseGameEffect[player.AppliedEffects.Keys.Count];
                player.AppliedEffects.Keys.CopyTo(keys, 0);
                for (int j = 0; j < keys.Length; j++)
                {
                    for (int l = 0; l < player.AppliedEffects[keys[j]].Count; l++)
                    {
                        var se = gameEffectLoader.SerializeGameEffect(player.AppliedEffects[keys[j]][l]);
                        p.Effects.Add(se);
                    }
                }

                p.Cooldowns = new int[2];
                for (int j = 0; j < 2; j++)
                {
                    p.Cooldowns[j] = player.Skills[j].cooldownTimer;
                }
            }

            partyData.Add(p);
        }

        for (int i = 0; i < enemyController.Enemies.Length; i++)
        {
            BattleState.EnemyState d = null;
            var enemy = enemyController.Enemies[i];
            if (enemy)
            {
                d = new BattleState.EnemyState();
                d.Health = enemy.CurrentHealth;
                d.Crit = enemy.CritLevel;
                d.Effects = new List<BattleState.SerializedEffect>();

                BaseGameEffect[] keys = new BaseGameEffect[enemy.AppliedEffects.Keys.Count];
                enemy.AppliedEffects.Keys.CopyTo(keys, 0);
                for (int j = 0; j < keys.Length; j++)
                {
                    for (int l = 0; l < enemy.AppliedEffects[keys[j]].Count; l++)
                    {
                        var se = gameEffectLoader.SerializeGameEffect(enemy.AppliedEffects[keys[j]][l]);
                        d.Effects.Add(se);
                    }
                }
            }

            waveData.Add(d);
        }

        BattleData.PlayerStates = partyData;
        BattleData.EnemyStates = waveData;

        BattleData.StoredCharge = canteenSystem.AvailableCharge;
        BattleData.TurnCount = gameManager.TurnCount;
        BattleData.MoveCount = battleSystem.MoveCount;
        BattleData.SelectedEnemy = new List<EnemyCharacter>(enemyController.Enemies).IndexOf(battleSystem.ActiveEnemy);

        battleStateManager.SaveData();
    }

    public void LoadBattleState()
    {
        gameManager.TurnCount = BattleData.TurnCount;
        battleSystem.SetMoveCount(BattleData.MoveCount);
        canteenSystem.SetCanteenCharge(BattleData.StoredCharge);
        if (enemyController.Enemies[BattleData.SelectedEnemy].IsDead)
        {
            battleSystem.SetActiveEnemy(enemyController.RandomEnemy);
        }
        else
        {
            battleSystem.SetActiveEnemy(enemyController.Enemies[BattleData.SelectedEnemy]);
        }

        characterLoader.ApplyPlayerSavedEffects();
        characterLoader.ApplyEnemySavedEffects();
    }

    public void ReturnToMapVictorious()
    {
        PlayerData.Exp += ExpGained;
        PlayerData.InBattle = false;
        PlayerData.BattlesFought++;
        PlayerData.Canteens = canteenSystem.StoredCharge;

        for (int i = 0; i < battleSystem.PlayerCharacters.Length; i++)
        {
            if (!battleSystem.PlayerCharacters[i]) continue;
            var j = BattleData.PlayerStates[i].Index;
            PlayerData.MaggotStates[j].Health = battleSystem.PlayerCharacters[i].CurrentHealth;
            PlayerData.MaggotStates[j].SkillCoodowns[0] = battleSystem.PlayerCharacters[i].Skills[0].cooldownTimer;
            PlayerData.MaggotStates[j].SkillCoodowns[1] = battleSystem.PlayerCharacters[i].Skills[1].cooldownTimer;
        }

        battleStateManager.DeleteData();

        playerDataManager.SaveData();

        sceneLoader.SwitchScene(mapScene.SceneName);
    }

    public void RestartBattle()
    {
        sceneLoader.ReloadScene();
    }
}