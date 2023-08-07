using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ForceFieldFX : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] Vector2 alphaRange;
    [SerializeField] Vector2 fresnelRange;
    [SerializeField] float tweenTime = 0.5f;

    [Header("Object References")]
    [SerializeField] new Renderer renderer;
    Material mat;

    int alphaStrengthID;
    int fresnelID;

    BaseCharacter character;

    // Start is called before the first frame update
    void Start()
    {
        alphaStrengthID = Shader.PropertyToID("_AlphaStrength");
        fresnelID = Shader.PropertyToID("_FresnelPower");

        mat = renderer.material;
        mat.SetFloat(alphaStrengthID, alphaRange.x);
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
        mat.SetFloat(alphaStrengthID, alphaRange.y);
        mat.SetFloat(fresnelID, fresnelRange.y);
        mat.DOFloat(alphaRange.x, alphaStrengthID, tweenTime);
        mat.DOFloat(fresnelRange.x, fresnelID, tweenTime);
    }

    private void OnDestroy()
    {
        mat.DOKill();
        Destroy(mat);
    }
}
