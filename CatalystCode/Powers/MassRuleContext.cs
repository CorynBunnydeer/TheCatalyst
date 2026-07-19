using System.Runtime.CompilerServices;
using System.Threading;
using Catalyst.CatalystCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Catalyst.CatalystCode.Powers;

internal static class MassRuleContext
{
    private sealed class ModeBox
    {
        public required MassRuleMode Mode;
    }

    private static readonly ConditionalWeakTable<PowerModel, ModeBox> Modes = new();
    private static readonly AsyncLocal<int> PotionUseDepth = new();
    private static readonly AsyncLocal<int> SentinelNormalizationDepth = new();

    internal static bool IsPotionUse => PotionUseDepth.Value > 0;
    internal static bool IsSentinelNormalization => SentinelNormalizationDepth.Value > 0;

    internal static void EnterPotionUse() => PotionUseDepth.Value++;

    internal static void ExitPotionUse()
    {
        if (PotionUseDepth.Value > 0)
            PotionUseDepth.Value--;
    }

    internal static void EnterSentinelNormalization() => SentinelNormalizationDepth.Value++;

    internal static void ExitSentinelNormalization()
    {
        if (SentinelNormalizationDepth.Value > 0)
            SentinelNormalizationDepth.Value--;
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
        Creature? applier,
        Creature? receiver)
    {
        if (power is not ShrinkPower || amount != -1M)
            return amount;

        if (!ShouldTreatShrinkSentinelAsFinite(applier, receiver))
            return amount;

        // Bell-qualified or Grow-interacting Shrink treats the sentinel as one finite
        // stack. Potion-originated Shrink remains vanilla.
        return 1M;
    }

    internal static decimal NormalizeExistingShrinkSentinelOffset(
        PowerModel power,
        decimal offset,
        Creature? applier,
        Creature? receiver)
    {
        offset = NormalizeShrinkAmountIfQualified(power, offset, applier, receiver);

        if (power is not ShrinkPower shrink || shrink.Amount != -1M || offset == 0M)
            return offset;

        if (!ShouldTreatShrinkSentinelAsFinite(applier, receiver))
            return offset;

        // The stored -1 represents one finite Shrink in this interaction. Adding two
        // to the incoming offset changes the old amount from -1 to 1 before applying
        // the incoming change, without bypassing PowerCmd.ModifyAmount.
        return offset + 2M;
    }

    internal static void PromoteToStackIfQualified(
        PowerModel power,
        decimal amount,
        Creature? applier,
        Creature? receiver)
    {
        if (amount <= 0M || power is not (GrowPower or ShrinkPower))
            return;

        if (IsPotionUse ||
            (!HasMassDifferentialRelic(applier?.Player) &&
             !HasMassDifferentialRelic(receiver?.Player)))
            return;

        SetMode(power, MassRuleMode.Stack);
    }

    private static bool ShouldTreatShrinkSentinelAsFinite(
        Creature? applier,
        Creature? receiver)
    {
        if (IsPotionUse)
            return false;

        if (HasMassDifferentialRelic(applier?.Player) ||
            HasMassDifferentialRelic(receiver?.Player))
            return true;

        return receiver?.GetPower<GrowPower>()?.Amount > 0;
    }

    private static void SetMode(PowerModel power, MassRuleMode mode)
    {
        Modes.Remove(power);
        Modes.Add(power, new ModeBox { Mode = mode });
    }

    private static bool HasMassDifferentialRelic(Player? player) =>
        player?.GetRelic<BlueSleighBell>() is not null;

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
            Creature target,
            Creature? applier)
        {
            amount = NormalizeShrinkAmountIfQualified(power, amount, applier, target);
            PromoteToStackIfQualified(power, amount, applier, target);
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
            if (IsSentinelNormalization)
                return;

            Creature receiver = power.Owner;
            offset = NormalizeExistingShrinkSentinelOffset(
                power,
                offset,
                applier,
                receiver);
            PromoteToStackIfQualified(power, offset, applier, receiver);
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
