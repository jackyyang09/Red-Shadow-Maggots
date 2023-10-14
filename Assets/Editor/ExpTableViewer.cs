using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExpTableViewer : ScriptableWizard
{
    [Header("y = mx^z + b")]
    [SerializeField] float m = 1;
    [SerializeField] float z = 1;
    [SerializeField] float b = 1;

    [SerializeField] Vector2[] table = new Vector2[101];

    [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + "EXP Table Viewer")]
    static void CreateWindow()
    {
        var w = DisplayWizard<ExpTableViewer>("Character Stat Wizard", "Close", "Fill Table");

        w.m = CharacterObject.m;
        w.z = CharacterObject.z;
        w.b = CharacterObject.b;

        w.FillTables();
    }

    private void OnWizardOtherButton()
    {
        FillTables();
    }

    public void FillTables()
    {
        for (int i = 1; i < table.Length; i++)
        {
            table[i].y = Mathf.Ceil(m * Mathf.Pow(i, z) + b);
            table[i].x = table[i].y - table[i - 1].y;
        }
    }

    void OnWizardUpdate()
    {
    }

    void OnWizardCreate()
    {
    }

    protected override bool DrawWizardGUI()
    {
        EditorGUILayout.HelpBox(new GUIContent(
            "x - The amount of xp required to go from the previous level to this level\n" +
            "y - The total amount of xp required to be at this level."
            ));

        return base.DrawWizardGUI();
    }
}