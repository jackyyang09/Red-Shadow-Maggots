using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave
{
    [System.Serializable]
    public class MaggotState
    {
        public string GUID;
        public float Health;
        public float Exp;
    }

    /// <summary>
    /// Uses MaggotStates indices
    /// </summary>
    public int[] Party = new int[] { -1, -1, -1, -1 };
    public List<MaggotState> MaggotStates = new List<MaggotState>();
    public int BattlesFought { get; set; }
    public int NodesTravelled { get; set; }
    public int Exp { get; set; }
    public int Money { get; set; }
    public bool InBattle { get; set; }
    public Vector2 MapPosition { get; set; }

    public PlayerSave()
    {
        MapPosition = new Vector2(-3, -2.5f);
    }
}

public class PlayerSaveManager : BaseSaveManager<PlayerSave>
{
    public override string FILE_NAME => "PlayerData";

    public static System.Action OnMaggotStatesChanged;
    public static System.Action<int> OnUpdateEXP;
    public static System.Action<int> OnUpdateFloorCount;

    private void OnEnable()
    {
        Map.MapPlayerTracker.OnEnterNode += OnAdvanceFloor;
    }

    private void OnDisable()
    {
        Map.MapPlayerTracker.OnEnterNode -= OnAdvanceFloor;
    }

    public void AddNewMaggot(PlayerSave.MaggotState newState)
    {
        loadedData.MaggotStates.Add(newState);

        int index = loadedData.MaggotStates.Count - 1;
        for (int i = 0; i < loadedData.Party.Length; i++)
        {
            if (loadedData.Party[i] == -1)
            {
                loadedData.Party[i] = index;
                break;
            }
        }
        SaveData();

        OnMaggotStatesChanged?.Invoke();
    }

    public void SetExp(int exp)
    {
        LoadedData.Exp = exp;
        SaveData();
        OnUpdateEXP?.Invoke(LoadedData.Exp);
    }

    private void OnAdvanceFloor(Map.NodeType obj)
    {
        LoadedData.NodesTravelled++;
        OnUpdateFloorCount?.Invoke(LoadedData.NodesTravelled);
    }

#if UNITY_EDITOR
    [IngameDebugConsole.ConsoleMethod(nameof(GiveXP), "Gives a bunch of EXP")]
    public static void GiveXP()
    {
        (Instance as PlayerSaveManager).SetExp(9999);

        Debug.Log("Enemies damaged!");
    }
#endif
}
