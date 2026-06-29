using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SkillCondition
{
    public virtual bool CanUse(BaseCharacter character) => true;
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

    public override bool CanUse(BaseCharacter c)
    {
        if (c)
        {
            int pass = 0;
            foreach (var item in effects)
            {
                if (!c.EffectDictionary.ContainsKey(item.effect))
                {
                    return false;
                }
                if (c.EffectDictionary[item.effect].Count >= item.stacks)
                {
                    pass++;
                }
            }
            return effects.Count == pass;
        }

        return base.CanUse(c);
    }
}

//#if UNITY_EDITOR
//[CustomPropertyDrawer(typeof(SkillCondition))]
//public class SkillConditionDrawer : PropertyDrawer
//{
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return EditorGUI.GetPropertyHeight(property, true);
//    }
//
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        EditorGUI.BeginProperty(position, label, property);
//
//        EditorGUI.PropertyField(
//            position,
//            property,
//            true);
//
//        EditorGUI.EndProperty();
//    }
//}
//#endif