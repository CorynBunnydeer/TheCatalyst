using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

using Catalyst.CatalystCode.Cards.Infrastructure;

namespace Catalyst.CatalystCode.Cards.Basics;

// [ ] Add card to starter deck or another test path so it can appear in-game
// [ ] Build for code-only changes; publish when localization/images changed


public class DefendCatalyst() : CatalystCard(
    1,
    CardType.Skill,
    CardRarity.Basic,
    TargetType.Self)
{

    public override bool GainsBlock => true;
    
    protected override HashSet<CardTag> CanonicalTags
    {
        get => new HashSet<CardTag>() { CardTag.Defend };
    }

    

