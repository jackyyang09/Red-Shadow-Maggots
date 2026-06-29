using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static Facade;
using static BaseCharacter;

[System.Serializable]
public class BaseApplicationStyle
{
    protected float Delay => sceneTweener.SkillEffectApplyDelay;

    public virtual string ProcessTargetDescriptor(BaseEffectTarget s) => s.Descriptor;

    protected int ApplyEffects(EffectGroup group, BaseCharacter caster, BaseCharacter castTarget)
    {
        int applied = 0;

        if (group.effectProps.effect)
        {
            var t = new TargetProps() { Caster = caster, Targets = new[] { castTarget } };
            if (ApplyEffectToCharacter(group.effectProps, t))
            {
                GlobalEvents.OnGameEffectApplied?.Invoke(group.effectProps.effect);
                applied++;
            }
        }

        return applied;
    }

    public virtual IEnumerator Apply(EffectGroup effects, BaseCharacter caster, BaseCharacter target)
    {
        int applied = 0;
        var effectTargets = effects.effectTarget.GetTargets(caster, target);

        foreach (var t in effectTargets)
        {
            applied += ApplyEffects(effects, caster, t);
        }

        // Wait 0 or 1 times the delay factor
        yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);
    }
}

[System.Serializable]
public class RepeatedApplication : BaseApplicationStyle
{
    [Min(1)]
    public int Repeats = 1;

    public override IEnumerator Apply(EffectGroup effects, BaseCharacter caster, BaseCharacter target)
    {
        var effectTargets = effects.effectTarget.GetTargets(caster, target);

        for (int i = 0; i < 1 + Repeats; i++)
        {
            int applied = 0;
            foreach (var t in effectTargets)
            {
                applied += ApplyEffects(effects, caster, t);
            }

            // Wait 0 or 1 times the delay factor
            yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);
        }
    }
}

[System.Serializable]
public class RandomApplication : RepeatedApplication
{
    public override string ProcessTargetDescriptor(BaseEffectTarget s)
    {
        return s.Descriptor + " (Applied to 1 random target, " + Repeats + " times)";
    }

    public override IEnumerator Apply(EffectGroup effects, BaseCharacter caster, BaseCharacter target)
    {
        var effectTargets = effects.effectTarget.GetTargets(caster, target);

        BaseCharacter rTarget = target;
        for (int i = 0; i < 1 + Repeats; i++)
        {
            int applied = ApplyEffects(effects, caster, rTarget);

            // Wait 0 or 1 times the delay factor
            yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);

            rTarget = effectTargets[Random.Range(0, effectTargets.Length)];
        }
    }
}

[System.Serializable]
public class BounceApplication : RepeatedApplication
{
    public override IEnumerator Apply(EffectGroup effects, BaseCharacter caster, BaseCharacter target)
    {
        //var target = potentialTargets[0];
        //for (int i = 0; i < 1 + Repeats; i++)
        //{
        //    int applied = ApplyEffects(effects, caster, target);
        //
        //    // Wait 0 or 1 times the delay factor
        //    yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);
        //
        //    Debug.LogWarning("Bounce hasn't been implemented!");
        //    target = potentialTargets[Random.Range(0, potentialTargets.Count)];
        //}
        Debug.LogError("Bounce hasn't been implemented!");
        yield return null;
    }
}