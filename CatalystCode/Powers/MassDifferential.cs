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

        // ((REFERENCE)) Nanairo design: Grow/Shrink cancellation is mod-owned. STS2's
        // Creature.GetPower<T>() is the standard way to retrieve mutable Power instances
        // before using PowerCmd.Remove or PowerCmd.ModifyAmount.
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
            await PowerCmd.Remove(shrink);
            await PowerCmd.ModifyAmount(
                choiceContext,
                grow,
                -cancelled,
                applier,
                cardSource,
                false);
            return;
        }

        await PowerCmd.Remove(grow);
        await PowerCmd.ModifyAmount(
            choiceContext,
            shrink,
            -cancelled,
            applier,
            cardSource,
            false);
    }

    internal static void Refresh(Creature owner, bool animate = true)
    {
        // ((REFERENCE)) STS2: DynamicVars are mutable per Power instance. Updating the
        // DamageIncrease value here makes smartDescription and damage calculation read
        // the same derived number without replacing the Power model.
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

    internal static void RefreshShrink(ShrinkPower shrink)
    {
        // ((REFERENCE)) STS2: base Shrink exposes its computed reduction through the
        // DynamicVar key "DamageDecrease". This key and vanilla 30% value were taken
        // from MegaCrit.Sts2.Core.Models.Powers.ShrinkPower on the beta branch.
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
        // ((REFERENCE)) STS2: Vantom is a special case in the base Shrink implementation
        // and owns visual scaling on its MonsterModel, so it must use Vantom.ScaleTo.
        // Vantom owns its visual scaling on the model rather than through the ordinary
        // NCreature path. This mirrors the base game's Shrink implementation.
        if (owner.Monster is Vantom vantom)
        {
            vantom.ScaleTo(scale, (float)duration);
            return;
        }

        // ((REFERENCE)) STS2: ordinary combatants are scaled through their NCreature
        // node obtained from NCombatRoom.Instance.GetCreatureNode(...), matching the
        // base Shrink visual path.
        NCombatRoom.Instance?
            .GetCreatureNode(owner)?
            .ScaleTo(scale, duration);
    }
}
