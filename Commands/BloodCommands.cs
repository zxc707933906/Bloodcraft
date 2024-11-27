using Bloodcraft.Services;
using Bloodcraft.Systems.Legacies;
using Bloodcraft.Utilities;
using ProjectM;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Entities;
using VampireCommandFramework;
using static Bloodcraft.Services.PlayerService;
using static Bloodcraft.Systems.Legacies.BloodManager;
using static VCF.Core.Basics.RoleCommands;
using User = ProjectM.Network.User;

namespace Bloodcraft.Commands;

[CommandGroup("bloodlegacy", "bl")]
internal static class BloodCommands
{
    static EntityManager EntityManager => Core.EntityManager;
    static ServerGameManager ServerGameManager => Core.ServerGameManager;

    [Command(name: "get", adminOnly: false, usage: ".bl get [BloodType]", description: "Display your current blood legacy progress.")]
    public static void GetLegacyCommand(ChatCommandContext ctx, string blood = "")
    {
        if (!ConfigService.BloodSystem)
        {
            LocalizationService.HandleReply(ctx, "Blood Legacies are not enabled.");
            return;
        }

        Entity character = ctx.Event.SenderCharacterEntity;
        Blood playerBlood = character.Read<Blood>();
        BloodType bloodType = GetCurrentBloodType(character);

        if (string.IsNullOrEmpty(blood))
        {
            bloodType = BloodSystem.GetBloodTypeFromPrefab(playerBlood.BloodType);
        }
        else if (!Enum.TryParse<BloodType>(blood, true, out bloodType))
        {
            LocalizationService.HandleReply(ctx, "Invalid blood type, use '.bl l' to see options.");
            return;
        }

        ulong steamID = ctx.Event.User.PlatformId;
        IBloodHandler bloodHandler = BloodHandlerFactory.GetBloodHandler(bloodType);

        if (bloodHandler == null)
        {
            LocalizationService.HandleReply(ctx, "Invalid blood type.");
            return;
        }

        var data = bloodHandler.GetLegacyData(steamID);
        int progress = (int)(data.Value - BloodSystem.ConvertLevelToXp(data.Key));

        int prestigeLevel = steamID.TryGetPlayerPrestiges(out var prestiges) ? prestiges[BloodSystem.BloodTypeToPrestigeMap[bloodType]] : 0;

        if (data.Key > 0)
        {
            LocalizationService.HandleReply(ctx, $"你当前的血型等级为 [<color=white>{data.Key}</color>][<color=#90EE90>{prestigeLevel}</color>]  <color=yellow>{progress}</color> <color=#FFC0CB>essence</color> (<color=white>{BloodSystem.GetLevelProgress(steamID, bloodHandler)}%</color>)  <color=red>{bloodHandler.GetBloodType()}</color>");

            if (steamID.TryGetPlayerBloodStats(out var bloodStats) && bloodStats.TryGetValue(bloodType, out var stats))
            {
                List<KeyValuePair<BloodStats.BloodStatType, string>> bonusBloodStats = [];

                foreach (var stat in stats)
                {
                    float bonus = CalculateScaledBloodBonus(bloodHandler, steamID, bloodType, stat);
                    string bonusString = (bonus * 100).ToString("F0") + "%";
                    bonusBloodStats.Add(new KeyValuePair<BloodStats.BloodStatType, string>(stat, bonusString));
                }

                for (int i = 0; i < bonusBloodStats.Count; i += 6)
                {
                    var batch = bonusBloodStats.Skip(i).Take(6);
                    string bonuses = string.Join(", ", batch.Select(stat => $"<color=#00FFFF>{stat.Key}</color>: <color=white>{stat.Value}</color>"));
                    LocalizationService.HandleReply(ctx, $"当前的血型加成: {bonuses}");
                }
            }
            else
            {
                LocalizationService.HandleReply(ctx, "没有转生加成.");
            }
        }
        else
        {
            LocalizationService.HandleReply(ctx, $"No progress in <color=red>{bloodHandler.GetBloodType()}</color> yet.");
        }
    }

