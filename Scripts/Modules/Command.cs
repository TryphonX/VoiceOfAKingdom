using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceOfAKingdomDiscord.Scripts.Modules
{
    abstract class Command
    {
        public string Name { get; protected set; }
        public List<string> Abbreviations { get; protected set; }
        public Permission RequiredPermission { get; protected set; }
        
        public virtual void Run(CommandHandler commandHandler)
        {
            CommonScript.DebugLog($"init");
        }
    }
}
