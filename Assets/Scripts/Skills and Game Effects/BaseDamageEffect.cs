using UnityEngine;

public class BaseDamageEffect : BaseGameEffect
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
}