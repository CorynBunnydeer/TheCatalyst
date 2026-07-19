using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.Skills;

/// <summary>X-cost Skill that applies Grow five times per Energy spent.</summary>
public class UraniumSnack() : CatalystCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<GrowPower>(5M)];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.FromPower<GrowPower>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        int xValue = ResolveEnergyXValue();
        for (int i = 0; i < xValue; i++)
        {
            await CommonActions.ApplySelf<GrowPower>(
                choiceContext,
                this,
                DynamicVars.Power<GrowPower>().BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
