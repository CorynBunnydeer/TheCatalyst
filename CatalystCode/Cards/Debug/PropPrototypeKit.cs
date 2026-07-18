using Catalyst.CatalystCode.Cards.Tokens.Props;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>
/// Token-rarity access card for spawning unfinished Props during development. It is
/// intentionally excluded from ordinary rewards and is not a committed pool design.
/// </summary>
public class PropPrototypeKit() : CatalystDebugCard(
    0,
    CardType.Skill,

