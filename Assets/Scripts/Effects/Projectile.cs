using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    [SerializeField] float flightTime;

    [SerializeField] Renderer[] renderers;

    [SerializeField] GameObject impactPrefab;
    GameObject impactInstance;

    BaseCharacter character;
    Transform parent;

    public void InitializeWithTarget(BaseCharacter target, Transform parent)
    {
        foreach (var r in renderers)
        {
            r.enabled = false;
        }
        character = target;
        this.parent = parent;

        if (impactPrefab)
        {
            impactInstance = Instantiate(impactPrefab);
            impactInstance.gameObject.SetActive(false);
        }
    }

    public void Launch()
    {
        foreach (var r in renderers)
        {
            r.enabled = true;
        }
        transform.rotation = parent.rotation;
        transform.position = parent.position;
        transform.DOMove(character.EffectRegion.position, flightTime).SetEase(Ease.Linear)
            .OnComplete(DestroySelf);
    }

    private void DestroySelf()
    {
        impactInstance.transform.position = transform.position;
        impactInstance.transform.rotation = transform.rotation;
        impactInstance.SetActive(true);
        Destroy(impactInstance, 2);
        Destroy(gameObject);
    }
}