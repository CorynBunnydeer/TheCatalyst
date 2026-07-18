using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using HarmonyLib;
using Catalyst.CatalystCode.Relics;

namespace Catalyst.CatalystCode.Powers;

internal static class MassRuleContext
{
    private sealed class ModeBox
    {
        public required MassRuleMode Mode;
    }

    private static readonly ConditionalWeakTable<PowerModel, ModeBox> Modes = new();
    private static readonly AsyncLocal<int> PotionUseDepth = new();

    internal static bool IsPotionUse => PotionUseDepth.Value > 0;

    internal static void EnterPotionUse() => PotionUseDepth.Value++;

    internal static void ExitPotionUse()
    {
        if (PotionUseDepth.Value > 0)
            PotionUseDepth.Value--;
    }

    internal static MassRuleMode GetMode(PowerModel power)
    {
        if (Modes.TryGetValue(power, out ModeBox? box))
            return box.Mode;

        return MassRuleMode.Vanilla;
    }

    internal static decimal NormalizeShrinkAmountIfQualified(
        PowerModel power,
        decimal amount,
        Creature? applier)
    {
        if (power is not ShrinkPower)
            return amount;

        if (amount != -1M)
            return amount;

        if (!HasMassDifferentialRelic(applier?.Player))
            return amount;

        if (IsPotionUse)
            return amount;

        // Bell-qualified Shrink should treat the base game's -1 sentinel as one
        // finite Mass Differential stack instead of the vanilla infinite special case.
        return 1M;
    }

    internal static void PromoteToStackIfQualified(
        PowerModel power,
        decimal amount,
        Creature? applier)
    {
        if (amount <= 0M)
            return;

        if (!IsGrowOrShrink(power))
            return;

        if (!HasMassDifferentialRelic(applier?.Player))
            return;

        if (IsPotionUse)
            return;

        SetMode(power, MassRuleMode.Stack);
    }

    private static void SetMode(PowerModel power, MassRuleMode mode)
    {
        Modes.Remove(power);
        Modes.Add(power, new ModeBox { Mode = mode });
    }

    private static bool IsGrowOrShrink(PowerModel power) =>
        power is GrowPower or ShrinkPower;

    private static bool HasMassDifferentialRelic(Player? player)
    {
        return player?.GetRelic<BlueSleighBell>() is not null;
    }

    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply),
        typeof(PlayerChoiceContext),
        typeof(PowerModel),
        typeof(Creature),
        typeof(decimal),
        typeof(Creature),
        typeof(CardModel),
        typeof(bool))]
    private static class PowerApplyPatch
    {
        [HarmonyPrefix]
        private static void PromoteModeBeforeApply(
            PowerModel power,
            ref decimal amount,
            Creature? applier)
        {
            amount = NormalizeShrinkAmountIfQualified(power, amount, applier);
            PromoteToStackIfQualified(power, amount, applier);
        }
    }

    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount))]
    private static class PowerModifyPatch
    {
        [HarmonyPrefix]
        private static void PromoteModeBeforeModify(
            PowerModel power,
            ref decimal offset,
            Creature? applier)
        {
            offset = NormalizeShrinkAmountIfQualified(power, offset, applier);
            PromoteToStackIfQualified(power, offset, applier);
        }
    }

    [HarmonyPatch(typeof(PotionModel), nameof(PotionModel.OnUseWrapper))]
    private static class PotionUseWrapperPatch
    {
        [HarmonyPrefix]
        private static void EnterPotionScope()
        {
            EnterPotionUse();
        }

        [HarmonyPostfix]
        private static void ExitPotionScope()
        {
            ExitPotionUse();
        }
    }
}
