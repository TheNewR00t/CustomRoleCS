using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SidaLand.CustomRoles.Commands.List
{

    using System;
    using System.Linq;
    using System.Text;

    using CommandSystem;

    using Exiled.API.Features.Pools;
    using Exiled.Permissions.Extensions;
    using PluginAPI.Roles;
    using SidaLand.CustomRoles.API.Features;

    internal sealed class Registered : ICommand
    {
        private Registered()
        {
        }
        public static Registered Instance { get; } = new();
        public string Command { get; } = "registered";
        public string[] Aliases { get; } = { "r" };
        public string Description { get; } = "Gets a list of registered custom roles.";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customrolescs.list.registered"))
            {
                response = "Permiso denegado, requieres: customrolescs.list.registered";
                return false;
            }

            if (CustomRoleCS.Registered.Count == 0)
            {
                response = "No hay roles custom en este servidor.";
                return false;
            }

            StringBuilder builder = StringBuilderPool.Pool.Get().AppendLine();

            builder.Append("[Roles customs registrados (").Append(CustomRoleCS.Registered.Count).AppendLine(")]");

            foreach (CustomRoleCS role in CustomRoleCS.Registered.OrderBy(r => r.Id))
                builder.Append('[').Append(role.Id).Append(". ").Append(role.Name).Append(" (").Append(role.Role).Append(')').AppendLine("]");

            response = StringBuilderPool.Pool.ToStringReturn(builder);
            return true;
        }
    }
}
