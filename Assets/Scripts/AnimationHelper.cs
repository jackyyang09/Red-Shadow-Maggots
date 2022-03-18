using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using DG.Tweening;
using static Facade;

public class AnimationHelper : MonoBehaviour
{
    BaseCharacter baseCharacter;
    public BaseCharacter BaseCharacter { get { return baseCharacter; } }

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

    [SerializeField] CritGlowEffect[] critEffects = null;

    [SerializeField] Rigidbody[] rigidBodies = null;
    [SerializeField] Collider[] colliders = null;
    [SerializeField] Transform skeletonRoot = null;
    public Transform SkeletonRoot { get { return skeletonRoot; } }

    List<Action> onFinishSkillAnimation = new List<Action>();
    public void RegisterOnFinishSkillAnimation(Action newAction) => onFinishSkillAnimation.Add(newAction);

    int exposureID;
    private void Awake()
    {
        baseCharacter = GetComponentInParent<BaseCharacter>();

        DisableRagdoll();

        exposureID = Shader.PropertyToID("_Exposure");
    }

    private void OnDisable()
    {
        ResetSky();
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

    public void BeginSuperCritical()
    {
        EnterCutscene();
        ShowVirtualCamera();
        DarkenSky();
        AudioManager.PlaySound(BattleSceneSounds.SuperCritical);
    }

    public void EndSuperCritical()
    {
        HideVirtualCamera();
    }

    // TODO: Try the priority system instead
    public void ShowVirtualCamera() => vCam.enabled = true;
    public void HideVirtualCamera()
    {
        vCam.enabled = false;
    }
    
    public void MakePlayerCamLookAtMe()
    {
        sceneTweener.PlayerCamera.LookAt = transform;
    }

    [ContextMenu("Test Impulse")]
    public void ShakeCharacterCam()
    {
        impulse.GenerateImpulse();
    }

    public void MakeTargetHoldHitFrame()
    {
        battleSystem.ActiveEnemy.AnimHelper.HoldHitFrame();
    }

    public void MakeTargetFaceAttacker()
    {
        battleSystem.ActiveEnemy.AnimHelper.FaceAttacker();
    }

    public void FaceAttacker()
    {
        baseCharacter.CharacterMesh.transform.LookAt(battleSystem.ActivePlayer.transform);
    }

    public void HoldHitFrame() => anim.Play("Hit Reaction Frame");

    public void MakeTargetShakeMesh()
    {
        battleSystem.ActiveEnemy.AnimHelper.ShakeMesh();
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
    }

    public void EnableCrits()
    {
        for (int i = 0; i < critEffects.Length; i++)
        {
            critEffects[i].EnableCrits();
        }
    }

    public void DisableCrits()
    {
        for (int i = 0; i < critEffects.Length; i++)
        {
            critEffects[i].DisableCrits();
        }
    }

    public void SkillUntween()
    {
        sceneTweener.LerpCamera.enabled = true;
        sceneTweener.LerpCamera.transform.position = vCam.transform.position;
        Invoke(nameof(DisableLerpCam), 1.5f);
    }
    void DisableLerpCam() => sceneTweener.LerpCamera.enabled = false;

    public void SuperCritUntween()
    {
        sceneTweener.LerpCamera.enabled = true;
        sceneTweener.LerpCamera.transform.position = vCam.transform.position;
        sceneTweener.RotateBackInstantly();
        Invoke(nameof(DisableLerpCam), 0.01f);
    }

    public void SpawnMiscEffectAtIndex(int index)
    {
        battleSystem.ActiveEnemy.SpawnEffectPrefab(baseCharacter.Reference.extraEffectPrefabs[index], true);
    }

    public void SpawnAndParentMiscEffectAtIndex(int index)
    {
        battleSystem.ActiveEnemy.SpawnEffectPrefab(baseCharacter.Reference.extraEffectPrefabs[index]);
    }

    public void SpawnWorldEffectAtIndex(int index)
    {
        Instantiate(baseCharacter.Reference.extraEffectPrefabs[index]);
    }

    public void PlayMiscSoundAtIndex(int index)
    {
        AudioManager.PlaySound(baseCharacter.Reference.extraSounds[index]);
    }

    public void StopMiscSoundAtIndex(int index)
    {
        AudioManager.StopSoundIfPlaying(baseCharacter.Reference.extraSounds[index], null);
    }

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
        RenderSettings.skybox.DOFloat(0, exposureID, 0.25f);
    }

    public void BrightenSky()
    {
        RenderSettings.skybox.DOFloat(0.95f, exposureID, 0.25f);
    }

    void ResetSky()
    {
        RenderSettings.skybox.SetFloat(exposureID, 0.95f);
    }
}