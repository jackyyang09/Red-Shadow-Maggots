using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum DamageType
{
    Light,
    Medium,
    Heavy
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
    [SerializeField]
    GameObject damageNumberPrefab;

    [SerializeField]
    Vector3 spawnOffset;

    [SerializeField]
    float numberLifetime = 3;

    [SerializeField]
    DamageNumberProperties[] damageNumberProps;

    [Header("Effectiveness Properties")]
    [SerializeField]
    string resistDescriptor = "RESIST!";
    [SerializeField]
    Color resistColor = Color.blue;

    [SerializeField]
    string effectiveDescriptor = "EFFECTIVE!";
    [SerializeField]
    Color effectiveColor = Color.red;

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

    public void SpawnDamageNumberAt(float damageValue, Vector3 position, DamageType damageType = DamageType.Light, DamageEffectivess effectivity = DamageEffectivess.Normal)
    {
        GameObject number = Instantiate(damageNumberPrefab, position + spawnOffset, Quaternion.identity);

        TMPro.TextMeshProUGUI[] texts = number.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        TMPro.TextMeshProUGUI numberText = texts[0];
        TMPro.TextMeshProUGUI effectiveness = texts[1];

        numberText.text = "-" + Mathf.RoundToInt(damageValue).ToString();

        numberText.font = damageNumberProps[(int)damageType].font;
        numberText.fontStyle = damageNumberProps[(int)damageType].textStyle;
        numberText.fontSize = damageNumberProps[(int)damageType].textSize;

        switch (effectivity)
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

        number.transform.DOMoveY(number.transform.position.y + 0.3f, numberLifetime).SetEase(Ease.OutCubic);

        Destroy(number, numberLifetime);
    }
}