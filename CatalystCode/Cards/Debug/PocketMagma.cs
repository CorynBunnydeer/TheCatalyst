using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

using Catalyst.CatalystCode.Cards.Infrastructure;

namespace Catalyst.CatalystCode.Cards.Debug;

// Checklist:
// [X] Create card from template
// [X] Fill in CanonicalVars
//     Attack example: [new DamageVar(6, ValueProp.Move)]
//     Block example: [new BlockVar(9, ValueProp.Move)]
// [ ] Fill in OnPlay
//     Attack example: await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
//     Block example: await CommonActions.CardBlock(this, cardPlay);
// [ ] Fill in OnUpgrade
//     Attack example: DynamicVars.Damage.UpgradeValueBy(3);
//     Block example: DynamicVars.Block.UpgradeValueBy(3);
// [ ] Add title + description to Catalyst/localization/eng/cards.json
// [ ] If description uses !D! or !B!, start it with #
// [ ] Add card to starter deck or another test path so it can appear in-game
// [ ] Build for code-only changes; publish when localization/images changed


public class PocketMagma() : CatalystDebugCard(
    1,
    CardType.Power,
    CardRarity.Basic,
    TargetType.Self)
{

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<GrowPower>(3M), new HpLossVar(2M)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
       await CommonActions.ApplySelf<GrowPower>(choiceContext, this, DynamicVars.Power<GrowPower>().BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Power<GrowPower>().UpgradeValueBy(1);
    }
}
