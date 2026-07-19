using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Catalyst.CatalystCode.Cards.Skills;

/// <summary>Applies self-Shrink immediately and queues Grow through Concentrate.</summary>
public class Duping() : CatalystCard(
    2,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShrinkPower>(1M),
        new PowerVar<GrowPower>(5M)
    ];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
    [
        HoverTipFactory.FromPower<ShrinkPower>(),
        HoverTipFactory.FromPower<GrowPower>(),
        HoverTipFactory.Static(CatalystKeywords.Concentrate)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await MassDifferential.ApplyShrinkWithCancellation(
            choiceContext,
            Owner.Creature,
            DynamicVars.Power<ShrinkPower>().BaseValue,
            Owner.Creature,
            this);

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
