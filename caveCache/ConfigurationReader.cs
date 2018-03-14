using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace caveCache
{
    interface IConfiguration
    {        
        string ConnectionString { get; }
        string ImageDirectory { get; }
        int MaxMediaSize { get; }
    }

    class ConfigurationReader : IConfiguration
    {
        private Dictionary<string, string> _Config;

        private string GetValue(string key, string defaultValue = null)
        {
            string v;
            if (_Config.TryGetValue(key, out v))
                return v;
            else
                return defaultValue ?? string.Empty;
        }

        public string ConnectionString { get => GetValue("ConnectionString"); }
        public string ImageDirectory { get => GetValue("MediaDirectory", "media"); }
        public int MaxMediaSize
        {
            get
            {
                string v = GetValue("MaxMediaSize"); 
                int iv;
                if (int.TryParse(v, out iv))
                    return iv;
                else
                    return 5 * 1024 * 1024; // 5MB is the default max size of any piece of media                 
            }
        }

        public ConfigurationReader()
        {
            _Config = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText("config.json"));
        }
    }
}
