﻿using ProjectM;
using System.Text.Json;
using static Cobalt.Systems.Bloodline.BloodMasteryStatsSystem;
using static Cobalt.Systems.Weapon.WeaponStatsSystem;

namespace Cobalt.Core
{
    public class DataStructures
    {
        // Encapsulated fields with properties

        private static readonly JsonSerializerOptions prettyJsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        // structures to write to json for permanence

        private static Dictionary<ulong, KeyValuePair<int, float>> playerExperience = [];
        private static Dictionary<ulong, Dictionary<string, bool>> playerBools = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerWoodcutting = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerMining = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerFishing = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerBlacksmithing = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerTailoring = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerJewelcrafting = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerAlchemy = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerHarvesting = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerSwordMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerAxeMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerMaceMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerSpearMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerCrossbowMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerGreatSwordMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerSlashersMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerPistolsMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerReaperMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerLongbowMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerWhipMastery = [];
        private static Dictionary<ulong, KeyValuePair<int, float>> playerBloodMastery = [];
        private static Dictionary<ulong, Dictionary<int, PlayerWeaponStats>> playerWeaponStats = [];
        private static Dictionary<ulong, BloodMasteryStats> playerBloodlineStats = [];

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerExperience
        {
            get => playerExperience;
            set => playerExperience = value;
        }

