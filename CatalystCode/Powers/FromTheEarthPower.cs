using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Catalyst.CatalystCode.Powers;

/// <summary>Renews From the Earth's breakable Concentrate payload each turn.</summary>
public class FromTheEarthPower : CatalystPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStartLate(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner.Player != player || Amount <= 0)
            return;

        await PowerCmd.Apply<FromTheEarthConcentrationPower>(
            choiceContext,
            Owner,
            Amount,
            Owner,
            null,
            false);
    }
}
