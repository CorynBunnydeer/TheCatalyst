using BaseLib.Utils;
using BaseLib.Extensions;
using Catalyst.CatalystCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Catalyst.CatalystCode.Cards.Tokens.Props;

/// <summary>
/// A charged physical Prop. Nanairo converts the stored electricity into body mass,
/// leaving the depleted casing available as a projectile.
/// </summary>
public class Battery() : CatalystPropCard(1, CardType.Skill, TargetType.Self)
{

    protected override IEnumerable<IHoverTip> CanonicalHoverTips =>
        [HoverTipFactory.FromPower<GrowPower>(), HoverTipFactory.FromCard<DeadBattery>(IsUpgraded)];

