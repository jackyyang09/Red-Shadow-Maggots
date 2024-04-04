using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;

public class SkillFixWizard : ScriptableWizard
{
    const string WINDOW_NAME = "Skill Fix Wizard";

    [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + WINDOW_NAME)]
    public static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<SkillFixWizard>
            (WINDOW_NAME, "Close", "Fix Skills");
    }

    //protected override bool DrawWizardGUI()
    //{
    //
    //}

    //void OnWizardUpdate()
    //{
    //
    //}

    void OnWizardCreate()
    {

    }

    private void OnWizardOtherButton()
    {
        var guids = AssetDatabase.FindAssets("t:BaseAbilityObject");

        SerializedProperty effects, gameEffects, damageEffects;

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var skill = AssetDatabase.LoadAssetAtPath<BaseAbilityObject>(path);

            var so = new SerializedObject(skill);

            so.Update();

            gameEffects = so.FindProperty(nameof(gameEffects));
            damageEffects = so.FindProperty(nameof(damageEffects));
            effects = so.FindProperty(nameof(effects));

            effects.arraySize = Mathf.Max(gameEffects.arraySize, damageEffects.arraySize);

            Selection.activeObject = skill;

            if (skill.gameEffects != null)
            {
                for (int i = 0; i < skill.gameEffects.Length; i++)
                {
                    var e = new EffectGroup();
                    e.effectProps = skill.gameEffects[i];
                    e.targetOverride = skill.gameEffects[i].targetOverride;
                    e.appStyle = new BaseApplicationStyle();
                    effects.GetArrayElementAtIndex(i).managedReferenceValue = e;
                }
            }
            
            if (skill.damageEffects != null)
            {
                for (int i = 0; i < skill.damageEffects.Length; i++)
                {
                    var e = new EffectGroup();
                    e.damageProps = skill.damageEffects[i];
                    e.targetOverride = skill.damageEffects[i].targetOverride;
                    e.appStyle = new BaseApplicationStyle();
                    effects.GetArrayElementAtIndex(i).managedReferenceValue = e;
                }
            }

            so.ApplyModifiedProperties();
        }

        Debug.Log("Fixed " + guids.Length + " skills");
    }
}