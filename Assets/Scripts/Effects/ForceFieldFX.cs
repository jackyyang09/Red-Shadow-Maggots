using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ForceFieldFX : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] float idleAlpha;
    [SerializeField] float hitAlpha;
    [SerializeField] float tweenTime = 0.5f;

    [Header("Object References")]
    [SerializeField] new Renderer renderer;
    Material mat;

    int alphaStrengthID;

    BaseCharacter character;

    // Start is called before the first frame update
    void Start()
    {
        alphaStrengthID = Shader.PropertyToID("_AlphaStrength");

        mat = renderer.material;
        mat.SetFloat(alphaStrengthID, idleAlpha);
    }

    public void Initialize(BaseCharacter character)
    {
        character.onTakeDamage += OnTakeDamage;
    }

    private void OnDisable()
    {
        if (character)
        {
            character.onTakeDamage -= OnTakeDamage;
        }
    }

    private void OnTakeDamage(float obj)
    {
        if (!mat) return;
        mat.SetFloat(alphaStrengthID, hitAlpha);
        mat.DOFloat(idleAlpha, alphaStrengthID, tweenTime);
    }

    private void OnDestroy()
    {
        mat.DOKill();
        Destroy(mat);
    }
}
