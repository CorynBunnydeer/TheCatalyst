using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Attacks;

public class ForWhomTheBellTolls() : CatalystCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10M, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Fatal)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Target is not { IsAlive: true } target)
            return;

        bool shouldTriggerFatal = target.Powers.All(
            power => power.ShouldOwnerDeathTriggerFatal());

        var attack = CommonActions.CardAttack(this, cardPlay);
        await attack.Execute(choiceContext);

        if (!shouldTriggerFatal || !attack.Results
                .SelectMany(hit => hit)
                .Any(result => result.WasTargetKilled))
            return;

        Owner.GetRelic<BlueSleighBell>()?.AddStack();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
    }
}
