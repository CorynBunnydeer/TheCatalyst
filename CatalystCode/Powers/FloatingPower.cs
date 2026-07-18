using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Missing gravity makes the owner recoil after committing to an Attack. Recoil is
/// evaluated once per AttackCommand, regardless of hit count, and is ordinary
/// blockable, unpowered damage. One stack decays at the end of the owner's turn.
/// </summary>
public class FloatingPower : CatalystPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterAttack(
        PlayerChoiceContext choiceContext,
        AttackCommand command)
    {

