using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facade;

[System.Serializable]
public abstract class BaseEffectTarget
{
    public abstract BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target);
}

public class TargetSelf : BaseEffectTarget
{
    /// <summary>
    /// Specifically returns the target in-case the caster forces the target to hit itself
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        return new BaseCharacter[] { target };
    }
}

public class TargetTarget : BaseEffectTarget
{
    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        return new BaseCharacter[] { target };
    }
}

public class AllAllies : BaseEffectTarget
{
    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        if (caster.IsPlayer())
        {
            return battleSystem.PlayerList.ToArray();
        }
        else return enemyController.LivingEnemies.ToArray();
    }
}

public class AllAlliesExceptCaster : BaseEffectTarget
{
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

public class AllAlliesExceptTarget : BaseEffectTarget
{
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

public class AllEnemies : BaseEffectTarget
{
    public override BaseCharacter[] GetTargets(BaseCharacter caster, BaseCharacter target)
    {
        if (!caster.IsPlayer()) return battleSystem.PlayerList.ToArray();
        else return enemyController.LivingEnemies.ToArray();
    }
}