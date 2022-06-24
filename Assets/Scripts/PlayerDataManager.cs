using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public class MaggotState
    {
        public string GUID;
        public float Health;
        public int Exp;
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
}

public class PlayerDataManager : BaseSaveManager<PlayerData>
{
    public override string FILE_NAME => "PlayerData";
}
