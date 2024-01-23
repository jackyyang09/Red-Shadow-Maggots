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

    public BaseCharacter Character => character;
    BaseCharacter character;
    float previousWait;

    private void OnEnable()
    {
        colours = new Color[][] { enemyColour, playerColour };
        fillColour = waitFill.color;
    }

    private void OnDisable()
    {
        character.OnWaitTimeChanged -= UpdateWait;
        character.OnWaitLimitChanged -= OnWaitLimitChanged;
        character.OnStartTurn -= OnStartTurn;
        character.OnEndTurn -= OnEndTurn;

        if (character.IsEnemy(out EnemyCharacter e))
        {
            EnemyCharacter.OnSelectedEnemyCharacterChange -= OnSelectedEnemyCharacterChange;
            e.OnStartTurn -= OnStartEnemyTurn;
            e.OnEndTurn -= OnEndEnemyTurn;
        }
    }

    public void InitializeWithCharacter(BaseCharacter c, bool myTurn = false)
    {
        character = c;
        image.sprite = c.Reference.headshotSprite;
        panelColours = colours[c.IsPlayer().ToInt()];
        background.color = panelColours[0];

        ForceUpdateWait();

        character.OnWaitTimeChanged += UpdateWait;
        character.OnWaitLimitChanged += OnWaitLimitChanged;
        character.OnStartTurn += OnStartTurn;
        character.OnEndTurn += OnEndTurn;

        previousWait = character.WaitTimer;

        if (character.IsEnemy(out EnemyCharacter e))
        {
            if (BattleSystem.Instance.CurrentPhase == BattlePhases.PlayerTurn)
            {
                OnSelectedEnemyCharacterChange(BattleSystem.Instance.PlayerAttackTarget);
            }
            EnemyCharacter.OnSelectedEnemyCharacterChange += OnSelectedEnemyCharacterChange;
            e.OnStartTurn += OnStartEnemyTurn;
            e.OnEndTurn += OnEndEnemyTurn;
        }

        if (myTurn) OnStartTurn();
    }

    private void UpdateWait()
    {
        if (character.IsOverWait)
        {
            ForceUpdateWait();
            //ForceUpdateWait(0);
        }
        else
        {
            DOTween.To(x => previousWait = x,
            previousWait,
            character.WaitPercentage, dialTweenTime).SetEase(dialEase).OnUpdate(() =>
            {
                waitLabel.text = previousWait.FormatToDecimal() + "%";
                waitFill.fillAmount = previousWait / character.WaitLimitModified;
            });
        }
    }

    void ForceUpdateWait()
    {
        previousWait = character.Wait;
        waitLabel.text = character.WaitPercentage.FormatToDecimal() + "%";
        waitFill.fillAmount = character.WaitPercentage;
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
        OnSelectedEnemyCharacterChange(character as EnemyCharacter);
    }
    
    void OnWaitLimitChanged()
    {
        if (Character.WaitLimitModified >= Character.WaitLimit)
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
            character.WaitPercentage, dialTweenTime).SetEase(dialEase).OnUpdate(() =>
            {
                waitLabel.text = previousWait.FormatToDecimal() + "%";
                waitFill.fillAmount = previousWait / character.WaitLimitModified;
            });
    }

    void OnSelectedEnemyCharacterChange(EnemyCharacter e)
    {
        stroke.enabled = e == character;
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
        OnSelectedEnemyCharacterChange(character as EnemyCharacter);
    }
#endif
}