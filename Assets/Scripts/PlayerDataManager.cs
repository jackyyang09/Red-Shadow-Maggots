﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public class MaggotState
    {
        public string GUID;
        public float Health;
        public float Exp;
    }

    /// <summary>
    /// Uses MaggotStates indices
    /// </summary>
    public int[] Party = new int[3] { -1, -1, -1 };
    public List<MaggotState> MaggotStates = new List<MaggotState>();
    public int BattlesFought { get; set; }
    public int NodesTravelled { get; set; }
    public int Exp { get; set; }
    public int Money { get; set; }
    public bool InBattle { get; set; }
    public Vector2 MapPosition { get; set; }

    public PlayerData()
    {
        MapPosition = new Vector2(-3, -2.5f);
    }
}

public class PlayerDataManager : BaseSaveManager<PlayerData>
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

    public void AddNewMaggot(PlayerData.MaggotState newState)
    {
        loadedData.MaggotStates.Add(newState);

        int index = loadedData.MaggotStates.Count - 1;
        if (loadedData.Party[1] == -1) loadedData.Party[1] = index;
        else if (loadedData.Party[0] == -1) loadedData.Party[0] = index;
        else if (loadedData.Party[2] == -1) loadedData.Party[2] = index;

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
        SaveData();
        OnUpdateFloorCount?.Invoke(LoadedData.NodesTravelled);
    }
}
