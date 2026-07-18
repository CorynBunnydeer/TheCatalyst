using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

using Catalyst.CatalystCode.Cards.Infrastructure;

namespace Catalyst.CatalystCode.Cards.Basics;

// Checklist:
// [ ] Add card to starter deck or another test path so it can appear in-game
// [ ] Build for code-only changes; publish when localization/images changed


public class StrikeCatalyst() : CatalystCard(
    1,
    CardType.Attack,
    CardRarity.Basic,
    TargetType.AnyEnemy)
{
    
    protected override HashSet<CardTag> CanonicalTags
    {
        get => new HashSet<CardTag>() { CardTag.Strike };
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    { 
        DynamicVars.Damage.UpgradeValueBy(3);
    } 
}
