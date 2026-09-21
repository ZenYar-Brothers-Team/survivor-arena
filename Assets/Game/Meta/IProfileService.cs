using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Character;
using Game.Content;
using Game.Run;
namespace Game.Meta
{
    public interface IProfileService
    {
        event Action Changed;
        ProfileState State { get; }
        string Message { get; }
        long Currency { get; }
        MetaCatalog Catalog { get; }
        bool RunActive { get; }
        bool CanStart { get; }
        bool CanReset { get; }
        MetaRunReceipt LastReceipt { get; }
        bool IsUnlocked(string id);
        int Level(string upgrade, string character = null);
        string PurchaseLockReason(string id, string character = null);
        CharacterStatModifier Modifier(string character);
        Task LoadAsync();
        Task ResetAsync();
        Task<bool> PurchaseAsync(string id, int expectedLevel, string character = null);
        Task<bool> ApplyAsync(RunOutcome outcome, bool started);
        Task<bool> RetrySaveAsync();
        void SetRunActive(bool active);
    }
}
