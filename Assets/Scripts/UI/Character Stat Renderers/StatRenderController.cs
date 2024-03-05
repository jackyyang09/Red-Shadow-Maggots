using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RSMConstants;

public class StatRenderController : MonoBehaviour
{
    [SerializeField] protected Color positiveModifier;
    public Color PositiveColor => positiveModifier;
    [SerializeField] protected Color negativeModifier;
    public Color NegativeColor => negativeModifier;
    [SerializeField] protected TextMeshProUGUI nameLabel;
    public TextMeshProUGUI NameLabel => nameLabel;
    [SerializeField] protected TextMeshProUGUI valueLabel;
    public TextMeshProUGUI ValueLabel => valueLabel;
    [SerializeField] new BaseStatRenderer renderer;

    public virtual void UpdateStat(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        renderer.RenderState(state, character, isEnemy);
    }

    public virtual void UpdateStat(BaseCharacter character)
    {
        renderer.RenderInBattle(character);
    }

    public void RefreshValueLabelLayout()
    {
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(valueLabel.transform.parent.GetRectTransform());
    }

    /// <summary>
    /// Changing the description label takes too long and ruins layout. 
    /// A rebuild is necessary at this stage
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedRebuild()
    {
        yield return null;

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }

    [ContextMenu(nameof(Test))]
    void Test()
    {
        UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(valueLabel.transform.parent.GetRectTransform());
    }

    [ContextMenu(nameof(Test2))]
    void Test2()
    {
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(valueLabel.transform.parent.GetRectTransform());
    }
}