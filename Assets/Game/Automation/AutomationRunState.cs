namespace Game.Automation
{
    /// <summary>One-run lifecycle. Only AutomationRunHost changes this state; terminal states do not resume.</summary>
    public enum AutomationRunState
    {
        WaitingForProfile,
        SelectRun,
        Running,
        ResolveDraft,
        AwaitResultSave,
        Completed,
        Stopped,
        Failed
    }
}
