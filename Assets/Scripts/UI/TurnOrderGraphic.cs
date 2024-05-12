using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MPUIKIT;

public class TurnOrderGraphic : MonoBehaviour
{
    [SerializeField] Color[] playerColour;
    [SerializeField] Color[] enemyColour;
    Color[][] colours;
    Color[] panelColours;

    [Header("Wait Changed")]
    [SerializeField] Color fastColour;
    [SerializeField] Color slowColour;
    [SerializeField] float waitChangeDelay = 1;
    [SerializeField] float waitChangeTime = 1;
    [SerializeField] Ease waitChangeEase;
    Color fillColour;

    [Header("On My Turn")]
    [SerializeField] float heightOffset;
    [SerializeField] float dialTweenTime;
    [SerializeField] Ease dialEase;

    [Header("Selected Enemy Ease")]
    [SerializeField] float flashTweenTime = 1;
    [SerializeField] Ease flashTweenEase;

    [Header("Object References")]
    [SerializeField] RectTransform parent;
    [SerializeField] Image image;
    [SerializeField] MPImage background;
    [SerializeField] MPImage stroke;
    [SerializeField] TextMeshProUGUI waitLabel;
    [SerializeField] Image waitFill;

    WaitListEntity waitee;
    public WaitListEntity Waitee => waitee;
    float previousWait;

    private void OnEnable()
    {
        colours = new Color[][] { enemyColour, playerColour };
        fillColour = waitFill.color;
    }

    private void OnDisable()
    {
        waitee.OnWaitTimerChanged -= UpdateWaitTimer;
        waitee.OnWaitLimitChanged -= OnWaitLimitChanged;
        waitee.OnStartTurn -= OnStartTurn;
        waitee.OnEndTurn -= OnEndTurn;

        if (!waitee.MovesOnPlayerTurn)
        {
            EnemyCharacter.OnSelectedEnemyCharacterChange -= OnSelectedEnemyCharacterChange;
            waitee.OnStartTurn -= OnStartEnemyTurn;
            waitee.OnEndTurn -= OnEndEnemyTurn;
        }
    }

    public void InitializeWithEntity(WaitListEntity w, bool myTurn = false)
    {
        waitee = w;
        image.sprite = w.Headshot;
        panelColours = colours[w.MovesOnPlayerTurn.ToInt()];
        background.color = panelColours[0];

        ForceUpdateWait();

        waitee.OnWaitTimerChanged += UpdateWaitTimer;
        waitee.OnWaitLimitChanged += OnWaitLimitChanged;
        waitee.OnStartTurn += OnStartTurn;
        waitee.OnEndTurn += OnEndTurn;

        previousWait = waitee.WaitTimer;

        if (!waitee.MovesOnPlayerTurn)
        {
            if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
            {
                OnSelectedEnemyCharacterChange(BattleSystem.Instance.PlayerAttackTarget);
            }
            EnemyCharacter.OnSelectedEnemyCharacterChange += OnSelectedEnemyCharacterChange;
            waitee.OnStartTurn += OnStartEnemyTurn;
            waitee.OnEndTurn += OnEndEnemyTurn;

            if (BattleSystem.Instance.ActiveEnemy == waitee.Character)
            {
                OnSelectedEnemyCharacterChange(BattleSystem.Instance.ActiveEnemy);
            }
        }

        if (myTurn) OnStartTurn();
    }

    private void UpdateWaitTimer()
    {
        if (waitee.IsOverWait)
        {
            ForceUpdateWait();
            //ForceUpdateWait(0);
        }
        else
        {
            DOTween.To(x => previousWait = x,
            previousWait,
            waitee.WaitPercentage, dialTweenTime).SetEase(dialEase).OnUpdate(() =>
            {
                waitLabel.text = previousWait.FormatToDecimal() + "%";
                waitFill.fillAmount = previousWait / waitee.WaitLimit;
            });
        }
    }

    void ForceUpdateWait()
    {
        previousWait = waitee.Wait;
        waitLabel.text = waitee.WaitPercentage.FormatToDecimal() + "%";
        waitFill.fillAmount = waitee.WaitPercentage;
    }

    void OnStartTurn()
    {
        parent.anchoredPosition = new Vector2(0, heightOffset);
        background.color = panelColours[1];
    }

    void OnStartEnemyTurn()
    {
        OnSelectedEnemyCharacterChange(null);
    }

    void OnEndTurn()
    {
        parent.anchoredPosition = Vector2.zero;
        background.color = panelColours[0];
    }

    void OnEndEnemyTurn()
    {
        OnSelectedEnemyCharacterChange(waitee.Character as EnemyCharacter);
    }
    
    void OnWaitLimitChanged()
    {
        if (waitee.WaitLimit >= waitee.WaitLimit)
        {
            waitFill.color = fastColour;
        }
        else
        {
            waitFill.color = slowColour;
        }
        waitFill.DOColor(fillColour, waitChangeTime).SetEase(waitChangeEase).SetDelay(waitChangeDelay);

        DOTween.To(x => previousWait = x,
            previousWait,
            waitee.WaitPercentage, dialTweenTime).SetEase(dialEase).OnUpdate(() =>
            {
                waitLabel.text = previousWait.FormatToDecimal() + "%";
                waitFill.fillAmount = previousWait / waitee.WaitLimit;
            });
    }

    void OnSelectedEnemyCharacterChange(EnemyCharacter e)
    {
        stroke.enabled = e == waitee.Character;
        if (!stroke.enabled)
        {
            DOTween.Kill(GetInstanceID());
            background.OutlineWidth = 0;
        }
        else
        {
            stroke.color = Color.white;
            DOTween.To(() => stroke.color, x => stroke.color = x, Color.gray, flashTweenTime)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(GetInstanceID());
        }
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(TestTween))]
    void TestTween()
    {
        DOTween.Kill(GetInstanceID());
        OnSelectedEnemyCharacterChange(waitee.Character as EnemyCharacter);
    }
#endif
}