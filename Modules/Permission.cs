using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Permission
    {
        public string Name { get; private set; }
        public PermissionPower Power { get; private set; }

        public static PermissionPower GetUserPermissionPower(SocketUser user)
        {
            // Owner
            if (Config.OwnerID == user.Id)
                return OwnerPermission.Power;

            return AnyonePermission.Power;

        }

        public static Permission AnyonePermission { get { return new Permission(PermissionPower.Anyone); } }

        public static Permission OwnerPermission { get { return new Permission(PermissionPower.Owner); } }

        private Permission(PermissionPower power)
        {
            Name = power.ToString();
            Power = power;
        }

    }
    public enum PermissionPower
    {
        Anyone,
        Owner
    }
}
