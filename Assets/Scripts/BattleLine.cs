using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

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
        UIManager.OnRemovePlayerControl += HideLine;
        UIManager.OnResumePlayerControl += ShowLine;
        UIManager.OnAttackCommit += HideLine;
    }

    private void OnDisable()
    {
        EnemyController.OnChangedAttackers -= UpdateLine;
        EnemyController.OnChangedAttackTargets -= UpdateLine;
        UIManager.OnRemovePlayerControl -= HideLine;
        UIManager.OnResumePlayerControl -= ShowLine;
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

    void ShowLine()
    {
        if (!BattleSystem.Instance.EnemyAttacker.CanCrit) return;
        lineRenderer.enabled = true;
        sparks.Play();
    }

    void HideLine()
    {
        lineRenderer.enabled = false;
        sparks.Stop();
    }
}