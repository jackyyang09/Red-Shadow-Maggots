using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JSAM.JSAMEditor;
using ClosedXML.Excel;
using System.IO;
using System.Linq;
using IngameDebugConsole;

namespace RSM.Editor.CharacterDesignMatrix
{
    public enum Roles
    { 
        Attacker,
        Protector,
        BuffSupport,
        DebuffSupport,
        Healer
    }

    [System.Serializable]
    public class CharacterBreakdown
    {
        public CharacterObject Character;
        public float BurstOrSustain;
        public float SingleOrAOE;
        public float[] Roles = new float[5];
    }

    [System.Serializable]
    public class EffectParameters
    {
        public BaseGameEffect effect;
        public float attackerMod;
        public float protectorMod;
        public float buffSupportMod;
        public float debuffSupportMod;
        public float healerMod;
    }

    public class CharacterDesignMatrix : ScriptableObject
    {
        [SerializeReference] public List<CharacterBreakdown> breakDowns = new List<CharacterBreakdown>();
        public List<CharacterObject> characters = new List<CharacterObject>();
        public List<EffectParameters> effectParams = new List<EffectParameters>();
        public List<float> strengthModifiers = new List<float>();

        public MatrixMode matrixMode;
    }

    public enum MatrixMode
    {
        Setup,
        Visualizer
    }

    public class CharacterDesignMatrixEditor : BaseSuperWizard<CharacterDesignMatrix, CharacterDesignMatrixEditor>
    {
        SerializedProperty breakDowns;
        SerializedProperty characters;
        SerializedProperty effectParams;
        SerializedProperty matrixMode;

        protected override string WindowName => "Character Design Matrix";

        static string SECONDARY_CHAR_PATH = "Assets/ScriptableObjects/Character Data/Secondary Characters";

        Dictionary<BaseGameEffect, EffectParameters> effectDictionary = new Dictionary<BaseGameEffect, EffectParameters>();

        float scroll;

        protected override void DesignateSerializedProperties()
        {
            breakDowns = FindProp(nameof(breakDowns));
            characters = FindProp(nameof(characters));
            effectParams = FindProp(nameof(effectParams));
            matrixMode = FindProp(nameof(matrixMode));

            base.DesignateSerializedProperties();
        }

        [MenuItem(RSMEditorTools.RSM_TOOLS_MENU + "Character Design Matrix")]
        static void Init()
        {
            if (!Window) Window.DesignateSerializedProperties();
        }

        private void OnGUI()
        {
            SerializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Setup"))
            {
                matrixMode.enumValueIndex = (int)MatrixMode.Setup;
            }
            if (GUILayout.Button("Visualizer"))
            {
                matrixMode.enumValueIndex = (int)MatrixMode.Visualizer;
            }
            if (GUILayout.Button("Lol Test"))
            {
                GenerateSpreadsheet();
            }
            EditorGUILayout.EndHorizontal();

            var s = EditorGUILayout.BeginScrollView(new Vector2(0, scroll));
            scroll = s.y;

            switch ((MatrixMode)matrixMode.enumValueIndex)
            {
                case MatrixMode.Setup:
                    EditorGUILayout.PropertyField(effectParams);

                    EditorGUILayout.PropertyField(characters);

                    EditorGUILayout.PropertyField(breakDowns);
                    if (GUILayout.Button("Load Secondary Characters"))
                    {
                        var c = JSAMEditorHelper.ImportAssetsOrFoldersAtPath<CharacterObject>(SECONDARY_CHAR_PATH);
                        characters.arraySize = c.Count;
                        for (int i = 0; i < characters.arraySize; i++)
                        {
                            characters.GetArrayElementAtIndex(i).objectReferenceValue = c[i];
                        }
                    }

                    if (GUILayout.Button("Calculate Stats"))
                    {
                        GenerateEffectDictionary();

                        breakDowns.ClearArray();
                        for (int i = 0; i < characters.arraySize; i++)
                        {
                            var b = new CharacterBreakdown();
                            var prop = breakDowns.AddAndReturnNewArrayElement();
                            var c = characters.GetArrayElementAtIndex(i).objectReferenceValue as CharacterObject;
                            b.Character = c;
                            for (int j = 0; j < 2; j++)
                            {
                                var skill = c.skills[j];

                                var fraction = 1f / skill.effects.Length;
                                for (int l = 0; l < skill.effects.Length; l++)
                                {
                                    // Reimplement this
                                    //var effect = skill.effects[l];
                                    //if (!effectDictionary.ContainsKey(effect.effect)) continue;
                                    //var p = effectDictionary[effect.effect];
                                    //b.Roles[0] += p.attackerMod;
                                    //b.Roles[1] += p.protectorMod;
                                    //b.Roles[2] += p.buffSupportMod;
                                    //b.Roles[3] += p.debuffSupportMod;
                                    //b.Roles[4] += p.healerMod;
                                    //
                                    //var time = effect.effectDuration;
                                    //float lerp = Mathf.InverseLerp(0, 6, time);
                                    //b.BurstOrSustain += fraction * Mathf.Lerp(-1, 1, lerp);
                                    //
                                    //var target = effect.targetOverride != TargetMode.None ?
                                    //    effect.targetOverride : skill.targetMode;
                                    //
                                    //switch (target)
                                    //{
                                    //    case TargetMode.OneAlly:
                                    //    case TargetMode.OneEnemy:
                                    //    case TargetMode.Self:
                                    //        b.SingleOrAOE -= fraction;
                                    //        break;
                                    //    case TargetMode.AllAllies:
                                    //    case TargetMode.AllEnemies:
                                    //        b.SingleOrAOE += fraction;
                                    //        break;
                                    //}
                                }
                            }
                            prop.managedReferenceValue = b;
                        }
                    }
                    break;
                case MatrixMode.Visualizer:
                    var rect = new Rect(Window.position);
                    rect.position = Vector2.zero;
                    GUI.Box(rect, "");
                    break;
            }
            EditorGUILayout.EndScrollView();

            SerializedObject.ApplyModifiedProperties();
        }

