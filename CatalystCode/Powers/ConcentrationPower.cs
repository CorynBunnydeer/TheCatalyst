using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Basic Concentrate payload: its amount is pending Grow. Multiple applications share
/// one interruption check and combine their rewards. Losing HP breaks the entire
/// concentration; otherwise it resolves at the start of Nanairo's next turn.
/// </summary>
public class ConcentrationPower : CatalystPower
{

