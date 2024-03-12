using DG.Tweening;
using JSAM;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Facade;

public class AnimationHelper : MonoBehaviour
{
    BaseCharacter baseCharacter;
    public BaseCharacter BaseCharacter { get { return baseCharacter; } }

    [Header("Effect Properties")]
    [SerializeField] Transform[] projectileOrigins;

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

    [SerializeField] Cinemachine.CinemachineVirtualCamera vCam;
    [SerializeField] Cinemachine.CinemachineImpulseSource impulse;
    [SerializeField] Cinemachine.CinemachineVirtualCamera detailsCam;
    public void ShowDetailsCam() => detailsCam.enabled = true;
    public void HideDetailsCam() => detailsCam.enabled = false;

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
    }

    private void OnDisable()
    {
        OnShowAllRenderers -= ShowRenderers;

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

        for (int i = 0; i < ragdollRenderers.Length; i++)
        {
            ragdollRenderers[i].enabled = false;
        }

        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].enabled = true;
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
    
    // TODO: Consider merging this with FinishSuperCritAttack()
    public void EndSuperCritical()
    {
        HideVirtualCamera();
        ExitCutscene();
    }

    public void ShowVirtualCamera() => vCam.enabled = true;
    public void HideVirtualCamera() => vCam.enabled = false;

    public void ShowPlayerTeamCam() => sceneTweener.PlayerTeamCam.enabled = true;
    public void HidePlayerTeamCam() => sceneTweener.PlayerTeamCam.enabled = false;

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

    public void MakeTargetShakeMesh()
    {
        if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        {
            battleSystem.ActiveEnemy.AnimHelper.ShakeMesh();
        }
        else
        {
            battleSystem.ActivePlayer.AnimHelper.ShakeMesh();
        }
    }

    public void ShakeMesh() => baseCharacter.CharacterMesh.transform.DOShakePosition(0.5f, shakeStrength, shakeVibrato);

    public void DealDamage() => DealPercentage();
    public void DealPercentage(float percentage = 1)
    {
        BaseCharacter.IncomingDamage.Percentage = percentage;

        BaseCharacter.OnCharacterExecuteAttack?.Invoke(BaseCharacter);

        baseCharacter.DealDamage(battleSystem.OpposingCharacter);
    }

    public void DealAOEDamage() => DealAOEPercentage();

    public void DealAOEPercentage(float percentage = 1)
    {
        BaseCharacter.IncomingDamage.Percentage = percentage;
        BaseCharacter.IncomingDamage.IsAOE = true;

        var targets = new List<BaseCharacter>();
        if (battleSystem.CurrentPhase == BattlePhases.PlayerTurn)
        {
            targets.AddRange(enemyController.LivingEnemies);
        }
        else if (battleSystem.CurrentPhase == BattlePhases.EnemyTurn)
        {
            targets.AddRange(battleSystem.LivingPlayers);
        }

        BaseCharacter.OnCharacterExecuteAttack?.Invoke(BaseCharacter);

        foreach (var character in targets)
        {
            baseCharacter.DealDamage(character);
        }
    }

    public void FinishAttack() => baseCharacter.FinishAttack();

    public void FinishSuperCritAttack() => baseCharacter.FinishSuperCritAttack();

    public void TeleportBack() => sceneTweener.TeleportBackToPosition();

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

    public void SuperCritCutUntween()
    {
        sceneTweener.RotateBackInstantly();
        Invoke(nameof(DisableLerpCam), 0.01f);
    }

    List<Projectile> spawnedProjectile = new List<Projectile>();
    public void SpawnProjectileEffectAtIndex(int index)
    {
        var effect = Instantiate(baseCharacter.Reference.projectileEffectPrefabs[index]).GetComponent<Projectile>();
        effect.InitializeWithTarget(battleSystem.OpposingCharacter, projectileOrigins[index]);
        spawnedProjectile.Add(effect);
    }

    public void LaunchProjectile()
    {
        foreach (var item in spawnedProjectile)
        {
            item.Launch();
        }
        spawnedProjectile.Clear();
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
        if (baseCharacter as PlayerCharacter)
        {
            var t = Instantiate(baseCharacter.Reference.extraEffectPrefabs[index]).transform;
            t.forward = -t.forward;
        }
        else
        {
            Instantiate(baseCharacter.Reference.extraEffectPrefabs[index]);
        }
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
        if (RenderSettings.skybox)
        {
            RenderSettings.skybox.SetFloat(exposureID, 0.95f);
        }
    }

    public void ShowAllRenderers()
    {
        OnShowAllRenderers?.Invoke();
    }

    List<BaseCharacter> GetAllAllies()
    {
        if (baseCharacter.IsPlayer())
        {
            return battleSystem.LivingPlayers.ToList<BaseCharacter>();
        }
        else
        {
            return enemyController.LivingEnemies.ToList<BaseCharacter>();
        }
    }

    List<BaseCharacter> GetAllEnemies()
    {
        if (!baseCharacter.IsPlayer())
        {
            return battleSystem.LivingPlayers.ToList<BaseCharacter>();
        }
        else
        {
            return enemyController.LivingEnemies.ToList<BaseCharacter>();
        }
    }

    public void HideAllAlliesExceptSelf()
    {
        var allies = GetAllAllies();
        allies.Remove(baseCharacter);
        HideAll(allies);
    }

    public void HideAllAllyRenderers()
    {
        HideAll(GetAllAllies());
    }

    public void HideAllOppositionRenderers()
    {
        HideAll(GetAllEnemies());
    }

    void ShowRenderers()
    {
        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].gameObject.layer = DEFAULT_LAYER_ID;
        }
    }

    void HideAll(List<BaseCharacter> characters)
    {
        foreach (var c in characters)
        {
            c.AnimHelper.HideRenderers();
        }
    }

    void HideRenderers()
    {
        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].gameObject.layer = IGNORE_LAYER_ID;
        }
    }

    public void EnableRenderers()
    {
        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].enabled = true;
        }
    }

    public void DisableRenderers()
    {
        for (int i = 0; i < nonRagdollRenderers.Length; i++)
        {
            nonRagdollRenderers[i].enabled = false;
        }
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(SetupRagdollComponents))]
    void SetupRagdollComponents()
    {
        
    }
#endif
}