using CustomRolesCS.API.Features.Enums;
using Exiled.API.Enums;
using Exiled.API.Features.Doors;
using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomRolesCS.API.Features
{
    public static class SpawnExtensionsCS
    {
        public static readonly SpawnLocationTypeCS[] ReversedLocations = new SpawnLocationTypeCS[36]
        {
            SpawnLocationTypeCS.CheckPointEZA,
            SpawnLocationTypeCS.CheckPointEZB,
            SpawnLocationTypeCS._049_Armory,
            SpawnLocationTypeCS._079_Armory,
            SpawnLocationTypeCS._079_First,
            SpawnLocationTypeCS._079_Second,
            SpawnLocationTypeCS._096,
            SpawnLocationTypeCS._106_Primary,
            SpawnLocationTypeCS._106_Secondary,
            SpawnLocationTypeCS._173_Armory,
            SpawnLocationTypeCS._173_Bottom,
            SpawnLocationTypeCS._173_Connector,
            SpawnLocationTypeCS._173_Gate,
            SpawnLocationTypeCS._330,
            SpawnLocationTypeCS._330_Chamber,
            SpawnLocationTypeCS._914,
            SpawnLocationTypeCS._939_Cryo,
            SpawnLocationTypeCS.ChechPointLCZB,
            SpawnLocationTypeCS.ChechPointLCZA,
            SpawnLocationTypeCS.EscapeFinal,
            SpawnLocationTypeCS.Escape_Primary,
            SpawnLocationTypeCS.Escape_Secondary,
            SpawnLocationTypeCS.Gate_A,
            SpawnLocationTypeCS.Gate_B,
            SpawnLocationTypeCS.GR18,
            SpawnLocationTypeCS.GR18Inner,
            SpawnLocationTypeCS.HCZArmory,
            SpawnLocationTypeCS.HIDChmabear,
            SpawnLocationTypeCS.HIDLower,
            SpawnLocationTypeCS.HIDUpper,
            SpawnLocationTypeCS.Intercom,
            SpawnLocationTypeCS.LCZArmory,  
            SpawnLocationTypeCS.LCZCafe,
            SpawnLocationTypeCS.LCZWZ,  
            SpawnLocationTypeCS.SurfaceGate,
            SpawnLocationTypeCS.SurfaceNuke
        };

        public static Transform GetDoor(this SpawnLocationTypeCS location)
        {
            string doorName = location.GetDoorName();
            if (string.IsNullOrEmpty(doorName))
            {
                return null;
            }

            if (!DoorNametagExtension.NamedDoors.TryGetValue(doorName, out var value))
            {
                return null;
            }

            return value.transform;
        }

        public static Vector3 GetPosition(this SpawnLocationTypeCS location)
        {
            Transform door = location.GetDoor();
            if ((object)door == null)
            {
                return default(Vector3);
            }

            return door.position + Vector3.up * 1.5f + door.forward * (ReversedLocations.Contains(location) ? (-1.5f) : 3f);
        }

        public static string GetDoorName(this SpawnLocationTypeCS spawnLocation)
        {
            return spawnLocation switch
            {
                SpawnLocationTypeCS._330 => "330",
                SpawnLocationTypeCS._096 => "096",
                SpawnLocationTypeCS._914 => "914",
                SpawnLocationTypeCS.HIDChmabear => "HID_CHAMBER",
                SpawnLocationTypeCS.GR18 => "GR18",
                SpawnLocationTypeCS.Gate_A => "GATE_A",
                SpawnLocationTypeCS.Gate_B => "GATE_B",
                SpawnLocationTypeCS.LCZWZ => "LCZ_WC",
                SpawnLocationTypeCS.HIDLower => "HID_LOWER",
                SpawnLocationTypeCS.LCZCafe => "LCZ_CAFE",
                SpawnLocationTypeCS._173_Gate => "173_GATE",
                SpawnLocationTypeCS.Intercom => "INTERCOM",
                SpawnLocationTypeCS.HIDUpper => "HID_UPPER",
                SpawnLocationTypeCS._079_First => "079_FIRST",
                SpawnLocationTypeCS._330_Chamber => "330_CHAMBER",
                SpawnLocationTypeCS._049_Armory => "049_ARMORY",
                SpawnLocationTypeCS._173_Armory => "173_ARMORY",
                SpawnLocationTypeCS._173_Bottom => "173_BOTTOM",
                SpawnLocationTypeCS.LCZArmory => "LCZ_ARMORY",
                SpawnLocationTypeCS.HCZArmory => "HCZ_ARMORY",
                SpawnLocationTypeCS.SurfaceNuke => "SURFACE_NUKE",
                SpawnLocationTypeCS._079_Second => "079_SECOND",
                SpawnLocationTypeCS._173_Connector => "173_CONNECTOR",
                SpawnLocationTypeCS.Escape_Primary => "ESCAPE_PRIMARY",
                SpawnLocationTypeCS.Escape_Secondary => "ESCAPE_SECONDARY",
                SpawnLocationTypeCS.CheckPointEZA => "CHECKPOINT_EZ_HCZ_A",
                SpawnLocationTypeCS.CheckPointEZB => "CHECKPOINT_EZ_HCZ_B",
                _ => null,
            };
        }
    }
}
