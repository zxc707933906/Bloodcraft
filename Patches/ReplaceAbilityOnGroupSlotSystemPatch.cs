﻿using Bloodcraft.Services;
using Bloodcraft.Utilities;
using HarmonyLib;
using ProjectM;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace Bloodcraft.Patches;

[HarmonyPatch]
internal static class ReplaceAbilityOnGroupSlotSystemPatch
{
    static ServerGameManager ServerGameManager => Core.ServerGameManager;

    static readonly bool Classes = ConfigService.SoftSynergies || ConfigService.HardSynergies;

    public static Dictionary<int, int> ClassSpells = [];

    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), nameof(ReplaceAbilityOnSlotSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(ReplaceAbilityOnSlotSystem __instance)
    {
        NativeArray<Entity> entities = __instance.__query_1482480545_0.ToEntityArray(Allocator.Temp); // All Components: ProjectM.EntityOwner [ReadOnly], ProjectM.Buff [ReadOnly], ProjectM.ReplaceAbilityOnSlotData [ReadOnly], ProjectM.ReplaceAbilityOnSlotBuff [Buffer] [ReadOnly], Unity.Entities.SpawnTag [ReadOnly]
        try
        {
            foreach (Entity entity in entities)
            {
                if (!Core.hasInitialized) return;

                if (entity.GetOwner().TryGetPlayer(out Entity character))
                {
                    ulong steamId = character.GetSteamId();
                    string prefabName = entity.Read<PrefabGUID>().LookupName().ToLower();

                    bool slotSpells = prefabName.Contains("unarmed") || prefabName.Contains("fishingpole");
                    bool shiftSpell = prefabName.Contains("weapon");

                    (int FirstSlot, int SecondSlot, int ShiftSlot) spells;
                    if (ConfigService.UnarmedSlots && slotSpells && steamId.TryGetPlayerSpells(out spells))
                    {
                        HandleExtraSpells(entity, steamId, spells);
                    }
                    else if (ConfigService.ShiftSlot && shiftSpell && steamId.TryGetPlayerSpells(out spells))
                    {
                        HandleShiftSpell(entity, steamId, spells);
                    }
                    else if (!entity.Has<WeaponLevel>() && PlayerUtilities.GetPlayerBool(steamId, "SpellLock") && steamId.TryGetPlayerSpells(out spells))
                    {
                        SetSpells(entity, character, steamId, spells);
                    }
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
    static void HandleExtraSpells(Entity entity, ulong steamId, (int FirstSlot, int SecondSlot, int ShiftSlot) spells)
    {
        var buffer = entity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
        if (!spells.FirstSlot.Equals(0))
        {
            ReplaceAbilityOnSlotBuff buff = new()
            {
                Slot = 1,
                NewGroupId = new(spells.FirstSlot),
                CopyCooldown = true,
                Priority = 0,
            };

            buffer.Add(buff);
        }

        if (!spells.SecondSlot.Equals(0))
        {
            ReplaceAbilityOnSlotBuff buff = new()
            {
                Slot = 4,
                NewGroupId = new(spells.SecondSlot),
                CopyCooldown = true,
                Priority = 0,
            };

            buffer.Add(buff);
        }

        if (PlayerUtilities.GetPlayerBool(steamId, "ShiftLock") && !spells.ShiftSlot.Equals(0))
        {
            ReplaceAbilityOnSlotBuff buff = new()
            {
                Slot = 3,
                NewGroupId = new(spells.ShiftSlot),
                CopyCooldown = true,
                Priority = 0,
            };

            buffer.Add(buff);
        }
    }
    static void HandleShiftSpell(Entity entity, ulong steamId, (int FirstSlot, int SecondSlot, int ShiftSlot) spells)
    {
        var buffer = entity.ReadBuffer<ReplaceAbilityOnSlotBuff>(); // prevent people switching jewels if item with spellmod is equipped?
        if (PlayerUtilities.GetPlayerBool(steamId, "ShiftLock") && !spells.ShiftSlot.Equals(0))
        {
            ReplaceAbilityOnSlotBuff buff = new()
            {
                Slot = 3,
                NewGroupId = new(spells.ShiftSlot),
                CopyCooldown = true,
                Priority = 0,
            };

            buffer.Add(buff);
        }
    }
    static void SetSpells(Entity entity, Entity player, ulong steamId, (int FirstSlot, int SecondSlot, int ShiftSlot) spells)
    {
        var buffer = entity.ReadBuffer<ReplaceAbilityOnSlotBuff>();

        foreach (var buff in buffer)
        {
            if (buff.Slot == 5)
            {
                spells = (buff.NewGroupId.GuidHash, spells.SecondSlot, spells.ShiftSlot); // then want to check on the spell in shift and get rid of it if the same prefab, same for slot 6 below
                HandleDuplicate(entity, buff, player, steamId, spells);
            }

            if (buff.Slot == 6)
            {
                spells = (spells.FirstSlot, buff.NewGroupId.GuidHash, spells.ShiftSlot);
                HandleDuplicate(entity, buff, player, steamId, spells);
            }
        }

        steamId.SetPlayerSpells(spells);
    }
    static void HandleDuplicate(Entity entity, ReplaceAbilityOnSlotBuff buff, Entity player, ulong steamId, (int FirstSlot, int SecondSlot, int ShiftSlot) spells)
    {
        Entity abilityGroup = ServerGameManager.GetAbilityGroup(player, 3); // get ability currently on shift, if it exists and matches what was just equipped set shift to default extra spell instead

        if (abilityGroup.Exists())
        {
            PrefabGUID abilityPrefab = abilityGroup.Read<PrefabGUID>();

            if (buff.NewGroupId == abilityPrefab)
            {
                ServerGameManager.ModifyAbilityGroupOnSlot(entity, player, 3, new(ConfigService.DefaultClassSpell));
                spells.ShiftSlot = ConfigService.DefaultClassSpell;
                steamId.SetPlayerSpells(spells);
            }
        }
    }
}