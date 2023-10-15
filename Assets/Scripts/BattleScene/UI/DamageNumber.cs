using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] float verticalMovement = 0.5f;

    [SerializeField] float numberLifetime = 1.5f;
    [SerializeField] float numberFadeTime = 0.5f;
    [SerializeField] float numberFadeDelay = 1;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image shield;
    [SerializeField] ViewportBillboard billBoard;
    [SerializeField] TextMeshProUGUI numberLabel;
    [SerializeField] TextMeshProUGUI effectiveNessLabel;

    public void Initialize(Camera cam, Transform t, int damage, int shieldedDamage)
    {
        billBoard.EnableWithSettings(cam, t);

        bool shielded = shieldedDamage > 0;

        if (shielded)
        {
            numberLabel.text = "-" + shieldedDamage.ToString();
            shield.enabled = true;
        }
        else
        {
            numberLabel.text = "-" + damage.ToString();
        }

        DOTween.To(() => billBoard.offset.y, x => billBoard.offset.y = x, billBoard.offset.y + verticalMovement, numberLifetime).SetEase(Ease.OutCubic);

        canvasGroup.DOFade(0, numberFadeTime).SetDelay(numberFadeDelay);

        Destroy(gameObject, numberFadeDelay + numberLifetime);
    }
}
