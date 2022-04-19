﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Door Poison", menuName = "ScriptableObjects/Game Effects/Door Poison", order = 1)]
public class DoorPoison : DamagePerTurnEffect
{
    [Header("Effect Properties")]
    [SerializeField] Vector3 spawnOffset = new Vector3(-1, 4, 3);
    [SerializeField] float throwTime = 0.25f;
    [SerializeField] float explodeTime = 0.2f;
    [SerializeField] float explosionForce = 300;
    [SerializeField] float explosionRadius = 10;
    [SerializeField] float doorShrinkDelay = 0;
    [SerializeField] float doorLifetime = 2;

    System.Action damageDelegate;

    public GameObject doorPrefab = null;

    public override void Tick(BaseCharacter target, EffectStrength strength, float[] customValues)
    {
        target.StartCoroutine(PlayEffect(target, () => base.Tick(target, strength, customValues)));
    }

    public override void TickCustom(BaseCharacter target, List<object> values)
    {
        target.StartCoroutine(PlayEffect(target, () => base.TickCustom(target, values)));
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
        
        Destroy(door, doorLifetime);
    }
}