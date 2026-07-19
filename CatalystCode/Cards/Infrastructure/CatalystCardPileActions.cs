using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

public static class CatalystCardPileActions
{
    private const float DrawPileStageOffset = 360f;
    private const float MaximumStackSpread = 72f;
    private const float MaximumStackStep = 18f;

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

    private static async Task StageOutgoingCards(
        IReadOnlyList<CardModel> outgoingCards)
    {
        if (outgoingCards.Count == 0 ||
            !LocalContext.IsMe(outgoingCards[0].Owner) ||
            NCombatRoom.Instance is not { } combatRoom)
        {
            return;
        }

        List<NCard> cardNodes = outgoingCards
            .Select(card => NCard.FindOnTable(card))
            .Where(node => node is not null &&
                           GodotObject.IsInstanceValid(node) &&
                           node.IsInsideTree())
            .Cast<NCard>()
            .ToList();
        if (cardNodes.Count == 0)
            return;

        float duration = GetPileTweenDuration();
        float totalStartStagger = cardNodes.Count > 1
            ? duration * 0.6f
            : 0f;
        float startStagger = cardNodes.Count > 1
            ? totalStartStagger / (cardNodes.Count - 1)
            : 0f;
        float stackStep = cardNodes.Count > 1
            ? Math.Min(MaximumStackStep, MaximumStackSpread / (cardNodes.Count - 1))
            : 0f;
        float stackCenter = (cardNodes.Count - 1) * 0.5f;

        Tween tween = combatRoom.CreateTween().SetParallel();
        for (int index = 0; index < cardNodes.Count; index++)
        {
            NCard cardNode = cardNodes[index];
            float horizontalStackOffset = (index - stackCenter) * stackStep;
            Vector2 targetPosition =
                PileType.Play.GetTargetPosition(cardNode) +
                Vector2.Left * DrawPileStageOffset +
                Vector2.Right * horizontalStackOffset;

            tween.TweenProperty(cardNode, "position", targetPosition, duration)
                .SetDelay(index * startStagger)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
        }

        tween.Play();
        await tween.AwaitFinished(combatRoom);
    }

    private static float GetPileTweenDuration()
    {
        return SaveManager.Instance.PrefsSave.FastMode switch
        {
            FastModeType.Instant => 0.01f,
            FastModeType.Fast => 0.1f,
            _ => 0.25f,
        };
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

        await StageOutgoingCards([outgoingCard]);

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

        await StageOutgoingCards(setAsideCards);

        foreach (CardModel _ in setAsideCards)
            await DrawRandom(choiceContext, owner);

        int switchedCount = 0;
        for (int index = 0; index < setAsideCards.Count; index++)
        {
            CardModel outgoingCard = setAsideCards[index];
            if (await InsertIntoRandomDrawPile(outgoingCard))
            {
                switchedCount++;
            }
            else
            {
                MainFile.Logger.Info(
                    $"Warning: SwitchCards failed to reinsert card '{outgoingCard}' into Draw.",
                    1);
            }

            if (index < setAsideCards.Count - 1)
            {
                float divisor = Math.Max(1, setAsideCards.Count - 1);
                await Cmd.CustomScaledWait(0.05f / divisor, 0.125f / divisor);
            }
        }

        return switchedCount;
    }
}
