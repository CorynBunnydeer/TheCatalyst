using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Attacks;


public class SingleStrokeBattle() : CatalystCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("ExecuteThreshold", 77M)];
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var target = play.Target;

        if (target is null || !target.IsAlive)
            return;

        if (target.CurrentHp > DynamicVars["ExecuteThreshold"].IntValue)
            return;

        await CreatureCmd.Kill(target, false);
    }   

    protected override void OnUpgrade()
    {
        //DynamicVars["ExecuteTreshold"].UpgradeValueBy(5M); //old upgrade idea; Keeping the 77 is cooler though
        EnergyCost.UpgradeBy(-1);
    }
}