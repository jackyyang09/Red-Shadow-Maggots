using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalDamageCounter : BasicSingleton<TotalDamageCounter>
{
    [SerializeField] OptimizedCanvas canvas;
    [SerializeField] TMPro.TextMeshProUGUI damageLabel;

    float damage;

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
        float damageCount = 0;
        if (!canvas.IsVisible)
        {
            damageCount = 0;
            canvas.Show();
        }
        damageCount += damage.TotalDamage;
        damageLabel.text = ((int)damageCount).ToString();
    }
}