    [Command(name: "log", adminOnly: false, usage: ".bl log", description: "Toggles Legacy progress logging.")]
    public static void LogLegacyCommand(ChatCommandContext ctx)
    {
        if (!ConfigService.BloodSystem)
        {
            LocalizationService.HandleReply(ctx, "Blood Legacies are not enabled.");
            return;
        }

        var SteamID = ctx.Event.User.PlatformId;

        PlayerUtilities.TogglePlayerBool(SteamID, "BloodLogging");
        LocalizationService.HandleReply(ctx, $"Blood Legacy logging {(PlayerUtilities.GetPlayerBool(SteamID, "BloodLogging") ? "<color=green>enabled</color>" : "<color=red>disabled</color>")}.");
    }

    [Command(name: "choosestat", shortHand: "cst", adminOnly: false, usage: ".bl cst [Blood] [BloodStat]", description: "选择一个血型加成属性.")]
    public static void ChooseBloodStat(ChatCommandContext ctx, string blood, string statType)
    {
        if (!ConfigService.BloodSystem)
        {
            LocalizationService.HandleReply(ctx, "Legacies are not enabled.");
            return;
        }

        if (!Enum.TryParse<BloodStats.BloodStatType>(statType, true, out var StatType))
        {
            LocalizationService.HandleReply(ctx, "无效的选择, 输入 '.bl lst' 查看可选属性.");
            return;
        }

        if (!Enum.TryParse<BloodType>(blood, true, out var BloodType))
        {
            LocalizationService.HandleReply(ctx, "无效的选择, use '.bl l' 查看可选属性.");
            return;
        }

        ulong steamID = ctx.Event.User.PlatformId;

        if (BloodType.Equals(BloodType.GateBoss) || BloodType.Equals(BloodType.None) || BloodType.Equals(BloodType.VBlood))
        {
            LocalizationService.HandleReply(ctx, $"No legacy available for <color=white>{BloodType}</color>.");
            return;
        }

        if (ChooseStat(steamID, BloodType, StatType))
        {
            LocalizationService.HandleReply(ctx, $"<color=#00FFFF>{StatType}</color> 已被选择 <color=red>{BloodType}</color> 重新吸血获得加成.");

            Entity player = ctx.Event.SenderCharacterEntity;
            BloodType bloodType = GetCurrentBloodType(player);

            //UpdateBloodStats(player, bloodType);
        }
        else
        {
            LocalizationService.HandleReply(ctx, $"You have already chosen {ConfigService.LegacyStatChoices} stats for this legacy, the stat has already been chosen for this legacy, or the stat is not allowed for your class.");
        }
    }

    [Command(name: "resetstats", shortHand: "rst", adminOnly: false, usage: ".bl rst", description: "Reset stats for current blood.")]
    public static void ResetBloodStats(ChatCommandContext ctx)
    {
        if (!ConfigService.BloodSystem)
        {
            LocalizationService.HandleReply(ctx, "Legacies are not enabled.");
            return;
        }

        Entity character = ctx.Event.SenderCharacterEntity;
        User user = ctx.Event.User;
        ulong steamID = user.PlatformId;
        BloodType bloodType = GetCurrentBloodType(character);

        if (bloodType.Equals(BloodType.GateBoss) || bloodType.Equals(BloodType.None) || bloodType.Equals(BloodType.VBlood))
        {
            LocalizationService.HandleReply(ctx, $"No legacy available for <color=white>{bloodType}</color>.");
            return;
        }

        if (!ConfigService.ResetLegacyItem.Equals(0))
        {
            PrefabGUID item = new(ConfigService.ResetLegacyItem);
            int quantity = ConfigService.ResetLegacyItemQuantity;

            if (InventoryUtilities.TryGetInventoryEntity(EntityManager, ctx.User.LocalCharacter._Entity, out Entity inventoryEntity) && ServerGameManager.GetInventoryItemCount(inventoryEntity, item) >= quantity)
            {
                if (ServerGameManager.TryRemoveInventoryItem(inventoryEntity, item, quantity))
                {
                    ResetStats(steamID, bloodType);
                    //UpdateBloodStats(character, bloodType);

                    LocalizationService.HandleReply(ctx, $"血型已重置 <color=red>{bloodType}</color>.");
                }
            }
            else
            {
                LocalizationService.HandleReply(ctx, $"没有足够的物品来重置血型 (<color=#ffd9eb>{item.GetPrefabName()}</color> x<color=white>{quantity}</color>)");
            }
        }
        else
        {
            ResetStats(steamID, bloodType);
            //UpdateBloodStats(character, bloodType);

            LocalizationService.HandleReply(ctx, $"Your blood stats have been reset for <color=red>{bloodType}</color>.");
        }
    }

