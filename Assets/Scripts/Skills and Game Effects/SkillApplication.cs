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

    protected int ApplyEffects(EffectGroup group, BaseCharacter caster, BaseCharacter target)
    {
        int applied = 0;

        if (group.damageProps.effect)
        {
            if (ApplyEffectToCharacter(group.damageProps, caster, target))
            {
                GlobalEvents.OnGameEffectApplied?.Invoke(group.damageProps.effect);
                applied++;
            }
        }
        if (group.effectProps.effect)
        {
            if (ApplyEffectToCharacter(group.effectProps, caster, target))
            {
                GlobalEvents.OnGameEffectApplied?.Invoke(group.effectProps.effect);
                applied++;
            }
        }

        return applied;
    }

    public virtual IEnumerator Apply(EffectGroup effects, BaseCharacter caster, List<BaseCharacter> potentialTargets)
    {
        int applied = ApplyEffects(effects, caster, potentialTargets[0]);

        // Wait 0 or 1 times the delay factor
        yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);
    }
}

public class RepeatedApplication : BaseApplicationStyle
{
    [Min(1)]
    public int Repeats = 1;

    public override IEnumerator Apply(EffectGroup effects, BaseCharacter caster, List<BaseCharacter> potentialTargets)
    {
        for (int i = 0; i < Repeats; i++)
        {
            int applied = ApplyEffects(effects, caster, potentialTargets[0]);

            // Wait 0 or 1 times the delay factor
            yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);
        }
    }
}

public class RandomApplication : RepeatedApplication
{
    public override IEnumerator Apply(EffectGroup effects, BaseCharacter caster, List<BaseCharacter> potentialTargets)
    {
        var target = potentialTargets[0];
        for (int i = 0; i < Repeats; i++)
        {
            int applied = ApplyEffects(effects, caster, target);

            // Wait 0 or 1 times the delay factor
            yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);

            target = potentialTargets[Random.Range(0, potentialTargets.Count)];
        }
    }
}

public class BounceApplication : RepeatedApplication
{
    public override IEnumerator Apply(EffectGroup effects, BaseCharacter caster, List<BaseCharacter> potentialTargets)
    {
        var target = potentialTargets[0];
        for (int i = 0; i < Repeats; i++)
        {
            int applied = ApplyEffects(effects, caster, target);

            // Wait 0 or 1 times the delay factor
            yield return new WaitForSeconds(Mathf.Min(applied, 1) * Delay);

            Debug.LogWarning("Bounce hasn't been implemented!");
            target = potentialTargets[Random.Range(0, potentialTargets.Count)];
        }
    }
}

[System.Serializable]
public class EffectGroup
{
    public EffectProperties damageProps;
    public EffectProperties effectProps;
    [SerializeReference] public BaseApplicationStyle appStyle = new BaseApplicationStyle();
    public TargetMode targetOverride;

    public EffectGroup Copy()
    {
        var group = MemberwiseClone() as EffectGroup;
        group.damageProps = damageProps.Copy();
        group.effectProps = effectProps.Copy();
        return group;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EffectGroup))]
public class EffectGroupDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty damageProps, effectProps, appStyle, targetOverride;
        damageProps = property.FindPropertyRelative(nameof(damageProps));
        effectProps = property.FindPropertyRelative(nameof(effectProps));
        appStyle = property.FindPropertyRelative(nameof(appStyle));
        targetOverride = property.FindPropertyRelative(nameof(targetOverride));

        //EditorGUI.PropertyField(position, damageProps, false);

        EditorGUI.PropertyField(
            position,
            property,
            true);

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(BaseApplicationStyle))]
public class SkillApplicationDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var defaultHeight = EditorGUI.GetPropertyHeight(property, true) + 2;

        if (property.managedReferenceValue == null) return defaultHeight;

        var n = property.managedReferenceValue.GetType().Name;
        if (n == nameof(BaseApplicationStyle)) return defaultHeight;
        else return defaultHeight * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        //EditorGUI.PropertyField(
        //    position,
        //    property,
        //    true);

        var list = new List<string>
        {
            nameof(BaseApplicationStyle),
            nameof(RepeatedApplication),
            nameof(RandomApplication),
            nameof(BounceApplication)
        };

        int index = 0;
        if (property.managedReferenceValue != null)
        {
            var t = property.managedReferenceValue.GetType().Name;
            index = list.IndexOf(t);
        }
        else
        {
            property.managedReferenceValue = new BaseApplicationStyle();
        }

        float labelHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.String, GUIContent.none);
        var labelRect = new Rect(position);
        labelRect.height = labelHeight;
        var cRect = EditorGUI.PrefixLabel(labelRect, label);
        cRect.xMin -= 15;

        EditorGUI.BeginChangeCheck();
        index = EditorGUI.Popup(cRect, index, list.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            switch (index)
            {
                case 0:
                    property.managedReferenceValue = new BaseApplicationStyle();
                    break;
                case 1:
                    property.managedReferenceValue = new RepeatedApplication();
                    break;
                case 2:
                    property.managedReferenceValue = new RandomApplication();
                    break;
                case 3:
                    property.managedReferenceValue = new BounceApplication();
                    break;
            }
        }

        if (index > 0)
        {
            var h = EditorGUI.GetPropertyHeight(SerializedPropertyType.Generic, GUIContent.none);
            var rect = new Rect(position);
            rect.height = h;
            rect.y += h + 2f;
            cRect = EditorGUI.PrefixLabel(rect, new GUIContent("Repeats"));
            cRect.xMin -= 15;
            EditorGUI.PropertyField(cRect, property.FindPropertyRelative("Repeats"), GUIContent.none);
        }

        EditorGUI.EndProperty();
    }
}
#endif