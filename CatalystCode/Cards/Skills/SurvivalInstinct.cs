using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Skills;

/// <summary>Immediate Energy that also Marks and Switches Hand cards.</summary>
public class SurvivalInstinct() : CatalystCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CatalystKeywords.Mark),
        HoverTipFactory.FromKeyword(CatalystKeywords.Switch)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(1, Owner);

        CardSelectorPrefs markPrefs = new(SelectionScreenPrompt, 1);
        CardModel? markedCard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            markPrefs,
            null,
            null!)).FirstOrDefault();

        if (markedCard is not null)
            await CatalystMarkSystem.MarkCard(markedCard);

        CardSelectorPrefs switchPrefs = new(SelectionScreenPrompt, 1);
        CardModel? switchedCard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            switchPrefs,
            null,
            null!)).FirstOrDefault();

        if (switchedCard is not null)
            await CatalystCardPileActions.SwitchCard(choiceContext, switchedCard);
    }

    protected override void OnUpgrade()
    {
    }
}
