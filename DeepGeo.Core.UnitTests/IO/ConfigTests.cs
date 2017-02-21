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
    class ConfigTests
    {
        [SetUp]
        public void ResetConfigJson()
        {
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

        [Test]
        public void GetGetsUpdatedValueIfFileChanges()
        {
            var config = new Config("data/config.json");
            File.WriteAllText("data/config.json", "{ 'Algorithm': 'MD5' }");
            // This is not a great unit test, sorry. We just need basic testing here.
            Thread.Sleep(100);
            Assert.That(config.Get<string>("Algorithm"), Is.EqualTo("MD5"));
        }

        public void SetConfigJson(string contents)
        {
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }
            File.WriteAllText("data/config.json", contents);
        }
    }
}
