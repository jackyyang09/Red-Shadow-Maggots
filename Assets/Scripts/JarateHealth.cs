using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class JarateHealth : MonoBehaviour
{
    BaseCharacter character = null;

    [SerializeField] Transform liquidTransform = null;

    [SerializeField] Vector2 liquidRange = new Vector2();

    [SerializeField] float colourTweenDelay = 0.05f;
    [SerializeField] float colourTweenTime = 0.25f;
    [SerializeField] float liquidTweenDelay = 0.25f;
    [SerializeField] float liquidTweenTime = 0.25f;

    [SerializeField] Color tweenColor = Color.red;

    [SerializeField] new Renderer renderer = null;

    int colorProp;

    void Start()
    {
        liquidTransform.localPosition = new Vector3(0, liquidRange.y, 0);
        colorProp = Shader.PropertyToID("_Color");
    }

    private void OnEnable()
    {
        character = GetComponentInParent<BaseCharacter>();
        character.onTakeDamage += TweenLiquid;
    }

    private void OnDisable()
    {
        character.onTakeDamage -= TweenLiquid;
    }

    [ContextMenu(nameof(TweenLiquid))]
    private void TweenLiquid()
    {
        float lerpValue = character.GetHealthPercent();
        liquidTransform.DOLocalMoveY(Mathf.Lerp(liquidRange.x, liquidRange.y, lerpValue), liquidTweenTime).SetDelay(liquidTweenDelay);
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            if (renderer.materials[i].HasProperty(colorProp))
            {
                Color savedColour = renderer.materials[i].GetColor(colorProp);
                renderer.materials[i].SetColor(colorProp, tweenColor);
                renderer.materials[i].DOColor(savedColour, colourTweenTime).SetDelay(colourTweenDelay);
            }
        }
    }
}