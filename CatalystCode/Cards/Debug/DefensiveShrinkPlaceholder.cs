using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>
/// Development-only home for Tail Whip's former defensive design.
/// </summary>
public class DefensiveShrinkPlaceholder() : CatalystDebugCard(
    2,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9, ValueProp.Move),
        new PowerVar<ShrinkPower>(2M)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);

        if (cardPlay.Target is { } target)
            await CommonActions.Apply<ShrinkPower>(choiceContext, target, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars.Power<ShrinkPower>().UpgradeValueBy(1M);
    }
}
