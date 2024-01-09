using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacterPassive : MonoBehaviour
{
    protected BaseCharacter baseCharacter;

    protected virtual void OnEnable()
    {
        baseCharacter = GetComponentInParent<BaseCharacter>();
        if (!baseCharacter) enabled = false;
    }

    protected virtual void ApplyEffect(BaseGameEffect e, int s)
    {
        if (s == 0) return;
        EffectProperties props = new EffectProperties() { effect = e, effectDuration = -1, stacks = s };
        BaseCharacter.ApplyEffectToCharacter(props, baseCharacter, baseCharacter);
    }

    protected virtual void ApplyEffect(EffectProperties props)
    {
        BaseCharacter.ApplyEffectToCharacter(props, baseCharacter, baseCharacter);
    }

    protected virtual void ApplyEffectToCharacter(BaseCharacter character, EffectProperties props)
    {
        BaseCharacter.ApplyEffectToCharacter(props, baseCharacter, character);
    }
}
