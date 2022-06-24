using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class SimpleHealth : MonoBehaviour
{
    [SerializeField] float updateDelay = 0.5f;

    [SerializeField] float catchupTime = 0.5f;

    [SerializeField] TMPro.TextMeshProUGUI healthText = null;

    [SerializeField] Image healthBar = null;

    [SerializeField] Image tweenBar = null;

    [SerializeField] BaseCharacter baseCharacter = null;

    // Start is called before the first frame update
    void Start()
    {
        if (baseCharacter) InitializeWithCharacter(baseCharacter);
    }

    public void InitializeWithCharacter(BaseCharacter character)
    {
        baseCharacter = character;

        baseCharacter.onTakeDamage += OnTakeDamage;
        baseCharacter.onSetHealth += OnSetHealth;
        baseCharacter.onHeal += OnHeal;

        OnSetHealth();
    }

    private void OnDisable()
    {
        if (baseCharacter)
        {
            baseCharacter.onTakeDamage -= OnTakeDamage;
            baseCharacter.onSetHealth -= OnSetHealth;
            baseCharacter.onHeal -= OnHeal;
        }
    }

    public void OnTakeDamage()
    {
        tweenBar.fillAmount = healthBar.fillAmount;
        healthBar.fillAmount = baseCharacter.GetHealthPercent();
        tweenBar.DOFillAmount(healthBar.fillAmount, catchupTime).SetEase(Ease.OutCubic).SetDelay(updateDelay);
        healthText.text = ((int)baseCharacter.CurrentHealth).ToString();
    }

    private void OnSetHealth()
    {
        healthBar.fillAmount = baseCharacter.GetHealthPercent();
        tweenBar.fillAmount = healthBar.fillAmount;
        healthText.text = ((int)baseCharacter.CurrentHealth).ToString();
    }

    public void OnHeal()
    {
        var tween = healthBar.DOFillAmount(baseCharacter.GetHealthPercent(), catchupTime).OnUpdate(() =>
        {
            healthText.text = ((int)Mathf.Lerp(0, baseCharacter.MaxHealth, healthBar.fillAmount)).ToString();
        }).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            healthBar.fillAmount = baseCharacter.GetHealthPercent();
            healthText.text = baseCharacter.CurrentHealth.ToString();
        });
    }
}
