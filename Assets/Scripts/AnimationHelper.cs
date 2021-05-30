using JSAM;
using System.Collections;
using UnityEngine;

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

    [Header("Object References")]

    [SerializeField] Animator anim = null;

    [SerializeField] Cinemachine.CinemachineVirtualCamera vCam = null;
    [SerializeField] Cinemachine.CinemachineImpulseSource impulse = null;

    [SerializeField] Rigidbody[] rigidBodies = null;
    [SerializeField] Collider[] colliders = null;

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

    public void ShowVirtualCamera() => vCam.enabled = true;
    public void HideVirtualCamera() => vCam.enabled = false;

    public void MakePlayerCamLookAtMe()
    {
        SceneTweener.instance.PlayerCamera.LookAt = transform;
    }

    [ContextMenu("Test Impulse")]
    public void ShakeCharacterCam()
    {
        impulse.GenerateImpulse();
    }

    public void DealDamage()
    {
        baseCharacter.DealDamage();
    }

    public void FinishAttack()
    {
        baseCharacter.FinishAttack();
    }

    public void PlayMiscSoundAtIndex(int index)
    {
        AudioManager.instance.PlaySoundInternal(baseCharacter.Reference.extraSounds[index]);
    }

    public void PlayMiscLoopingSoundAtIndex(int index)
    {
        AudioManager.instance.PlaySoundLoopInternal(baseCharacter.Reference.extraSounds[index]);
    }

    public void StopMiscLoopingSoundAtIndex(int index)
    {
        AudioManager.instance.StopSoundLoopInternal(baseCharacter.Reference.extraSounds[index]);
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
}