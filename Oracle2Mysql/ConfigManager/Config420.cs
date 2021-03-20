using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oracle2Mysql.ConfigManager
{
    class Config420
    {
        Program Base;
        ConfigInfo Info;
        string file = @".\Config.json";
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public Config420 (Program bas)
        {
            Base = bas;
            if (!File.Exists(file))
            {
                Info = new ConfigInfo();
                SaveConfig();
            }
            else
            {
                Info = JsonSerializer.Deserialize<ConfigInfo>(File.ReadAllText(file), options);   
               
            }
        }

        public ConfigInfo Config()
        {
            return Info;
        }



        public void SaveConfig()
        {
            if (File.Exists(file))
                File.Delete(file);
            File.WriteAllText(file, JsonSerializer.Serialize(Info, options)) ;

        }
    }
}
