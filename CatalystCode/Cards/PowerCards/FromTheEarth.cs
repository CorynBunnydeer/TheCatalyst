using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.PowerCards;

public class FromTheEarth() : CatalystCard(
    1,
    CardType.Power,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
    [
        HoverTipFactory.Static(CatalystKeywords.Concentrate),
        HoverTipFactory.FromPower<GrowPower>(),
        HoverTipFactory.FromPower<FloatingPower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<GrowPower>(1M),
        new PowerVar<FloatingPower>(2M)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<FromTheEarthPower>(choiceContext, this, 1M);
        await ConcentrationCmd.Queue(
            choiceContext,
            Owner.Creature,
            ConcentrationEffect.FromTheEarth(1),
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
