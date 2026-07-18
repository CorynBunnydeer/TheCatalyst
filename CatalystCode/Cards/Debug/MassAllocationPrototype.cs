using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Tokens.Props;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>
/// Development-only proof of the choice UI between applying Grow to Nanairo and
/// enlarging an eligible Prop. A real card must establish the conversion source.
/// </summary>
public class MassAllocationPrototype() : CatalystDebugCard(
    0,
    CardType.Skill,
    CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<GrowPower>(1M)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ICombatState combatState = CombatState ??
            throw new InvalidOperationException("Mass allocation can only occur during combat.");


