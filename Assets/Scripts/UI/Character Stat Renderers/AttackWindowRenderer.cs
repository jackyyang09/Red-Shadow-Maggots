public class AttackWindowRenderer : BaseStatRenderer
{
    public override void RenderState(PlayerSave.MaggotState state, CharacterObject character, bool isEnemy)
    {
        ValueText = character.attackLeniency.FormatPercentage();
    }

    public override void RenderInBattle(BaseCharacter character)
    {
        var modifier = character.AttackLeniencyModifier.FormatPercentage();
        ValueText = character.AttackLeniencyModified.FormatPercentage();
        if (character.AttackLeniencyModifier > 0)
        {
            ValueText += RenderPositiveMod(modifier);
        }
        else if (character.AttackLeniencyModifier < 0)
        {
            ValueText += RenderNegativeMod(modifier);
        }
    }
}