    [Command(name: "liststats", shortHand: "lst", adminOnly: false, usage: ".bl lst", description: "Lists blood stats available.")]
    public static void ListBloodStatsAvailable(ChatCommandContext ctx)
    {
        if (!ConfigService.BloodSystem)
        {
            LocalizationService.HandleReply(ctx, "Legacies are not enabled.");
            return;
        }

        var bloodStatsWithCaps = Enum.GetValues(typeof(BloodStats.BloodStatType))
            .Cast<BloodStats.BloodStatType>()
            .Select(stat =>
                $"<color=#00FFFF>{stat}</color>: <color=white>{BloodStats.BloodStatValues[stat]}</color>")
            .ToArray();

        int halfLength = bloodStatsWithCaps.Length / 2;

        string bloodStatsLine1 = string.Join(", ", bloodStatsWithCaps.Take(halfLength));
        string bloodStatsLine2 = string.Join(", ", bloodStatsWithCaps.Skip(halfLength));

        LocalizationService.HandleReply(ctx, $"可用的血型属性选择 (1/2): {bloodStatsLine1}");
        LocalizationService.HandleReply(ctx, $"可用的血型属性选择 (2/2): {bloodStatsLine2}");
    }

    [Command(name: "set", adminOnly: true, usage: ".bl set [Player] [Blood] [Level]", description: "Sets player Blood Legacy level.")]
    public static void SetBloodLegacyCommand(ChatCommandContext ctx, string name, string blood, int level)
    {
        if (!ConfigService.BloodSystem)
        {
            LocalizationService.HandleReply(ctx, "Blood Legacies are not enabled.");
            return;
        }

        PlayerInfo playerInfo = PlayerCache.FirstOrDefault(kvp => kvp.Key.ToLower() == name.ToLower()).Value;
        if (!playerInfo.UserEntity.Exists())
        {
            ctx.Reply($"Couldn't find player.");
            return;
        }

        User foundUser = playerInfo.User;
        if (level < 0 || level > ConfigService.MaxBloodLevel)
        {
            LocalizationService.HandleReply(ctx, $"Level must be between 0 and {ConfigService.MaxBloodLevel}.");
            return;
        }

        if (!Enum.TryParse<BloodType>(blood, true, out var bloodType))
        {
            LocalizationService.HandleReply(ctx, "Invalid blood legacy.");
            return;
        }

        var BloodHandler = BloodHandlerFactory.GetBloodHandler(bloodType);
        if (BloodHandler == null)
        {
            LocalizationService.HandleReply(ctx, "Invalid blood legacy.");
            return;
        }

        ulong steamId = foundUser.PlatformId;
        var xpData = new KeyValuePair<int, float>(level, BloodSystem.ConvertLevelToXp(level));
        BloodHandler.SetLegacyData(steamId, xpData);

        LocalizationService.HandleReply(ctx, $"<color=red>{BloodHandler.GetBloodType()}</color> legacy set to <color=white>{level}</color> for <color=green>{foundUser.CharacterName}</color>");
    }

    [Command(name: "list", shortHand: "l", adminOnly: false, usage: ".bl l", description: "Lists blood legacies available.")]
    public static void ListBloodTypesCommand(ChatCommandContext ctx)
    {
        if (!ConfigService.BloodSystem)
        {
            LocalizationService.HandleReply(ctx, "Blood Legacies are not enabled.");
            return;
        }

        var excludedBloodTypes = new List<BloodType> { BloodType.None, BloodType.VBlood, BloodType.GateBoss };
        var bloodTypes = Enum.GetValues(typeof(BloodType))
                              .Cast<BloodType>()
                              .Where(b => !excludedBloodTypes.Contains(b))
                              .Select(b => b.ToString());

        string bloodTypesList = string.Join(", ", bloodTypes);
        LocalizationService.HandleReply(ctx, $"Available Blood Legacies: <color=red>{bloodTypesList}</color>");
    }
}
