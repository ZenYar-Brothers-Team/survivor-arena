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
        /// <summary>Saved progress toward a numeric unlock; zero for other rules or before load.</summary>
        long UnlockProgress(string id);
        int Level(string upgrade, string character = null);
        string PurchaseLockReason(string id, string character = null);
        /// <summary>DECISION-0064: when true, <see cref="Modifier"/> grants nothing although purchased levels are kept.</summary>
        bool UpgradesDisabled { get; }
        /// <summary>Null when the upgrades switch can be changed now (profile ready, between runs).</summary>
        string UpgradesToggleLockReason { get; }
        Task<bool> SetUpgradesDisabledAsync(bool disabled);
        CharacterStatModifier Modifier(string character);
        int ExtraRerolls(string character);
        int ExtraBanishes(string character);
        long Invested(string character);
        string RefundLockReason(string character);
        Task<bool> RefundAsync(string character, long expectedInvestment);
        Task LoadAsync();
        Task ResetAsync();
        Task<bool> PurchaseAsync(string id, int expectedLevel, string character = null);
        Task<bool> ApplyAsync(RunOutcome outcome, bool started);
        Task<bool> RetrySaveAsync();
        void SetRunActive(bool active);
        /// <summary>Development-only: unlocks every catalog entry of the given kinds without spending currency.</summary>
        Task<bool> UnlockAllForDevelopmentAsync(params string[] kinds);
        /// <summary>Development-only: adds currency to the profile and saves it.</summary>
        Task<bool> GrantCurrencyForDevelopmentAsync(long amount);
        /// <summary>Development-only: replaces the profile with a new one; previous files are preserved by the store.</summary>
        Task<bool> ResetForDevelopmentAsync();
    }
}
