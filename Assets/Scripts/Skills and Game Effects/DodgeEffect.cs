using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DodgeEffect), menuName = "ScriptableObjects/Game Effects/Dodge Effect", order = 1)]
public class DodgeEffect : BaseGameEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        effect.Target.IsDodging = true;

        BaseCharacter.OnCharacterAttackDodged += (c) => OnDodge(c, effect);
        // TODO: Re-applying dodge effect should remove other dodge instances
        Debug.LogWarning(nameof(DodgeEffect) + ": Re-applying dodge effect should remove other dodge instances?");
        return false;
    }

    void OnDodge(BaseCharacter character, AppliedEffect effect)
    {
        effect.UseOnce();
    }

    public override void OnExpire(AppliedEffect effect)
    {
        effect.Target.IsDodging = false;
    }
}