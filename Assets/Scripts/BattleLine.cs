using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;
using static Facade;

public class BattleLine : MonoBehaviour
{
    [SerializeField] float height = 1.75f;
    [SerializeField] float lineLength = 1;

    [SerializeField] VolumetricLineBehavior line = null;
    [SerializeField] ParticleSystem sparks = null;
    [SerializeField] Renderer lineRenderer;

    private void OnEnable()
    {
        EnemyController.OnChangedAttackers += UpdateLine;
        EnemyController.OnChangedAttackTargets += UpdateLine;
        UIManager.OnHideBattleUI += HideLine;
        UIManager.OnShowBattleUI += ShowLine;
        UIManager.OnAttackCommit += HideLine;
    }

    private void OnDisable()
    {
        EnemyController.OnChangedAttackers -= UpdateLine;
        EnemyController.OnChangedAttackTargets -= UpdateLine;
        UIManager.OnHideBattleUI -= HideLine;
        UIManager.OnShowBattleUI -= ShowLine;
        UIManager.OnAttackCommit -= HideLine;
    }

    private void UpdateLine()
    {
        if (!BattleSystem.Instance.EnemyAttacker || !BattleSystem.Instance.EnemyAttackTarget) return;
        line.StartPos = BattleSystem.Instance.EnemyAttacker.transform.position + height * Vector3.up;
        line.EndPos = BattleSystem.Instance.EnemyAttackTarget.transform.position + height * Vector3.up;
        Vector3 dir = (line.StartPos - line.EndPos).normalized;
        
        Vector3 midPoint = Vector3.Lerp(line.StartPos, line.EndPos, 0.5f);

        float halfLength = lineLength / 5;
        line.StartPos = midPoint * halfLength;
        line.EndPos = midPoint * halfLength;

        line.StartPos = midPoint - dir * halfLength;
        line.EndPos = midPoint + dir * halfLength;

        sparks.transform.position = Vector3.Lerp(line.StartPos, line.EndPos, 0.5f);
    }

    public void ShowLine()
    {
        /*if (!battleSystem.EnemyAttacker.CanCrit || enemyController.WillUseSkill)*/ return;
        lineRenderer.enabled = true;
        sparks.Play();
    }

    public void HideLine()
    {
        lineRenderer.enabled = false;
        sparks.Stop();
    }
}