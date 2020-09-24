using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;

namespace VoiceOfAKingdomDiscord.Scripts.Modules
{
    class Config
    {
        public static string Token { get; private set; }
        public static string OwnerID { get; private set; }
        public static bool IsDebug { get; private set; }
        public static string Prefix { get; private set; }

        private const string CONFIG_PATH = "./config.ini";

        private static string ReadConfig(string section, string key)
        {
            IniFile ini = new IniFile(CONFIG_PATH);
            return ini.IniReadValue(section, key);
        }


        public static void ReloadConfig()
        {
            try
            {
                Token = ReadConfig(ConfigSection.App, ConfigKey.Token).Trim();
                OwnerID = ReadConfig(ConfigSection.App, ConfigKey.OwnerID).Trim();
                IsDebug = ReadConfig(ConfigSection.App, ConfigKey.IsDebug).Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase);
                Prefix = ReadConfig(ConfigSection.App, ConfigKey.Prefix);
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
            }
        }

        #region Fake-Enums for INI
        /// <summary>
        /// Very enum-like
        /// Used as an Enum for the Ini file sections
        /// </summary>
        private class ConfigSection
        {
            #region Setup
            private ConfigSection(string value) =>
                Value = value;

            private string Value { get; }
            #endregion

            public static string App { get { return new ConfigSection("App").Value; } }
            public static string Preferences { get { return new ConfigSection("Preferences").Value; } }
        }

        private class ConfigKey
        {
            #region Setup
            private ConfigKey(string value) =>
                Value = value;

            private string Value { get; }
            #endregion

            public static string Token { get { return new ConfigKey("Token").Value; } }
            public static string OwnerID { get { return new ConfigKey("OwnerID").Value; } }
            public static string IsDebug { get { return new ConfigKey("IsDebug").Value; } }
            public static string Prefix { get { return new ConfigKey("Prefix").Value; } }
        }

        #endregion
    }
}
