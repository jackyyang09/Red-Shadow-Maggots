using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SimpleHealth : MonoBehaviour
{
    [SerializeField, Tooltip("The delay between taking damage and the health bar updating to reflect that damage")]
    private float updateDelay = 0.5f;

    [SerializeField,
     Tooltip("The time it takes for the health bar to catch up to the character's current health after taking damage")]
    private float catchupTime = 0.5f;

    [SerializeField, Tooltip("The text element displaying the character's current health")]
    private TMP_Text healthText = null;

    [SerializeField, Tooltip("The image element representing the character's health bar")]
    private Image healthBar = null;

    [SerializeField, Tooltip("The material used for the health bar image")]
    private Material _healthBarMaterial;

    [SerializeField, Tooltip("The character for which the health bar is displaying information"), HideInInspector]
    private BaseCharacter baseCharacter = null;

    [SerializeField, Tooltip("The percentage of damage currently displayed on the health bar"), HideInInspector]
    private float _displayDamagePercent = 0;

    /// <summary>
    /// Initializes the health bar with the character's max health and sets up event listeners for when the character takes damage, sets their health, or heals.
    /// </summary>
    /// <param name="character">The character for which to initialize the health bar.</param>
    public void InitializeWithCharacter(BaseCharacter character)
    {
        //create instance of the material
        healthBar.material = new Material(_healthBarMaterial);

        baseCharacter = character;
        baseCharacter.onTakeDamage += OnTakeDamage;
        baseCharacter.onSetHealth += OnSetHealth;
        baseCharacter.onHeal += OnHeal;

        //set "steps" property (there will be new sector for each 100 health points)
        healthBar.material.SetFloat("_Steps", character.MaxHealth / 100);

        OnSetHealth();
    }

    /// <summary>
    /// Removes event listeners from the character when the health bar is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (!baseCharacter) return;
        baseCharacter.onTakeDamage -= OnTakeDamage;
        baseCharacter.onSetHealth -= OnSetHealth;
        baseCharacter.onHeal -= OnHeal;
    }

    /// <summary>
    /// Handles the display of damage taken on the health bar and updates the health text.
    /// </summary>
    /// <param name="damage">The amount of damage taken by the character.</param>
    private void OnTakeDamage(float damage)
    {
        _displayDamagePercent = damage / baseCharacter.MaxHealth;

        //Display damage can't be higher than the current health
        var currentDisplayHealth = healthBar.material.GetFloat("_Percent");
        _displayDamagePercent = Mathf.Clamp(_displayDamagePercent, 0, currentDisplayHealth);

        healthBar.material.SetFloat("_DamagesPercent", _displayDamagePercent);
        healthBar.material.SetFloat("_Percent", baseCharacter.GetHealthPercent());

        //decrease _damages using DOTween and apply it to the material property
        DOTween.To(() => _displayDamagePercent, x => _displayDamagePercent = x, 0, catchupTime).SetEase(Ease.OutCubic)
            .SetDelay(updateDelay)
            .OnUpdate(() => { healthBar.material.SetFloat("_DamagesPercent", _displayDamagePercent); });


        healthText.text = ((int)baseCharacter.CurrentHealth).ToString();
    }

    /// <summary>
    /// Handles the display of the character's current health on the health bar and updates the health text.
    /// </summary>
    private void OnSetHealth()
    {
        healthBar.material.SetFloat("_Percent", baseCharacter.GetHealthPercent());
        healthText.material.SetFloat("_DamagePercent", 0);
        healthText.text = ((int)baseCharacter.CurrentHealth).ToString();
    }

    /// <summary>
    /// Handles the display of healing on the health bar and updates the health text.
    /// </summary>
    private void OnHeal()
    {
        var tween = healthBar.DOFillAmount(baseCharacter.GetHealthPercent(), catchupTime).OnUpdate(() =>
        {
            healthText.text = ((int)Mathf.Lerp(0, baseCharacter.MaxHealth, healthBar.fillAmount)).ToString();
        }).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            healthBar.material.SetFloat("_Percent", baseCharacter.GetHealthPercent());
            healthText.text = baseCharacter.CurrentHealth.ToString();
        });
    }
}