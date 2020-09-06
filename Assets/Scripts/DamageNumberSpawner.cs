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

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject damageNumberPrefab;

    [SerializeField]
    Vector3 spawnOffset;

    [SerializeField]
    float numberLifetime = 3;

    [Header("Light Damage Properties")]
    [SerializeField]
    float lightTextSize = 0.05f;
    [SerializeField]
    Color lightColor = Color.gray;

    [Header("Medium Damage Properties")]
    [SerializeField]
    float mediumTextSize = 0.075f;
    [SerializeField]
    Color mediumColor = Color.yellow;

    [Header("Heavy Damage Properties")]
    [SerializeField]
    float heavyTextSize = 0.1f;
    [SerializeField]
    Color heavyColor = Color.red;

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

    public void SpawnDamageNumberAt(float damageValue, Vector3 position, DamageType damageType = DamageType.Light)
    {
        GameObject number = Instantiate(damageNumberPrefab, position + spawnOffset, Quaternion.identity);

        TMPro.TextMeshProUGUI numberText = number.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        numberText.text = "-" + Mathf.RoundToInt(damageValue).ToString();

        switch (damageType)
        {
            case DamageType.Light:
                numberText.color = lightColor;
                numberText.fontSize = lightTextSize;
                break;
            case DamageType.Medium:
                numberText.color = mediumColor;
                numberText.fontSize = mediumTextSize;
                break;
            case DamageType.Heavy:
                numberText.color = heavyColor;
                numberText.fontSize = heavyTextSize;
                break;
        }

        number.transform.DOMoveY(number.transform.position.y + 0.3f, numberLifetime).SetEase(Ease.OutCubic);

        Destroy(number, numberLifetime);
    }
}