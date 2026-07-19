using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Skills;

/// <summary>Block card that Switches one chosen card from the Hand.</summary>
public class Obscure() : CatalystCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(7M, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);

        CardSelectorPrefs selectorPrefs = new(SelectionScreenPrompt, 1);
        IEnumerable<CardModel> selection = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            selectorPrefs,
            null,
            null!);
        CardModel? selectedCard = selection.FirstOrDefault();

        if (selectedCard is not null)
        {
            await CatalystCardPileActions.SwitchCard(choiceContext, selectedCard);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3M);
    }
}
