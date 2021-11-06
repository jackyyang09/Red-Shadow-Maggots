using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

        healthBar.fillAmount = baseCharacter.GetHealthPercent();
        healthText.text = baseCharacter.CurrentHealth.ToString();

        baseCharacter.onTakeDamage += UpdateValue;
        baseCharacter.onHeal += UpdateValue;
    }

    private void OnDisable()
    {
        if (baseCharacter)
        {
            baseCharacter.onTakeDamage -= UpdateValue;
            baseCharacter.onHeal -= UpdateValue;
        }
    }

    // Update is called once per frame
    public void UpdateValue()
    {
        tweenBar.fillAmount = healthBar.fillAmount;
        healthBar.fillAmount = baseCharacter.GetHealthPercent();
        tweenBar.DOFillAmount(healthBar.fillAmount, catchupTime).SetEase(Ease.OutCubic).SetDelay(updateDelay);
        healthText.text = ((int)baseCharacter.CurrentHealth).ToString();
    }
}
