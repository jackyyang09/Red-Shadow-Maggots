﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EffectTextSpawner : MonoBehaviour
{
    [SerializeField] float numberLifetime = 3;
    [SerializeField] float textVerticalMovement = 50;

    [SerializeField] Camera cam = null;

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
        DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + textVerticalMovement, numberLifetime)/*.SetEase(Ease.OutCubic)*/;

        text.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f);

        Destroy(text.gameObject, numberLifetime);
    }

    public GameObject SpawnCriticalHitAt(Transform trans)
    {
        return Instantiate(critTextPrefab, trans);
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

        Destroy(billboard.gameObject, numberLifetime);
    }
}
