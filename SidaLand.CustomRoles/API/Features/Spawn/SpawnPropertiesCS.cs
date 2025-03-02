using Exiled.API.Features.Spawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRolesCS.API.Features
{
    public class SpawnPropertiesCS
    {
        public uint Limit { get; set; }

        public List<DynamicSpawnPoint> DynamicSpawnPoints { get; set; } = new List<DynamicSpawnPoint>();


        public List<StaticSpawnPoint> StaticSpawnPoints { get; set; } = new List<StaticSpawnPoint>();


        public List<RoleSpawnPoint> RoleSpawnPoints { get; set; } = new List<RoleSpawnPoint>();


        public List<RoomSpawnPoint> RoomSpawnPoints { get; set; } = new List<RoomSpawnPoint>();


        public List<LockerSpawnPoint> LockerSpawnPoints { get; set; } = new List<LockerSpawnPoint>();

        public List<DynamicSpawnPointCS> DynamicSpawnPointsCS { get; set; } = new List<DynamicSpawnPointCS>();


        public int Count()
        {
            return DynamicSpawnPoints.Count + StaticSpawnPoints.Count + RoleSpawnPoints.Count + RoomSpawnPoints.Count + LockerSpawnPoints.Count + DynamicSpawnPointsCS.Count;
        }
    }
}
