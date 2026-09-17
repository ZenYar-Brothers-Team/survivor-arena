namespace Game.Progression
{
    public interface ISetExtraAbilityFactory
    {
        ISetExtraAbility Create(SetDefinition definition);
    }
}
