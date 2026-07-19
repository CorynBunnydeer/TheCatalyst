using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// A temporary increase in mass. Positive stacks increase the owner's Attack damage and
/// naturally decay by one at the end of the owner's turn.
///
/// Grow and positive Shrink cancel one another one-for-one. Negative Grow is reserved as
/// the same "infinite" sentinel that the base game's Shrink power uses.
/// </summary>
public class GrowPower : CatalystPower
{
    public const decimal DamageIncreasePerStack = 15M;
    public const decimal InfiniteDamageIncrease = 30M;

    internal const string DamageIncreaseKey = "DamageIncrease";
    internal const string DamageIncreasePerStackKey = "DamageIncreasePerStack";

    public override PowerType Type => PowerType.Buff;

    public override bool AllowNegative => true;

    public bool IsInfinite => Amount < 0;

    public override PowerStackType StackType =>
        IsInfinite ? PowerStackType.Single : PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(DamageIncreaseKey, DamageIncreasePerStack),
        new DynamicVar(DamageIncreasePerStackKey, DamageIncreasePerStack)
    ];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        MassDifferential.Refresh(Owner);
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        MassDifferential.Refresh(oldOwner);
        return Task.CompletedTask;
    }

    /// <summary>
    /// STS2 calls this hook for both first-time applications and changes to an existing
    /// power. That makes it the reliable place to update visuals and reconcile opposing
    /// Grow/Shrink stacks.
    /// </summary>
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power.Owner != Owner || power is not (GrowPower or ShrinkPower))
            return;

        await MassDifferential.ReconcileOpposingStacks(
            choiceContext,
            Owner,
            applier,
            cardSource);

        MassDifferential.Refresh(Owner);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (IsInfinite || !participants.Contains(Owner))
            return;

        await PowerCmd.Decrement(this);
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner != dealer || !props.IsPoweredAttack())
            return 1M;

        return (100M + DynamicVars[DamageIncreaseKey].BaseValue) / 100M;
    }
}