        public static Dictionary<ulong, Dictionary<string, bool>> PlayerBools
        {
            get => playerBools;
            set => playerBools = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerWoodcutting
        {
            get => playerWoodcutting;
            set => playerWoodcutting = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerMining
        {
            get => playerMining;
            set => playerMining = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerFishing
        {
            get => playerFishing;
            set => playerFishing = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerBlacksmithing
        {
            get => playerBlacksmithing;
            set => playerBlacksmithing = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerTailoring
        {
            get => playerTailoring;
            set => playerTailoring = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerJewelcrafting
        {
            get => playerJewelcrafting;
            set => playerJewelcrafting = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerAlchemy
        {
            get => playerAlchemy;
            set => playerAlchemy = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerHarvesting
        {
            get => playerHarvesting;
            set => playerHarvesting = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerSwordMastery
        {
            get => playerSwordMastery;
            set => playerSwordMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerAxeMastery
        {
            get => playerAxeMastery;
            set => playerAxeMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerMaceMastery
        {
            get => playerMaceMastery;
            set => playerMaceMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerSpearMastery
        {
            get => playerSpearMastery;
            set => playerSpearMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerCrossbowMastery
        {
            get => playerCrossbowMastery;
            set => playerCrossbowMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerGreatSwordMastery
        {
            get => playerGreatSwordMastery;
            set => playerGreatSwordMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerSlashersMastery
        {
            get => playerSlashersMastery;
            set => playerSlashersMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerPistolsMastery
        {
            get => playerPistolsMastery;
            set => playerPistolsMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerReaperMastery
        {
            get => playerReaperMastery;
            set => playerReaperMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerLongbowMastery
        {
            get => playerLongbowMastery;
            set => playerLongbowMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerWhipMastery
        {
            get => playerWhipMastery;
            set => playerWhipMastery = value;
        }

        public static Dictionary<ulong, KeyValuePair<int, float>> PlayerBloodMastery
        {
            get => playerBloodMastery;
            set => playerBloodMastery = value;
        }

        public static Dictionary<ulong, Dictionary<int, PlayerWeaponStats>> PlayerWeaponStats
        {
            get => playerWeaponStats;
            set => playerWeaponStats = value;
        }

        public static Dictionary<ulong, BloodMasteryStats> PlayerBloodStats
        {
            get => playerBloodlineStats;
            set => playerBloodlineStats = value;
        }

        // cache-only
        private static Dictionary<ulong, Dictionary<PrefabGUID, bool>> playerCraftingJobs = [];

        public static Dictionary<ulong, Dictionary<PrefabGUID, bool>> PlayerCraftingJobs
        {
            get => playerCraftingJobs;
            set => playerCraftingJobs = value;
        }

        // file paths dictionary
        private static readonly Dictionary<string, string> filePaths = new()
        {
            {"Experience", Plugin.PlayerExperienceJson},
            {"PlayerBools", Plugin.PlayerBoolsJson},
            {"Woodcutting", Plugin.PlayerWoodcuttingJson},
            {"Mining", Plugin.PlayerMiningJson},
            {"Fishing", Plugin.PlayerFishingJson},
            {"Blacksmithing", Plugin.PlayerBlacksmithingJson},
            {"Tailoring", Plugin.PlayerTailoringJson},
            {"Jewelcrafting", Plugin.PlayerJewelcraftingJson},
            {"Alchemy", Plugin.PlayerAlchemyJson},
            {"Harvesting", Plugin.PlayerHarvestingJson},
            {"SwordMastery", Plugin.PlayerSwordMasteryJson },
            {"AxeMastery", Plugin.PlayerAxeMasteryJson},
            {"MaceMastery", Plugin.PlayerMaceMasteryJson},
            {"SpearMastery", Plugin.PlayerSpearMasteryJson},
            {"CrossbowMastery", Plugin.PlayerCrossbowMasteryJson},
            {"GreatSwordMastery", Plugin.PlayerGreatSwordMastery},
            {"SlashersMastery", Plugin.PlayerSlashersMasteryJson},
            {"PistolsMastery", Plugin.PlayerPistolsMasteryJson},
            {"ReaperMastery", Plugin.PlayerReaperMastery},
            {"LongbowMastery", Plugin.PlayerLongbowMasteryJson},
            {"WhipMastery", Plugin.PlayerWhipMasteryJson},
            {"BloodMastery", Plugin.PlayerBloodMasteryJson},
            {"WeaponStats", Plugin.PlayerWeaponStatsJson},
            {"BloodStats", Plugin.PlayerBloodStatsJson}
        };

        // Generic method to save any type of dictionary.

        public static void LoadData<T>(ref Dictionary<ulong, T> dataStructure, string key)
        {
            string path = filePaths[key];
            if (!File.Exists(path))
            {
                // If the file does not exist, create a new empty file to avoid errors on initial load.
                File.Create(path).Dispose();
                dataStructure = []; // Initialize as empty if file does not exist.
                Plugin.Log.LogInfo($"{key} file created as it did not exist.");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<Dictionary<ulong, T>>(json, prettyJsonOptions);
                dataStructure = data ?? []; // Ensure non-null assignment.
                Plugin.Log.LogInfo($"{key} data loaded successfully.");
            }
            catch (IOException ex)
            {
                Plugin.Log.LogError($"Error reading {key} data from file: {ex.Message}");
                dataStructure = []; // Provide default empty dictionary on error.
            }
            catch (JsonException ex)
            {
                Plugin.Log.LogError($"JSON deserialization error when loading {key} data: {ex.Message}");
                dataStructure = []; // Provide default empty dictionary on error.
            }
        }

        public static void LoadPlayerExperience() => LoadData(ref playerExperience, "Experience");

        public static void LoadPlayerBools() => LoadData(ref playerBools, "PlayerBools");

        public static void LoadPlayerWoodcutting() => LoadData(ref playerWoodcutting, "Woodcutting");

        public static void LoadPlayerMining() => LoadData(ref playerMining, "Mining");

        public static void LoadPlayerFishing() => LoadData(ref playerFishing, "Fishing");

        public static void LoadPlayerBlacksmithing() => LoadData(ref playerBlacksmithing, "Blacksmithing");

        public static void LoadPlayerTailoring() => LoadData(ref playerTailoring, "Tailoring");

        public static void LoadPlayerJewelcrafting() => LoadData(ref playerJewelcrafting, "Jewelcrafting");

        public static void LoadPlayerAlchemy() => LoadData(ref playerAlchemy, "Alchemy");

        public static void LoadPlayerHarvesting() => LoadData(ref playerHarvesting, "Harvesting");

        public static void LoadPlayerSwordMastery() => LoadData(ref playerSwordMastery, "SwordMastery");

        public static void LoadPlayerAxeMastery() => LoadData(ref playerAxeMastery, "AxeMastery");

        public static void LoadPlayerMaceMastery() => LoadData(ref playerMaceMastery, "MaceMastery");

        public static void LoadPlayerSpearMastery() => LoadData(ref playerSpearMastery, "SpearMastery");

        public static void LoadPlayerCrossbowMastery() => LoadData(ref playerCrossbowMastery, "CrossbowMastery");

        public static void LoadPlayerGreatSwordMastery() => LoadData(ref playerGreatSwordMastery, "GreatSwordMastery");

        public static void LoadPlayerSlashersMastery() => LoadData(ref playerSlashersMastery, "SlashersMastery");

        public static void LoadPlayerPistolsMastery() => LoadData(ref playerPistolsMastery, "PistolsMastery");

        public static void LoadPlayerReaperMastery() => LoadData(ref playerReaperMastery, "ReaperMastery");

        public static void LoadPlayerLongbowMastery() => LoadData(ref playerLongbowMastery, "LongbowMastery");

        public static void LoadPlayerWhipMastery() => LoadData(ref playerWhipMastery, "WhipMastery");

        public static void LoadPlayerBloodMastery() => LoadData(ref playerBloodMastery, "BloodMastery");

        public static void LoadPlayerWeaponStats() => LoadData(ref playerWeaponStats, "WeaponStats");

        public static void LoadPlayerBloodStats() => LoadData(ref playerBloodlineStats, "BloodStats");

        public static void SaveData<T>(Dictionary<ulong, T> data, string key)
        {
            string path = filePaths[key];
            try
            {
                string json = JsonSerializer.Serialize(data, prettyJsonOptions);
                File.WriteAllText(path, json);
                //Plugin.Log.LogInfo($"{key} data saved successfully.");
            }
            catch (IOException ex)
            {
                Plugin.Log.LogError($"Failed to write {key} data to file: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Plugin.Log.LogError($"JSON serialization error when saving {key} data: {ex.Message}");
            }
        }

        public static void SavePlayerExperience() => SaveData(PlayerExperience, "Experience");

        public static void SavePlayerBools() => SaveData(PlayerBools, "PlayerBools");

        public static void SavePlayerWoodcutting() => SaveData(PlayerWoodcutting, "Woodcutting");

        public static void SavePlayerMining() => SaveData(PlayerMining, "Mining");

        public static void SavePlayerFishing() => SaveData(PlayerFishing, "Fishing");

        public static void SavePlayerBlacksmithing() => SaveData(PlayerBlacksmithing, "Blacksmithing");

        public static void SavePlayerTailoring() => SaveData(PlayerTailoring, "Tailoring");

        public static void SavePlayerJewelcrafting() => SaveData(PlayerJewelcrafting, "Jewelcrafting");

        public static void SavePlayerAlchemy() => SaveData(PlayerAlchemy, "Alchemy");

        public static void SavePlayerHarvesting() => SaveData(PlayerHarvesting, "Harvesting");

        public static void SavePlayerSwordMastery() => SaveData(PlayerSwordMastery, "SwordMastery");

        public static void SavePlayerAxeMastery() => SaveData(PlayerAxeMastery, "AxeMastery");

        public static void SavePlayerMaceMastery() => SaveData(PlayerMaceMastery, "MaceMastery");

        public static void SavePlayerSpearMastery() => SaveData(PlayerSpearMastery, "SpearMastery");

        public static void SavePlayerCrossbowMastery() => SaveData(PlayerCrossbowMastery, "CrossbowMastery");

        public static void SavePlayerGreatSwordMastery() => SaveData(PlayerGreatSwordMastery, "GreatSwordMastery");

        public static void SavePlayerSlashersMastery() => SaveData(PlayerSlashersMastery, "SlashersMastery");

        public static void SavePlayerPistolsMastery() => SaveData(PlayerPistolsMastery, "PistolsMastery");

        public static void SavePlayerReaperMastery() => SaveData(PlayerReaperMastery, "ReaperMastery");

        public static void SavePlayerLongbowMastery() => SaveData(PlayerLongbowMastery, "LongbowMastery");

        public static void SavePlayerWhipMastery() => SaveData(PlayerWhipMastery, "WhipMastery");

        public static void SavePlayerBloodMastery() => SaveData(PlayerBloodMastery, "BloodMastery");

        public static void SavePlayerWeaponStats() => SaveData(PlayerWeaponStats, "WeaponStats");

        public static void SavePlayerBloodStats() => SaveData(PlayerBloodStats, "BloodStats");
    }
}