using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Tokens.Props;

public class Lance() : CatalystPropCard(1, CardType.Attack, TargetType.AnyEnemy)
{
    public override PropStage Stage => PropStage.Transformed;

    public override int PropSize => 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    // ((REFERENCE)) STS2: this is the same DamageVar pattern used by ordinary base
    // Attacks; ValueProp.Move keeps the damage powered and blockable.
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(16, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // ((REFERENCE)) BaseLib: CommonActions.CardAttack is preferred over manually
        // duplicating AttackCommand setup for a conventional single-hit card Attack.
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
