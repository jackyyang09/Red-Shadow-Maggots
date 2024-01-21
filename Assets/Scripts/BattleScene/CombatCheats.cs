using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using static Facade;

public class CombatCheats : MonoBehaviour
{
    [IngameDebugConsole.ConsoleMethod(nameof(MaxPlayerCrit), "Set player characters crit chance to 100%")]
    public static void MaxPlayerCrit()
    {
        foreach (var item in battleSystem.PlayerList)
        {
            item.ApplyCritChanceModifier(1);
        }

        Debug.Log("Crit rate maxed!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(AddPlayerCrit),
        "Increase player characters crit chance modifier by a number from 0 to 1")]
    public static void AddPlayerCrit(float value)
    {
        foreach (var item in battleSystem.PlayerList)
        {
            item.ApplyCritChanceModifier(value);
        }

        Debug.Log("Added " + value + "% to player crit rate to!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(ShieldPlayers), "Provide Max Shield Effect to Players")]
    public static void ShieldPlayers()
    {
        foreach (var item in battleSystem.PlayerList)
        {
            item.GiveShield(item.MaxHealth, null);
        }

        Debug.Log("Maxxed Player's shields!");
    }

    [IngameDebugConsole.ConsoleMethod(nameof(CripplePlayers), "Instantly hurt players, leaving them at 1 health")]
    public static void CripplePlayers()
    {
        foreach (var item in battleSystem.PlayerList)
        {
            BaseCharacter.IncomingDamage.TrueDamage = item.CurrentHealth - 1;
            item.TakeDamage();
        }

        Debug.Log("Players damaged!");
    }
}
