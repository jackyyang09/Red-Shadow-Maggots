using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalDamageCounter : BasicSingleton<TotalDamageCounter>
{
    [SerializeField] OptimizedCanvas canvas;
    [SerializeField] TMPro.TextMeshProUGUI damageLabel;

    float damageTotal = 0;

    void ShowUI() => canvas.Show();
    void HideUI() => canvas.Hide();

    private void OnEnable()
    {
        BaseCharacter.OnCharacterReceivedDamage += AddDamage;
        BattleSystem.OnEndPhase[(int)BattlePhases.PlayerTurn] += HideUI;
        BattleSystem.OnEndPhase[(int)BattlePhases.EnemyTurn] += HideUI;
    }

    private void OnDisable()
    {
        BaseCharacter.OnCharacterReceivedDamage -= AddDamage;
        BattleSystem.OnEndPhase[(int)BattlePhases.PlayerTurn] -= HideUI;
        BattleSystem.OnEndPhase[(int)BattlePhases.EnemyTurn] -= HideUI;
    }

    void AddDamage(BaseCharacter character, DamageStruct damage)
    {
        if (!canvas.IsVisible)
        {
            damageTotal = 0;
            canvas.Show();
        }
        damageTotal += damage.TotalDamage;
        damageLabel.text = ((int)damageTotal).ToString();
    }
}