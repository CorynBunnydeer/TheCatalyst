using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Cards.Tokens.Props;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Skills;

public class Scavenge() : CatalystCard(
    0,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        int xValue = ResolveEnergyXValue();
        if (xValue <= 0)
            return;

        // Card OnPlay is only reached through the combat card-play path.
        ICombatState combatState = CombatState!;

        CardModel[] propPrototypes =
        [
            ModelDb.Card<Battery>(),
            ModelDb.Card<Fork>(),
            ModelDb.Card<Needle>()
        ];

        for (int i = 0; i < xValue * 2; i++)
        {
            CardModel prototype = Owner.RunState.Rng.CombatCardGeneration
                .NextItem(propPrototypes)
                ?? throw new InvalidOperationException(
                    "Scavenge could not select a Prop prototype.");
            CardModel prop = combatState.CreateCard(prototype, Owner);

            await CardPileCmd.AddGeneratedCardToCombat(
                prop,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
