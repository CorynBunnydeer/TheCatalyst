using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// A temporary increase in mass. Positive stacks increase the owner's Attack damage and
/// naturally decay by one at the end of the owner's turn.
///
/// Grow and positive Shrink cancel one another one-for-one. Negative Grow is reserved as
/// the same "infinite" sentinel that the base game's Shrink power uses.
/// </summary>
public class GrowPower : CatalystPower
{
    public const decimal DamageIncreasePerStack = 15M;
    public const decimal InfiniteDamageIncrease = 30M;

    internal const string DamageIncreaseKey = "DamageIncrease";
    internal const string DamageIncreasePerStackKey = "DamageIncreasePerStack";

    public override PowerType Type => PowerType.Buff;

    public override bool AllowNegative => true;


