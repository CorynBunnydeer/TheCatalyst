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
    // ((REFERENCE)) STS2: CardModel.CanonicalKeywords is the normal declaration point
    // for built-in keyword behavior. CardKeyword.Exhaust makes the played Battery move
    // to the Exhaust pile through the ordinary card-play result flow.
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    // ((REFERENCE)) BaseLib: PowerVar<T> gives the card a typed dynamic variable that
    // can be displayed as !GrowPower! in localization and retrieved with
    // DynamicVars.Power<T>() via BaseLib.Extensions.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<GrowPower>(1M)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // ((REFERENCE)) BaseLib: CommonActions.ApplySelf<TPower> wraps the current
        // PowerCmd.Apply flow while supplying this card, its owner, and multiplayer
        // choice context consistently.
        await CommonActions.ApplySelf<GrowPower>(
            choiceContext,
            this,
            DynamicVars.Power<GrowPower>().BaseValue);

        // ((REFERENCE)) Mod helper: CreateInHand<T> is defined on CatalystCard and
        // follows STS2's generated-card insertion pattern used by base-game cards.
        await CreateInHand<DeadBattery>(IsUpgraded);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Power<GrowPower>().UpgradeValueBy(1M);  
    }
}
