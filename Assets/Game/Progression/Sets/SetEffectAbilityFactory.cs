using System;
namespace Game.Progression
{
    public sealed class SetEffectAbilityFactory : ISetExtraAbilityFactory
    {
        private readonly ISetEffectHost _host;
        public SetEffectAbilityFactory(ISetEffectHost host) { _host = host ?? throw new ArgumentNullException(nameof(host)); }
        public ISetExtraAbility Create(SetDefinition definition) => new SetEffectAbility(definition, _host);
    }
}
