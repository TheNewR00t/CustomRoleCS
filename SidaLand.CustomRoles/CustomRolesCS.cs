using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Loader;
using Exiled.Loader.Features.Configs.CustomConverters;
using SidaLand.CustomRoles.API.Features;
using SidaLand.CustomRoles.API.Features.Parsers;
using SidaLand.CustomRoles.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace SidaLand.CustomRoles
{
    public class CustomRolesCS : Plugin<Config>
    {
        public override string Author { get; } = "Davilone32";
        public override string Name { get; } = "SidaLandRoles";
        public override string Prefix { get; } = "SLR";
        public override Version Version => new Version(1, 0, 0);
        public override PluginPriority Priority => PluginPriority.First;
        public override Version RequiredExiledVersion => new Version(9, 5, 0);


        private PlayerHandlers? playerHandlers;
        private KeypressActivatorCS? keypressActivator;
        public CustomRolesCS()
        {
            Loader.Deserializer = new DeserializerBuilder()
                .WithTypeConverter(new VectorsConverter())
                .WithTypeConverter(new ColorConverter())
                .WithTypeConverter(new AttachmentIdentifiersConverter())
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithNodeDeserializer(inner => new AbstractClassNodeTypeResolverCS(inner, new AggregateExceptionTypeResolveCS<CustomAbilityCS>(UnderscoredNamingConvention.Instance)), s => s.InsteadOf<ObjectNodeDeserializer>())
                .IgnoreFields()
                .IgnoreUnmatchedProperties()
                .Build();
        }
        public static CustomRolesCS Instance { get; private set; } = null!;

        internal List<Player> StopRagdollPlayers { get; } = new();
        public override void OnEnabled()
        {
            Instance = this;
            playerHandlers = new PlayerHandlers(this);

            if (Config.UseKeypressActivation)
                keypressActivator = new();
            Exiled.Events.Handlers.Player.SpawningRagdoll += playerHandlers.OnSpawningRagdoll;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.SpawningRagdoll -= playerHandlers!.OnSpawningRagdoll;
            keypressActivator = null;
            base.OnDisabled();
        }
    }
}
