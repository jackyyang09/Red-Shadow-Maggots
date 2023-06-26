using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Effect", menuName = "ScriptableObjects/Game Effects/Shield", order = 1)]
public class BaseShieldEffect : BaseGameEffect
{
    public enum ScalingStat
    {
        Health,
        Attack
    }

    [SerializeField] ScalingStat stat;
    [SerializeField] GameObject forceFieldPrefab;
    BaseCharacter targetCharacter;
    ForceFieldFX forceFieldInstance;

    public override void Activate(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

        switch (stat)
        {
            case ScalingStat.Health:
                target.GiveShield(user.MaxHealth * user.DefenseModified * percentageChange);
                break;
            case ScalingStat.Attack:
                target.GiveShield(user.AttackModified * user.DefenseModified * percentageChange);
                break;
        }

        targetCharacter = target;

        forceFieldInstance = Instantiate(forceFieldPrefab, target.CharacterMesh.transform).GetComponent<ForceFieldFX>();
        forceFieldInstance.Initialize(target);

        target.OnShieldBroken += OnShieldBroken;
    }

    public override void OnExpire(BaseCharacter user, BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        base.OnExpire(user, target, strength, customValues);
        Destroy(forceFieldInstance.gameObject);
    }

    void OnShieldBroken()
    {
        targetCharacter.RemoveAllEffectsOfType(this, true);
        Destroy(forceFieldInstance.gameObject);
    }

    public override string GetEffectDescription(TargetMode targetMode, EffectStrength strength, float[] customValues, int duration)
    {
        float percentageChange = (float)GetEffectStrength(strength, customValues);

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

        s += "a shield that absorbs Damage equal to " + percentageChange * 100 + "% " +
            "of your Defense times your ";

        switch (stat)
        {
            case ScalingStat.Health:
                s += "Max Health";
                break;
            case ScalingStat.Attack:
                s += "Attack";
                break;
        }

        return s + " " + DurationDescriptor(duration);
    }

    public override object GetEffectStrength(EffectStrength strength, float[] customValues)
    {
        switch (strength)
        {
            case EffectStrength.Custom:
                return customValues[0];
            case EffectStrength.Weak:
                return 0.5f;
            case EffectStrength.Small:
                return 0.75f;
            case EffectStrength.Medium:
                return 1f;
            case EffectStrength.Large:
                return 1.5f;
            case EffectStrength.EX:
                return 2f;
        }
        return 0;
    }
}
