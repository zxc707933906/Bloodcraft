﻿using Bloodcraft.Services;
using Bloodcraft.SystemUtilities.Legacies;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using static Bloodcraft.SystemUtilities.Experience.LevelingSystem;
using Random = System.Random;

namespace Bloodcraft.Patches;

[HarmonyPatch]
internal static class StatChangeSystemPatches
{
    static EntityManager EntityManager => Core.EntityManager;
    static ServerGameManager ServerGameManager => Core.ServerGameManager;
    static SystemService SystemService => Core.SystemService;
    static ConfigService ConfigService => Core.ConfigService;
    static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;

    static readonly GameModeType GameMode = SystemService.ServerGameSettingsSystem.Settings.GameModeType;

    static readonly Random Random = new();

    static readonly PrefabGUID pvpProtBuff = new(1111481396);
    static readonly PrefabGUID stormShield03 = new(1095865904);
    static readonly PrefabGUID stormShield02 = new(-1192885497);
    static readonly PrefabGUID stormShield01 = new(1044565673);
    static readonly PrefabGUID garlicDebuff = new(-1701323826);
    static readonly PrefabGUID silverDebuff = new(853298599);

    [HarmonyPatch(typeof(StatChangeMutationSystem), nameof(StatChangeMutationSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(StatChangeMutationSystem __instance)
    {
        NativeArray<Entity> entities = __instance._StatChangeEventQuery.ToEntityArray(Allocator.Temp);
        try
        {
            foreach (Entity entity in entities)
            {
                if (!Core.hasInitialized) continue;
                if (!entity.Exists()) continue;

                if (ConfigService.BloodSystem && ConfigService.BloodQualityBonus && entity.Has<StatChangeEvent>() && entity.Has<BloodQualityChange>())
                {
                    StatChangeEvent statChangeEvent = entity.Read<StatChangeEvent>();
                    BloodQualityChange bloodQualityChange = entity.Read<BloodQualityChange>();

                    if (!statChangeEvent.Entity.Has<Blood>()) continue;

                    BloodSystem.BloodType bloodType = BloodSystem.GetBloodTypeFromPrefab(bloodQualityChange.BloodType);
                    ulong steamID = statChangeEvent.Entity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;

                    IBloodHandler bloodHandler = BloodHandlerFactory.GetBloodHandler(bloodType);
                    if (bloodHandler == null) continue;
                    
                    float legacyKey = bloodHandler.GetLegacyData(steamID).Value;

                    if (ConfigService.PrestigeSystem && Core.DataStructures.PlayerPrestiges.TryGetValue(steamID, out var prestiges) && prestiges.TryGetValue(BloodSystem.BloodPrestigeMap[bloodType], out var bloodPrestige) && bloodPrestige > 0)
                    {
                        legacyKey = (float)bloodPrestige * ConfigService.PrestigeBloodQuality;
                        if (legacyKey > 0)
                        {
                            bloodQualityChange.Quality += legacyKey;
                            bloodQualityChange.ForceReapplyBuff = true;
                            entity.Write(bloodQualityChange);
                        }
                    }
                    else if (!ConfigService.PrestigeSystem)
                    {
                        if (legacyKey > 0)
                        {
                            bloodQualityChange.Quality += legacyKey;
                            bloodQualityChange.ForceReapplyBuff = true;
                            entity.Write(bloodQualityChange);
                        }
                    }
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }

    [HarmonyPatch(typeof(DealDamageSystem), nameof(DealDamageSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(DealDamageSystem __instance)
    {
        NativeArray<Entity> entities = __instance._Query.ToEntityArray(Allocator.Temp);
        try
        {
            foreach (Entity entity in entities)
            {
                if (!Core.hasInitialized) continue;
                if (!ConfigService.ClassSpellSchoolOnHitEffects || !ConfigService.Classes) continue;
                if (!entity.Exists() || !entity.Has<DealDamageEvent>()) continue;

                DealDamageEvent dealDamageEvent = entity.Read<DealDamageEvent>();

                if (dealDamageEvent.MainType != MainDamageType.Physical && dealDamageEvent.MainType != MainDamageType.Spell) continue;

                if (!dealDamageEvent.Target.Exists() || !dealDamageEvent.SpellSource.Exists()) continue; // null entities are NOT to make it in here, don't know why or how but last time it happened messed up the save pretty badly

                PrefabGUID sourcePrefab = dealDamageEvent.SpellSource.Read<PrefabGUID>();

                //Core.Log.LogInfo($"Source Prefab: {sourcePrefab.LookupName()}");

                if (sourcePrefab.Equals(silverDebuff) || sourcePrefab.Equals(garlicDebuff)) continue;

                if (dealDamageEvent.SpellSource.GetOwner().HasPlayer(out Entity player) && !dealDamageEvent.Target.IsVampire())
                {                    
                    Entity userEntity = player.Read<PlayerCharacter>().UserEntity;
                    ulong steamId = userEntity.Read<User>().PlatformId;
                    if (!HasClass(steamId)) continue;

                    PlayerClasses playerClass = GetPlayerClass(steamId);
                    if (Random.NextDouble() <= ConfigService.OnHitProcChance)
                    {
                        PrefabGUID prefabGUID = ClassOnHitDebuffMap[playerClass];

                        FromCharacter fromCharacter = new()
                        {
                            Character = dealDamageEvent.Target,
                            User = userEntity
                        };

                        ApplyBuffDebugEvent applyBuffDebugEvent = new()
                        {
                            BuffPrefabGUID = prefabGUID,
                        };
                        
                        if (ServerGameManager.HasBuff(dealDamageEvent.Target, prefabGUID.ToIdentifier()))
                        {
                            applyBuffDebugEvent.BuffPrefabGUID = ClassOnHitEffectMap[playerClass];
                            fromCharacter.Character = player;

                            if (playerClass.Equals(PlayerClasses.DemonHunter))
                            {
                                if (ServerGameManager.TryGetBuff(player, stormShield01.ToIdentifier(), out Entity firstBuff))
                                {
                                    firstBuff.Write(new LifeTime { Duration = 5f, EndAction = LifeTimeEndAction.Destroy });
                                }
                                else if (ServerGameManager.TryGetBuff(player, stormShield02.ToIdentifier(), out Entity secondBuff))
                                {
                                    secondBuff.Write(new LifeTime { Duration = 5f, EndAction = LifeTimeEndAction.Destroy });
                                }
                                else if (ServerGameManager.TryGetBuff(player, stormShield03.ToIdentifier(), out Entity thirdBuff))
                                {
                                    thirdBuff.Write(new LifeTime { Duration = 5f, EndAction = LifeTimeEndAction.Destroy });
                                }
                                else
                                {
                                    DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);
                                }
                            }
                            else
                            {
                                DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);
                            }
                        }
                        else
                        {
                            DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);
                            if (ServerGameManager.TryGetBuff(dealDamageEvent.Target, applyBuffDebugEvent.BuffPrefabGUID.ToIdentifier(), out Entity buff))
                            {
                                buff.Write(new EntityOwner { Owner = player });
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}
