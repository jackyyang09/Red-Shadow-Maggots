using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShieldUI : MonoBehaviour
{
    [SerializeField] float barTween;
    [SerializeField] Image shieldBar;

    BaseCharacter baseCharacter;

    public void InitializeWithCharacter(BaseCharacter character)
    {
        baseCharacter = character;

        baseCharacter.OnShielded += OnShielded;
        baseCharacter.OnTakeDamage += OnTakeDamage;

        OnSetShield();
    }

    private void OnDisable()
    {
        if (!baseCharacter) return;
        baseCharacter.OnShielded -= OnShielded;
        baseCharacter.OnTakeDamage -= OnTakeDamage;
    }

    private void OnSetShield()
    {
        shieldBar.fillAmount = baseCharacter.ShieldPercent;
    }

    private void OnShielded()
    {
        shieldBar.DOFillAmount(baseCharacter.ShieldPercent, barTween);
    }

    private void OnTakeDamage(float trueDamage, DamageStruct damage)
    {
        shieldBar.DOFillAmount(baseCharacter.ShieldPercent, barTween);
    }
}