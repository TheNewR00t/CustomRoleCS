using CommandSystem;
using Exiled.API.Features;
using SidaLand.CustomRoles.API.Features;
using SidaLand.CustomRoles.API;
using System;
using System.Linq;

namespace SidaLand.CustomRoles.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class UseAbility
    {
        public string Command { get; } = "useability";
        public string[] Aliases { get; } = { "special" };
        public string Description { get; } = "Use your custom roles special ability, if available.";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get((CommandSender)sender);
            string abilityName = string.Empty;
            ActiveAbilityCS? ability;

            if (arguments.Count > 0)
            {
                foreach (string s in arguments.Skip(1))
                    abilityName += s;

                if (!CustomAbilityCS.TryGet(abilityName, out CustomAbilityCS? customAbility) || customAbility is null)
                {
                    response = $"Abilidad {abilityName} No existe.";
                    return false;
                }

                if (customAbility is not ActiveAbilityCS activeAbility)
                {
                    response = $"{abilityName} No es una abilidad que se active.";
                    return false;
                }

                ability = activeAbility;
            }
            else
            {
                ability = player.GetSelectedAbility();
            }

            if (ability is null)
            {
                response = "Abilidad no seleccionada.";
                return false;
            }

            if (!ability.CanUseAbility(player, out response, CustomRolesCS.Instance.Config.ActivateOnlySelected))
                return false;
            response = $"{ability.Name} Ha sido usada.";
            ability.UseAbility(player);
            return true;
        }
    }
}
