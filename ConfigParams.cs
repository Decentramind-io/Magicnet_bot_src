using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace AppSettings
{
    public class ConfigParams
    {
        public static string _configFilePath = "";

        public static void setConfigFilePath(string filePath)
        {
            _configFilePath = filePath;
        }

        public static string getValueAsString(string key)
        {
            if (!File.Exists(_configFilePath)) throw new Exception("config file not found(" + _configFilePath + ")");

            JObject o = JObject.Parse(File.ReadAllText(_configFilePath));

            return (string)o[key];
        }

        public static int getValueAsInt(string key)
        {
            if (!File.Exists(_configFilePath)) throw new Exception("config file not found(" + _configFilePath + ")");

            JObject o = JObject.Parse(File.ReadAllText(_configFilePath));

            return (int)o[key];
        }

        public static long getValueAsLong(string key)
        {
            if (!File.Exists(_configFilePath)) throw new Exception("config file not found(" + _configFilePath + ")");

            JObject o = JObject.Parse(File.ReadAllText(_configFilePath));

            return (long)o[key];
        }
    }
}