using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

using Catalyst.CatalystCode.Cards.Infrastructure;

namespace Catalyst.CatalystCode.Cards.Skills;

/// <summary>
/// Deck Rescue common: gains Block, then places one chosen card from the Discard Pile
/// second from the top of the player's Draw Pile.
/// </summary>
public class ErmActually() : CatalystCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>

