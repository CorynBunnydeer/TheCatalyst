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

    // ((REFERENCE)) STS2: base Shrink uses a negative Amount as an infinite/sentinel
    // state. Grow mirrors that convention so source-linked infinite effects can retain
    // the game's established Power semantics.
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
        // ((REFERENCE)) STS2: PowerModel.AfterApplied is the standard post-application
        // hook. We use it to recalculate dynamic values and creature scale after the
        // game has assigned Owner, Applier, Amount, and other mutable Power state.
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
        // ((REFERENCE)) STS2: AbstractModel.AfterPowerAmountChanged is emitted for both
        // initial application and later amount changes. It is therefore safer than
        // updating visuals only in AfterApplied when stacks can change repeatedly.
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
        // ((REFERENCE)) STS2: AfterSideTurnEnd + participants.Contains(Owner) is the
        // standard actor-turn duration pattern. PowerCmd.Decrement performs synchronized
        // amount changes and removes the Power when its normal finite duration expires.
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
        // ((REFERENCE)) STS2: AbstractModel.ModifyDamageMultiplicative is the current
        // powered-damage hook. ValuePropExtensions.IsPoweredAttack filters out recoil,
        // HP loss, and other Unpowered damage. dealer identifies the attacking owner.
        if (Owner != dealer || !props.IsPoweredAttack())
            return 1M;

        return (100M + DynamicVars[DamageIncreaseKey].BaseValue) / 100M;
    }
}
