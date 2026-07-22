using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Cards.Tokens.Props;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Catalyst.CatalystCode.Cards.Skills;

public class FoxTrinkets() : CatalystCard(
    2,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.AllAllies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ICombatState combatState = CombatState ??
            throw new InvalidOperationException(
                "Fox Trinkets can only be played during combat.");

        CardModel[] propPrototypes = ModelDb
            .CardPool<TokenCardPool>()
            .AllCards
            .Where(card => card is CatalystPropCard)
            // Future option: only Props that can still transform.
            // .Where(card => card is IEnlargeableProp)
            .ToArray();

        foreach (var creature in combatState.GetTeammatesOf(Owner.Creature)
                     .Where(creature => creature is { IsAlive: true, IsPlayer: true }))
        {
            if (creature.Player is not { } player)
                continue;

            List<CardModel> props = [];
            for (int i = 0; i < 2; i++)
            {
                CardModel prototype = Owner.RunState.Rng.CombatCardGeneration
                    .NextItem(propPrototypes)
                    ?? throw new InvalidOperationException(
                        "Fox Trinkets could not select a Prop prototype.");
                props.Add(combatState.CreateCard(prototype, player));
            }

            await CardPileCmd.AddGeneratedCardsToCombat(
                props,
                PileType.Hand,
                player,
                CardPilePosition.Bottom);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
