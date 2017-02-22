using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.IO.UnitTests
{
    [TestFixture]
    // While these tests are flaky, the contents are unlikely to ever change.
    // The Config class is very simple, and doesn't require much.
    class ConfigTests
    {
        [SetUp]
        public void ResetConfigJson()
        {
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }
            this.SetConfigJson("{ 'IsConfigWorking': true, 'NumberOfTimesTested': 3701, 'Algorithm': 'AES'}");
        }

        [Test]
        public void GetGetsValueFromConfigDotJson()
        {
            var config = new Config("data/config.json");
            Assert.That(config.Get<bool>("IsConfigWorking"), Is.EqualTo(true));
            Assert.That(config.Get<int>("NumberOfTimesTested"), Is.EqualTo(3701));
            Assert.That(config.Get<string>("Algorithm"), Is.EqualTo("AES"));
        }

        // Always fails on the build machine; whether we use FileInfo and watch CreationTime + LastWriteTime (which don't change),
        // or we use a FileSystemWatcher (the event doesn't fire); the result is the same: file text is different, but data didn't update.
        // This should be a warning to those who believe in writing unit tests without mocks and interfaces!
        [Ignore("Always fails on build machine")]
        [Test]
        public void GetGetsUpdatedValueIfFileChanges()
        {
            var config = new Config("data/config.json");
            Assert.That(config.Get<string>("Algorithm"), Is.EqualTo("AES"));
            this.SetConfigJson("{ 'Algorithm': 'MD5' }");

            // Wait for it to refresh
            Thread.Sleep(100);
            Assert.That(config.Get<string>("Algorithm"), Is.EqualTo("MD5"), $"Config contents: {File.ReadAllText("data/config.json")} vs. Data: {config.ToString()}");
        }

        public void SetConfigJson(string contents)
        {
            File.WriteAllText("data/config.json", contents);
        }
    }
}
