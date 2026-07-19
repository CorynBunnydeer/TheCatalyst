using BaseLib.Utils;
using BaseLib.Extensions;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.Tokens.Props;

/// <summary>
/// A charged physical Prop. Nanairo converts the stored electricity into body mass,
/// leaving the depleted casing available as a projectile.
/// </summary>
public class Battery() : CatalystPropCard(1, CardType.Skill, TargetType.Self)
{
    public override PropStage Stage => PropStage.Base;

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.FromCard<DeadBattery>(IsUpgraded)];
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<GrowPower>(1M)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<GrowPower>(
            choiceContext,
            this,
            DynamicVars.Power<GrowPower>().BaseValue);

        await CreateInHand<DeadBattery>(IsUpgraded);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Power<GrowPower>().UpgradeValueBy(1M);  
    }
}
