namespace SidaLand.CustomRoles.API.Features
{

    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using Exiled.API.Features;
    using Exiled.API.Features.Pools;
    using Exiled.API.Features.Roles;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;

    using MEC;

    using PlayerRoles.FirstPersonControl;
    using SidaLand.CustomRoles.API.Features.Enums;
    using SidaLand.CustomRoles.API;

    internal class KeypressActivatorCS
    {
        private readonly Dictionary<Player, int> altTracker = DictionaryPool<Player, int>.Pool.Get();
        private readonly Dictionary<Player, CoroutineHandle> coroutineTracker = DictionaryPool<Player, CoroutineHandle>.Pool.Get();

        internal KeypressActivatorCS()
        {
            Exiled.Events.Handlers.Player.TogglingNoClip += OnTogglingNoClip;
            Exiled.Events.Handlers.Server.EndingRound += OnEndingRound;
        }

        ~KeypressActivatorCS()
        {
            Exiled.Events.Handlers.Player.TogglingNoClip -= OnTogglingNoClip;
            Exiled.Events.Handlers.Server.EndingRound -= OnEndingRound;
            DictionaryPool<Player, int>.Pool.Return(altTracker);
            DictionaryPool<Player, CoroutineHandle>.Pool.Return(coroutineTracker);
        }

        private void OnTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            if (ev.Player.IsNoclipPermitted)
                return;

            if (!ActiveAbilityCS.AllActiveAbilities.ContainsKey(ev.Player))
                return;

            if (!altTracker.ContainsKey(ev.Player))
                altTracker.Add(ev.Player, 0);

            altTracker[ev.Player]++;

            if (!coroutineTracker.ContainsKey(ev.Player))
                coroutineTracker.Add(ev.Player, default);

            if (!coroutineTracker[ev.Player].IsRunning)
                coroutineTracker[ev.Player] = Timing.RunCoroutine(ProcessAltKey(ev.Player));
        }

        private void OnEndingRound(EndingRoundEventArgs ev)
        {
            altTracker.Clear();
            foreach (CoroutineHandle handle in coroutineTracker.Values)
                Timing.KillCoroutines(handle);
            coroutineTracker.Clear();
        }

        private IEnumerator<float> ProcessAltKey(Player player)
        {
            yield return Timing.WaitForSeconds(0.25f);

            if (!altTracker.TryGetValue(player, out int pressCount))
                yield break;

            Log.Debug($"{player.Nickname}: {pressCount} {(player.Role is FpcRole fpc ? fpc.MoveState : false)}");
            KeyPressTriggerTypeCS type = pressCount switch
            {
                1 when player.Role is FpcRole { MoveState: PlayerMovementState.Sneaking } => KeyPressTriggerTypeCS.DisplayInfo,
                1 => KeyPressTriggerTypeCS.Activate,
                2 when player.Role is FpcRole { MoveState: PlayerMovementState.Sneaking } => KeyPressTriggerTypeCS.SwitchBackward,
                2 => KeyPressTriggerTypeCS.SwitchForward,
                _ => KeyPressTriggerTypeCS.None,
            };

            bool preformed = PreformAction(player, type, out string response);
            switch (preformed)
            {
                case true when type == KeyPressTriggerTypeCS.Activate:
                    string[] split = response.Split('|');
                    response = string.Format($"Ability {0} has been activated.\n{1}", split);
                    break;
                case true when type is KeyPressTriggerTypeCS.SwitchBackward or KeyPressTriggerTypeCS.SwitchForward:
                    response = string.Format($"abilidad seleccionada {0}", response);
                    break;
                case false:
                    response = string.Format($"Failed to preform action: {0}", response);
                    break;
            }

            float dur = type switch
            {
                KeyPressTriggerTypeCS.Activate when preformed => 5,
                KeyPressTriggerTypeCS.SwitchBackward or KeyPressTriggerTypeCS.SwitchForward when preformed => 5,
                _ => 5,
            };

            player.ShowHint(response, dur);
            altTracker[player] = 0;
        }

        private bool PreformAction(Player player, KeyPressTriggerTypeCS type, out string response)
        {
            ActiveAbilityCS? selected = player.GetSelectedAbility();
            if (type == KeyPressTriggerTypeCS.Activate)
            {
                if (selected is null)
                {
                    response = "No selected abilities.";
                    return false;
                }

                if (!selected.CanUseAbility(player, out response, CustomRolesCS.Instance.Config.ActivateOnlySelected))
                    return false;
                response = $"{selected.Name}|{selected.Description}";
                selected.UseAbility(player);
                return true;
            }

            if (type is KeyPressTriggerTypeCS.SwitchForward or KeyPressTriggerTypeCS.SwitchBackward)
            {
                List<ActiveAbilityCS> abilities = ListPool<ActiveAbilityCS>.Pool.Get(player.GetActiveAbilities());

                if (abilities.Count == 0)
                {
                    response = "No abilities to switch to.";
                    return false;
                }

                if (selected is not null)
                {
                    int index = abilities.IndexOf(selected);
                    int mod = type == KeyPressTriggerTypeCS.SwitchForward ? 1 : -1;
                    if (index + mod > abilities.Count - 1)
                        index = 0;
                    else if (index + mod < 0)
                        index = abilities.Count - 1;
                    else
                        index += mod;

                    if (index < 0 || index > abilities.Count - 1)
                    {
                        Log.Warn("Joker can't do math.");
                        response = "Jokey did a fucky wucky wif his maths";
                        return false;
                    }

                    if (abilities.Count <= 1)
                    {
                        response = "No abilities to switch to.";
                        return false;
                    }

                    selected.UnSelectAbility(player);
                    abilities[index].SelectAbility(player);
                    response = $"{abilities[index].Name}";
                    return true;
                }

                abilities[0].SelectAbility(player);
                response = $"{abilities[0].Name}";
                return true;
            }

            if (type == KeyPressTriggerTypeCS.DisplayInfo)
            {
                if (selected is null)
                {
                    response = "No ability selected.";
                    return false;
                }

                StringBuilder builder = StringBuilderPool.Pool.Get();
                builder.AppendLine(selected.Name);
                builder.AppendLine(selected.Description);
                builder.AppendLine(selected.Duration.ToString(CultureInfo.InvariantCulture)).Append(" (").Append(selected.Cooldown).Append(") ").AppendLine();
                builder.AppendLine($"Usable: ").Append(selected.CanUseAbility(player, out string res));
                if (!string.IsNullOrEmpty(res))
                    builder.Append(" [").Append(res).Append("]");
                response = StringBuilderPool.Pool.ToStringReturn(builder);
                return true;
            }

            response = $"Invalid action: {type}.";
            return false;
        }
    }
}
