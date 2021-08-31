using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using DG.Tweening;

public class AnimationHelper : MonoBehaviour
{
    BaseCharacter baseCharacter;

    [Header("Knockback Properties")]

    [SerializeField] Transform knockbackOrigin = null;
    [SerializeField] float knockbackRadius = 1;

    [SerializeField] float knockbackForce = 10;
    [SerializeField] float knockbackUp = 1;

    [Header("Explosion Properties")]

    [SerializeField] float explosionForces = 10;
    [SerializeField] float explosionUp = 1;

    [Header("Mesh Shake Properties")]
    [SerializeField] float shakeStrength = 1;
    [SerializeField] int shakeVibrato = 10;

    [Header("Object References")]

    [SerializeField] Animator anim = null;

    [SerializeField] Cinemachine.CinemachineVirtualCamera vCam = null;
    [SerializeField] Cinemachine.CinemachineImpulseSource impulse = null;

    [SerializeField] Rigidbody[] rigidBodies = null;
    [SerializeField] Collider[] colliders = null;

    List<Action> onFinishSkillAnimation = new List<Action>();
    public void RegisterOnFinishSkillAnimation(Action newAction) => onFinishSkillAnimation.Add(newAction);

    private void Awake()
    {
        baseCharacter = GetComponentInParent<BaseCharacter>();

        DisableRagdoll();
    }

    [ContextMenu("Setup Ragdoll References")]
    void GetRagdollComponents()
    {
        rigidBodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    [ContextMenu("Enable Ragdoll")]
    public void EnableRagdoll()
    {
        anim.enabled = false;

        for (int i = 0; i < rigidBodies.Length; i++)
        {
            rigidBodies[i].isKinematic = false;
            rigidBodies[i].AddExplosionForce(knockbackForce, knockbackOrigin.position, knockbackRadius, knockbackUp);
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }

    [ContextMenu("Enable Ragdoll Explosion")]
    public void EnableRagdollExplosion()
    {
        anim.enabled = false;

        for (int i = 0; i < rigidBodies.Length; i++)
        {
            rigidBodies[i].isKinematic = false;
            rigidBodies[i].AddExplosionForce(explosionForces, knockbackOrigin.position, knockbackRadius, explosionUp);
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }

    [ContextMenu("Disable Ragdoll")]
    public void DisableRagdoll()
    {
        anim.enabled = true;

        for (int i = 0; i < rigidBodies.Length; i++)
        {
            rigidBodies[i].isKinematic = true;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    public void EnterCutscene()
    {
        GlobalEvents.OnEnterBattleCutscene?.Invoke();
    }

    public void ExitCutscene()
    {
        GlobalEvents.OnExitBattleCutscene?.Invoke();
    }

    // TODO: Try the priority system instead
    public void ShowVirtualCamera() => vCam.enabled = true;
    public void HideVirtualCamera()
    {
        vCam.enabled = false;
    }
    
    public void MakePlayerCamLookAtMe()
    {
        SceneTweener.Instance.PlayerCamera.LookAt = transform;
    }

    [ContextMenu("Test Impulse")]
    public void ShakeCharacterCam()
    {
        impulse.GenerateImpulse();
    }

    public void MakeTargetHoldHitFrame()
    {
        BattleSystem.Instance.GetActiveEnemy().AnimHelper.HoldHitFrame();
    }

    public void MakeTargetFaceAttacker()
    {
        BattleSystem.Instance.GetActiveEnemy().AnimHelper.FaceAttacker();
    }

    public void FaceAttacker()
    {
        baseCharacter.CharacterMesh.transform.LookAt(BattleSystem.Instance.GetActivePlayer().transform);
    }

    public void HoldHitFrame() => anim.Play("Hit Reaction Frame");

    public void MakeTargetShakeMesh()
    {
        BattleSystem.Instance.GetActiveEnemy().AnimHelper.ShakeMesh();
    }

    public void ShakeMesh() => baseCharacter.CharacterMesh.transform.DOShakePosition(0.5f, shakeStrength, shakeVibrato);

    public void DealDamage()
    {
        baseCharacter.DealDamage();
    }

    public void DealAOEDamage()
    {
        baseCharacter.DealAOEDamage();
    }

    public void FinishAttack()
    {
        baseCharacter.FinishAttack();
    }

    public void FinishSkill()
    {
        for (int i = 0; i < onFinishSkillAnimation.Count; i++)
        {
            onFinishSkillAnimation[i]();
        }
        onFinishSkillAnimation.Clear();

        SceneTweener.Instance.LerpCamera.enabled = true;
        SceneTweener.Instance.LerpCamera.transform.position = vCam.transform.position;        
        Invoke(nameof(DisableLerpCam), 1.5f);
    }

    void DisableLerpCam() => SceneTweener.Instance.LerpCamera.enabled = false;

    public void SpawnMiscEffectAtIndex(int index)
    {
        BattleSystem.Instance.GetActiveEnemy().SpawnEffectPrefab(baseCharacter.Reference.extraEffectPrefabs[index], true);
    }

    public void SpawnAndParentMiscEffectAtIndex(int index)
    {
        BattleSystem.Instance.GetActiveEnemy().SpawnEffectPrefab(baseCharacter.Reference.extraEffectPrefabs[index]);
    }

    public void PlayMiscSoundAtIndex(int index)
    {
        AudioManager.PlaySound(baseCharacter.Reference.extraSounds[index]);
    }
    //
    //public void PlayMiscLoopingSoundAtIndex(int index)
    //{
    //    AudioManager.PlaySoundLoop(baseCharacter.Reference.extraSounds[index]);
    //}
    //
    //public void StopMiscLoopingSoundAtIndex(int index)
    //{
    //    AudioManager.Instance.StopSoundLoopInternal(baseCharacter.Reference.extraSounds[index]);
    //}

    public void DashForward() => anim.Play("Dash");

    public void WalkForward(float walkTime)
    {
        StartCoroutine(WalkRoutine(walkTime));
    }

    IEnumerator WalkRoutine(float walkTime)
    {
        anim.Play("Walk");
        yield return new WaitForSeconds(walkTime);
        anim.SetBool("Walk", false);
    }

    public void DarkenSky()
    {
        RenderSettings.skybox.DOFloat(0, "_Exposure", 0.25f);
    }

    public void BrightenSky()
    {
        RenderSettings.skybox.DOFloat(0.95f, "_Exposure", 0.25f);
    }
}