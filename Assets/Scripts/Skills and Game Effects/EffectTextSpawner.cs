using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EffectTextSpawner : BasicSingleton<EffectTextSpawner>
{
    [SerializeField] float numberLifetime = 3;
    [SerializeField] float textVerticalMovement = 50;
    [SerializeField] float textFadeTime;
    [SerializeField] float textFadeDelay;

    [SerializeField] Camera cam;

    [SerializeField] GameObject healthTextPrefab;
    [SerializeField] GameObject critTextPrefab;
    [SerializeField] GameObject effectTextPrefab;

    [SerializeField] TMPro.TMP_FontAsset buffTextColour;
    [SerializeField] TMPro.TMP_FontAsset debuffTextColour;

    [SerializeField] GameObject missPrefab;
    
    public void SpawnHealNumberAt(float healAmount, Transform trans)
    {
        var text = Instantiate(healthTextPrefab, transform.GetChild(0)).GetComponentInChildren<TMPro.TextMeshProUGUI>();
        var billboard = text.GetComponent<ViewportBillboard>();
        text.text = "+" + ((int)healAmount).ToString();
        billboard.EnableWithSettings(cam, trans);
        DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + textVerticalMovement, numberLifetime)/*.SetEase(Ease.OutCubic)*/;

        text.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f);

        Destroy(text.gameObject, numberLifetime);
    }

    public GameObject SpawnCriticalHitAt(Transform trans)
    {
        return Instantiate(critTextPrefab, trans);
    }

    [ContextMenu(nameof(Test))]
    public void Test()
    {
        SpawnMissAt(transform);
    }

    public void SpawnMissAt(Transform t)
    {
        var billboard = Instantiate(missPrefab, transform.GetChild(0)).GetComponent<ViewportBillboard>();
        billboard.EnableWithSettings(cam, t);
        Debug.Log(t.name);

        DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + textVerticalMovement, numberLifetime)/*.SetEase(Ease.OutCubic)*/;
        
        var billboardText = billboard.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        billboardText.DOFade(0, textFadeTime).SetDelay(textFadeDelay);
    }

    public void SpawnEffectAt(BaseGameEffect effect, Transform trans)
    {
        var billboard = Instantiate(effectTextPrefab, transform.GetChild(0)).GetComponent<ViewportBillboard>();
        billboard.EnableWithSettings(cam, trans);

        var billboardImage = billboard.GetComponentInChildren<UnityEngine.UI.Image>();
        if (effect.effectIcon) billboardImage.sprite = effect.effectIcon;
        else billboardImage.color = Color.clear;

        var billboardText = billboard.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        billboardText.text = effect.effectText;

        billboard.GetComponentInChildren<UIElementCenterer>().Recenter();

        switch (effect.effectType)
        {
            case EffectType.Buff:
                billboardText.font = buffTextColour;
                DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + textVerticalMovement, numberLifetime - 0.5f).SetEase(Ease.OutCubic);
                break;
            case EffectType.Debuff:
                billboardText.font = debuffTextColour;
                billboard.offset.y += textVerticalMovement;
                DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y - textVerticalMovement, numberLifetime - 0.5f).SetEase(Ease.OutCubic);
                break;
        }

        billboardText.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f);
        billboardImage.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f); 

        Destroy(billboard.gameObject, numberLifetime + 0.5f);
    }
}