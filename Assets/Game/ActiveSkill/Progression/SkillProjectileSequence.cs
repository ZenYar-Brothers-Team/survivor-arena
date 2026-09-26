namespace Game.ActiveSkill
{
    /// <summary>
    /// Per-skill-instance count of projectiles launched by ordinary activations, used by "every N-th projectile"
    /// set replacements (SET-008). Lives as long as the skill instance, so a new run starts from zero.
    /// </summary>
    public sealed class SkillProjectileSequence
    {
        public int Launched { get; private set; }

        /// <summary>Registers one launched projectile and says whether it is the N-th one.</summary>
        public bool NextIsEvery(int everyNth)
        {
            Launched++;
            return everyNth > 0 && Launched % everyNth == 0;
        }
    }
}
