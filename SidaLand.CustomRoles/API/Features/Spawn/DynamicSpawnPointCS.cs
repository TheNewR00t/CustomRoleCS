using CustomRolesCS.API.Features.Enums;
using Exiled.API.Features.Spawn;
using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace CustomRolesCS.API.Features
{
    public class DynamicSpawnPointCS : SpawnPoint
    {
        public SpawnLocationTypeCS Location { get; set; }

        public override float Chance { get; set; }

        [YamlIgnore]
        public override string Name
        {
            get
            {
                return Location.ToString();
            }
            set
            {
                throw new InvalidOperationException("El nombre de una ubicación de generación dinámica no se puede cambiar.");
            }
        }

        [YamlIgnore]
        public override Vector3 Position
        {
            get
            {
                return Location.GetPosition();
            }
            set
            {
                throw new InvalidOperationException("El vector de generación de una ubicación de generación dinámica no se puede cambiar.");
            }
        }
    }
}
