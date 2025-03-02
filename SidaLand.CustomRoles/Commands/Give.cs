
namespace SidaLand.CustomRoles.Commands
{

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CommandSystem;

    using Exiled.API.Features;
    using Exiled.API.Features.Pools;
    using Exiled.Permissions.Extensions;

    using RemoteAdmin;
    using SidaLand.CustomRoles.API.Features;
    using Utils;
    internal sealed class Give : ICommand
    {
        private Give()
        {
        }

        public static Give Instance { get; } = new();
        public string Command { get; } = "give";
        public string[] Aliases { get; } = { "g" };
        public string Description { get; } = "Gives the specified custom role to the indicated player(s).";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (!sender.CheckPermission("customroles.give"))
                {
                    response = "Permission Denied, required: customroles.give";
                    return false;
                }

                if (arguments.Count == 0)
                {
                    response = "give <Custom role name/Custom role ID> [Nickname/PlayerID/UserID/all/*]";
                    return false;
                }

                if (!CustomRoleCS.TryGet(arguments.At(0), out CustomRoleCS? role) || role is null)
                {
                    response = $"Custom role {arguments.At(0)} not found!";
                    return false;
                }

                if (arguments.Count == 1)
                {
                    if (sender is PlayerCommandSender playerCommandSender)
                    {
                        Player player = Player.Get(playerCommandSender);

                        role.AddRole(player);
                        response = $"{role.Name} given to {player.Nickname}.";
                        return true;
                    }

                    response = "Failed to provide a valid player.";
                    return false;
                }

                string identifier = string.Join(" ", arguments.Skip(1));

                switch (identifier)
                {
                    case "*":
                    case "all":
                        List<Player> players = ListPool<Player>.Pool.Get(Player.List);

                        foreach (Player player in players)
                            role.AddRole(player);

                        response = $"Custom role {role.Name} given to all players.";
                        ListPool<Player>.Pool.Return(players);
                        return true;
                    default:
                        break;
                }

                IEnumerable<Player> list = Player.GetProcessedData(arguments, 1);
                if (list.IsEmpty())
                {
                    response = "Cannot find player! Try using the player ID!";
                    return false;
                }

                foreach (Player player in list)
                {
                    role.AddRole(player);
                }

                response = $"Customrole {role.Name} given to {list.Count()} players!";

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                response = "Error";
                return false;
            }
        }
    }
}
