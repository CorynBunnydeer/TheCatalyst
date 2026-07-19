using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Character;
using Catalyst.CatalystCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>
/// This is the base class for your mod's cards, which is set up to load the card's images from your mod's resources.
/// When creating a card, right click the Cards folder and create a new file with the Custom Card template.
/// This will generate a class that extends this one.
/// You can also just create the class manually; just make sure to inherit from this class.
/// </summary>
// ((REFERENCE)) BaseLib: PoolAttribute registers every derived custom card with
// CatalystCardPool unless a more-derived class supplies another PoolAttribute (Props
// override this with STS2's TokenCardPool).
[Pool(typeof(CatalystCardPool))]
public abstract class CatalystCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    // ((REFERENCE)) BaseLib: CustomCardModel is the custom-content replacement for
    // inheriting CardModel directly. It participates in BaseLib model registration,
    // localization analysis, custom image paths, and pool injection.
    CustomCardModel(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public override string BetaPortraitPath =>
        $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    /// <summary>
    /// Additional card-preview tips supplied by individual Catalyst cards.
    /// </summary>
    protected virtual IEnumerable<IHoverTip> CanonicalHoverTips => [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        base.ExtraHoverTips.Concat(CanonicalHoverTips);

    /// <summary>
    /// Creates a custom card for this card's owner and adds it to Hand.
    /// This wraps STS2's mutable combat-card creation and synchronized generated-card
    /// insertion path used by cards such as Shiv and Manifest Authority.
    /// </summary>
    protected async Task<CardModel> CreateInHand<T>(bool upgraded = false)
        where T : CardModel
    {
        ICombatState combatState = CombatState ??
            throw new InvalidOperationException(
                "Cards can only be generated during combat.");

        CardModel generated = combatState.CreateCard(ModelDb.Card<T>(), Owner);

        if (upgraded)
            generated.UpgradeInternal();

        await CardPileCmd.AddGeneratedCardToCombat(
            generated,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
        return generated;
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation result = base.GetResultLocationForCardPlay();

        if (result.pileType == PileType.Discard && Keywords.Contains(CatalystKeywords.Switch))
        {
            return new CardLocation(result.player, PileType.Draw, CardPilePosition.Random);
        }

        return result;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (!ReferenceEquals(cardPlay.Card, this)) return;

        if (!Keywords.Contains(CatalystKeywords.Switch)) return;

        if (cardPlay.ResultPile != PileType.Draw) return;

        if (cardPlay.PlayIndex != cardPlay.PlayCount - 1) return;

        await CatalystCardPileActions.DrawRandom(choiceContext, Owner);
    }
}
