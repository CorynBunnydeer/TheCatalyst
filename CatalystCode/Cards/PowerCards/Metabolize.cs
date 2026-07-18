using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.PowerCards;

// Checklist:
// [X] Create card from template
// [ ] Fill in OnUpgrade
//     Attack example: DynamicVars.Damage.UpgradeValueBy(3);
//     Block example: DynamicVars.Block.UpgradeValueBy(3);
// [ ] Add title + description to Catalyst/localization/eng/cards.json
// [ ] Add card to starter deck or another test path so it can appear in-game
// [ ] Build for code-only changes; publish when localization/images changed

public class Metabolize() : CatalystCard(
    1,
    CardType.Power,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<GrowPower>(3M), new HpLossVar(2M)];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.FromPower<GrowPower>()];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        //From what I saw, Baselib does not have HP Reduction on self available [?]; I might be wrong, but since
        //I'm not certain, will just clone Bloodwall's implementation 
        //Unsure why it uses Unpowered + Move 「(°ヘ°✿
        //but... if Mr. Iron "IloseHpFromCards" Chad himself does it like this... I will just follow suit 
        await CreatureCmd.Damage(choiceContext, this.Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
        await CommonActions.ApplySelf<GrowPower>(choiceContext, this, DynamicVars.Power<GrowPower>().BaseValue);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.Power<GrowPower>().UpgradeValueBy(1M);
        this.DynamicVars.HpLoss.UpgradeValueBy(-1M);
    }
}