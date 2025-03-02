using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SidaLand.CustomRoles.Commands.List
{
    internal sealed class List : ParentCommand
    {
        private List()
        {
            LoadGeneratedCommands();
        }
        public static List Instance { get; } = new();
        public override string Command { get; } = "list";
        public override string[] Aliases { get; } = { "l" };
        public override string Description { get; } = "Gets a list of all currently registered custom roles.";
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(Registered.Instance);
            RegisterCommand(new Abilities());
        }
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.IsEmpty() && TryGetCommand(Registered.Instance.Command, out ICommand command))
            {
                command.Execute(arguments, sender, out response);
                response += $"\nTo view all abilities registered use command: {string.Join(" ", arguments.Array)} abilities";
                return true;
            }

            response = "Invalid subcommand! Available: registered, abilities";
            return false;
        }
    }
}
