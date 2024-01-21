public struct DamageStruct
{
    /// <summary>
    /// The attacking character, if any
    /// </summary>
    public BaseCharacter Source;

    /// <summary>
    /// Final damage passed to the character for damage calculation
    /// </summary>
    public float TrueDamage;

    /// <summary>
    /// Amount of damage absorbed by a shield
    /// </summary>
    public float ShieldedDamage;

    /// <summary>
    /// The sum of TrueDamage and ShieldedDamage
    /// </summary>
    public float TotalDamage => TrueDamage + ShieldedDamage;

    /// <summery>
    /// If true, target evaded damage by means of a game effect
    /// </summary>
    public bool Evaded;

    /// <summary>
    /// QuickTime value
    /// </summary>
    public float QTEValue;
    public float QTEPlayer;
    public float QTEEnemy;

    /// <summary>
    /// Amount of damage to deal relative to attack stat
    /// </summary>
    public float Percentage;

    public float BarFill;
    public float CritDamageModifier;
    public DamageType DamageType;
    public DamageEffectivess Effectivity;
    public QuickTimeBase.QTEResult QTEResult;
    public bool IsCritical;
    public bool IsSuperCritical;
    public bool IsAOE;
    public int ChargeLevel;
}
