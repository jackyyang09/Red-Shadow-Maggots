using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Door Poison", menuName = "ScriptableObjects/Game Effects/Door Poison", order = 1)]
public class DoorPoison : DamagePerTurnEffect
{
    public override float TickAnimationTime => 2;

    [Header("Effect Properties")]
    [SerializeField] Vector3 spawnOffset = new Vector3(-1, 4, 3);
    [SerializeField] float throwTime = 0.25f;
    [SerializeField] float explodeTime = 0.2f;
    [SerializeField] float explosionForce = 300;
    [SerializeField] float explosionRadius = 10;
    [SerializeField] float doorShrinkDelay = 0;
    [SerializeField] float doorLifetime = 2;

    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Door";
    public override string ExplainerDescription =>
        "Every turn, get hit by a door and lose " + RSMConstants.Keywords.Short.HEALTH + ".";

    public override string GetEffectDescription(EffectStrength strength, float[] customValues)
    {
        return ExplainerDescription.Split('.')[0] + " equal to " + value.GetDescription(strength);
    }

    public GameObject doorPrefab;

    public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    {
        string s = TargetModeDescriptor(targetMode);

        switch (targetMode)
        {
            case TargetMode.Self:
                s += "Get ";
                break;
            case TargetMode.OneAlly:
            case TargetMode.OneEnemy:
                s += "gets ";
                break;
            case TargetMode.AllAllies:
            case TargetMode.AllEnemies:
                s += "get ";
                break;
        }

        //s += (int)(change * 100) + "% " + Keywords.Short.MAX_HEALTH + 
        //    " every turn by getting hit by a door ";
        s += "hit by a <u>Door</u> every turn, taking " + value.GetDescription(props.strength) +
            " in damage " + DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);

        return s;
    }

    public override void Tick(AppliedEffect effect)
    {
        effect.target.StartCoroutine(PlayEffect(effect.target, () => DealDamage(effect.target, effect.strength, effect.customValues)));
    }

    public override void TickCustom(BaseCharacter user, BaseCharacter target, List<object> values)
    {
        //target.StartCoroutine(PlayEffect(target, () => DealDamage(target, strength, values)));
    }

    IEnumerator PlayEffect(BaseCharacter target, System.Action damageDelegate)
    {
        Vector3 targetPos = target.EffectRegion.transform.position;
        GameObject door = target.SpawnEffectPrefab(doorPrefab, true, false);
        door.transform.LookAt(targetPos);
        door.transform.position += new Vector3(spawnOffset.x * (target.CharacterMesh.transform.right.z), spawnOffset.y, spawnOffset.z);
        Vector3 spawnPosition = door.transform.position;
        door.transform.DOMove(targetPos, throwTime).SetEase(Ease.Linear);
        door.transform.DORotate(Random.rotationUniform.eulerAngles, throwTime, RotateMode.LocalAxisAdd).SetEase(Ease.Linear);

        yield return new WaitForSeconds(explodeTime);

        door.transform.GetChild(0).gameObject.SetActive(false);
        GameObject brokeMesh = door.transform.GetChild(1).gameObject;
        Rigidbody[] rigidbodies = brokeMesh.GetComponentsInChildren<Rigidbody>();
        brokeMesh.SetActive(true);

        door.transform.DOKill();
        Vector3 explosionPosition = target.transform.position;
        explosionPosition.x = spawnPosition.x;
        explosionPosition.z = Mathf.Clamp01(explosionPosition.z);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Vector3 direction = (rigidbodies[i].position - explosionPosition).normalized;
            rigidbodies[i].velocity = direction * explosionForce;
            rigidbodies[i].transform.DOScale(0.01f, doorLifetime - doorShrinkDelay).SetDelay(doorShrinkDelay);
        }
        damageDelegate.Invoke();

        yield return new WaitForSeconds(doorLifetime);

        foreach (var item in rigidbodies)
        {
            item.transform.DOKill();
        }
        Destroy(door);
    }
}