using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(SkillObject))]
public class SkillObjectEditor : Editor
{
    SkillObject targetObject;

    SerializedProperty skillName;
    SerializedProperty skillDescription;
    SerializedProperty sprite;
    SerializedProperty targetMode;
    SerializedProperty skillCooldown;
    SerializedProperty gameEffects;

    const string SHOW_GAMEEFFECTS = "SKILLOBJECT_GAMEEFFECTS";
    static bool gameEffectsFoldout
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

    Texture cachedPreview;
    UnityEngine.Object lastSprite;
    Color buffColour = new Color(0.6f, 0.8f, 1f);
    Color debuffColour = new Color(1, 0.25f, 0.25f);

    private void OnEnable()
    {
        targetObject = target as SkillObject;

        skillName = serializedObject.FindProperty(nameof(skillName));
        skillDescription = serializedObject.FindProperty(nameof(skillDescription));
        sprite = serializedObject.FindProperty(nameof(sprite));
        targetMode = serializedObject.FindProperty(nameof(targetMode));
        skillCooldown = serializedObject.FindProperty(nameof(skillCooldown));
        gameEffects = serializedObject.FindProperty(nameof(gameEffects));

        CacheSpriteAssetPreview();
    }

    void CacheSpriteAssetPreview()
    {
        cachedPreview = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GetAssetPath(sprite.objectReferenceValue));
        lastSprite = sprite.objectReferenceValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

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

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(skillName);
        if (EditorHelper.CondensedButton("Use Object Name"))
        {
            skillName.stringValue = target.name;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(targetMode);
        EditorGUILayout.PropertyField(skillCooldown);

        var skillDescriptions = targetObject.GetSkillDescriptions();

        for (int i = 0; i < gameEffects.arraySize; i++)
        {
            var nullStyle = new GUIStyle(EditorStyles.label).ApplyTextAnchor(TextAnchor.MiddleCenter);
            var buffStyle = new GUIStyle(EditorStyles.label).SetTextColor(buffColour).ApplyTextAnchor(TextAnchor.MiddleCenter);
            var debuffStyle = new GUIStyle(EditorStyles.label).SetTextColor(debuffColour).ApplyTextAnchor(TextAnchor.MiddleCenter);

            var gameEffect = targetObject.gameEffects[i];

            if (gameEffect.effect == null)
            {
                EditorGUILayout.LabelField("None", nullStyle);
                continue;
            }

            switch (gameEffect.effect.effectType)
            {
                case EffectType.None:
                    EditorGUILayout.LabelField(skillDescriptions[i], nullStyle);
                    break;
                case EffectType.Heal:
                case EffectType.Buff:
                    EditorGUILayout.LabelField(skillDescriptions[i], buffStyle);
                    break;
                case EffectType.Debuff:
                case EffectType.Damage:
                    EditorGUILayout.LabelField(skillDescriptions[i], debuffStyle);
                    break;
            }
        }

        gameEffectsFoldout = EditorGUILayout.Foldout(gameEffectsFoldout, gameEffects.GUIContent());
        //gameEffectsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(gameEffectsFoldout, gameEffects.GUIContent());
        if (gameEffectsFoldout && Selection.objects.Length == 1)
        {
            var cachedSize = gameEffects.arraySize;
            gameEffects.arraySize = EditorGUILayout.IntField("Size", gameEffects.arraySize);
            for (int i = 0; i < Mathf.Min(cachedSize, gameEffects.arraySize); i++)
            {
                EditorGUI.indentLevel++;
                var prop = gameEffects.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                var gameEffect = targetObject.gameEffects[i];
                var effectProp = prop.FindPropertyRelative("effect");
                var strengthProp = prop.FindPropertyRelative("strength");
                var customValuesProp = prop.FindPropertyRelative("customValues");

                EditorGUILayout.PropertyField(strengthProp);
                object value = null;
                if (gameEffect.effect != null) value = gameEffect.effect.GetEffectStrength(gameEffect.strength, gameEffect.customValues);
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
        //EditorGUILayout.EndFoldoutHeaderGroup();

        if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
    }
}