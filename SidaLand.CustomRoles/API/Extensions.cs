namespace SidaLand.CustomRoles.API
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Exiled.API.Features;
    using SidaLand.CustomRoles.API.Features;
    using SidaLand.CustomRoles.API.Features.Enums;
    using Utils.NonAllocLINQ;

    public static class Extensions
    {

        public static ReadOnlyCollection<CustomRoleCS> GetCustomRoles(this Player player)
        {
            List<CustomRoleCS> roles = new();

            foreach (CustomRoleCS customRole in CustomRoleCS.Registered)
            {
                if (customRole.Check(player))
                    roles.Add(customRole);
            }

            return roles.AsReadOnly();
        }

        public static void Register(this IEnumerable<CustomRoleCS> customRoles)
        {
            if (customRoles is null)
                throw new ArgumentNullException(nameof(customRoles));

            foreach (CustomRoleCS customRole in customRoles)
                customRole.TryRegister();
        }

        public static void Register(this CustomRoleCS role) => role.TryRegister();

        public static void Register(this CustomAbilityCS ability) => ability.TryRegister();

        public static void Unregister(this IEnumerable<CustomRoleCS> customRoles)
        {
            if (customRoles is null)
                throw new ArgumentNullException(nameof(customRoles));

            foreach (CustomRoleCS customRole in customRoles)
                customRole.TryUnregister();
        }

        public static void Unregister(this CustomRoleCS role) => role.TryUnregister();

        public static void Unregister(this CustomAbilityCS ability) => ability.TryUnregister();

        public static IEnumerable<ActiveAbilityCS>? GetActiveAbilities(this Player player) => !ActiveAbilityCS.AllActiveAbilities.TryGetValue(player, out HashSet<ActiveAbilityCS> abilities) ? null : abilities;

        public static ActiveAbilityCS? GetSelectedAbility(this Player player) => !ActiveAbilityCS.AllActiveAbilities.TryGetValue(player, out HashSet<ActiveAbilityCS> abilities) ? null : abilities.FirstOrDefault(a => a.Check(player, CheckTypeCS.Selected));
    }
}