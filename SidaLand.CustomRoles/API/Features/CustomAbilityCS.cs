using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Interfaces;

using YamlDotNet.Serialization;

namespace SidaLand.CustomRoles.API.Features
{
    public abstract class CustomAbilityCS
    {
        public CustomAbilityCS()
        {
            AbilityType = GetType().Name;
        }

        public static HashSet<CustomAbilityCS> Registered { get; } = new();

        public abstract string Name { get; set; }

        public abstract string Description { get; set; }

        [YamlIgnore]
        public HashSet<Player> Players { get; } = new();

        [Description("Changing this will likely break your config.")]
        public string AbilityType { get; }

        public static CustomAbilityCS? Get(string name) => Registered?.FirstOrDefault(r => r.Name == name);


        public static CustomAbilityCS? Get(Type type) => Registered?.FirstOrDefault(r => r.GetType() == type);


        public static bool TryGet(Type type, out CustomAbilityCS? customAbility)
        {
            customAbility = Get(type);

            return customAbility is not null;
        }

        public static bool TryGet(string name, out CustomAbilityCS? customAbility)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            customAbility = Get(name);

            return customAbility is not null;
        }

        public static IEnumerable<CustomAbilityCS> RegisterAbilities(bool skipReflection = false, object? overrideClass = null)
        {
            List<CustomAbilityCS> abilities = new();
            Assembly assembly = Assembly.GetCallingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(CustomAbilityCS) || type.GetCustomAttribute(typeof(CustomAbilityAttribute)) is null)
                    continue;

                CustomAbilityCS? customAbility = null;

                if (!skipReflection && Server.PluginAssemblies.ContainsKey(assembly))
                {
                    IPlugin<IConfig> plugin = Server.PluginAssemblies[assembly];

                    foreach (PropertyInfo property in overrideClass?.GetType().GetProperties() ??
                                                      plugin.Config.GetType().GetProperties())
                    {
                        if (property.PropertyType != type)
                            continue;

                        customAbility = property.GetValue(overrideClass ?? plugin.Config) as CustomAbilityCS;
                        break;
                    }
                }

                if (customAbility is null)
                    customAbility = (CustomAbilityCS)Activator.CreateInstance(type);

                if (customAbility.TryRegister())
                    abilities.Add(customAbility);
            }

            return abilities;
        }

        public static IEnumerable<CustomAbilityCS> RegisterAbilities(IEnumerable<Type> targetTypes, bool isIgnored = false, bool skipReflection = false, object? overrideClass = null)
        {
            List<CustomAbilityCS> abilities = new();
            Assembly assembly = Assembly.GetCallingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (((type.BaseType != typeof(CustomAbilityCS)) && !type.IsSubclassOf(typeof(CustomAbilityCS))) || type.GetCustomAttribute(typeof(CustomAbilityAttribute)) is null ||
                    (isIgnored && targetTypes.Contains(type)) || (!isIgnored && !targetTypes.Contains(type)))
                    continue;

                CustomAbilityCS? customAbility = null;

                if (!skipReflection && Server.PluginAssemblies.ContainsKey(assembly))
                {
                    IPlugin<IConfig> plugin = Server.PluginAssemblies[assembly];

                    foreach (PropertyInfo property in overrideClass?.GetType().GetProperties() ?? plugin.Config.GetType().GetProperties())
                    {
                        if (property.PropertyType != type)
                            continue;

                        customAbility = property.GetValue(overrideClass ?? plugin.Config) as CustomAbilityCS;
                    }
                }

                if (customAbility is null)
                    customAbility = (CustomAbilityCS)Activator.CreateInstance(type);

                if (customAbility.TryRegister())
                    abilities.Add(customAbility);
            }

            return abilities;
        }
        public static IEnumerable<CustomAbilityCS> UnregisterAbilities()
        {
            List<CustomAbilityCS> unregisteredAbilities = new();

            foreach (CustomAbilityCS customAbility in Registered)
            {
                customAbility.TryUnregister();
                unregisteredAbilities.Add(customAbility);
            }

            return unregisteredAbilities;
        }

        public static IEnumerable<CustomAbilityCS> UnregisterAbilities(IEnumerable<Type> targetTypes, bool isIgnored = false)
        {
            List<CustomAbilityCS> unregisteredAbilities = new();

            foreach (CustomAbilityCS customAbility in Registered)
            {
                if ((targetTypes.Contains(customAbility.GetType()) && isIgnored) || (!targetTypes.Contains(customAbility.GetType()) && !isIgnored))
                    continue;

                customAbility.TryUnregister();
                unregisteredAbilities.Add(customAbility);
            }

            return unregisteredAbilities;
        }
        public static IEnumerable<CustomAbilityCS> UnregisterAbilities(IEnumerable<CustomAbilityCS> targetAbilities, bool isIgnored = false) => UnregisterAbilities(targetAbilities.Select(x => x.GetType()), isIgnored);


        public virtual bool Check(Player player) => player is not null && Players.Contains(player);

        public void AddAbility(Player player)
        {
            Log.Debug($"Added {Name} to {player.Nickname}");
            Players.Add(player);
            AbilityAdded(player);
        }

        public void RemoveAbility(Player player)
        {
            Log.Debug($"Removed {Name} from {player.Nickname}");
            Players.Remove(player);
            AbilityRemoved(player);
        }

        public void Init() => SubscribeEvents();

        public void Destroy() => UnsubscribeEvents();

        internal bool TryRegister()
        {
            if (!CustomRolesCS.Instance!.Config.IsEnabled)
                return false;

            if (!Registered.Contains(this))
            {
                Registered.Add(this);
                Init();

                Log.Debug($"{Name} has been successfully registered.");

                return true;
            }

            Log.Warn($"Couldn't register {Name} as it already exists.");

            return false;
        }

        internal bool TryUnregister()
        {
            Destroy();

            if (!Registered.Remove(this))
            {
                Log.Warn($"Cannot unregister {Name}, it hasn't been registered yet.");

                return false;
            }

            return true;
        }

        protected virtual void SubscribeEvents()
        {
        }

        protected virtual void UnsubscribeEvents()
        {
        }

        protected virtual void AbilityAdded(Player player)
        {
        }

        protected virtual void AbilityRemoved(Player player)
        {
        }
    }
}
