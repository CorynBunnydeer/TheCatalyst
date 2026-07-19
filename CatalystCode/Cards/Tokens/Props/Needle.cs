using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Tokens.Props;

public class Needle() : CatalystEnlargeablePropCard<Lance>(
    0,
    CardType.Attack,
    TargetType.AnyEnemy)
{
    public override PropStage Stage => PropStage.Base;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(2, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
      await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
