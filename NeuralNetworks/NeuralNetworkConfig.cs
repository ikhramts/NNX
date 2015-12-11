using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NeuralNetworks
{
    public class NeuralNetworkConfig
    {
        public string NetworkType { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public double[][] Weights { get; set; }

        public int GetSettingInt(string settingName)
        {
            var settingString = GetSettingString(settingName);
            int setting;

            if (!int.TryParse(settingString, out setting))
                throw new NeuralNetworkException("Config setting '" + settingName + "' is not an int.");

            return setting;
        }

        public double GetSettingDouble(string settingName)
        {
            var settingString = GetSettingString(settingName);
            double setting;

            if (!double.TryParse(settingString, out setting))
                throw new NeuralNetworkException("Config setting '" + settingName + "' is not a a double.");

            return setting;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static NeuralNetworkConfig FromJson(string json)
        {
            NeuralNetworkConfig config;

            try
            {
                config =  JsonConvert.DeserializeObject<NeuralNetworkConfig>(json);

                if (config.NetworkType == null)
                    throw new NeuralNetworkException("Config does not contain field 'NetworkType'");
            }
            catch (Exception ex)
            {
                throw new NeuralNetworkException("Could not deserialize config from Json.", ex);
            }

            return config;
        }

        public static NeuralNetworkConfig FromFile(string filePath)
        {
            return FromJson(File.ReadAllText(filePath));
        }

        public void SaveToFile(string filePath)
        {
            File.WriteAllText(filePath, ToJson());
        }

        private string GetSettingString(string settingName)
        {
            string setting;

            if (!Settings.TryGetValue(settingName, out setting))
                throw new NeuralNetworkException("Invalid config setting name: " + settingName);

            return setting;
        }
    }
}
