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


