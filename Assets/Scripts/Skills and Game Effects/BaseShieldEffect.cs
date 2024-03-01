using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Effect", menuName = "ScriptableObjects/Game Effects/Shield", order = 1)]
public class BaseShieldEffect : BaseStatScaledEffect
{
    [SerializeField] GameObject forceFieldPrefab;

    /// <summary>
    /// Applies a Shield to all allies, absorbing DMG equal to 45% of Gepard's DEF plus 600 for 3 turn(s).
    /// </summary>
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Shield";
    public override string ExplainerDescription =>
        "Takes " + RSMConstants.Keywords.Short.DAMAGE + " in place of " + 
        RSMConstants.Keywords.Short.HEALTH + ". ";
        //RSMConstants.Keywords.Short.DAMAGE + " taken is reduced by " + RSMConstants.Keywords.Short.DEFENSE + ".";

    public override bool Activate(AppliedEffect effect)
    {
        var target = effect.target;

        effect.instantiatedObjects = new GameObject[1];
        if (effect.target.ShieldPercent == 0)
        {
            var ff = Instantiate(forceFieldPrefab, target.CharacterMesh.transform).GetComponent<ForceFieldFX>();
            ff.Initialize(target);
            effect.instantiatedObjects[0] = ff.gameObject;

            effect.customCallbacks = new System.Action[1];
            effect.customCallbacks[0] = () => OnSpecialCallback(effect);
            target.OnShieldBroken += effect.customCallbacks[0];
        }

        if (effect.cachedValues.Count > 0)
        {
            target.GiveShield(effect.cachedValues[0], effect);
        }
        else
        {
            float value = GetValue(stat, effect.values[0], effect.caster);

            target.GiveShield(value, effect);
            effect.cachedValues.Add(value);
        }

        return true;
    }

    public override void OnExpire(AppliedEffect effect)
    {
        base.OnExpire(effect);
        Destroy(effect.instantiatedObjects[0]);
    }

    public override void OnSpecialCallback(AppliedEffect effect)
    {
        effect.target.OnShieldBroken -= effect.customCallbacks[0];
        Destroy(effect.instantiatedObjects[0]);
        base.OnSpecialCallback(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        return ExplainerDescription;
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.None:
            case TargetMode.Self:
                s = "Receive ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "receives ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "receive ";
                break;
        }

        s += EffectValueDescriptor(props.effectValues[0], "your", stat);

        s += "as a <u>Shield</u> ";

        return s + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    }
}