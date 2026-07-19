using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Debug;

public class MechanicsPrototypeKit() : CatalystDebugCard(
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
        ICombatState combatState = CombatState ??
            throw new InvalidOperationException("The mechanics prototype kit can only be used during combat.");

        CardModel[] cards =
        [
            combatState.CreateCard(ModelDb.Card<ConcentratedGrowthPrototype>(), Owner),
            combatState.CreateCard(ModelDb.Card<GravitationalReleasePrototype>(), Owner),
            combatState.CreateCard(ModelDb.Card<HeatPrototype>(), Owner),
            combatState.CreateCard(ModelDb.Card<CoolPrototype>(), Owner)
        ];

        foreach (CardModel card in cards)
            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
    }
}
