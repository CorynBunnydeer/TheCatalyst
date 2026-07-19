using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

using Catalyst.CatalystCode.Cards.Infrastructure;

namespace Catalyst.CatalystCode.Cards.Basics;

// [ ] Add card to starter deck or another test path so it can appear in-game
// [ ] Build for code-only changes; publish when localization/images changed


public class DefendCatalyst() : CatalystCard(
    1,
    CardType.Skill,
    CardRarity.Basic,
    TargetType.Self)
{

    public override bool GainsBlock => true;
    
    protected override HashSet<CardTag> CanonicalTags
    {
        get => new HashSet<CardTag>() { CardTag.Defend };
    }

    
    // ((REFERENCE)) STS2: a BlockVar with Move properties follows the ordinary card
    // Block pipeline, including Dexterity and other move-Block modifiers.
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // ((REFERENCE)) BaseLib: CommonActions.CardBlock applies the card's standard
        // BlockVar to its owner through the current game command API.
        await CommonActions.CardBlock(this, cardPlay);
    }

    protected override void OnUpgrade()
    { 
        DynamicVars.Block.UpgradeValueBy(3);
    } 
}
