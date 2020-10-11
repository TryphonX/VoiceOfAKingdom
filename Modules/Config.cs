using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Config
    {
        public static string Token { get; private set; }
        public static ulong OwnerID { get; private set; }
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
                Token = ReadConfig(ConfigSection.App, ConfigKey.Token);
                OwnerID = ulong.Parse(ReadConfig(ConfigSection.App, ConfigKey.OwnerID));
                IsDebug = ReadConfig(ConfigSection.App, ConfigKey.IsDebug).Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase);
                Prefix = ReadConfig(ConfigSection.Preferences, ConfigKey.Prefix);
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
            }
        }

        #region Fake Enums
        /// <summary>
        /// Very enum-like
        /// Used as an Enum for the Ini file sections
        /// </summary>
        private class ConfigSection
        {
            public static string App => "App";
            public static string Preferences => "Preferences";
        }

        private class ConfigKey
        {
            public static string Token => "Token";
            public static string OwnerID => "OwnerID";
            public static string IsDebug => "IsDebug";
            public static string Prefix => "Prefix";
            public static string GamesCategoryID => "GamesCategoryID";
        }

        #endregion
    }
}
