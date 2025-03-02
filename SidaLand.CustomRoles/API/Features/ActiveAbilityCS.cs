
namespace SidaLand.CustomRoles.API.Features
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;

    using MEC;
    using SidaLand.CustomRoles.API.Features.Enums;
    using YamlDotNet.Serialization;
    public abstract class ActiveAbilityCS : CustomAbilityCS
    {
        public static Dictionary<Player, HashSet<ActiveAbilityCS>> AllActiveAbilities { get; } = new();
        public abstract float Duration { get; set; }
        public abstract float Cooldown { get; set; }
        [YamlIgnore]
        public virtual Func<bool>? CanUseOverride { get; set; }
        [YamlIgnore]
        public Dictionary<Player, DateTime> LastUsed { get; } = new();

        [YamlIgnore]
        public HashSet<Player> ActivePlayers { get; } = new();

        [YamlIgnore]
        public HashSet<Player> SelectedPlayers { get; } = new();
        public void UseAbility(Player player)
        {
            ActivePlayers.Add(player);
            LastUsed[player] = DateTime.Now;
            AbilityUsed(player);
            Timing.CallDelayed(Duration, () => EndAbility(player));
        }

        public void EndAbility(Player player)
        {
            if (!ActivePlayers.Contains(player))
                return;

            ActivePlayers.Remove(player);
            AbilityEnded(player);
        }


        public void SelectAbility(Player player)
        {
            if (!SelectedPlayers.Contains(player))
            {
                SelectedPlayers.Add(player);
                Selected(player);
            }
        }

        public void UnSelectAbility(Player player) 
        {
            if (SelectedPlayers.Contains(player))
            {
                SelectedPlayers.Remove(player);
                if (Check(player, CheckTypeCS.Active))
                    EndAbility(player);
                Unselected(player);
            }
        }
        public override bool Check(Player player) => Check(player, CheckTypeCS.Active);
        public virtual bool Check(Player player, CheckTypeCS type)
        {
            if (player is null)
                return false;
            bool result = type switch
            {
                CheckTypeCS.Active => ActivePlayers.Contains(player),
                CheckTypeCS.Selected => SelectedPlayers.Contains(player),
                CheckTypeCS.Available => Players.Contains(player),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return result;
        }

        public virtual bool CanUseAbility(Player player, out string response, bool selectedOnly = false)
        {
            if (CanUseOverride is not null)
            {
                response = string.Empty;
                return CanUseOverride.Invoke();
            }

            if (selectedOnly && !SelectedPlayers.Contains(player))
            {
                response = $"{Name} not selected.";
                return false;
            }

            if (!LastUsed.ContainsKey(player))
            {
                response = string.Empty;
                return true;
            }

            DateTime usableTime = LastUsed[player] + TimeSpan.FromSeconds(Cooldown);
            if (DateTime.Now > usableTime)
            {
                response = string.Empty;

                return true;
            }

            response =
                $"You must wait another {Math.Round((usableTime - DateTime.Now).TotalSeconds, 2)} seconds to use {Name}";

            return false;
        }

        protected override void AbilityAdded(Player player)
        {
            if (!AllActiveAbilities.ContainsKey(player))
                AllActiveAbilities.Add(player, new());

            if (!AllActiveAbilities[player].Contains(this))
                AllActiveAbilities[player].Add(this);
            base.AbilityAdded(player);
        }
        protected override void AbilityRemoved(Player player)
        {
            if (!AllActiveAbilities.ContainsKey(player))
                return;

            SelectedPlayers.Remove(player);

            AllActiveAbilities[player].Remove(this);
            base.AbilityRemoved(player);
        }

        protected virtual void AbilityUsed(Player player)
        {
        }

        protected virtual void AbilityEnded(Player player)
        {
        }

        protected virtual void Selected(Player player)
        {
        }

        protected virtual void Unselected(Player player)
        {
        }
    }
}
