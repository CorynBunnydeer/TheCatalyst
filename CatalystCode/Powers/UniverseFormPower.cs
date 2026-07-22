using BaseLib.Extensions;
using Catalyst.CatalystCode.Cards.PowerCards;
using Catalyst.CatalystCode.Cards.Statuses;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Tracks Universe Form's escalating turn-start payload.
/// </summary>
public class UniverseFormPower : CatalystPower
{
    public const string GrowAmountKey = "GrowAmount";

    private const decimal StartingGrowPerCopy = 2M;
    private const decimal GrowIncreasePerCopy = 2M;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(GrowAmountKey, StartingGrowPerCopy)
    ];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        decimal startingGrow = GetStartingGrow(cardSource);

        // PowerCmd may apply more than one initial stack. Represent the same
        // result as that many independent, newly played Universe Forms.
        DynamicVars[GrowAmountKey].BaseValue = startingGrow * Amount;

        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (!ReferenceEquals(power, this) || amount <= 0M)
            return Task.CompletedTask;

        int addedCopies = (int)amount;
        int previousCopies = Amount - addedCopies;
        if (addedCopies <= 0 || previousCopies <= 0)
        {
            // The first application was initialized by AfterApplied.
            return Task.CompletedTask;
        }

        decimal startingGrow = GetStartingGrow(cardSource);
        DynamicVars[GrowAmountKey].BaseValue += startingGrow * addedCopies;
        InvokeDisplayAmountChanged();

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartLate(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner.Player != player || Amount <= 0)
            return;

        decimal grow = DynamicVars[GrowAmountKey].BaseValue;

        await PowerCmd.Apply<GrowPower>(
            choiceContext,
            Owner,
            grow,
            Owner,
            null,
            false);

        ICombatState combatState = CombatState;
        List<CardModel> lustCards = new(Amount);
        for (int i = 0; i < Amount; i++)
        {
            lustCards.Add(combatState.CreateCard(
                ModelDb.Card<Lust>(),
                player));
        }

        await CardPileCmd.AddGeneratedCardsToCombat(
            lustCards,
            PileType.Hand,
            player,
            CardPilePosition.Bottom);

        DynamicVars[GrowAmountKey].BaseValue +=
            GrowIncreasePerCopy * Amount;
        InvokeDisplayAmountChanged();
    }

    private static decimal GetStartingGrow(
        CardModel? cardSource)
    {
        if (cardSource is UniverseForm card)
            return card.DynamicVars.Power<GrowPower>().BaseValue;

        return StartingGrowPerCopy;
    }
}
