using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace caveCache
{
    interface IConfiguration
    {
        IReadOnlyDictionary<string, string> Config { get; }
    }

    class ConfigurationReader : IConfiguration
    {
        private Dictionary<string, string> _Config;
        public IReadOnlyDictionary<string, string> Config { get { return _Config; } }

        public ConfigurationReader()
        {
            _Config = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText("config.json"));            
        }
    }
}
