using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Attacks;

/// <summary>Zero-cost Attack that marks one card in the Hand.</summary>
public class RainbowClaws() : CatalystCard(
    0,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.FromKeyword(CatalystKeywords.Mark)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6M, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);

        CardSelectorPrefs selectorPrefs = new(SelectionScreenPrompt, 1);
        IEnumerable<CardModel> selection = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            selectorPrefs,
            null,
            this);
        CardModel? selectedCard = selection.FirstOrDefault();

        if (selectedCard is not null)
            await CatalystMarkSystem.MarkCard(selectedCard);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
    }
}
