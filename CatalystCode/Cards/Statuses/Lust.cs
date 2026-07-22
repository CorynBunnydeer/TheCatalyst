using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Statuses;

/// <summary>
/// A self-propagating Status that can be suppressed by spending Energy to put
/// it on the bottom of the Draw Pile.
/// </summary>
[Pool(typeof(StatusCardPool))]
public sealed class Lust() : CatalystCard(
    1,
    CardType.Status,
    CardRarity.Status,
    TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public override bool HasTurnEndInHandEffect => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<GrowPower>(1M),
        new HpLossVar(1M)
    ];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.FromPower<GrowPower>()];

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // The suppression effect is the card's result location below.
        return Task.CompletedTask;
    }

    protected override CardLocation GetResultLocationForCardPlay() =>
        new(Owner, PileType.Draw, CardPilePosition.Bottom);

    protected override async Task OnTurnEndInHand(
        PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<GrowPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Power<GrowPower>().BaseValue,
            Owner.Creature,
            this,
            false);

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this,
            null);

        if (CombatState is not { } combatState || Owner.Creature.IsDead)
            return;

        await PowerCmd.Apply<LustNextTurnPower>(
            choiceContext,
            Owner.Creature,
            1M,
            Owner.Creature,
            this,
            false);

        CardModel drawPileLust = combatState.CreateCard(
            ModelDb.Card<Lust>(),
            Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            drawPileLust,
            PileType.Draw,
            Owner,
            CardPilePosition.Random);
    }
}
