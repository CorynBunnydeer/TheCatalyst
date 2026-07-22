using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Cards.Statuses;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.PowerCards;

/// <summary>
/// Catalyst's proposed Darv/Dusty Tome Ancient Card.
/// </summary>
public class UniverseForm() : CatalystCard(
    3,
    CardType.Power,
    CardRarity.Ancient,
    TargetType.Self), ITomeCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<GrowPower>(2M)
    ];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
    [
        HoverTipFactory.FromPower<GrowPower>(),
        .. HoverTipFactory.FromCardWithCardHoverTips<Lust>(false)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<UniverseFormPower>(
            choiceContext,
            this,
            1M);
    }

    protected override void OnUpgrade()
    {
        // The Ancient card's upgrade behavior is intentionally not finalized yet.
    }
}
