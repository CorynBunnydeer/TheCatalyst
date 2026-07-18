using BaseLib.Extensions;
using BaseLib.Utils;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>Development-only proof of self gravitational conversion.</summary>
public class GravitationalReleasePrototype() : CatalystDebugCard(
    1,
    CardType.Skill,
    CardRarity.Token,
    TargetType.Self)
{

