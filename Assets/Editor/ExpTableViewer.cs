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

    [SerializeField] Vector3[] table = new Vector3[101];

    [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + "EXP Table Viewer")]
    static void CreateWindow()
    {
        var w = DisplayWizard<ExpTableViewer>("Character Stat Wizard", "Close", "Fill Table");
    }

    private void OnWizardOtherButton()
    {
        for (int i = 1; i < table.Length; i++)
        {
            table[i].y = m * Mathf.Pow(i, z) + b;
            table[i].x = table[i].y - table[i - 1].y;
        }
    }

    void OnWizardUpdate()
    {
    }

    void OnWizardCreate()
    {
    }
}