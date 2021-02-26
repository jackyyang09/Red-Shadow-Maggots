using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EffectTextSpawner : MonoBehaviour
{
    [SerializeField] Camera cam = null;

    [SerializeField] float numberLifetime = 3;

    [SerializeField] GameObject healthTextPrefab = null;
    [SerializeField] GameObject critTextPrefab = null;
    [SerializeField] GameObject effectTextPrefab = null;

    [SerializeField] TMPro.TMP_FontAsset buffTextColour = null;
    [SerializeField] TMPro.TMP_FontAsset debuffTextColour = null;

    public static EffectTextSpawner instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    public void SpawnHealNumberAt(float healAmount, Transform trans)
    {
        var text = Instantiate(healthTextPrefab, transform.GetChild(0)).GetComponentInChildren<TMPro.TextMeshProUGUI>();
        var billboard = text.GetComponent<ViewportBillboard>();
        text.text = "+" + healAmount.ToString();
        billboard.EnableWithSettings(cam, trans);
        DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + 0.5f, numberLifetime)/*.SetEase(Ease.OutCubic)*/;

        text.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f);

        Destroy(text.gameObject, numberLifetime);
    }

    public void SpawnCriticalHitAt(Transform trans)
    {
        var billboard = Instantiate(critTextPrefab, transform.GetChild(0)).GetComponent<ViewportBillboard>();
        billboard.EnableWithSettings(cam, trans);
        DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + 0.5f, numberLifetime).SetEase(Ease.OutCubic);

        Destroy(billboard.gameObject, numberLifetime);
    }

    public void SpawnEffectAt(BaseGameEffect effect, Transform trans)
    {
        var billboard = Instantiate(effectTextPrefab, transform.GetChild(0)).GetComponent<ViewportBillboard>();
        billboard.EnableWithSettings(cam, trans);

        var billboardImage = billboard.GetComponentInChildren<UnityEngine.UI.Image>();
        billboardImage.sprite = effect.effectIcon;

        var billboardText = billboard.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        billboardText.text = effect.effectText;

        billboard.GetComponentInChildren<UIElementCenterer>().Recenter();

        switch (effect.effectType)
        {
            case EffectType.Buff:
                billboardText.font = buffTextColour;
                DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + 0.5f, numberLifetime - 0.5f).SetEase(Ease.OutCubic);
                break;
            case EffectType.Debuff:
                billboardText.font = debuffTextColour;
                billboard.offset.y += 0.5f;
                DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y - 0.5f, numberLifetime - 0.5f).SetEase(Ease.OutCubic);
                break;
        }

        billboardText.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f);
        billboardImage.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f); 

        Destroy(billboard.gameObject, numberLifetime);
    }
}
