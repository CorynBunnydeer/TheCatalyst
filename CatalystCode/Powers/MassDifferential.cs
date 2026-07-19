using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Shared rules for the mod's mass axis.
///
/// Grow is mod-owned, while Shrink belongs to the base game. Their intensified
/// stack-scaling only appears on powers that have been promoted into Stack mode.
/// </summary>
internal static class MassDifferential
{
    private const string ShrinkDamageDecreaseKey = "DamageDecrease";
    private const decimal VanillaShrinkDamageDecrease = 30M;
    private const decimal VanillaGrowDamageIncrease = 30M;
    private const double ScaleTweenSeconds = 0.75;
    private const float VisualScalePerDoubling = 0.25F;

    internal static async Task ReconcileOpposingStacks(
        PlayerChoiceContext choiceContext,
        Creature owner,
        Creature? applier,
        CardModel? cardSource)
    {
        if (MassRuleContext.IsSentinelNormalization)
            return;

        GrowPower? grow = owner.GetPower<GrowPower>();
        ShrinkPower? shrink = owner.GetPower<ShrinkPower>();

        // Nanairo Grow turns Shrinker Beetle's base-game sentinel into one finite Shrink
        // before cancellation. Use the command path so normal amount-change bookkeeping
        // remains intact, while the context guard prevents recursive reconciliation.
        if (grow is not null && shrink is not null && shrink.Amount == -1 &&
            grow.Amount > 0 &&
            !MassRuleContext.IsPotionUse)
        {
            MassRuleContext.EnterSentinelNormalization();
            try
            {
                await PowerCmd.ModifyAmount(
                    choiceContext,
                    shrink,
                    2M,
                    applier,
                    cardSource,
                    false);
            }
            finally
            {
                MassRuleContext.ExitSentinelNormalization();
            }
        }

        // Negative amounts are special source-linked, infinite powers. They retain their
        // vanilla semantics and do not participate in finite mass cancellation.
        if (grow is null || shrink is null || grow.Amount <= 0 || shrink.Amount <= 0)
            return;

        int cancelled = Math.Min(grow.Amount, shrink.Amount);

        if (grow.Amount == shrink.Amount)
        {
            await PowerCmd.Remove(shrink);
            await PowerCmd.Remove(grow);
            return;
        }

        if (grow.Amount > shrink.Amount)
        {
            await RemoveOrReducePower(
                choiceContext,
                shrink,
                cancelled,
                applier,
                cardSource);
            await RemoveOrReducePower(
                choiceContext,
                grow,
                cancelled,
                applier,
                cardSource);
            return;
        }

        await RemoveOrReducePower(
            choiceContext,
            grow,
            cancelled,
            applier,
            cardSource);
        await RemoveOrReducePower(
            choiceContext,
            shrink,
            cancelled,
            applier,
            cardSource);
    }

    internal static async Task ApplyShrinkWithCancellation(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource,
        bool silent = false)
    {
        if (amount <= 0M)
            return;

        GrowPower? grow = target.GetPower<GrowPower>();
        if (grow is { Amount: > 0 })
        {
            decimal cancelled = Math.Min(grow.Amount, amount);
            await RemoveOrReducePower(
                choiceContext,
                grow,
                cancelled,
                applier,
                cardSource);
            amount -= cancelled;
        }

        if (amount > 0M)
        {
            await PowerCmd.Apply<ShrinkPower>(
                choiceContext,
                target,
                amount,
                applier,
                cardSource,
                silent);
        }
    }

    internal static void Refresh(Creature owner, bool animate = true)
    {
        GrowPower? grow = owner.GetPower<GrowPower>();
        ShrinkPower? shrink = owner.GetPower<ShrinkPower>();

        if (grow is not null)
        {
            grow.DynamicVars[GrowPower.DamageIncreaseKey].BaseValue =
                GetGrowDamageIncrease(grow);
        }

        if (shrink is not null)
            RefreshShrink(shrink);

        ScaleCreature(
            owner,
            CalculateScale(GetVisualGrowAmount(grow), GetVisualShrinkAmount(shrink)),
            animate ? ScaleTweenSeconds : 0D);
    }

    private static async Task RemoveOrReducePower(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0M)
            return;

        if (amount >= power.Amount)
        {
            await PowerCmd.Remove(power);
            return;
        }

        await PowerCmd.ModifyAmount(
            choiceContext,
            power,
            -amount,
            applier,
            cardSource,
            false);
    }

    internal static void RefreshShrink(ShrinkPower shrink)
    {
        if (MassRuleContext.GetMode(shrink) != MassRuleMode.Stack || shrink.Amount <= 0)
        {
            shrink.DynamicVars[ShrinkDamageDecreaseKey].BaseValue =
                VanillaShrinkDamageDecrease;
            return;
        }

        decimal multiplier = 1M /
            (1M + GrowPower.DamageIncreasePerStack / 100M * shrink.Amount);
        decimal damageDecrease = (1M - multiplier) * 100M;

        // Two decimals keep the smart description readable while remaining accurate
        // enough for the game's damage calculation.
        shrink.DynamicVars[ShrinkDamageDecreaseKey].BaseValue =
            decimal.Round(damageDecrease, 2, MidpointRounding.AwayFromZero);

    }

    private static decimal GetGrowDamageIncrease(GrowPower grow)
    {
        if (grow.IsInfinite)
            return GrowPower.InfiniteDamageIncrease;

        return MassRuleContext.GetMode(grow) == MassRuleMode.Stack
            ? GrowPower.DamageIncreasePerStack * grow.Amount
            : VanillaGrowDamageIncrease;
    }

    private static int? GetVisualGrowAmount(GrowPower? grow)
    {
        if (grow is null)
            return null;

        if (grow.Amount <= 0)
            return grow.Amount;

        return MassRuleContext.GetMode(grow) == MassRuleMode.Stack ? grow.Amount : 1;
    }

    private static int? GetVisualShrinkAmount(ShrinkPower? shrink)
    {
        if (shrink is null)
            return null;

        if (shrink.Amount <= 0)
            return shrink.Amount;

        return MassRuleContext.GetMode(shrink) == MassRuleMode.Stack ? shrink.Amount : 1;
    }

    private static float CalculateScale(int? growAmount, int? shrinkAmount)
    {
        if (growAmount < 0)
            return 1.5F;

        if (shrinkAmount < 0)
            return 0.5F;

        int netMass = Math.Max(growAmount ?? 0, 0) - Math.Max(shrinkAmount ?? 0, 0);
        if (netMass == 0)
            return 1F;

        float magnitude = 1F +
            VisualScalePerDoubling * MathF.Log2(1F + Math.Abs(netMass));

        return netMass > 0 ? magnitude : 1F / magnitude;
    }

    private static void ScaleCreature(Creature owner, float scale, double duration)
    {
        if (owner.Monster is Vantom vantom)
        {
            vantom.ScaleTo(scale, (float)duration);
            return;
        }

        NCombatRoom.Instance?
            .GetCreatureNode(owner)?
            .ScaleTo(scale, duration);
    }
}
