// -----------------------------------------------------------------------
// <copyright file="PlayerHandlers.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace SidaLand.CustomRoles.Events
{
    using Exiled.Events.EventArgs.Player;

    /// <summary>
    /// Handles general events for players.
    /// </summary>
    public class PlayerHandlers
    {
        private readonly CustomRolesCS plugin;

        public PlayerHandlers(CustomRolesCS plugin)
        {
            this.plugin = plugin;
        }

        internal void OnSpawningRagdoll(SpawningRagdollEventArgs ev)
        {
            if (plugin.StopRagdollPlayers.Contains(ev.Player))
            {
                ev.IsAllowed = false;
                plugin.StopRagdollPlayers.Remove(ev.Player);
            }
        }
    }
}