using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

public abstract class BaseEffectEditor<T> : Editor where T : ScriptableObject
{
    protected T targetObject;

    protected Texture cachedPreview;
    protected UnityEngine.Object lastSprite;

    protected abstract string SHOW_GAMEEFFECTS { get; }
    protected bool gameEffectsFoldout
    {
        get
        {
            if (!EditorPrefs.HasKey(SHOW_GAMEEFFECTS))
            {
                EditorPrefs.SetBool(SHOW_GAMEEFFECTS, false);
            }
            return EditorPrefs.GetBool(SHOW_GAMEEFFECTS);
        }
        set
        {
            EditorPrefs.SetBool(SHOW_GAMEEFFECTS, value);
        }
    }

    protected static Color buffColour = new Color(0.6f, 0.8f, 1f);
    protected static Color debuffColour = new Color(1, 0.25f, 0.25f);

    GUIStyle nullStyle;
    protected GUIStyle NullStyle
    {
        get
        {
            if (nullStyle == null)
            {
                nullStyle = new GUIStyle(EditorStyles.label)
                    .ApplyTextAnchor(TextAnchor.MiddleCenter)
                    .ApplyWordWrap()
                    .ApplyRichText();
            }
            return nullStyle;
        }
    }
    GUIStyle buffStyle;
    protected GUIStyle BuffStyle
    {
        get
        {
            if (buffStyle == null)
            {
                buffStyle = new GUIStyle(NullStyle).SetTextColor(buffColour);
            }
            return buffStyle;
        }
    }
    GUIStyle debuffStyle;
    protected GUIStyle DebuffStyle
    {
        get
        {
            if (debuffStyle == null)
            {
                debuffStyle = new GUIStyle(NullStyle).SetTextColor(debuffColour);
            }
            return debuffStyle;
        }
    }

    protected virtual void OnEnable()
    {
        targetObject = target as T;

        InitializeSerializedProperties();

        CacheSpriteAssetPreview();
    }   

    protected SerializedProperty abilityName;
    protected SerializedProperty sprite;
    protected SerializedProperty condition;
    protected SerializedProperty effects;
    protected SerializedProperty events;

    public virtual void InitializeSerializedProperties()
    {
        abilityName = serializedObject.FindProperty(nameof(abilityName));
        sprite = serializedObject.FindProperty(nameof(sprite));
        condition = serializedObject.FindProperty(nameof(condition));
        effects = serializedObject.FindProperty(nameof(effects));
        events = serializedObject.FindProperty(nameof(events));
    }

