using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JackysEditorHelpers;
using Object = UnityEngine.Object;

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
        var guids = AssetDatabase.FindAssets("t:StatChangeEffect");

        List<Object> failed = new List<Object>();
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var skill = AssetDatabase.LoadAssetAtPath<StatChangeEffect>(path);

            var so = new SerializedObject(skill);

            so.Update();

            if (!MultiStatFixer(skill, so))
            {
                failed.Add(skill);
            }

            so.ApplyModifiedProperties();
        }

        Debug.Log("Fixed " + guids.Length + " skills");
        if (failed.Count > 0) 
        {
            string m = "The following skills failed to get fixed: ";
            foreach (var a in failed)
            {
                m += "\n" + a.name;
            }
            Debug.LogWarning(m);
        }
    }

    bool MultiStatFixer(StatChangeEffect skill, SerializedObject so)
    {
        SerializedProperty 
            stats,
            stat
            ;

        stats = so.FindProperty(nameof(stats));
        stat = so.FindProperty(nameof(stat));

        if (stats.arraySize == 0) return false;
        stat.objectReferenceValue = stats.GetArrayElementAtIndex(0).objectReferenceValue;
        return true;
    }

    //void EffectsFixer(BaseAbilityObject skill, SerializedObject so)
    //{
    //    SerializedProperty effects, gameEffects, damageEffects;
    //
    //    gameEffects = so.FindProperty(nameof(gameEffects));
    //    damageEffects = so.FindProperty(nameof(damageEffects));
    //    effects = so.FindProperty(nameof(effects));
    //
    //    effects.arraySize = Mathf.Max(gameEffects.arraySize, damageEffects.arraySize);
    //
    //    Selection.activeObject = skill;
    //
    //    if (skill.gameEffects != null)
    //    {
    //        for (int i = 0; i < skill.gameEffects.Length; i++)
    //        {
    //            var e = new EffectGroup();
    //            e.effectProps = skill.gameEffects[i];
    //            e.targetOverride = skill.gameEffects[i].targetOverride;
    //            e.appStyle = new BaseApplicationStyle();
    //            effects.GetArrayElementAtIndex(i).managedReferenceValue = e;
    //        }
    //    }
    //
    //    if (skill.damageEffects != null)
    //    {
    //        for (int i = 0; i < skill.damageEffects.Length; i++)
    //        {
    //            var e = new EffectGroup();
    //            e.damageProps = skill.damageEffects[i];
    //            e.targetOverride = skill.damageEffects[i].targetOverride;
    //            e.appStyle = new BaseApplicationStyle();
    //            effects.GetArrayElementAtIndex(i).managedReferenceValue = e;
    //        }
    //    }
    //}
}