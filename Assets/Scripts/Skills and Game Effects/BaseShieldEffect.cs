using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Effect", menuName = "ScriptableObjects/Game Effects/Shield", order = 1)]
public class BaseShieldEffect : BaseGameEffect
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
        var target = effect.Target;

        effect.instantiatedObjects = new GameObject[1];
        if (effect.Target.ShieldPercent == 0)
        {
            var ff = Instantiate(forceFieldPrefab).GetComponent<ForceFieldFX>();
            ff.transform.SetParent(target.AnimHelper.SkeletonRoot);
            ff.transform.localPosition = Vector2.zero;
            ff.Initialize(target);
            effect.instantiatedObjects[0] = ff.gameObject;

            effect.customCallbacks = new System.Action[1];
            effect.customCallbacks[0] = () => OnSpecialCallback(effect);
            target.OnShieldBroken += effect.customCallbacks[0];
        }

        if (effect.cachedValues.Count > 0)
        {
            target.GiveShield(effect.cachedValues[0].Value, effect);
        }
        else
        {
            float value = effect.value.GetValue(effect.targetProps);

            target.GiveShield(value, effect);
            effect.cachedValues.Add(new() { Value = value, Type = ValueType.Value });
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
        effect.Target.OnShieldBroken -= effect.customCallbacks[0];
        Destroy(effect.instantiatedObjects[0]);
        base.OnSpecialCallback(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = base.GetEffectDescription(effect);
        d = d.Replace("$SHIELD", effect.cachedValues[0].String());

        return d;
    }
}