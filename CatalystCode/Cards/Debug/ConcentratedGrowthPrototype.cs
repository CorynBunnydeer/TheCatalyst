using BaseLib.Extensions;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>
/// Development-only, untargeted proof of the Concentrate timing and interruption rule.
/// </summary>
public class ConcentratedGrowthPrototype() : CatalystDebugCard(
    1,
    CardType.Skill,
    CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.Static(CatalystKeywords.Concentrate)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<GrowPower>(2M)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await ConcentrationCmd.QueueGrow(
            choiceContext,
            Owner.Creature,
            DynamicVars.Power<GrowPower>().BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
    }
}
