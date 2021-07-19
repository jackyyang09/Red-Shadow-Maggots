using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum DamageType
{
    Light,
    Medium,
    Heavy,
    Heal
}

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] Camera cam = null;

    [SerializeField] float numberLifetime = 1.5f;
    [SerializeField] float numberFadeDelay = 1;
    [SerializeField] float numberFadeTime = 0.5f;

    [SerializeField] float verticalMovement = 0.5f;

    [SerializeField] GameObject[] damageNumberPrefabs = null;

    public static DamageNumberSpawner instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    public void SpawnDamageNumberAt(Transform trans, DamageStruct damage)
    {
        GameObject number = Instantiate(damageNumberPrefabs[(int)damage.effectivity], transform.GetChild(0));
        var billboard = number.GetComponent<ViewportBillboard>();
        billboard.EnableWithSettings(cam, trans);

        TMPro.TextMeshProUGUI[] texts = number.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        TMPro.TextMeshProUGUI numberText = texts[0];

        numberText.text = "-" + Mathf.RoundToInt(damage.damage).ToString();

        if (damage.isCritical)
        {
            var criticalText = EffectTextSpawner.instance.SpawnCriticalHitAt(number.transform).GetComponentInChildren<TMPro.TextMeshProUGUI>();
            criticalText.DOFade(0, numberFadeTime).SetDelay(numberFadeDelay);
        }

        DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + verticalMovement, numberLifetime).SetEase(Ease.OutCubic);

        numberText.DOFade(0, numberFadeTime).SetDelay(numberFadeDelay);
        // Tween effectiveness if it exists
        if (texts.Length > 1)
        {
            texts[1].DOFade(0, numberFadeTime).SetDelay(numberFadeDelay);
        }

        Destroy(number, numberLifetime);
    }
}