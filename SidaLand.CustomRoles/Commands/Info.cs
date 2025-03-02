using CommandSystem;
using Exiled.API.Features.Pools;
using Exiled.Permissions.Extensions;
using SidaLand.CustomRoles.API.Features;
using System;
using System.Text;

namespace SidaLand.CustomRoles.Commands
{
    internal sealed class Info : ICommand
    {
        private Info()
        {
        }
        public static Info Instance { get; } = new();
        public string Command { get; } = "info";
        public string[] Aliases { get; } = { "i" };
        public string Description { get; } = "Recibes mas informacion sobre un custom role";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("customrolescs.info"))
            {
                response = "Permiso denegado, requieres: customrolescs.info";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "info [Custom role name/Custom role ID]";
                return false;
            }

            if ((!(uint.TryParse(arguments.At(0), out uint id) && CustomRoleCS.TryGet(id, out CustomRoleCS? role)) && !CustomRoleCS.TryGet(arguments.At(0), out role)) || role is null)
            {
                response = $"{arguments.At(0)} no es un customrole valido.";
                return false;
            }

            StringBuilder builder = StringBuilderPool.Pool.Get().AppendLine();

            builder.Append("<color=#E6AC00>-</color> <color=#00D639>").Append(role.Name)
                .Append("</color> <color=#05C4E8>(").Append(role.Id).Append(")</color>")
                .Append("- ").AppendLine(role.Description)
                .AppendLine(role.Role.ToString())
                .Append("- Health: ").AppendLine(role.MaxHealth.ToString()).AppendLine();

            response = StringBuilderPool.Pool.ToStringReturn(builder);
            return true;
        }
    }
}
