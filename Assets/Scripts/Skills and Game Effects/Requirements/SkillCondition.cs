using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SkillCondition
{
    public virtual bool CanUse() => true;
}

[System.Serializable]
public class AppliedEffectCondition : SkillCondition
{
    [System.Serializable]
    public class EffectStacks
    {
        public BaseGameEffect effect;
        public int stacks;
    }

    [SerializeField] public List<EffectStacks> effects;

    public override bool CanUse()
    {
        //foreach (var item in effects)
        //{
        //    if (item.stacks)
        //}
        return base.CanUse();
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SkillCondition))]
public class SkillConditionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PropertyField(
            position,
            property,
            true);

        EditorGUI.EndProperty();
    }
}
#endif