    protected void CacheSpriteAssetPreview()
    {
        cachedPreview = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GetAssetPath(sprite.objectReferenceValue));
        lastSprite = sprite.objectReferenceValue;
    }

    protected void RenderConditionProperty()
    {
        var list = new List<string>
        {
            "None",
            nameof(AppliedEffectCondition),
        };

        int index = 0;
        if (condition.managedReferenceValue != null)
        {
            var t = condition.managedReferenceValue.GetType().Name;
            index = list.IndexOf(t);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Condition Type");

        EditorGUI.BeginChangeCheck();
        index = EditorGUILayout.Popup(index, list.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            switch (index)
            {
                case 0:
                    condition.managedReferenceValue = null;
                    break;
                case 1:
                    condition.managedReferenceValue = new AppliedEffectCondition();
                    break;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (index > 0)
        {
            EditorGUILayout.PropertyField(condition);
        }
    }

    protected void RenderDragAndDropSprite()
    {
        Rect dragRect = EditorGUILayout.BeginVertical(GUI.skin.box);
        GUIStyle style = new GUIStyle(EditorStyles.label).ApplyTextAnchor(TextAnchor.MiddleCenter);
        GUILayout.Label(cachedPreview, style, new GUILayoutOption[] { GUILayout.MaxHeight(150), GUILayout.MinWidth(150), GUILayout.ExpandWidth(true) });

        if (EditorHelper.DragAndDropRegion(dragRect, "", "Drop Sprite Here"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]));
            if (asset)
            {
                sprite.objectReferenceValue = asset;
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sprite);
        if (EditorGUI.EndChangeCheck() || sprite.objectReferenceValue != lastSprite)
        {
            CacheSpriteAssetPreview();
        }
    }

    protected void RenderEffectDescriptions(TargetMode target, EffectGroup[] group, string[] skillDescriptions)
    {
        if (group.Length == 0)
        {
            EditorGUILayout.LabelField("None", NullStyle);
            return;
        }

        for (int i = 0; i < group.Length; i++)
        {
            var ge = group[i];

            if (ge.damageProps == null && ge.effectProps == null)
            {
                EditorGUILayout.LabelField("None", NullStyle);
                continue;
            }

            var desc = skillDescriptions[i];
            if (desc.Contains("<u>"))
            {
                desc = desc.Replace("u>", "b>");
            }

            TargetMode t = target;
            if (ge.targetOverride != TargetMode.None) t = ge.targetOverride;

            if (!ge.effectProps.effect) continue;
            switch (ge.effectProps.effect.effectType)
            {
                case EffectType.None:
                    EditorGUILayout.LabelField(desc, NullStyle);
                    break;
                case EffectType.Heal:
                case EffectType.Buff:
                    switch (t)
                    {
                        case TargetMode.OneEnemy:
                        case TargetMode.AllEnemies:
                            EditorGUILayout.LabelField(desc, DebuffStyle);
                            break;
                        case TargetMode.OneAlly:
                        case TargetMode.AllAllies:
                        case TargetMode.Self:
                            EditorGUILayout.LabelField(desc, BuffStyle);
                            break;
                    }
                    break;
                case EffectType.Debuff:
                case EffectType.Damage:
                    switch (t)
                    {
                        case TargetMode.OneEnemy:
                        case TargetMode.AllEnemies:
                            EditorGUILayout.LabelField(desc, BuffStyle);
                            break;
                        case TargetMode.OneAlly:
                        case TargetMode.AllAllies:
                        case TargetMode.Self:
                            EditorGUILayout.LabelField(desc, DebuffStyle);
                            break;
                    }
                    break;
            }
        }
    }

    protected void RenderEffectProperties(EffectProperties[] gameEffects, SerializedProperty effectsProp)
    {
        EditorGUILayout.PropertyField(effectsProp);
        return;

        gameEffectsFoldout = EditorGUILayout.Foldout(gameEffectsFoldout, effectsProp.GUIContent());
        //gameEffectsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(gameEffectsFoldout, gameEffects.GUIContent());
        if (gameEffectsFoldout && Selection.objects.Length == 1)
        {
            var cachedSize = effectsProp.arraySize;
            effectsProp.arraySize = EditorGUILayout.IntField("Size", effectsProp.arraySize);
            for (int i = 0; i < Mathf.Min(cachedSize, effectsProp.arraySize); i++)
            {
                EditorGUI.indentLevel++;
                var prop = effectsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                var ge = gameEffects[i];
                var effectProp = prop.FindPropertyRelative("effect");
                var strengthProp = prop.FindPropertyRelative("strength");
                var customValuesProp = prop.FindPropertyRelative("customValues");

                EditorGUILayout.PropertyField(strengthProp);
                object value = null;
                string valueLabel = "None";
                if (value != null) valueLabel = value.ToString();
                EditorGUILayout.LabelField("Value: ", valueLabel);

                EditorGUILayout.PropertyField(effectProp);
                EditorHelper.RenderSequentialIntPopup(prop.FindPropertyRelative("effectDuration"), 0, 10);
                EditorGUILayout.PropertyField(prop.FindPropertyRelative("targetOverride"));

                EditorGUILayout.PropertyField(customValuesProp);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
        }
    }
}