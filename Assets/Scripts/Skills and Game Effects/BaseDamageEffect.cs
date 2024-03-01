using UnityEngine;

public class BaseDamageEffect : BaseStatScaledEffect
{
    public virtual void DealDamage(float value, BaseCharacter target)
    {
        DamageStruct damageStruct = new DamageStruct
        {
            Percentage = 1,
            TrueDamage = value,
            Effectivity = DamageEffectivess.Normal,
            QTEResult = QuickTimeBase.QTEResult.Early
        };
        target.TakeDamage(damageStruct);
    }

    public virtual void ConsumeHealth(float value, BaseCharacter target)
    {
        DamageStruct damageStruct = new DamageStruct
        {
            Percentage = 1,
            TrueDamage = value,
            Effectivity = DamageEffectivess.Normal,
            QTEResult = QuickTimeBase.QTEResult.Early
        };
        target.ConsumeHealth(damageStruct);
    }

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.Self:
                s += "Lose ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "lose ";
                break;
        }

        s += RSMConstants.Keywords.Short.HEALTH + " equal to " + 
            EffectValueDescriptor(props.effectValues[0], "your", stat);

        return s;
    }
}