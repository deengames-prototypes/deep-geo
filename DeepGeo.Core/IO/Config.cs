using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeenGames.DeepGeo.Core.IO
{
    public class Config
    {
        private static Config instance;
        private JObject data;

        public static Config Instance { get { return instance; } }

        public Config(string configFile)
        {
            instance = this;
            new FileWatcher(configFile, (contents) =>
            {
                this.data = JsonConvert.DeserializeObject<JObject>(contents);
            }).Watch();
        }

        public T Get<T>(string key)
        {
            return this.data[key].Value<T>();
        }
    }
}
