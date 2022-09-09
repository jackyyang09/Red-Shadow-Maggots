using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectActivationTiming
{
    BattlePhase
}

[CreateAssetMenu(fileName = "New Equip", menuName = "ScriptableObjects/Equipment", order = 1)]
public class EquipmentObject : ScriptableObject
{
    public string equipName;
    public string description;
    public Sprite sprite;

    public EffectProperties[] gameEffects;
    public EffectActivationTiming timing;
    public BattlePhases battlePhase;
}

/// <summary>
/// An instance of a EquipmentObject to used by EquipmentManager
/// </summary>
public class RuntimeEquipment
{
    public EquipmentObject Reference;

    public void Tick()
    {

    }
}