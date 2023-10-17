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

    protected virtual void OnEnable()
    {
        targetObject = target as T;

        InitializeSerializedProperties();

        CacheSpriteAssetPreview();
    }

    protected SerializedProperty sprite;

    public abstract void InitializeSerializedProperties();

    protected void CacheSpriteAssetPreview()
    {
        cachedPreview = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GetAssetPath(sprite.objectReferenceValue));
        lastSprite = sprite.objectReferenceValue;
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

    protected void RenderEffectDescriptions(EffectProperties[] gameEffects, string[] skillDescriptions)
    {
        for (int i = 0; i < gameEffects.Length; i++)
        {
            var nullStyle = new GUIStyle(EditorStyles.label)
                .ApplyTextAnchor(TextAnchor.MiddleCenter)
                .ApplyWordWrap()
                .ApplyRichText();

            var buffStyle = new GUIStyle(EditorStyles.label)
                .SetTextColor(buffColour)
                .ApplyTextAnchor(TextAnchor.MiddleCenter)
                .ApplyWordWrap()
                .ApplyRichText();

            var debuffStyle = new GUIStyle(EditorStyles.label)
                .SetTextColor(debuffColour)
                .ApplyTextAnchor(TextAnchor.MiddleCenter)
                .ApplyWordWrap()
                .ApplyRichText();

            var gameEffect = gameEffects[i];

            if (gameEffect.effect == null)
            {
                EditorGUILayout.LabelField("None", nullStyle);
                continue;
            }

            var desc = skillDescriptions[i];
            if (desc.Contains("<u>"))
            {
                desc = desc.Replace("u>", "b>");
            }

            switch (gameEffect.effect.effectType)
            {
                case EffectType.None:
                    EditorGUILayout.LabelField(desc, nullStyle);
                    break;
                case EffectType.Heal:
                case EffectType.Buff:
                    EditorGUILayout.LabelField(desc, buffStyle);
                    break;
                case EffectType.Debuff:
                case EffectType.Damage:
                    EditorGUILayout.LabelField(desc, debuffStyle);
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
                if (ge.effect != null) value = ge.effect.GetEffectStrength(ge.strength, ge.customValues);
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