        void GenerateEffectDictionary()
        {
            effectDictionary.Clear();
            foreach (var item in Data.effectParams)
            {
                effectDictionary.Add(item.effect, item);
            }
        }

        void GenerateSpreadsheet()
        {
            // The using to dispose is important!
            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Design Matrix");

            worksheet.Cell("A1").Value = "Secondary Characters";
            worksheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            var range = worksheet.Range("B1:C1").Merge();
            range.Value = "Offense";

            range = worksheet.Range("D1:E1").Merge();
            range.Value = "Defense";

            range = worksheet.Range("F1:G1").Merge();
            range.Value = "Support";

            range = worksheet.Range("A2:A3").Merge();
            range.Value = "Attacker";

            range = worksheet.Range("A4:A5").Merge();
            range.Value = "Protector";

            range = worksheet.Range("A6:A7").Merge();
            range.Value = "Buff-Support";

            range = worksheet.Range("A8:A9").Merge();
            range.Value = "Debuff-Support";

            range = worksheet.Range("A10:A11").Merge();
            range.Value = "Healer";

            var classColumns = new string[] { "B", "C", "D", "E", "F", "G" };
            var target = new int[] { 0, 1 };
            var effectDuration = new int[] { 0, 1 };

            foreach (var item in data.breakDowns)
            {
                var name = item.Character.characterName;

                var column = classColumns[
                    (int)item.Character.characterClass * 2 +
                    effectDuration[System.Convert.ToInt32(item.BurstOrSustain > 0)]];

                var row = 2 + item.Roles.ToList().IndexOf(item.Roles.Max()) * 2
                    + target[System.Convert.ToInt32(item.SingleOrAOE > 0)];

                var cell = column + row;

                switch (item.Character.attackQteType)
                {
                    case QTEType.SimpleBar:
                        worksheet.Cell(cell).Style.Fill.SetBackgroundColor(XLColor.LightGreen);
                        break;
                    case QTEType.Hold:
                        worksheet.Cell(cell).Style.Fill.SetBackgroundColor(XLColor.CandyAppleRed);
                        break;
                }

                worksheet.Cell(cell).Value = item.Character.characterName;

                //item.Character.attackQteType
                //target = item.SingleOrAOE;
            }

            //worksheet.Cell("A2").FormulaA1 = "MID(A1, 7, 5)";
            //worksheet.Cell("A2").Style.Fill.SetBackgroundColor(XLColor.MagicMint);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "DesignMatrix.xlsx");
            workbook.SaveAs(path);
            Application.OpenURL(Directory.GetCurrentDirectory());
        }
    }
}