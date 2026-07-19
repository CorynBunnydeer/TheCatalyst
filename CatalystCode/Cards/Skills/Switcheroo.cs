using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Skills;

public class Switcheroo() : CatalystCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars.Cards.BaseValue,
            Owner,
            false);

        CardSelectorPrefs selectorPrefs = new(SelectionScreenPrompt, 2);
        IReadOnlyList<CardModel> selection = (await CardSelectCmd.FromHand(choiceContext, Owner, selectorPrefs, null, null!)).ToList();

        await CatalystCardPileActions.SwitchCards(choiceContext, selection);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1M);
    }
}
