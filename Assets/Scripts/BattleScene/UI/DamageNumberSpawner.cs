using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

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

    private void OnEnable()
    {
        BaseCharacter.OnCharacterReceivedDamage += OnCharacterReceivedDamage;
        BaseCharacter.OnCharacterConsumedHealth += OnCharacterReceivedDamage;
    }

    private void OnDisable()
    {
        BaseCharacter.OnCharacterReceivedDamage -= OnCharacterReceivedDamage;
        BaseCharacter.OnCharacterConsumedHealth -= OnCharacterReceivedDamage;
    }

    private void OnCharacterReceivedDamage(BaseCharacter character, DamageStruct damage)
    {
        if (damage.TrueDamage > 0)
        {
            if (damage.ShieldedDamage > 0)
            {
                SpawnDamageNumberAt(character.transform, damage, (int)damage.ShieldedDamage);
                SpawnDamageNumberDelayed(character.transform, damage, 0);
            }
            else
            {   
                SpawnDamageNumberAt(character.transform, damage, 0);
            }
        }
        else if (damage.ShieldedDamage > 0)
        {
            SpawnDamageNumberAt(character.transform, damage, (int)damage.ShieldedDamage);
        }
    }

    public void SpawnDamageNumberAt(Transform t, DamageStruct damage, int shieldedDamage)
    {
        GameObject number = Instantiate(damageNumberPrefabs[(int)damage.Effectivity], transform.GetChild(0));
        var dmgNumber = number.GetComponent<DamageNumber>();

        dmgNumber.Initialize(cam, t, (int)damage.TrueDamage, shieldedDamage);
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