using Catalyst.CatalystCode.Cards.Tokens.Props;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Debug;

public class PropPrototypeKit() : CatalystDebugCard(
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
            throw new InvalidOperationException("The Prop prototype kit can only be used during combat.");

        CardModel[] cards =
        [
            combatState.CreateCard(ModelDb.Card<Battery>(), Owner),
            combatState.CreateCard(ModelDb.Card<Fork>(), Owner),
            combatState.CreateCard(ModelDb.Card<Needle>(), Owner),
            combatState.CreateCard(ModelDb.Card<MassAllocationPrototype>(), Owner)
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
