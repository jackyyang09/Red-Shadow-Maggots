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

    [Header("On My Turn")]
    [SerializeField] float heightOffset;
    [SerializeField] float dialTweenTime;
    [SerializeField] Ease dialEase;

    [Header("Selected Enemy Ease")]
    [SerializeField] float outlineWidth = 1;
    [SerializeField] float flashTweenTime = 1;
    [SerializeField] Ease flashTweenEase;

    [Header("Object References")]
    [SerializeField] RectTransform parent;
    [SerializeField] Image image;
    [SerializeField] MPImage background;
    [SerializeField] TextMeshProUGUI waitLabel;
    [SerializeField] Image waitFill;

    public BaseCharacter Character => character;
    BaseCharacter character;
    float previousWait;

    private void OnEnable()
    {
        colours = new Color[][] { enemyColour, playerColour };
    }

    private void OnDisable()
    {
        character.OnWaitChanged -= UpdateWait;
        character.OnStartTurn -= OnStartTurn;
        character.OnEndTurn -= OnEndTurn;

        if (character.IsEnemy(out EnemyCharacter e))
        {
            EnemyCharacter.OnSelectedEnemyCharacterChange -= OnSelectedEnemyCharacterChange;
            e.OnStartTurn -= OnStartEnemyTurn;
            e.OnEndTurn -= OnEndEnemyTurn;
        }
    }

    public void InitializeWithCharacter(BaseCharacter c)
    {
        character = c;
        image.sprite = c.Reference.headshotSprite;
        panelColours = colours[c.IsPlayer().ToInt()];
        background.color = panelColours[0];

        ForceUpdateWait(character.Wait);

        character.OnWaitChanged += UpdateWait;
        character.OnStartTurn += OnStartTurn;
        character.OnEndTurn += OnEndTurn;

        previousWait = character.Wait;

        if (character.IsEnemy(out EnemyCharacter e))
        {
            OnSelectedEnemyCharacterChange(e);
            EnemyCharacter.OnSelectedEnemyCharacterChange += OnSelectedEnemyCharacterChange;
            e.OnStartTurn += OnStartEnemyTurn;
            e.OnEndTurn += OnEndEnemyTurn;
        }
    }

    private void UpdateWait()
    {
        if (character.Wait >= 1)
        {
            ForceUpdateWait(character.Wait);
            //ForceUpdateWait(0);
        }
        else
        {
            DOTween.To(x => previousWait = x,
            previousWait,
            character.Wait, dialTweenTime).SetEase(dialEase).OnUpdate(() =>
            {
                waitLabel.text = previousWait.FormatPercentage().ToString();
                waitFill.fillAmount = previousWait;
            });
        }
    }

    void ForceUpdateWait(float wait)
    {
        previousWait = wait;
        waitLabel.text = previousWait.FormatPercentage().ToString();
        waitFill.fillAmount = previousWait;
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

    void OnSelectedEnemyCharacterChange(EnemyCharacter e)
    {
        if (e != character)
        {
            DOTween.Kill(GetInstanceID());
            background.OutlineWidth = 0;
        }
        else
        {
            background.OutlineWidth = outlineWidth;
            background.OutlineColor = Color.white;
            DOTween.To(() => background.OutlineColor, x => background.OutlineColor = x, Color.gray, flashTweenTime)
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