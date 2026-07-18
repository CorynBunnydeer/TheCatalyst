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
///
/// In summary: Only applies on mod-effect if origin is cards from the Blue Sleigh
/// Bell owner. Otherwise, behaviour will be vanilla.
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
        GrowPower? grow = owner.GetPower<GrowPower>();
        ShrinkPower? shrink = owner.GetPower<ShrinkPower>();

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
        // DynamicVars are mutable per Power instance. Updating the
        // DamageIncrease value here makes smartDescription and damage calculation read
        // the same derived number without replacing the Power model.
        GrowPower? grow = owner.GetPower<GrowPower>();
        ShrinkPower? shrink = owner.GetPower<ShrinkPower>();

        if (grow is not null)
        {
            grow.DynamicVars[GrowPower.DamageIncreaseKey].BaseValue = GetGrowDamageIncrease(grow);
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
        // base Shrink exposes its computed reduction through the
        // DynamicVar key "DamageDecrease". This key and vanilla 30% value were taken
        // from MegaCrit.Sts2.Core.Models.Powers.ShrinkPower on the beta branch.
        if (MassRuleContext.GetMode(shrink) != MassRuleMode.Stack || shrink.Amount <= 0)
        {
            shrink.DynamicVars[ShrinkDamageDecreaseKey].BaseValue = VanillaShrinkDamageDecrease;
            return;
        }

        decimal multiplier = 1M / (1M + GrowPower.DamageIncreasePerStack / 100M * shrink.Amount);
        decimal damageDecrease = (1M - multiplier) * 100M;

        // Two decimals keep the smart description readable while remaining accurate
        // enough for the game's damage calculation.
        // TODO: I will see if I can find something a bit more... "linear" that doesn't involve decimals later.
        shrink.DynamicVars[ShrinkDamageDecreaseKey].BaseValue =  decimal.Round(damageDecrease, 2, MidpointRounding.AwayFromZero);
    }

    private static float CalculateScale(int? growAmount, int? shrinkAmount)
    {
        if (growAmount < 0) return 1.5F;

        if (shrinkAmount < 0) return 0.5F;

        int netMass = Math.Max(growAmount ?? 0, 0) - Math.Max(shrinkAmount ?? 0, 0);
        if (netMass == 0) return 1F;

        float magnitude = 1F + VisualScalePerDoubling * MathF.Log2(1F + Math.Abs(netMass));

        return netMass > 0 ? magnitude : 1F / magnitude;
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
        if (grow is null) return null;

        if (grow.Amount <= 0) return grow.Amount;

        return MassRuleContext.GetMode(grow) == MassRuleMode.Stack ? grow.Amount : 1;
    }

    private static int? GetVisualShrinkAmount(ShrinkPower? shrink)
    {
        if (shrink is null) return null;

        if (shrink.Amount <= 0) return shrink.Amount;

        return MassRuleContext.GetMode(shrink) == MassRuleMode.Stack ? shrink.Amount : 1;
    }

    private static void ScaleCreature(Creature owner, float scale, double duration)
    {
        // As seen on decompiled Shrink class...
        // Vantom is a special case in the base Shrink implementation
        // and owns visual scaling on its MonsterModel, so it must use Vantom.ScaleTo.
        // Vantom owns its visual scaling on the model rather than through the ordinary
        // NCreature path. 
        if (owner.Monster is Vantom vantom)
        {
            vantom.ScaleTo(scale, (float)duration);
            return;
        }

        // normal one
        NCombatRoom.Instance?
            .GetCreatureNode(owner)?
            .ScaleTo(scale, duration);
    }
}
