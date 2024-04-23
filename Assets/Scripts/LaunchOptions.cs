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

        foreach (var s in commands)
        {
            DebugLogConsole.ExecuteCommand(s);
        }
    }
}