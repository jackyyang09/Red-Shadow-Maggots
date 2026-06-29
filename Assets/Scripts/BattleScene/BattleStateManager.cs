using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class BattleState
{
    [System.Serializable]
    public class SerializedValue
    {
        public string Type;
        public string[] Values;
        [SerializeReference] public SerializedValue[] NestedValues;

        private static readonly Dictionary<string, Func<SerializedValue, BaseEffectValue>> deserializeDict = new()
        {
            [nameof(FlatValue)] = sv => new FlatValue
            {
                Flat = float.Parse(sv.Values[0]),
                ValueType = (ValueType)Convert.ToInt16(sv.Values[1]),
            },
            [nameof(StackCountValue)] = sv => new StackCountValue
            {
                ValueType = (ValueType)Convert.ToInt16(sv.Values[0]),
                StackEffect = GameEffectLoader.Instance.FromIndex(Convert.ToInt16(sv.Values[1])),
                StackSource = Activator.CreateInstance(System.Type.GetType(sv.Values[2])) as BaseEffectTarget
            },
            [nameof(AdditionValueOp)] = sv => new AdditionValueOp
            {
                ValueType = (ValueType)Convert.ToInt16(sv.Values[0]),
                Left = Deserialize(sv.NestedValues[0]),
                Right = Deserialize(sv.NestedValues[1])
            },
            [nameof(MinusValueOp)] = sv => new MinusValueOp
            {
                ValueType = (ValueType)Convert.ToInt16(sv.Values[0]),
                Left = Deserialize(sv.NestedValues[0]),
                Right = Deserialize(sv.NestedValues[1])
            },
            [nameof(MultiplyValueOp)] = sv => new MultiplyValueOp
            {
                ValueType = (ValueType)Convert.ToInt16(sv.Values[0]),
                Left = Deserialize(sv.NestedValues[0]),
                Right = Deserialize(sv.NestedValues[1])
            },
            [nameof(AttackStat)] = sv => new AttackStat(),
            [nameof(AttackWindowStat)] = sv => new AttackWindowStat(),
            [nameof(CritChanceStat)] = sv => new CritChanceStat(),
            [nameof(CritDamageStat)] = sv => new CritDamageStat(),
            [nameof(CurrentHealthStat)] = sv => new CurrentHealthStat(),
            [nameof(DamageAbsorptionStat)] = sv => new DamageAbsorptionStat(),
            [nameof(DefenseStat)] = sv => new DefenseStat(),
            [nameof(DefenseWindowStat)] = sv => new DefenseWindowStat(),
            [nameof(HealInStat)] = sv => new HealInStat(),
            [nameof(MaxHealthStat)] = sv => new MaxHealthStat(),
            [nameof(WaitLimitStat)] = sv => new WaitLimitStat(),
            [nameof(WaitStat)] = sv => new WaitStat(),
        };

        public static BaseEffectValue Deserialize(SerializedValue e)
        {
            if (deserializeDict.TryGetValue(e.Type, out var value))
                return value(e);
            return null;
        }
    }

    [System.Serializable]
    public class SerializedEffect
    {
        public int Caster;
        public TargetMode TargetMode;
        public int EffectIndex;
        public int StartingTurns;
        public int RemainingTurns;
        public int Stacks;
        public List<CachedValue> CachedValues;
        public SerializedValue Values;
    }

    [System.Serializable]
    public class State
    {
        public float Health;
        public float WaitTimer;
        public List<SerializedEffect> Effects = new List<SerializedEffect>();
    }

    [System.Serializable]
    public class PlayerState : State
    {
        public int Index;
        public int[] Cooldowns = new int[2];
    }

    [System.Serializable]
    public class EnemyState : State
    {
        public int Crit;
    }

    [System.Serializable]
    public class EnemyWave
    {
        public List<string> EnemyGUIDs = new List<string>();
    }

    public List<PlayerState> PlayerStates = new List<PlayerState>();

    public List<EnemyWave> EnemyWaves = new List<EnemyWave>();
    public List<EnemyState> EnemyStates = new List<EnemyState>();

    public int Canteens;

    public int RoomLevel = 1;
    public int SavedSeed;
    public int WaveCount = 0;
    public int RoundCount;
    public int SelectedEnemy;
    public bool[] UseSpecialCam;
    public bool BattleWon;
}

public class BattleStateManager : JsonSaveManager<BattleState>
{
    public override string FILE_NAME => "BattleState";

    public void InitializeRandom()
    {
        if (GachaSystem.Instance.LegacyMode) return;
        if (LoadedData.SavedSeed > 0)
        {
            UnityEngine.Random.InitState(LoadedData.SavedSeed);
        }
    }
}