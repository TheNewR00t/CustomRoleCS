namespace SidaLand.CustomRoles.API.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Attributes;
    using Exiled.API.Features.Pools;
    using Exiled.API.Interfaces;
    using Exiled.CustomItems.API.Features;
    using Exiled.Events.EventArgs.Player;
    using Exiled.Loader;
    using global::CustomRolesCS.API.Features;
    using InventorySystem.Configs;

    using MEC;

    using PlayerRoles;

    using UnityEngine;

    using YamlDotNet.Serialization;
    public abstract class CustomRoleCS
    {
        private static Dictionary<Type, CustomRoleCS?> typeLookupTable = new();

        private static Dictionary<string, CustomRoleCS?> stringLookupTable = new();

        private static Dictionary<uint, CustomRoleCS?> idLookupTable = new();

        public static HashSet<CustomRoleCS> Registered { get; } = new();
        public abstract uint Id { get; set; }
        public abstract int MaxHealth { get; set; }
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract string CustomInfo { get; set; }
        [YamlIgnore]
        public HashSet<Player> TrackedPlayers { get; } = new();
        public virtual RoleTypeId Role { get; set; }
        public virtual List<CustomAbilityCS>? CustomAbilities { get; set; } = new();
        public virtual List<string> Inventory { get; set; } = new();
        public virtual Dictionary<AmmoType, ushort> Ammo { get; set; } = new();

        public virtual SpawnPropertiesCS SpawnProperties { get; set; } = new();

        public virtual bool KeepPositionOnSpawn { get; set; }

    
        public virtual bool KeepInventoryOnSpawn { get; set; }

        public virtual bool RemovalKillsPlayer { get; set; } = true;

  
        public virtual bool KeepRoleOnDeath { get; set; }


        public virtual float SpawnChance { get; set; }

        public virtual bool IgnoreSpawnSystem { get; set; }


        public virtual bool KeepRoleOnChangingRole { get; set; }

        public virtual Broadcast Broadcast { get; set; } = new Broadcast();


        public virtual bool DisplayCustomItemMessages { get; set; } = true;

        public virtual Vector3 Scale { get; set; } = Vector3.one;

        public virtual Dictionary<RoleTypeId, float> CustomRoleFFMultiplier { get; set; } = new();

        public virtual string ConsoleMessage { get; set; } = $"You have spawned as a custom role!";

        public virtual string AbilityUsage { get; set; } = "Enter \".special\" in the console to use your ability. If you have multiple abilities, you can use this command to cycle through them, or specify the one to use with \".special ROLENAME AbilityNum\"";


        public static CustomRoleCS? Get(uint id)
        {
            if (!idLookupTable.ContainsKey(id))
                idLookupTable.Add(id, Registered?.FirstOrDefault(r => r.Id == id));
            return idLookupTable[id];
        }

        public static CustomRoleCS? Get(Type t)
        {
            if (!typeLookupTable.ContainsKey(t))
                typeLookupTable.Add(t, Registered?.FirstOrDefault(r => r.GetType() == t));
            return typeLookupTable[t];
        }

        public static CustomRoleCS? Get(string name)
        {
            if (!stringLookupTable.ContainsKey(name))
                stringLookupTable.Add(name, Registered?.FirstOrDefault(r => r.Name == name));
            return stringLookupTable[name];
        }

        public static bool TryGet(uint id, out CustomRoleCS? customRole)
        {
            customRole = Get(id);

            return customRole is not null;
        }

        public static bool TryGet(string name, out CustomRoleCS? customRole)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            customRole = uint.TryParse(name, out uint id) ? Get(id) : Get(name);

            return customRole is not null;
        }

        public static bool TryGet(Type t, out CustomRoleCS? customRole)
        {
            customRole = Get(t);

            return customRole is not null;
        }

        public static bool TryGet(Player player, out IReadOnlyCollection<CustomRoleCS> customRoles)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            List<CustomRoleCS> tempList = ListPool<CustomRoleCS>.Pool.Get();
            tempList.AddRange(Registered?.Where(customRole => customRole.Check(player)) ?? Array.Empty<CustomRoleCS>());

            customRoles = tempList.AsReadOnly();
            ListPool<CustomRoleCS>.Pool.Return(tempList);

            return customRoles?.Count > 0;
        }

        public static IEnumerable<CustomRoleCS> RegisterRoles(bool skipReflection = false, object? overrideClass = null) => RegisterRoles(skipReflection, overrideClass, true, Assembly.GetCallingAssembly());

        public static IEnumerable<CustomRoleCS> RegisterRoles(bool skipReflection = false, object? overrideClass = null, bool inheritAttributes = true, Assembly? assembly = null)
        {
            List<CustomRoleCS> roles = new();

            Log.Warn("Registering roles...");

            assembly ??= Assembly.GetCallingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(CustomRoleCS) && type.GetCustomAttribute(typeof(CustomRoleAttribute), inheritAttributes) is null)
                {
                    Log.Debug($"{type} base: {type.BaseType} -- {type.GetCustomAttribute(typeof(CustomRoleAttribute), inheritAttributes) is null}");
                    continue;
                }

                Log.Debug($"Getting attributed for {type}");
                foreach (Attribute attribute in type.GetCustomAttributes(typeof(CustomRoleAttribute), inheritAttributes).Cast<Attribute>())
                {
                    CustomRoleCS? customRole = null;

                    if (!skipReflection && Server.PluginAssemblies.TryGetValue(assembly, out IPlugin<IConfig> plugin))
                    {
                        foreach (PropertyInfo property in overrideClass?.GetType().GetProperties() ?? plugin.Config.GetType().GetProperties())
                        {
                            if (property.PropertyType != type)
                                continue;

                            customRole = property.GetValue(overrideClass ?? plugin.Config) as CustomRoleCS;
                            break;
                        }
                    }

                    customRole ??= (CustomRoleCS)Activator.CreateInstance(type);

                    if (customRole.Role == RoleTypeId.None)
                        customRole.Role = ((CustomRoleAttribute)attribute).RoleTypeId;

                    if (customRole.TryRegister())
                        roles.Add(customRole);
                }
            }

            return roles;
        }

        public static IEnumerable<CustomRoleCS> RegisterRoles(IEnumerable<Type> targetTypes, bool isIgnored = false, bool skipReflection = false, object? overrideClass = null)
        {
            List<CustomRoleCS> roles = new();
            Assembly assembly = Assembly.GetCallingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(CustomItem) ||
                    type.GetCustomAttribute(typeof(CustomRoleAttribute)) is null ||
                    (isIgnored && targetTypes.Contains(type)) ||
                    (!isIgnored && !targetTypes.Contains(type)))
                {
                    continue;
                }

                foreach (Attribute attribute in type.GetCustomAttributes(typeof(CustomRoleAttribute), true).Cast<Attribute>())
                {
                    CustomRoleCS? customRole = null;

                    if (!skipReflection && Server.PluginAssemblies.ContainsKey(assembly))
                    {
                        IPlugin<IConfig> plugin = Server.PluginAssemblies[assembly];

                        foreach (PropertyInfo property in overrideClass?.GetType().GetProperties() ??
                                                          plugin.Config.GetType().GetProperties())
                        {
                            if (property.PropertyType != type)
                                continue;

                            customRole = property.GetValue(overrideClass ?? plugin.Config) as CustomRoleCS;
                        }
                    }

                    customRole ??= (CustomRoleCS)Activator.CreateInstance(type);

                    if (customRole.Role == RoleTypeId.None)
                        customRole.Role = ((CustomRoleAttribute)attribute).RoleTypeId;

                    if (customRole.TryRegister())
                        roles.Add(customRole);
                }
            }

            return roles;
        }

        public static IEnumerable<CustomRoleCS> UnregisterRoles()
        {
            List<CustomRoleCS> unregisteredRoles = new();

            foreach (CustomRoleCS customRole in Registered)
            {
                customRole.TryUnregister();
                unregisteredRoles.Add(customRole);
            }

            return unregisteredRoles;
        }

        public static IEnumerable<CustomRoleCS> UnregisterRoles(IEnumerable<Type> targetTypes, bool isIgnored = false)
        {
            List<CustomRoleCS> unregisteredRoles = new();

            foreach (CustomRoleCS customRole in Registered)
            {
                if ((targetTypes.Contains(customRole.GetType()) && isIgnored) || (!targetTypes.Contains(customRole.GetType()) && !isIgnored))
                    continue;

                customRole.TryUnregister();
                unregisteredRoles.Add(customRole);
            }

            return unregisteredRoles;
        }

        public static IEnumerable<CustomRoleCS> UnregisterRoles(IEnumerable<CustomRoleCS> targetRoles, bool isIgnored = false) => UnregisterRoles(targetRoles.Select(x => x.GetType()), isIgnored);

        public static void SyncPlayerFriendlyFire(CustomRoleCS roleToSync, Player player, bool overwrite = false)
        {
            if (overwrite)
            {
                player.TryAddCustomRoleFriendlyFire(roleToSync.Name, roleToSync.CustomRoleFFMultiplier, overwrite);
                player.UniqueRole = roleToSync.Name;
            }
            else
            {
                player.TryAddCustomRoleFriendlyFire(roleToSync.Name, roleToSync.CustomRoleFFMultiplier);
            }
        }

        public static void ForceSyncSetPlayerFriendlyFire(CustomRoleCS roleToSync, Player player)
        {
            player.TrySetCustomRoleFriendlyFire(roleToSync.Name, roleToSync.CustomRoleFFMultiplier);
        }

        public virtual bool Check(Player? player) => player is not null && TrackedPlayers.Contains(player);

        public virtual void Init()
        {
            idLookupTable.Add(Id, this);
            typeLookupTable.Add(GetType(), this);
            stringLookupTable.Add(Name, this);
            SubscribeEvents();
        }

        public virtual void Destroy()
        {
            idLookupTable.Remove(Id);
            typeLookupTable.Remove(GetType());
            stringLookupTable.Remove(Name);
            UnsubscribeEvents();
        }

        public virtual void AddRole(Player player)
        {
            Log.Debug($"{Name}: Adding role to {player.Nickname}.");
            TrackedPlayers.Add(player);

            if (Role != RoleTypeId.None)
            {
                switch (KeepPositionOnSpawn)
                {
                    case true when KeepInventoryOnSpawn:
                        player.Role.Set(Role, SpawnReason.ForceClass, RoleSpawnFlags.None);
                        break;
                    case true:
                        player.Role.Set(Role, SpawnReason.ForceClass, RoleSpawnFlags.AssignInventory);
                        break;
                    default:
                        if (KeepInventoryOnSpawn && player.IsAlive)
                            player.Role.Set(Role, SpawnReason.ForceClass, RoleSpawnFlags.UseSpawnpoint);
                        else
                            player.Role.Set(Role, SpawnReason.ForceClass, RoleSpawnFlags.All);
                        break;
                }
            }

            if (TrackedPlayers.Count > 1)
            {
                TrackedPlayers.Remove(player);
                Log.Warn($"{Name}: Un solo CustomRole permitido, no se asignará otro a {player.Nickname}.");
                return;
            }

            Timing.CallDelayed(0.5f, () =>
                {
                    if (!KeepInventoryOnSpawn)
                    {
                        Log.Debug($"{Name}: Clearing {player.Nickname}'s inventory.");
                        player.ClearInventory();
                    }

                    foreach (string itemName in Inventory)
                    {
                        Log.Debug($"{Name}: Adding {itemName} to inventory.");
                        TryAddItem(player, itemName);
                    }

                    if (Ammo.Count > 0)
                    {
                        Log.Debug($"{Name}: Adding Ammo to {player.Nickname} inventory.");
                        foreach (AmmoType type in EnumUtils<AmmoType>.Values)
                        {
                            if (type != AmmoType.None)
                                player.SetAmmo(type, Ammo.ContainsKey(type) ? Ammo[type] == ushort.MaxValue ? InventoryLimits.GetAmmoLimit(type.GetItemType(), player.ReferenceHub) : Ammo[type] : (ushort)0);
                        }
                    }
                });

            Log.Debug($"{Name}: Setting health values.");
            player.Health = MaxHealth;
            player.MaxHealth = MaxHealth;
            player.Scale = Scale;

            Vector3 position = GetSpawnPosition();
            if (position != Vector3.zero)
            {
                player.Position = position;
            }

            Log.Debug($"{Name}: Setting player info");

            player.CustomInfo = $"{player.CustomName}\n{CustomInfo}";
            player.InfoArea &= ~(PlayerInfoArea.Role | PlayerInfoArea.Nickname);

            if (CustomAbilities is not null)
            {
                foreach (CustomAbilityCS ability in CustomAbilities)
                    ability.AddAbility(player);
            }

            ShowMessage(player);
            ShowBroadcast(player);
            RoleAdded(player);
            player.UniqueRole = Name;
            player.TryAddCustomRoleFriendlyFire(Name, CustomRoleFFMultiplier);

            if (!string.IsNullOrEmpty(ConsoleMessage))
            {
                StringBuilder builder = StringBuilderPool.Pool.Get();

                builder.AppendLine(Name);
                builder.AppendLine(Description);
                builder.AppendLine();
                builder.AppendLine(ConsoleMessage);

                if (CustomAbilities?.Count > 0)
                {
                    builder.AppendLine(AbilityUsage);
                    builder.AppendLine("Your custom abilities are:");
                    for (int i = 1; i < CustomAbilities.Count + 1; i++)
                        builder.AppendLine($"{i}. {CustomAbilities[i - 1].Name} - {CustomAbilities[i - 1].Description}");

                    builder.AppendLine("You can keybind the command for this ability by using \"cmdbind .special KEY\", where KEY is any un-used letter on your keyboard. You can also keybind each specific ability for a role in this way. For ex: \"cmdbind .special g\" or \"cmdbind .special bulldozer 1 g\"");
                }

                player.SendConsoleMessage(StringBuilderPool.Pool.ToStringReturn(builder), "green");
            }
        }


        public virtual void RemoveRole(Player player)
        {
            if (!TrackedPlayers.Contains(player))
                return;
            Log.Debug($"{Name}: Removing role from {player.Nickname}");
            TrackedPlayers.Remove(player);
            player.CustomInfo = string.Empty;
            player.InfoArea |= PlayerInfoArea.Role | PlayerInfoArea.Nickname;
            player.Scale = Vector3.one;
            if (CustomAbilities is not null)
            {
                foreach (CustomAbilityCS ability in CustomAbilities)
                {
                    ability.RemoveAbility(player);
                }
            }

            RoleRemoved(player);
            player.UniqueRole = string.Empty;
            player.TryRemoveCustomeRoleFriendlyFire(Name);

            if (RemovalKillsPlayer)
                player.Role.Set(RoleTypeId.Spectator);
        }


        public void SetFriendlyFire(RoleTypeId roleToAdd, float ffMult)
        {
            if (CustomRoleFFMultiplier.ContainsKey(roleToAdd))
            {
                CustomRoleFFMultiplier[roleToAdd] = ffMult;
            }
            else
            {
                CustomRoleFFMultiplier.Add(roleToAdd, ffMult);
            }
        }

        public void SetFriendlyFire(KeyValuePair<RoleTypeId, float> roleFF)
        {
            SetFriendlyFire(roleFF.Key, roleFF.Value);
        }

        public bool TryAddFriendlyFire(RoleTypeId roleToAdd, float ffMult)
        {
            if (CustomRoleFFMultiplier.ContainsKey(roleToAdd))
            {
                return false;
            }

            CustomRoleFFMultiplier.Add(roleToAdd, ffMult);
            return true;
        }

        public bool TryAddFriendlyFire(KeyValuePair<RoleTypeId, float> pairedRoleFF) => TryAddFriendlyFire(pairedRoleFF.Key, pairedRoleFF.Value);

        public bool TryAddFriendlyFire(Dictionary<RoleTypeId, float> ffRules, bool overwrite = false)
        {
            Dictionary<RoleTypeId, float> temporaryFriendlyFireRules = DictionaryPool<RoleTypeId, float>.Pool.Get();

            foreach (KeyValuePair<RoleTypeId, float> roleFF in ffRules)
            {
                if (overwrite)
                {
                    SetFriendlyFire(roleFF);
                }
                else
                {
                    if (!CustomRoleFFMultiplier.ContainsKey(roleFF.Key))
                    {
                        temporaryFriendlyFireRules.Add(roleFF.Key, roleFF.Value);
                    }
                    else
                    {
                        // Contained Key but overwrite set to false so we do not add any.
                        return false;
                    }
                }
            }

            if (!overwrite)
            {
                foreach (KeyValuePair<RoleTypeId, float> roleFF in temporaryFriendlyFireRules)
                {
                    TryAddFriendlyFire(roleFF);
                }
            }

            DictionaryPool<RoleTypeId, float>.Pool.Return(temporaryFriendlyFireRules);
            return true;
        }

        internal bool TryRegister()
        {
            if (!CustomRolesCS.Instance!.Config.IsEnabled)
                return false;

            if (!Registered.Contains(this))
            {
                if (Registered.Any(r => r.Id == Id))
                {
                    Log.Warn($"{Name} has tried to register with the same Role ID as another role: {Id}. It will not be registered!");

                    return false;
                }

                Registered.Add(this);
                Init();

                Log.Debug($"{Name} ({Id}) has been successfully registered.");

                return true;
            }

            Log.Warn($"Couldn't register {Name} ({Id}) [{Role}] as it already exists.");

            return false;
        }
        internal bool TryUnregister()
        {
            Destroy();

            if (!Registered.Remove(this))
            {
                Log.Warn($"Cannot unregister {Name} ({Id}) [{Role}], it hasn't been registered yet.");

                return false;
            }

            return true;
        }
        protected bool TryAddItem(Player player, string itemName)
        {
            if (CustomItem.TryGet(itemName, out CustomItem? customItem))
            {
                customItem?.Give(player, DisplayCustomItemMessages);

                return true;
            }

            if (Enum.TryParse(itemName, out ItemType type))
            {
                if (type.IsAmmo())
                    player.Ammo[type] = 100;
                else
                    player.AddItem(type);

                return true;
            }

            Log.Warn($"{Name}: {nameof(TryAddItem)}: {itemName} is not a valid ItemType or Custom Item name.");

            return false;
        }

        protected Vector3 GetSpawnPosition()
        {
            if (SpawnProperties is null || SpawnProperties.Count() == 0)
                return Vector3.zero;

            if (SpawnProperties.StaticSpawnPoints.Count > 0)
            {
                foreach ((float chance, Vector3 pos) in SpawnProperties.StaticSpawnPoints)
                {
                    double r = Loader.Random.NextDouble() * 100;
                    if (r <= chance)
                        return pos;
                }
            }

            if (SpawnProperties.DynamicSpawnPoints.Count > 0)
            {
                foreach ((float chance, Vector3 pos) in SpawnProperties.DynamicSpawnPoints)
                {
                    double r = Loader.Random.NextDouble() * 100;
                    if (r <= chance)
                        return pos;
                }
            }

            if (SpawnProperties.DynamicSpawnPointsCS.Count > 0)
            {
                foreach ((float chance, Vector3 pos) in SpawnProperties.DynamicSpawnPointsCS)
                {
                    double r = Loader.Random.NextDouble() * 100;
                    if (r <= chance)
                        return pos;
                }
            } 
            
            if (SpawnProperties.RoleSpawnPoints.Count > 0)
            {
                foreach ((float chance, Vector3 pos) in SpawnProperties.RoleSpawnPoints)
                {
                    double r = Loader.Random.NextDouble() * 100;
                    if (r <= chance)
                        return pos;
                }
            }

            if (SpawnProperties.RoomSpawnPoints.Count > 0)
            {
                foreach ((float chance, Vector3 pos) in SpawnProperties.RoomSpawnPoints)
                {
                    double r = Loader.Random.NextDouble() * 100;
                    if (r <= chance)
                        return pos;
                }
            }

            return Vector3.zero;
        }

        protected virtual void SubscribeEvents()
        {
            Log.Debug($"{Name}: Loading events.");
            Exiled.Events.Handlers.Player.ChangingNickname += OnInternalChangingNickname;
            Exiled.Events.Handlers.Player.ChangingRole += OnInternalChangingRole;
            Exiled.Events.Handlers.Player.Spawned += OnInternalSpawned;
            Exiled.Events.Handlers.Player.SpawningRagdoll += OnSpawningRagdoll;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
        }

        protected virtual void UnsubscribeEvents()
        {
            foreach (Player player in TrackedPlayers)
                RemoveRole(player);

            Log.Debug($"{Name}: Unloading events.");
            Exiled.Events.Handlers.Player.ChangingNickname -= OnInternalChangingNickname;
            Exiled.Events.Handlers.Player.ChangingRole -= OnInternalChangingRole;
            Exiled.Events.Handlers.Player.Spawned -= OnInternalSpawned;
            Exiled.Events.Handlers.Player.SpawningRagdoll -= OnSpawningRagdoll;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
        }
        protected virtual void ShowMessage(Player player) => player.ShowHint($"Has spawneado como {Name}\n{Description} ", 6);
        protected virtual void ShowBroadcast(Player player) => player.Broadcast(Broadcast);

        protected virtual void RoleAdded(Player player)
        {
        }

        protected virtual void RoleRemoved(Player player)
        {
        }

        private void OnInternalChangingNickname(ChangingNicknameEventArgs ev)
        {
            if (!Check(ev.Player))
                return;

            ev.Player.CustomInfo = $"{ev.NewName}\n{CustomInfo}";
        }

        private void OnInternalSpawned(SpawnedEventArgs ev)
        {
            if (!IgnoreSpawnSystem && SpawnChance > 0 && !Check(ev.Player) && ev.Player.Role.Type == Role && Loader.Random.NextDouble() * 100 <= SpawnChance)
                AddRole(ev.Player);
        }

        private void OnInternalChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason == SpawnReason.Destroyed)
                return;

            if (Check(ev.Player) && ((ev.NewRole == RoleTypeId.Spectator && !KeepRoleOnDeath) || (ev.NewRole != RoleTypeId.Spectator && ev.NewRole != Role && !KeepRoleOnChangingRole)))
            {
                RemoveRole(ev.Player);
            }
        }

        private void OnSpawningRagdoll(SpawningRagdollEventArgs ev)
        {
            if (Check(ev.Player))
                ev.Role = Role;
        }

        private void OnDestroying(DestroyingEventArgs ev) => RemoveRole(ev.Player);
    }
}