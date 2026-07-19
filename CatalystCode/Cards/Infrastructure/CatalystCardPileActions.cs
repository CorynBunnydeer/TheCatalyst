using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

public static class CatalystCardPileActions
{
    public static async Task<bool> InsertIntoRandomDrawPile(CardModel card)
    {
        CardPileAddResult result = await CardPileCmd.Add(
            card,
            PileType.Draw.GetPile(card.Owner),
            CardPilePosition.Random,
            null,
            false);
        return result.success;
    }

    public static async Task<CardModel?> DrawRandom(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        await CardPileCmd.ShuffleIfNecessary(choiceContext, player);

        CardPile drawPile = PileType.Draw.GetPile(player);
        if (drawPile.Cards.Count == 0)
            return null;

        CardModel? chosenCard = player.RunState.Rng.CombatCardSelection.NextItem(drawPile.Cards);
        if (chosenCard is null)
            return null;

        drawPile.MoveToTopInternal(chosenCard);
        drawPile.InvokeContentsChanged();
        return await CardPileCmd.Draw(choiceContext, player);
    }

    public static async Task<bool> SwitchCard(
        PlayerChoiceContext choiceContext,
        CardModel outgoingCard)
    {
        CardPile playPile = PileType.Play.GetPile(outgoingCard.Owner);
        CardPileAddResult setAsideResult = await CardPileCmd.Add(
            outgoingCard,
            playPile,
            CardPilePosition.Bottom,
            null,
            false);
        if (!setAsideResult.success)
            return false;

        await DrawRandom(choiceContext, outgoingCard.Owner);

        return await InsertIntoRandomDrawPile(outgoingCard);
    }

    public static async Task<int> SwitchCards(
        PlayerChoiceContext choiceContext,
        IReadOnlyList<CardModel> outgoingCards)
    {
        if (outgoingCards.Count == 0)
            return 0;

        Player owner = outgoingCards[0].Owner;
        if (outgoingCards.Any(card => card.Owner != owner))
            throw new ArgumentException(
                "All outgoing cards must have the same owner.",
                nameof(outgoingCards));

        // Give the played card a moment to finish its own pile movement before the
        // visible Switch sequence starts moving other cards around.
        await Cmd.CustomScaledWait(0.15f, 0.25f, false, default);

        List<CardModel> setAsideCards = [];
        CardPile playPile = PileType.Play.GetPile(owner);

        foreach (CardModel outgoingCard in outgoingCards)
        {
            CardPileAddResult result = await CardPileCmd.Add(
                outgoingCard,
                playPile,
                CardPilePosition.Bottom,
                null,
                false);
            if (result.success)
                setAsideCards.Add(outgoingCard);
        }

        await Cmd.CustomScaledWait(0.1f, 0.25f, false, default);

        foreach (CardModel _ in setAsideCards)
            await DrawRandom(choiceContext, owner);

        await Cmd.CustomScaledWait(0.1f, 0.25f, false, default);

        int switchedCount = 0;
        foreach (CardModel outgoingCard in setAsideCards)
        {
            if (await InsertIntoRandomDrawPile(outgoingCard))
            {
                switchedCount++;
                continue;
            }

            MainFile.Logger.Info(
                $"Warning: SwitchCards failed to reinsert card '{outgoingCard}' into Draw.",
                1);
        }

        return switchedCount;
    }
}
