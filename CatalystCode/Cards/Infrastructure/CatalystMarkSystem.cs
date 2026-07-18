using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

public static class CatalystMarkSystem
{
    private const int DefaultMaxMarks = 1;
    private static readonly Dictionary<Player, List<CardModel>> markedCardsByPlayer = new();

    //Using this "Buffer" function here in case I decide to actually add the animated stuff;
    public static Task MarkCard(CardModel card)
    {
        ApplyMarkToCard(card);
        return Task.CompletedTask;
    }
    
    public static void ApplyMarkToCard(CardModel card)
    {
   
        //Checking if list already exists, if does not, thrusts it there >:3
        //Othewise, variable already exists in posterior code as markedCards
        if (!markedCardsByPlayer.TryGetValue(card.Owner, out List<CardModel>? markedCards))
        {
            markedCards = new List<CardModel>();
            markedCardsByPlayer.Add(card.Owner, markedCards);
        }

        //Guard against duplication, nya...
        if (markedCards.Contains(card))
        {
            return;
        }
        
        //For if we decide to unlimit Mark amounts or change it; For starters its 1 o:
        int markCount = markedCards.Count;
        if (markCount >= DefaultMaxMarks)
        {
            RemoveMarkFromCard(markedCards[0], markedCards);
        }
        
        CardCmd.ApplyKeyword(card, CatalystKeywords.Mark);
        
        markedCards.Add(card);
    }

    public static void RemoveMarkFromCard(CardModel card, List<CardModel> markedCards)
    {
        markedCards.Remove(card);
        //Safeguards, nya..
        if (!card.Keywords.Contains(CatalystKeywords.Mark))
        {
            return;
        }
        //removing card from the custom list AND the keyword from the card
        CardCmd.RemoveKeyword(card, CatalystKeywords.Mark);
    }

   //simple return
    public static IReadOnlyList<CardModel> GetMarkedCards(Player player)
    {
        if (!markedCardsByPlayer.TryGetValue(player, out List<CardModel>? markedCards))
        {
            return Array.Empty<CardModel>();
        }

        return markedCards;
    }

    //simple return
    public static int GetMaxMarks(Player player)
    {
        return DefaultMaxMarks; //TEMPORARY, till we define the rest later, if we use this
    }


    //iteration on drawpile to get position of marked cards; +1 due to index 0
    public static int? GetDrawDepth(CardModel card)
    {
        CardPile drawPile = PileType.Draw.GetPile(card.Owner);

        for (int index = 0; index < drawPile.Cards.Count; index++)
        {
            if (ReferenceEquals(drawPile.Cards[index], card))
                return index + 1;
        }

        return null;
    }
}