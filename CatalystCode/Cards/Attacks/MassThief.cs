using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Cards.Attacks;

public class MassThief() : CatalystCard(
    1,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7M, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.FromPower<GrowPower>()];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var attack = CommonActions.CardAttack(this, cardPlay);
        await attack.Execute(choiceContext);

        int directContacts = attack.Results
            .SelectMany(hit => hit)
            .Count(result => result.UnblockedDamage > 0M);

        if (directContacts > 0)
        {
            await CommonActions.ApplySelf<GrowPower>(
                choiceContext,
                this,
                directContacts);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
    }
}
