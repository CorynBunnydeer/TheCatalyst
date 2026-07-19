using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>Development-only Temperature control.</summary>
public class CoolPrototype() : CatalystDebugCard(
    0,
    CardType.Skill,
    CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // ((REFERENCE)) Nanairo design: cooling uses the same mod-owned Shift operation
        // as heating with a negative delta, avoiding duplicated boundary logic.
        await TemperatureSystem.Shift(
            choiceContext,
            Owner.Creature,
            -1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
    }
}
