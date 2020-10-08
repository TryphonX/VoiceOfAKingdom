using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    abstract class Command
    {
        public string Name { get; protected set; }
        public List<string> Abbreviations { get; protected set; } = new List<string>();
        public Permission RequiredPermission { get; protected set; } = Permission.AnyonePermission;
        public string Description { get; protected set; }
        public Dictionary<string, string> Parameters { get; protected set; }
        
        public virtual void Run(CommandHandler cmdHandler)
        {
            CommonScript.DebugLog($"init", true);
        }
    }
}
