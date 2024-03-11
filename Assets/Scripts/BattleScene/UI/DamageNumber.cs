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
    [SerializeField] float critChangeDelay = 0.5f;
    [SerializeField] float critChangeTime = 0.5f;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image shield;
    [SerializeField] ViewportBillboard billBoard;
    [SerializeField] TextMeshProUGUI numberLabel;
    [SerializeField] TextMeshProUGUI effectiveNessLabel;

    [SerializeField] GameObject critTextPrefab;
    [SerializeField] Gradient critGradient;

    public void Initialize(Camera cam, Transform t, int damage, int shieldedDamage, bool isCrit)
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

        if (isCrit) StartCoroutine(CritRoutine());

        Destroy(gameObject, numberFadeDelay + numberLifetime);
    }

    private IEnumerator CritRoutine()
    {
        var label = Instantiate(critTextPrefab, transform).GetComponent<TextMeshProUGUI>();

        yield return new WaitForSeconds(critChangeDelay);

        float timer = 0;
        while (timer < critChangeTime)
        {
            timer += Time.deltaTime;
            label.color = critGradient.Evaluate(timer / critChangeTime);
            yield return null;
        }
    }
}