using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Catalyst.CatalystCode.Powers;

/// <summary>A renewable From the Earth trigger that enemy-turn HP loss breaks.</summary>
public class FromTheEarthConcentrationPower : CatalystPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner &&
            delta < 0M &&
            CombatState.CurrentSide == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner.Player != player || Amount <= 0)
            return;

        int successfulTriggers = Amount;

        await PowerCmd.Apply<GrowPower>(
            choiceContext,
            Owner,
            successfulTriggers,
            Owner,
            null,
            false);

        await PowerCmd.Apply<FloatingPower>(
            choiceContext,
            CombatState.Creatures,
            successfulTriggers * 2M,
            Owner,
            null,
            false);

        await PowerCmd.Remove(this);
    }
}
