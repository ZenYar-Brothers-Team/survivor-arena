using System;
using System.Collections.Generic;
using Game.Content;
using Game.Enemy;
using Game.Progression;
using Game.Run;
using Game.Traveler;
using Game.UI;
namespace Game.Bootstrap
{
    /// <summary>Translates feature events without changing feature state; owns required special-kill accounting.</summary>
    public sealed class RunNotificationBinding : IDisposable, IRunOutcomeContributor
    {
        private readonly RunModel _run;
        private readonly PlayerExperienceRuntime _xp;
        private readonly LevelUpDraftRuntime _draft;
        private readonly IBossEncounterRuntime _boss;
        private readonly ITravelerRuntime _travelers;
        private readonly IReadOnlyList<SetDefinition> _sets;
        private readonly NotificationQueue _notifications;
        private readonly HashSet<ContentId> _recipes=new HashSet<ContentId>();
        private int _kills;
        public string Key => "special-enemy-kills";
        public RunNotificationBinding(RunModel run,PlayerExperienceRuntime xp,LevelUpDraftRuntime draft,
            IBossEncounterRuntime boss,ITravelerRuntime travelers,IReadOnlyList<SetDefinition> sets,NotificationQueue notifications)
        {
            _run=run;_xp=xp;_draft=draft;_boss=boss;_travelers=travelers;_sets=sets;_notifications=notifications;
            run.RegisterOutcomeContributor(this);xp.LevelUp+=Level;draft.SelectionApplied+=Selected;
            if(boss!=null)boss.LifeEvent+=Boss;
            if(travelers!=null)travelers.LifeEvent+=Traveler;
        }
        private void Level(int level) => _notifications?.Push(NotificationMessage.LevelUp(level));
        private void Selected(BuildSelectionResult result)
        {
            if(result.WasNewEntry&&result.Entry.Definition.Kind==BuildEntryKind.Set)_notifications?.Push(new NotificationMessage(NotificationKind.SetAcquired,result.Entry.Definition.DisplayName));
            foreach(var set in _sets)
                if(!_recipes.Contains(set.Id)&&set.IsRecipeFulfilled(_draft.Build)) { _recipes.Add(set.Id);_notifications?.Push(new NotificationMessage(NotificationKind.SetRecipeReady,set.DisplayName)); }
        }
        private void Boss(EnemyLifeEvent e)
        {
            if(e.Kind==EnemyLifeEventKind.Spawned&&e.Category==EnemyCategory.Boss)_notifications?.Push(new NotificationMessage(NotificationKind.BossIncoming,BossName(e)));
            if(e.Kind==EnemyLifeEventKind.Died)_kills++;
        }
        private string BossName(EnemyLifeEvent e)
        {
            if(_boss.FinalBoss!=null&&_boss.FinalBoss.LifeId==e.LifeId)return _boss.FinalDefinition?.DisplayName??"";
            if(_boss.MidBoss!=null&&_boss.MidBoss.LifeId==e.LifeId)return _boss.MidDefinition?.DisplayName??"";
            return "";
        }
        private void Traveler(TravelerEvent e)
        {
            if(e.Outcome=="Spawned")_notifications?.Push(new NotificationMessage(NotificationKind.TravelerAppeared,e.Traveler.Name));
            if(e.Outcome=="Escaped")_notifications?.Push(new NotificationMessage(NotificationKind.TravelerEscaped,e.Traveler.Name));
            if(e.Outcome=="Killed")_kills++;
        }
        public RunOutcomeContribution Capture() => new RunOutcomeContribution(kills:_kills);
        public void Dispose()
        {
            _run.UnregisterOutcomeContributor(this);_xp.LevelUp-=Level;_draft.SelectionApplied-=Selected;
            if(_boss!=null)_boss.LifeEvent-=Boss;
            if(_travelers!=null)_travelers.LifeEvent-=Traveler;
        }
    }
}
