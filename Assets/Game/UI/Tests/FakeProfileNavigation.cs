namespace Game.UI.Tests
{
    public sealed class FakeProfileNavigation : IProfileNavigation
    {
        public int Retries, Selections, Quits;
        public void RetryProfileRun() => Retries++;
        public void ReturnToProfileSelection() => Selections++;
        public void QuitProfileRun() => Quits++;
    }
}
