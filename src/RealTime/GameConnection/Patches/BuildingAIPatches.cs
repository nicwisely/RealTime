﻿// <copyright file="BuildingAIPatches.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Reflection;
    using ColossalFramework.Math;
    using RealTime.CustomAI;
    using RealTime.Simulation;
    using SkyTools.Patching;
    using UnityEngine;

    /// <summary>
    /// A static class that provides the patch objects for the building AI game methods.
    /// </summary>
    internal static class BuildingAIPatches
    {
        /// <summary>Gets or sets the custom AI object for buildings.</summary>
        public static RealTimeBuildingAI RealTimeAI { get; set; }

        /// <summary>Gets or sets the weather information service.</summary>
        public static IWeatherInfo WeatherInfo { get; set; }

        /// <summary>Gets the patch for the commercial building AI class.</summary>
        public static IPatch CommercialSimulation { get; } = new CommercialBuildingA_SimulationStepActive();

        /// <summary>Gets the patch for the private building AI method 'HandleWorkers'.</summary>
        public static IPatch HandleWorkers { get; } = new PrivateBuildingAI_HandleWorkers();

        /// <summary>Gets the patch for the private building AI method 'GetConstructionTime'.</summary>
        public static IPatch GetConstructionTime { get; } = new PrivateBuildingAI_GetConstructionTime();

        /// <summary>Gets the patch for the common building AI method 'GetColor'.</summary>
        public static IPatch GetColor { get; } = new CommonBuildingAI_GetColor();

        /// <summary>Gets the patch for the building AI method 'CalculateUnspawnPosition'.</summary>
        public static IPatch CalculateUnspawnPosition { get; } = new BuildingAI_CalculateUnspawnPosition();

        /// <summary>Gets the patch for the building AI method 'GetUpgradeInfo'.</summary>
        public static IPatch GetUpgradeInfo { get; } = new PrivateBuildingAI_GetUpgradeInfo();

        /// <summary>Gets the patch for the building manager method 'CreateBuilding'.</summary>
        public static IPatch CreateBuilding { get; } = new BuildingManager_CreateBuilding();

        /// <summary>Gets the patch for the building AI method 'ProduceGoods'.</summary>
        public static IPatch ProduceGoods { get; } = new PlayerBuildingAI_ProduceGoods();

        /// <summary>Gets the patch for the industry DLC building AI method 'HandleCrime'.</summary>
        public static IPatch HandleCrime { get; } = new IndustryBuildingAI_HandleCrime();

        private sealed class CommercialBuildingA_SimulationStepActive : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(CommercialBuildingAI).GetMethod(
                    "SimulationStepActive",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ref Building buildingData, ref byte __state)
            {
                __state = buildingData.m_outgoingProblemTimer;
                if (buildingData.m_customBuffer2 > 0)
                {
                    // Simulate some goods become spoiled; additionally, this will cause the buildings to never reach the 'stock full' state.
                    // In that state, no visits are possible anymore, so the building gets stuck
                    --buildingData.m_customBuffer2;
                }

                return true;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(ushort buildingID, ref Building buildingData, byte __state)
            {
                if (__state != buildingData.m_outgoingProblemTimer)
                {
                    RealTimeAI?.ProcessBuildingProblems(buildingID, __state);
                }
            }
        }

        private sealed class PrivateBuildingAI_HandleWorkers : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                Type refInt = typeof(int).MakeByRefType();

                return typeof(PrivateBuildingAI).GetMethod(
                    "HandleWorkers",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Citizen.BehaviourData).MakeByRefType(), refInt, refInt, refInt },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ref Building buildingData, ref byte __state)
            {
                __state = buildingData.m_workerProblemTimer;
                return true;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(ushort buildingID, ref Building buildingData, byte __state)
            {
                if (__state != buildingData.m_workerProblemTimer)
                {
                    RealTimeAI?.ProcessWorkerProblems(buildingID, __state);
                }
            }
        }

        private sealed class PrivateBuildingAI_GetConstructionTime : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(PrivateBuildingAI).GetMethod(
                    "GetConstructionTime",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ref int __result)
            {
                __result = RealTimeAI?.GetConstructionTime() ?? 0;
                return false;
            }
        }

        private sealed class BuildingAI_CalculateUnspawnPosition : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(BuildingAI).GetMethod(
                    "CalculateUnspawnPosition",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(CitizenInfo), typeof(ushort), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType(), typeof(Vector2).MakeByRefType(), typeof(CitizenInstance.Flags).MakeByRefType() },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(BuildingAI __instance, ushort buildingID, ref Building data, ref Randomizer randomizer, CitizenInfo info, ref Vector3 position, ref Vector3 target, ref CitizenInstance.Flags specialFlags)
            {
                if (WeatherInfo?.IsBadWeather != true || data.Info == null || data.Info.m_enterDoors == null)
                {
                    return;
                }

                BuildingInfo.Prop[] enterDoors = data.Info.m_enterDoors;
                bool doorFound = false;
                for (int i = 0; i < enterDoors.Length; ++i)
                {
                    PropInfo prop = enterDoors[i].m_finalProp;
                    if (prop == null)
                    {
                        continue;
                    }

                    if (prop.m_doorType == PropInfo.DoorType.Enter || prop.m_doorType == PropInfo.DoorType.Both)
                    {
                        doorFound = true;
                        break;
                    }
                }

                if (!doorFound)
                {
                    return;
                }

                __instance.CalculateSpawnPosition(buildingID, ref data, ref randomizer, info, out Vector3 spawnPosition, out Vector3 spawnTarget);

                position = spawnPosition;
                target = spawnTarget;
                specialFlags &= ~(CitizenInstance.Flags.HangAround | CitizenInstance.Flags.SittingDown);
            }
        }

        private sealed class PrivateBuildingAI_GetUpgradeInfo : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(PrivateBuildingAI).GetMethod(
                    "GetUpgradeInfo",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType() },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ref BuildingInfo __result, ushort buildingID, ref Building data)
            {
                if (RealTimeAI == null || (data.m_flags & Building.Flags.Upgrading) != 0)
                {
                    return true;
                }

                if (!RealTimeAI.CanBuildOrUpgrade(data.Info.GetService(), buildingID))
                {
                    __result = null;
                    return false;
                }

                return true;
            }
        }

        private sealed class BuildingManager_CreateBuilding : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(BuildingManager).GetMethod(
                    "CreateBuilding",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(ushort).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(BuildingInfo), typeof(Vector3), typeof(float), typeof(int), typeof(uint) },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(BuildingInfo info, ref bool __result)
            {
                if (RealTimeAI == null)
                {
                    return true;
                }

                if (!RealTimeAI.CanBuildOrUpgrade(info.GetService()))
                {
                    __result = false;
                    return false;
                }

                return true;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(bool __result, ref ushort building, BuildingInfo info)
            {
                if (__result && RealTimeAI != null)
                {
                    RealTimeAI.RegisterConstructingBuilding(building, info.GetService());
                }
            }
        }

        private sealed class PlayerBuildingAI_ProduceGoods : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(PlayerBuildingAI).GetMethod(
                    "ProduceGoods",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType(), typeof(int), typeof(int), typeof(Citizen.BehaviourData).MakeByRefType(), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            private static void Postfix(ushort buildingID, ref Building buildingData)
            {
                if ((buildingData.m_flags & Building.Flags.Active) != 0
                    && RealTimeAI?.ShouldSwitchBuildingLightsOff(buildingID) == true)
                {
                    buildingData.m_flags &= ~Building.Flags.Active;
                }
            }
        }

        private sealed class CommonBuildingAI_GetColor : PatchBase
        {
            private static Color negativeColor;
            private static Color targetColor;

            protected override MethodInfo GetMethod()
            {
                negativeColor = InfoManager.instance.m_properties.m_modeProperties[(int)InfoManager.InfoMode.TrafficRoutes].m_negativeColor;
                targetColor = InfoManager.instance.m_properties.m_modeProperties[(int)InfoManager.InfoMode.TrafficRoutes].m_targetColor;

                return typeof(CommonBuildingAI).GetMethod(
                    "GetColor",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(InfoManager.InfoMode) },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(ushort buildingID, InfoManager.InfoMode infoMode, ref Color __result)
            {
                if (RealTimeAI == null)
                {
                    return;
                }

                switch (infoMode)
                {
                    case InfoManager.InfoMode.TrafficRoutes:
                        __result = Color.Lerp(negativeColor, targetColor, RealTimeAI.GetBuildingReachingTroubleFactor(buildingID));
                        return;

                    case InfoManager.InfoMode.None:
                        if (RealTimeAI.ShouldSwitchBuildingLightsOff(buildingID))
                        {
                            __result.a = 0f;
                        }

                        return;
                }
            }
        }

        private sealed class IndustryBuildingAI_HandleCrime : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(IndustryBuildingAI).GetMethod(
                    "HandleCrime",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(int), typeof(int) },
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(ref ushort __state, ref Building data)
            {
                __state = data.m_crimeBuffer;
                return true;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(ushort __state, ref Building data)
            {
                // CO's code has a bug that leads to unbounded crime buffer growth for inactive industrial buildings.
                // Since Real Time switches buildings to inactive at night, we have to stop the crime growth for those buildings.
                if ((data.m_flags & Building.Flags.Active) == 0)
                {
                    data.m_crimeBuffer = __state;
                }
            }
        }
    }
}
