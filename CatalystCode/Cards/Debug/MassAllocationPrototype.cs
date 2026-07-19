using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Tokens.Props;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Catalyst.CatalystCode.Cards.Debug;

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

        bool canEnlarge = PileType.Hand
            .GetPile(Owner)
            .Cards
            .Any(card => card is IEnlargeableProp);

        List<CardModel> choices =
        [
            combatState.CreateCard(ModelDb.Card<MassAllocationGrowChoice>(), Owner)
        ];

        if (canEnlarge)
            choices.Add(combatState.CreateCard(ModelDb.Card<MassAllocationPropChoice>(), Owner));

        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            choices,
            Owner,
            false);

        if (selected is null)
            return;

        if (selected is MassAllocationGrowChoice)
        {
            await CommonActions.ApplySelf<GrowPower>(
                choiceContext,
                this,
                DynamicVars.Power<GrowPower>().BaseValue);
            return;
        }

        CardModel? prop = await CommonActions.SelectSingleCard(
            this,
            SelectionScreenPrompt,
            choiceContext,
            PileType.Hand,
            card => card is IEnlargeableProp);

        if (prop is IEnlargeableProp enlargeable)
            await enlargeable.Enlarge(CardPreviewStyle.HorizontalLayout);
    }

    protected override void OnUpgrade()
    {
    }
}
