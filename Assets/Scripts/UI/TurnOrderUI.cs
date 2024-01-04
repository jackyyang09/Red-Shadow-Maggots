using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Facade;

public class TurnOrderUI : BaseGameUI
{
    [Header("Tween Settings")]
    [SerializeField] float tweenTime = 0.5f;
    [SerializeField] Ease easeType;

    [SerializeField] Transform layoutRoot;
    [SerializeField] ContentSizeFitter fitter;
    [SerializeField] HorizontalLayoutGroup layoutGroup;

    [SerializeField] GameObject profilePrefab;
    List<TurnOrderGraphic> graphics = new List<TurnOrderGraphic>();
    [SerializeField] RoundChangeGraphic roundChangeGraphic;
    [SerializeField] Transform feather;
    [SerializeField] Transform weight;

    Dictionary<BaseCharacter, TurnOrderGraphic> characterToGraphic = new Dictionary<BaseCharacter, TurnOrderGraphic>();

    public override void ShowUI()
    {
        optimizedCanvas.Show();
    }

    public override void HideUI()
    {
        optimizedCanvas.Hide();
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitUntil(() => BattleSystem.Initialized);

        var characters = battleSystem.AllCharacters;
        for (int i = 0; i < characters.Count; i++)
        {
            var g = Instantiate(profilePrefab, layoutRoot).GetComponent<TurnOrderGraphic>();
            g.InitializeWithCharacter(characters[i]);
            graphics.Add(g);
            characterToGraphic.Add(characters[i], g);
        }

        UpdateGraphicsHierarchy(GetReorderedGraphics());

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(explainerParent);

        BattleSystem.OnMoveOrderUpdated += OnMoveOrderUpdated;
    }

    private void OnEnable()
    {
        UIManager.OnEnterSkillTargetMode += HideUI;
        UIManager.OnExitSkillTargetMode += ShowUI;
        UIManager.OnShowBattleUI += ShowUI;
        //UIManager.OnHideBattleUI += HideUI;

        BaseCharacter.OnCharacterDeath += OnCharacterDeath;
    }

    private void OnDisable()
    {
        UIManager.OnEnterSkillTargetMode -= HideUI;
        UIManager.OnExitSkillTargetMode -= ShowUI;
        UIManager.OnShowBattleUI -= ShowUI;
        //UIManager.OnHideBattleUI -= HideUI;

        BaseCharacter.OnCharacterDeath -= OnCharacterDeath;
    }

    private void OnDestroy()
    {
        BattleSystem.OnMoveOrderUpdated -= OnMoveOrderUpdated;
    }

    void OnCharacterDeath(BaseCharacter b)
    {
        characterToGraphic[b].gameObject.SetActive(false);
        graphics.Remove(characterToGraphic[b]);
    }

    void UpdateGraphicsHierarchy(List<TurnOrderGraphic> newOrder)
    {
        graphics = new List<TurnOrderGraphic>(newOrder);

        for (int i = 0; i < graphics.Count; i++)
        {
            if (!graphics[i]) continue;
            graphics[i].transform.SetSiblingIndex(i);
        }

        if (graphics.Remove(null))
        {
            roundChangeGraphic.transform.SetSiblingIndex(newOrder.IndexOf(null));
        }
        feather.SetAsFirstSibling();
        weight.SetAsLastSibling();
    }

    List<TurnOrderGraphic> GetReorderedGraphics()
    {
        return battleSystem.MoveOrder.Select(c => graphics.First(g => g.Character == c)).ToList();
    }

    void OnMoveOrderUpdated()
    {
        StartCoroutine(AnimateShift());
    }

    // I'm not proud of this code, but it works
    IEnumerator AnimateShift()
    {
        fitter.enabled = false;
        layoutGroup.enabled = false;

        var finalOrder = GetReorderedGraphics();

        var graphicsTransforms = graphics.Select(s => s.GetRectTransform()).ToList();

        bool anyoneOverwait = false;
        for (int i = 0; i < finalOrder.Count; i++)
        {
            if (finalOrder[i].Character.IsOverWait)
            {
                bool previouslyVisible = true;
                anyoneOverwait = true;
                if (!roundChangeGraphic.gameObject.activeSelf)
                {
                    roundChangeGraphic.transform.SetSiblingIndex(1);
                    roundChangeGraphic.ShowForRound(gameManager.RoundCount + 1);

                    fitter.enabled = true;
                    layoutGroup.enabled = true;

                    yield return null;

                    previouslyVisible = false;
                }

                float dest = 0;

                if (previouslyVisible)
                {
                    dest = graphicsTransforms[i].anchoredPosition.x;
                    graphicsTransforms.Insert(i + 1, roundChangeGraphic.GetRectTransform());
                }
                else
                {
                    dest = graphicsTransforms[i - 1].anchoredPosition.x;
                    graphicsTransforms.Insert(0, roundChangeGraphic.GetRectTransform());
                }
                roundChangeGraphic.GetRectTransform().DOAnchorPos3DX(dest, tweenTime).SetEase(easeType);
                //Debug.Log("RCG moving to " + graphics[i].Character.Reference.characterName + " at " + dest);


                fitter.enabled = false;
                layoutGroup.enabled = false;

                finalOrder.Insert(i, null);

                break;
            }
        }

        if (!anyoneOverwait)
        {
            if (roundChangeGraphic.gameObject.activeSelf)
            {
                roundChangeGraphic.Hide();

                fitter.enabled = true;
                layoutGroup.enabled = true;

                yield return null;

                fitter.enabled = false;
                layoutGroup.enabled = false;
            }
        }

        for (int i = 0; i < graphics.Count; i++)
        {
            var index = finalOrder.IndexOf(graphics[i]);
            if (index == -1) continue;
            var dest = graphicsTransforms[index].anchoredPosition.x;
            graphics[i].GetRectTransform().DOAnchorPosX(dest, tweenTime).SetEase(easeType);
            //Debug.Log(graphics[i].Character.Reference.characterName + " moving to " + graphicsTransforms[index] + " at " + dest);
        }

        yield return new WaitForSeconds(tweenTime);

        UpdateGraphicsHierarchy(finalOrder);

        fitter.enabled = true;
        layoutGroup.enabled = true;
    }
}