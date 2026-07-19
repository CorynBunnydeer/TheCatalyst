using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.Debug;

public class GravitationalReleasePrototype() : CatalystDebugCard(
    1,
    CardType.Skill,
    CardRarity.Token,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<GrowPower>(2M),
        new PowerVar<FloatingPower>(2M)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<GrowPower>(
            choiceContext,
            this,
            DynamicVars.Power<GrowPower>().BaseValue);
        await CommonActions.ApplySelf<FloatingPower>(
            choiceContext,
            this,
            DynamicVars.Power<FloatingPower>().BaseValue);
    }

    protected override void OnUpgrade()
    {
    }
}
