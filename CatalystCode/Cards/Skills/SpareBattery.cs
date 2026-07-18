using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Cards.Tokens.Props;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Skills;



// [ ] Fill in OnUpgrade
//     Attack example: DynamicVars.Damage.UpgradeValueBy(3);
//     Block example: DynamicVars.Block.UpgradeValueBy(3);

// [ ] Add card to starter deck or another test path so it can appear in-game
// [ ] Build for code-only changes; publish when localization/images changed

 public class SpareBattery() : CatalystCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> CanonicalHoverTips => 
        HoverTipFactory.FromCardWithCardHoverTips<Battery>(IsUpgraded);
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5M, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await CreateInHand<Battery>(IsUpgraded);
    }   

    protected override void OnUpgrade()
    {
        
    }
}
