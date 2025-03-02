using Bloodcraft.Services;
using Bloodcraft.Systems.Legacies;
using Bloodcraft.Systems.Professions;
using Bloodcraft.Utilities;
using HarmonyLib;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace Bloodcraft.Patches;

[HarmonyPatch]
internal static class DeathEventListenerSystemPatch
{
    static readonly bool Familiars = ConfigService.FamiliarSystem;
    static readonly bool Legacies = ConfigService.BloodSystem;
    static readonly bool Professions = ConfigService.ProfessionSystem;
    public class DeathEventArgs : EventArgs
    {
        public Entity Source { get; set; }
        public Entity Target { get; set; }
        public HashSet<Entity> DeathParticipants { get; set; }
    }

    public static event EventHandler<DeathEventArgs> OnDeathEvent;
    static void RaiseDeathEvent(DeathEventArgs deathEvent)
    {
        OnDeathEvent?.Invoke(null, deathEvent);
    }

    [HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
    [HarmonyPostfix]
    static void OnUpdatePostfix(DeathEventListenerSystem __instance)
    {
        if (!Core.hasInitialized) return;

        NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
        try
        {
            foreach (DeathEvent deathEvent in deathEvents)
            {
                if (!ValidateTarget(deathEvent)) continue;
                else if (deathEvent.Died.Has<Movement>())
                {
                    Entity deathSource = ValidateSource(deathEvent.Killer);

                    if (deathSource.Exists())
                    {
                        DeathEventArgs deathArgs = new()
                        {
                            Source = deathSource,
                            Target = deathEvent.Died,
                            DeathParticipants = PlayerUtilities.GetDeathParticipants(deathSource)
                        };

                        RaiseDeathEvent(deathArgs);

                        if (!Legacies) continue;
                        else if (deathEvent.StatChangeReason.Equals(StatChangeReason.HandleGameplayEventsBase_11)) BloodSystem.ProcessLegacy(deathArgs.Source, deathArgs.Target);
                    }
                }
                else if (Professions && deathEvent.Killer.IsPlayer())
                {
                    ProfessionSystem.UpdateProfessions(deathEvent.Killer, deathEvent.Died);
                }
            }
        }
        finally
        {
            deathEvents.Dispose();
        }
    }
    static Entity ValidateSource(Entity source)
    {
        Entity deathSource = Entity.Null;

        if (source.IsPlayer()) return source; // player kills

        if (!source.TryGetComponent(out EntityOwner entityOwner)) return deathSource;
        else if (entityOwner.Owner.TryGetPlayer(out Entity player)) deathSource = player; // player familiar and player summon kills
        else if (entityOwner.Owner.TryGetFollowedPlayer(out Entity followedPlayer)) deathSource = followedPlayer; // player familiar summon kills

        return deathSource;
    }
    static bool ValidateTarget(DeathEvent deathEvent)
    {
        if (Familiars && deathEvent.Died.TryGetFollowedPlayer(out Entity player)) // auto-clear active if familiar dies for easier rebinding
        {
            ulong steamId = player.GetSteamId();

            if (steamId.TryGetFamiliarActives(out var actives) && actives.FamKey.Equals(deathEvent.Died.Read<PrefabGUID>().GuidHash))
            {
                FamiliarUtilities.ClearFamiliarActives(steamId);

                return false;
            }
        }
        else if (deathEvent.Died.Has<VBloodConsumeSource>() || deathEvent.Killer == deathEvent.Died) return false;
        else if (deathEvent.Died.Has<Minion>() || deathEvent.Died.Has<Trader>() || deathEvent.Died.Has<BlockFeedBuff>()) return false;
        else if (!deathEvent.Died.Has<UnitLevel>()) return false;

        return true;
    }
}