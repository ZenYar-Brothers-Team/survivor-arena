using System;
using Game.Run;
using Game.Traveler;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Game.Telemetry.Tests
{
    public sealed class TravelerTelemetryTests
    {
        [Test]
        public void Export_IncludesScheduleLifeAndPlainPosition_AndDisposeUnsubscribes()
        {
            var run=new RunModel(); run.Start();
            var item=new TravelerSnapshot(Guid.NewGuid(),run.RunId,"FIXTURE-TRAVELER","Fixture","TEST",TravelerRole.Wanderer,
                new Vector2(20,3),50,100,10,70,1.5f,0);
            var source=new TravelerTelemetrySource {Snapshot=new[]{item},Schedule=new[]{new TravelerScheduleEntry(item.Id,10,0,1.5f)}};
            var sink=new FakePlaytestExportSink();
            using var session=new PlaytestSession(run,null,null,null,null,null,new JObject(),sink,()=>0,DateTime.UtcNow,travelers:source);
            source.Publish(new TravelerEvent(item,"Spawned")); session.Export(); session.Tick(); session.PendingExport.GetAwaiter().GetResult();
            var report=JObject.Parse(sink.Report.Json);
            Assert.AreEqual(20,(float)report["producers"]["travelers"][0]["x"]);
            Assert.AreEqual(10,(float)report["producers"]["travelerSchedule"][0]["time"]);
            Assert.AreEqual(1,(int)report["counters"]["traveler.Spawned"]);
            session.Dispose(); source.Publish(new TravelerEvent(item,"Escaped"));
            StringAssert.DoesNotContain("traveler.Escaped",session.Recorder.Snapshot(null,false).Json);
        }
    }
}
