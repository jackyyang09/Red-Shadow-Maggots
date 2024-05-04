using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;

public class LaunchOptions : MonoBehaviour
{
    [SerializeField] string[] commands;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitUntil(() => BattleSystem.Initialized);

        //  Not having this here risks invoking certain cheats too early, causing the game to hang
        yield return new WaitForSeconds(2);

        foreach (var s in commands)
        {
            DebugLogConsole.ExecuteCommand(s);
        }
    }
}