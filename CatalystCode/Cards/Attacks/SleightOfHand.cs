using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Attacks;

/// <summary>Common Attack replacement that exchanges one chosen Hand card.</summary>
public class SleightOfHand() : CatalystCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9M, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        CardSelectorPrefs selectorPrefs = new(SelectionScreenPrompt, 1);
        IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            selectorPrefs,
            null,
            this);
        CardModel? selectedCard = selectedCards.FirstOrDefault();

        if (selectedCard is not null)
            await CatalystCardPileActions.SwitchCard(choiceContext, selectedCard);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
    }
}
