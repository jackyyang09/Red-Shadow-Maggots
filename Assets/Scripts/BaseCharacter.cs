using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    [SerializeField]
    CharacterObject characterReference;

    [SerializeField]
    float health;

    [SerializeField]
    float maxHealth;

    [SerializeField]
    float attack;

    [SerializeField]
    AttackRange range;

    [Header("Object References")]

    [SerializeField]
    SpriteRenderer sprite;

    public System.Action onTakeDamage;

    [ContextMenu("Apply Reference")]
    public void ApplyReferenceProperties()
    {
        if (characterReference == null) return;
        maxHealth = characterReference.maxHealth;
        attack = characterReference.attack;
        sprite.sprite = characterReference.sprite;
        range = characterReference.range;

        health = maxHealth;
    }

    public void SetReference(CharacterObject newRef)
    {
        characterReference = newRef;
    }

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public virtual void CalculateAttackDamage()
    {

    }

    public virtual float CalculateDefenseDamage(float damage)
    {
        return damage;
    }

    public virtual void TakeDamage(float damage)
    {
        float trueDamage = CalculateDefenseDamage(damage);
        health = Mathf.Clamp(health - trueDamage, 0, maxHealth);
    }

    public virtual void Die()
    {

    }

    public float GetHealthPercent()
    {
        return health / maxHealth;
    }
}
