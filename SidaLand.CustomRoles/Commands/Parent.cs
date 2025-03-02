using CommandSystem;
using System;

namespace SidaLand.CustomRoles.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Parent : ParentCommand
    {
        public Parent()
        {
            LoadGeneratedCommands();
        }
        public override string Command { get; } = "customrolescs";
        public override string[] Aliases { get; } = { "cr", "crs" };
        public override string Description { get; } = string.Empty;
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(Give.Instance);
            RegisterCommand(Info.Instance);
            RegisterCommand(List.List.Instance);
        }
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Invalid subcommand! Available: give, info, list";
            return false;
        }
    }
}
