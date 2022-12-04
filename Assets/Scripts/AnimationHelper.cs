using DG.Tweening;
using JSAM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [UnityEngine.Serialization.FormerlySerializedAs("renderers")]
    [SerializeField] Renderer[] nonRagdollRenderers;
    [SerializeField] Renderer[] ragdollRenderers;
    [SerializeField] Transform skeletonRoot = null;
    public Transform SkeletonRoot { get { return skeletonRoot; } }

    List<System.Action> onFinishSkillAnimation = new List<System.Action>();
    public void RegisterOnFinishSkillAnimation(System.Action newAction) => onFinishSkillAnimation.Add(newAction);

    static System.Action OnShowAllRenderers;
    static System.Action<List<AnimationHelper>> OnHideRenderers;

    const int DEFAULT_LAYER_ID = 17; // TF2 Ragdoll
    const int IGNORE_LAYER_ID = 19; // Ignore Cutscene Cam

    int exposureID;
    private void Awake()
    {
        baseCharacter = GetComponentInParent<BaseCharacter>();

        DisableRagdoll();

        exposureID = Shader.PropertyToID("_Exposure");
    }

    private void OnEnable()
    {
        OnShowAllRenderers += ShowRenderers;
        OnHideRenderers += HideAll;
    }

    private void OnDisable()
    {
        OnShowAllRenderers -= ShowRenderers;
        OnHideRenderers -= HideAll;

        ResetSky();
    }

    [ContextMenu("Enable Ragdoll")]
    public void EnableRagdoll()
    {
        anim.enabled = false;

        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].enabled = false;
        }

        for (int i = 0; i < ragdollRenderers.Length; i++)
        {
            ragdollRenderers[i].enabled = true;
        }

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

        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].enabled = false;
        }

        for (int i = 0; i < ragdollRenderers.Length; i++)
        {
            ragdollRenderers[i].enabled = true;
        }

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

        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].enabled = true;
        }

        for (int i = 0; i < ragdollRenderers.Length; i++)
        {
            ragdollRenderers[i].enabled = false;
        }

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
        playerControlManager.SetControlMode(PlayerControlMode.InCutscene);
        GlobalEvents.OnEnterBattleCutscene?.Invoke();
    }

    public void ExitCutscene()
    {
        playerControlManager.ReturnControl();
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
    public void HideVirtualCamera() => vCam.enabled = false;

    public void MakePlayerCamLookAtMe() => sceneTweener.PlayerCamera.LookAt = transform;

    [ContextMenu("Test Impulse")]
    public void ShakeCharacterCam()
    {
        impulse.GenerateImpulse();
    }

    public void EnableTargetDeath() => battleSystem.ActiveEnemy.CanDie = true;

    public void DisableTargetDeath() => battleSystem.ActiveEnemy.CanDie = false;

    public void MakeTargetHoldHitFrame() => battleSystem.ActiveEnemy.AnimHelper.HoldHitFrame();

    public void MakeTargetFaceAttacker() => battleSystem.ActiveEnemy.AnimHelper.FaceAttacker();

    public void FaceAttacker() => baseCharacter.CharacterMesh.transform.LookAt(battleSystem.ActivePlayer.transform);

    public void HoldHitFrame() => anim.Play("Hit Reaction Frame");

    public void MakeTargetShakeMesh() => battleSystem.ActiveEnemy.AnimHelper.ShakeMesh();

    public void ShakeMesh() => baseCharacter.CharacterMesh.transform.DOShakePosition(0.5f, shakeStrength, shakeVibrato);

    public void DealDamage() => baseCharacter.DealDamage();
    public void DealPercentage(float percentage = 1) => baseCharacter.DealDamage(percentage);

    public void DealAOEDamage() => baseCharacter.DealAOEDamage();
    public void DealAOEPercentage(float percentage = 1) => baseCharacter.DealAOEDamage(percentage);

    public void FinishAttack() => baseCharacter.FinishAttack();

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

    public void SpawnEffectOnSelfAtIndex(int index)
    {
        Instantiate(baseCharacter.Reference.extraEffectPrefabs[index], SkeletonRoot);
    }

    public void SpawnWorldEffectAtIndex(int index)
    {
        Instantiate(baseCharacter.Reference.extraEffectPrefabs[index]);
    }

    public void PlayWeaponSound() => AudioManager.PlaySound(baseCharacter.Reference.weaponSound);

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

    public void ShowAllRenderers()
    {
        OnShowAllRenderers?.Invoke();
    }

    public void HideAllPlayersExceptSelf()
    {
        var players = battleSystem.PlayerCharacters;
        var toHide = new List<AnimationHelper>();
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i]) continue;
            if (!players[i].IsDead && players[i] != baseCharacter)
            {
                toHide.Add(players[i].AnimHelper);
            }
        }
        OnHideRenderers?.Invoke(toHide);
    }

    public void HideAllPlayerRenderers()
    {
        var players = battleSystem.PlayerCharacters;
        var toHide = new List<AnimationHelper>();
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i]) continue;
            if (!players[i].IsDead)
            {
                toHide.Add(players[i].AnimHelper);
            }
        }
        OnHideRenderers?.Invoke(toHide);
    }

    public void HideAllEnemyRenderers()
    {
        var enemies = enemyController.Enemies;
        var toHide = new List<AnimationHelper>();
        for (int i = 0; i < enemies.Length; i++)
        {
            if (!enemies[i]) continue;
            if (!enemies[i].IsDead)
            {
                toHide.Add(enemies[i].AnimHelper);
            }
        }
        OnHideRenderers?.Invoke(toHide);
    }

    void ShowRenderers()
    {
        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].gameObject.layer = DEFAULT_LAYER_ID;
        }
    }

    void HideAll(List<AnimationHelper> helpers)
    {
        if (helpers.Contains(this))
        {
            for (int i = 0; i < nonRagdollRenderers.Length; i++)
            {
                nonRagdollRenderers[i].gameObject.layer = IGNORE_LAYER_ID;
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(GetAllRenderers))]
    void GetAllRenderers()
    {
        UnityEditor.Undo.RecordObject(this, "Found all renderers");
        nonRagdollRenderers = GetComponentsInChildren<Renderer>();
    }

    [ContextMenu(nameof(SetupRagdollComponents))]
    void SetupRagdollComponents()
    {
        UnityEditor.Undo.RecordObject(this, "Found ragdoll components");
        rigidBodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }
#endif
}