using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
namespace Game.Meta.Tests
{
    public sealed class FileProfileStoreIntegrationTests
    {
        [Test] public async Task AtomicWrite_RetainsPreviousBackupAndNoTemporaryFile()
        {
            var directory=Path.Combine(Path.GetTempPath(),"survivor-profile-test-"+Guid.NewGuid().ToString("N"));
            var path=Path.Combine(directory,"profile.json");
            try
            {
                var store=new FileProfileStore(path);await store.WriteAsync("first");await store.WriteAsync("second");
                Assert.AreEqual("second",await store.ReadAsync(false));Assert.AreEqual("first",await store.ReadAsync(true));
                Assert.IsFalse(File.Exists(path+".tmp"));await store.PreserveAndResetAsync("reset");
                Assert.AreEqual(2,Directory.GetFiles(directory,"*.preserved-*").Length);
            }
            finally { if(Directory.Exists(directory)) Directory.Delete(directory,true); }
        }
    }
}
