﻿using Cobalt.Hooks;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;
using static Cobalt.Systems.Expertise.WeaponStats.WeaponStatManager;

namespace Cobalt.Systems.Expertise
{
    public class BloodSystem
    {
        private static readonly int UnitMultiplier = Plugin.UnitExpertiseMultiplier.Value; // Expertise points multiplier from normal units
        public static readonly int MaxBloodLevel = Plugin.MaxExpertiseLevel.Value; // maximum level
        private static readonly int VBloodMultiplier = Plugin.VBloodExpertiseMultiplier.Value; // Expertise points multiplier from VBlood units
        private static readonly float BloodConstant = 0.1f; // constant for calculating level from xp
        private static readonly int BloodXPPower = 2; // power for calculating level from xp

        public enum BloodType
        {
            Worker,
            Warrior,
            Scholar,
            Rogue,
            Mutant,
            VBlood,
            None,
            GateBoss,
            Draculin,
            DraculaTheImmortal,
            Creature,
            Brute
        }

        public static void UpdateBloodline(EntityManager entityManager, Entity Killer, Entity Victim)
        {
            if (Killer == Victim || entityManager.HasComponent<Minion>(Victim)) return;

            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(Killer).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;
            BloodSystem.BloodType bloodType = ModifyUnitStatBuffUtils.GetCurrentBloodType(Killer);
            if (bloodType.Equals(BloodType.None)) return;
            if (entityManager.HasComponent<UnitStats>(Victim))
            {
                var VictimStats = entityManager.GetComponentData<UnitStats>(Victim);
                float BloodValue = CalculateBloodValue(VictimStats, entityManager.HasComponent<VBloodConsumeSource>(Victim));

                IBloodHandler handler = BloodHandlerFactory.GetBloodHandler(bloodType);
                if (handler != null)
                {
                    // Check if the player leveled up
                    var xpData = handler.GetExperienceData(steamID);
                    float newExperience = xpData.Value + BloodValue;
                    int newLevel = ConvertXpToLevel(newExperience);
                    bool leveledUp = false;

                    if (newLevel > xpData.Key)
                    {
                        leveledUp = true;
                        if (newLevel > MaxBloodLevel)
                        {
                            newLevel = MaxBloodLevel;
                            newExperience = ConvertLevelToXp(MaxBloodLevel);
                        }
                    }
                    var updatedXPData = new KeyValuePair<int, float>(newLevel, newExperience);
                    handler.UpdateExperienceData(steamID, updatedXPData);
                    handler.SaveChanges();
                    NotifyPlayer(entityManager, user, bloodType, BloodValue, leveledUp, newLevel, handler);
                }
            }
        }

        private static float CalculateBloodValue(UnitStats VictimStats, bool isVBlood)
        {
            float WeaponExpertiseValue = VictimStats.SpellPower + VictimStats.PhysicalPower;
            if (isVBlood) return WeaponExpertiseValue * VBloodMultiplier;
            return WeaponExpertiseValue * UnitMultiplier;
        }

        public static void NotifyPlayer(EntityManager entityManager, User user, BloodSystem.BloodType bloodType, float gainedXP, bool leveledUp, int newLevel, IBloodHandler handler)
        {
            ulong steamID = user.PlatformId;
            gainedXP = (int)gainedXP; // Convert to integer if necessary
            int levelProgress = GetLevelProgress(steamID, handler); // Calculate the current progress to the next level

            string message;

            if (leveledUp)
            {
                Entity character = user.LocalCharacter._Entity;
                Equipment equipment = character.Read<Equipment>();
                message = $"<color=#c0c0c0>{bloodType}</color> improved to [<color=white>{newLevel}</color>]";
                ServerChatUtils.SendSystemMessageToClient(entityManager, user, message);
            }
            else
            {
                if (Core.DataStructures.PlayerBools.TryGetValue(steamID, out var bools) && bools["ExpertiseLogging"])
                {
                    message = $"+<color=yellow>{gainedXP}</color> <color=red>{bloodType}</color> expertise (<color=white>{levelProgress}%</color>)";
                    ServerChatUtils.SendSystemMessageToClient(entityManager, user, message);
                }
            }
        }

        public static int GetLevelProgress(ulong steamID, IBloodHandler handler)
        {
            float currentXP = GetXp(steamID, handler);
            int currentLevel = GetLevel(steamID, handler);
            int nextLevelXP = ConvertLevelToXp(currentLevel + 1);
            //Plugin.Log.LogInfo($"Lv: {currentLevel} | xp: {currentXP} | toNext: {nextLevelXP}");
            int percent = (int)(currentXP / nextLevelXP * 100);
            return percent;
        }

        public static int ConvertXpToLevel(float xp)
        {
            // Assuming a basic square root scaling for experience to level conversion
            return (int)(BloodConstant * Math.Sqrt(xp));
        }

        public static int ConvertLevelToXp(int level)
        {
            // Reversing the formula used in ConvertXpToLevel for consistency
            return (int)Math.Pow(level / BloodConstant, BloodXPPower);
        }

        private static float GetXp(ulong steamID, IBloodHandler handler)
        {
            var xpData = handler.GetExperienceData(steamID);
            return xpData.Value;
        }

        private static int GetLevel(ulong steamID, IBloodHandler handler)
        {
            return ConvertXpToLevel(GetXp(steamID, handler));
        }

        public static BloodType GetBloodTypeFromPrefab(PrefabGUID blood)
        {
            string bloodCheck = blood.LookupName().ToString().ToLower();
            foreach (BloodType type in Enum.GetValues(typeof(BloodType)))
            {
                if (bloodCheck.Contains(type.ToString().ToLower()))
                {
                    return type;
                }
            }
            throw new InvalidOperationException("Unrecognized blood type");
        }
    }
}