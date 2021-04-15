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

[System.Serializable]
public struct DamageNumberProperties
{
    public float textSize;
    public TMPro.TMP_FontAsset font;
    public TMPro.FontStyles textStyle;
}

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] Camera cam = null;

    [SerializeField] GameObject damageNumberPrefab = null;

    [SerializeField] float numberLifetime = 3;

    [SerializeField] DamageNumberProperties[] damageNumberProps = new DamageNumberProperties[0];

    [Header("Effectiveness Properties")]

    [SerializeField] string resistDescriptor = "RESIST!";

    [SerializeField] Color resistColor = Color.blue;

    [SerializeField] string effectiveDescriptor = "EFFECTIVE!";

    [SerializeField] Color effectiveColor = Color.red;

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
        GameObject number = Instantiate(damageNumberPrefab, transform.GetChild(0));
        var billboard = number.GetComponent<ViewportBillboard>();
        billboard.EnableWithSettings(cam, trans);

        TMPro.TextMeshProUGUI[] texts = number.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        TMPro.TextMeshProUGUI numberText = texts[0];
        TMPro.TextMeshProUGUI effectiveness = texts[1];

        numberText.text = "-" + Mathf.RoundToInt(damage.damage).ToString();

        numberText.font = damageNumberProps[(int)damage.damageType].font;
        numberText.fontStyle = damageNumberProps[(int)damage.damageType].textStyle;
        numberText.fontSize = damageNumberProps[(int)damage.damageType].textSize;

        if (damage.isCritical) EffectTextSpawner.instance.SpawnCriticalHitAt(trans);

        switch (damage.effectivity)
        {
            case DamageEffectivess.Normal:
                effectiveness.text = "";
                numberText.color = Color.white;
                break;
            case DamageEffectivess.Resist:
                effectiveness.text = resistDescriptor;
                effectiveness.color = resistColor;
                numberText.color = resistColor;
                break;
            case DamageEffectivess.Effective:
                effectiveness.text = effectiveDescriptor;
                effectiveness.color = effectiveColor;
                numberText.color = effectiveColor;
                break;
        }

        DOTween.To(() => billboard.offset.y, x => billboard.offset.y = x, billboard.offset.y + 0.5f, numberLifetime).SetEase(Ease.OutCubic);
        numberText.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f);
        effectiveness.DOFade(0, 0.5f).SetDelay(numberLifetime - 0.5f);

        Destroy(number, numberLifetime);
    }
}