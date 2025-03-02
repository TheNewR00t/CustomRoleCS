using CommandSystem;
using Exiled.API.Features.Pools;
using Exiled.Permissions.Extensions;
using PluginAPI.Roles;
using SidaLand.CustomRoles.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SidaLand.CustomRoles.Commands.List
{
    internal sealed class Abilities : ICommand
    {
        public string Command => "abilities";
        public string[] Aliases => new[] { "a" };
        public string Description => "Lists all abilities on the server.";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customroles.list.abilities"))
            {
                response = "Permiso denegado, requiere: customroles.list.abilities";
                return false;
            }

            if (CustomAbilityCS.Registered.Count == 0)
            {
                response = "No hay abilidades custom en este servidor";
                return false;
            }

            StringBuilder builder = StringBuilderPool.Pool.Get().AppendLine();
            builder.Append("[Abilidades registradas en este server: (").Append(CustomRoleCS.Registered.Count).AppendLine(")]");

            foreach (CustomAbilityCS ability in CustomAbilityCS.Registered.OrderBy(r => r.Name))
                builder.Append('[').Append(ability.Name).Append(" (").Append(ability.Description).Append(')').AppendLine("]");

            response = StringBuilderPool.Pool.ToStringReturn(builder);
            return true;
        }
    }
}
