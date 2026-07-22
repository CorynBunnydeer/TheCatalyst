using Catalyst.CatalystCode.Cards.Statuses;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Hidden queue for Lust copies that enter Hand before the next normal draw.
/// </summary>
public sealed class LustNextTurnPower : CatalystPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override bool ShouldPlayVfx => false;

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (Owner.Player != player || AmountOnTurnStart <= 0)
            return;

        int count = AmountOnTurnStart;

        // Remove before resolving so Lust queued during later hooks belongs to
        // the following turn instead of this draw.
        await PowerCmd.Remove(this);

        List<CardModel> lustCards = new(count);
        for (int i = 0; i < count; i++)
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
    }
}
