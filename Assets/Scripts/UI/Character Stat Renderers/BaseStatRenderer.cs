using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStatRenderer : MonoBehaviour
{
    [SerializeField] protected StatRenderController controller;

    protected string NameText
    {
        get => controller.NameLabel.text;
        set => controller.NameLabel.text = value;
    }

    protected string ValueText
    {
        get => controller.ValueLabel.text;
        set => controller.ValueLabel.text = value;
    }

    protected string BeginColourLabel(Color c) => " <color=#" + ColorUtility.ToHtmlStringRGBA(c) + ">";

    protected string POS_COLOUR => BeginColourLabel(controller.PositiveColor) + "(+";
    protected string NEG_COLOUR => BeginColourLabel(controller.NegativeColor) + "(";
    protected static string END_BRACKET = ")</color>";
    protected static string END_COLOUR = "</color>";

    protected string RenderPositiveMod(int v) => POS_COLOUR + v + END_BRACKET;
    protected string RenderPositiveMod(string s) => POS_COLOUR + s + END_BRACKET;
    protected string RenderPositiveModWithMinus(string s) => BeginColourLabel(controller.PositiveColor) + "(" + s + END_BRACKET;

    protected string RenderNegativeMod(int v) => NEG_COLOUR + v + END_BRACKET;
    protected string RenderNegativeMod(string s) => NEG_COLOUR + s + END_BRACKET;
    protected string RenderNegativeModWithPlus(string s) => BeginColourLabel(controller.NegativeColor) + "(+" + s + END_BRACKET;

    protected void RefreshValueLabelLayout()
    {
        controller.RefreshValueLabelLayout();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (controller == null)
        {
            UnityEditor.Undo.RecordObject(this, "Found " + nameof(StatRenderController));
            controller = GetComponent<StatRenderController>();

            if (controller)
            {
                var so = new UnityEditor.SerializedObject(controller);
                var renderer = so.FindProperty("renderer");
                renderer.objectReferenceValue = this;
                so.ApplyModifiedProperties();
            }
        }
    }
#endif

    public virtual void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
    }

    public virtual void RenderInBattle(BaseCharacter character)
    {
    }
}