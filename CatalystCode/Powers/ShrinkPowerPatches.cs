using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Narrow patches that let Nanairo-linked instances of the base game's Shrink power use
/// the Mass Differential rules without changing Shrink in unrelated combats.
/// </summary>
internal static class ShrinkPowerPatches
{
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount))]
    private static class AmountChangedPatch
    {
        [HarmonyPostfix]
        private static void RefreshShrink(PowerModel power)
        {
            if (power is ShrinkPower shrink)
                MassDifferential.Refresh(shrink.Owner);
        }
    }

    [HarmonyPatch(typeof(ShrinkPower), nameof(ShrinkPower.AfterApplied))]
    private static class AppliedPatch
    {
        [HarmonyPostfix]
        private static void RefreshStackModeShrink(ShrinkPower __instance)
        {
            // Let vanilla Shrink finish its normal setup, then immediately refresh the
            // owner from Catalyst's mode-aware rules. Fully replacing AfterApplied
            // caused Tail Whip's first Stack-mode Shrink application to crash.
            if (MassRuleContext.GetMode(__instance) != MassRuleMode.Stack ||
                __instance.Amount <= 0)
                return;

            MassDifferential.Refresh(__instance.Owner, animate: false);
        }
    }

    [HarmonyPatch(typeof(ShrinkPower), nameof(ShrinkPower.AfterRemoved))]
    private static class RemovedPatch
    {
        [HarmonyPostfix]
        private static void RefreshAfterRemoval(Creature oldOwner)
        {
            MassDifferential.Refresh(oldOwner);
        }
    }
}
