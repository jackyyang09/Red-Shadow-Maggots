using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Defense Effect", menuName = "ScriptableObjects/Game Stats/Defense", order = 1)]
public abstract class BaseGameStat : ScriptableObject
{
    public virtual string Name => "DEFAULT STAT NAME";
    public abstract float GetGameStat(BaseCharacter target);
    public virtual void SetGameStat(BaseCharacter target, float value) { }
    public virtual ValueType ValueType => ValueType.Value;
}

public class StatModifier
{
    float value;
    public float Value => value;
    AppliedEffect parentEffect;
    public AppliedEffect ParentEffect => parentEffect;

    public StatModifier(float v, AppliedEffect effect)
    {
        value = v;
        parentEffect = effect;
    }

    public void Deduct(float v)
    {
        value -= v;
    }
}