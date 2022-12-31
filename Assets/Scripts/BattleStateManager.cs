﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class BattleState
{
    public class SerializedEffect
    {
        public int EffectIndex;
        public int RemainingTurns;
        public EffectStrength Strength;
        public float[] CustomValues;
    }

    public class State
    {
        public float Health;
        public List<SerializedEffect> Effects = new List<SerializedEffect>();
    }

    public class PlayerState : State
    {
        public int Index;
        public int[] Cooldowns = new int[2];
    }

    public class EnemyState : State
    {
        public int Crit;
    }

    public List<PlayerState> PlayerStates = new List<PlayerState>();

    public List<List<string>> EnemyGUIDs = new List<List<string>>();
    public List<EnemyState> EnemyStates = new List<EnemyState>();

    public float StoredCharge;

    public int RoomLevel = 1;
    public int SavedSeed;
    public int WaveCount = 0;
    public int TurnCount;
    public bool[] IsBossWave;
    public bool[] UseSpecialCam;
    public bool BattleWon;
}

public class BattleStateManager : BaseSaveManager<BattleState>
{
    public override string FILE_NAME => "BattleState";

    public void InitializeRandom()
    {
        if (GachaSystem.Instance.LegacyMode) return;
        if (LoadedData.SavedSeed > 0)
        {
            Random.InitState(LoadedData.SavedSeed);
        }
    }
}