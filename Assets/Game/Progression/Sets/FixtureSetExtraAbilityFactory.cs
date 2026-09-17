namespace Game.Progression
{
    public sealed class FixtureSetExtraAbilityFactory : ISetExtraAbilityFactory
    {
        public ISetExtraAbility Create(SetDefinition definition)
        {
            return new FixtureSetExtraAbility();
        }
    }
}
