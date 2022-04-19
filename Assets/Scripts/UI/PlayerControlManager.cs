using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerControlMode
{
    FullControl,
    InCutscene,
    InSettings,
    Length
}

public class PlayerControlManager : BasicSingleton<PlayerControlManager>
{
    bool[] activeControlModes = new bool[(int)PlayerControlMode.Length];

    [SerializeField] PlayerControlMode currentMode;
    public PlayerControlMode CurrentMode { get { return currentMode; } }

    public static System.Action[] OnPlayerControlChanged = 
        new System.Action[(int)PlayerControlMode.Length];

    public void ReturnControl()
    {
        activeControlModes[(int)currentMode] = false;

        for (int i = (int)currentMode - 1; i > -1; i--)
        {
            if (activeControlModes[i])
            {
                currentMode = (PlayerControlMode)i;
                break;
            }
        }

        OnPlayerControlChanged[(int)currentMode]?.Invoke();
    }

    public void SetControlMode(PlayerControlMode mode)
    {
        activeControlModes[(int)mode] = true;

        for (int i = ((int)PlayerControlMode.Length - 1); i > -1; i--)
        {
            if (activeControlModes[i])
            {
                currentMode = (PlayerControlMode)i;
                OnPlayerControlChanged[i]?.Invoke();
                return;
            }
        }
    }
}
