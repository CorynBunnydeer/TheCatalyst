using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;


namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>Card-pile operations used by Catalyst cards but absent from STS2 commands.</summary>
public static class CatalystCardPileActions
{
    /// <summary>
    /// Move an existing combat card second from the top of its owner's Draw Pile.
    /// An empty Draw Pile necessarily leaves the card as its only/top card.
    /// </summary>
    public static async Task<bool> AddSecondFromTop(CardModel card)
    {
        CardPile drawPile = PileType.Draw.GetPile(card.Owner);
        CardModel? previousTop = drawPile.Cards.FirstOrDefault();


