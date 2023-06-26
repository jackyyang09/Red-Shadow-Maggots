using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum DamageType
{
    Light,
    Medium,
    Heavy,
    Heal
}

public class DamageNumberSpawner : BasicSingleton<DamageNumberSpawner>
{
    [SerializeField] Camera cam;

    [SerializeField] float numberLifetime = 1.5f;
    [SerializeField] float numberFadeDelay = 1;
    [SerializeField] float numberFadeTime = 0.5f;

    [SerializeField] float delayedSpawnDelay = 0.5f;

    [SerializeField] float verticalMovement = 0.5f;

    [SerializeField] GameObject[] damageNumberPrefabs;

    public void SpawnDamageNumberAt(Transform t, DamageStruct damage, int shieldedDamage)
    {
        GameObject number = Instantiate(damageNumberPrefabs[(int)damage.effectivity], transform.GetChild(0));
        var dmgNumber = number.GetComponent<DamageNumber>();

        dmgNumber.Initialize(cam, t, (int)damage.damage, shieldedDamage);
    }

    public void SpawnDamageNumberDelayed(Transform t, DamageStruct damage, int shieldedDamage)
    {
        StartCoroutine(DelayedSpawn(t, damage, shieldedDamage, delayedSpawnDelay));
    }

    IEnumerator DelayedSpawn(Transform t, DamageStruct damage, int shieldedDamage, float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnDamageNumberAt(t, damage, shieldedDamage);
    }
}