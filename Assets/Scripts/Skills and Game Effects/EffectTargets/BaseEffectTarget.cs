using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[System.Serializable]
public abstract class BaseEffectTarget
{
    public abstract string Descriptor { get; }
    public abstract bool IsPositive(BaseGameEffect e, TargetMode targetMode);
    public abstract BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target);
}

[System.Serializable]
public class TargetSelf : BaseEffectTarget
{
    public override string Descriptor => "Self";
    public override bool IsPositive(BaseGameEffect e, TargetMode targetMode)
    {
        switch (e.effectType)
        {
            case EffectType.Debuff:
            case EffectType.Damage:
                return false;
        }
        return true;
    }

    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        return new BaseCharacter[] { caster };
    }
}

[System.Serializable]
public class TargetTarget : BaseEffectTarget
{
    public override string Descriptor => "Target";
    public override bool IsPositive(BaseGameEffect e, TargetMode targetMode)
    {
        switch (targetMode)
        {
            case TargetMode.Self:
            case TargetMode.OneAlly:
            case TargetMode.AllAllies:
                switch (e.effectType)
                {
                    case EffectType.Buff:
                    case EffectType.Heal:
                        return true;
                }
                return false;
            case TargetMode.OneEnemy:
            case TargetMode.AllEnemies:
                switch (e.effectType)
                {
                    case EffectType.Debuff:
                    case EffectType.Damage:
                        return true;
                }
                return false;
        }

        return true;
    }

    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        return new BaseCharacter[] { target };
    }
}

[System.Serializable]
public class OpposingCharacter : TargetTarget
{
    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        return new BaseCharacter[] { battleSystem.OpposingCharacter };
    }
}

[System.Serializable]
public class AllAllies : BaseEffectTarget
{
    public override string Descriptor => "All Allies";
    public override bool IsPositive(BaseGameEffect e, TargetMode targetMode)
    {
        switch (e.effectType)
        {
            case EffectType.Debuff:
            case EffectType.Damage:
                return false;
        }
        return true;
    }

    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        if (caster.IsPlayer())
        {
            return battleSystem.PlayerList.ToArray();
        }
        else return enemyController.LivingEnemies.ToArray();
    }
}

[System.Serializable]
public class AllAlliesExceptCaster : BaseEffectTarget
{
    public override string Descriptor => "All Allies Except User";
    public override bool IsPositive(BaseGameEffect e, TargetMode targetMode)
    {
        switch (e.effectType)
        {
            case EffectType.Debuff:
            case EffectType.Damage:
                return false;
        }
        return true;
    }

    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        if (caster.IsPlayer())
        {
            var l = battleSystem.PlayerList;
            l.Remove(caster as PlayerCharacter);
            return l.ToArray();
        }
        else
        {
            var l = enemyController.LivingEnemies;
            l.Remove(caster as EnemyCharacter);
            return l.ToArray();
        }
    }
}

[System.Serializable]
public class AllAlliesExceptTarget : BaseEffectTarget
{
    public override string Descriptor => "All Allies Except Target";
    public override bool IsPositive(BaseGameEffect e, TargetMode targetMode)
    {
        switch (e.effectType)
        {
            case EffectType.Debuff:
            case EffectType.Damage:
                return false;
        }
        return true;
    }

    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        if (caster.IsPlayer())
        {
            var l = battleSystem.PlayerList;
            l.Remove(target as PlayerCharacter);
            return l.ToArray();
        }
        else
        {
            var l = enemyController.LivingEnemies;
            l.Remove(caster as EnemyCharacter);
            return l.ToArray();
        }
    }
}

[System.Serializable]
public class AllEnemies : BaseEffectTarget
{
    public override string Descriptor => "All Enemies";
    public override bool IsPositive(BaseGameEffect e, TargetMode targetMode)
    {
        switch (e.effectType)
        {
            case EffectType.Debuff:
            case EffectType.Damage:
                return true;
        }
        return false;
    }

    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        if (!caster.IsPlayer()) return battleSystem.PlayerList.ToArray();
        else return enemyController.LivingEnemies.ToArray();
    }
}