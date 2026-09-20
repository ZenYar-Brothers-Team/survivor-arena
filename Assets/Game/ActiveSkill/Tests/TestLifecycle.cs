using System.Reflection;

namespace Game.ActiveSkill.Tests
{
    internal static class TestLifecycle
    {
        public static void InvokeAwake(object behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }
    }
}
