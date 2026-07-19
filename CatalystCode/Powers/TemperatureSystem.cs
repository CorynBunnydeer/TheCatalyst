using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Powers;

public static class TemperatureSystem
{
    public static int Get(Creature indicatorOwner) =>
        indicatorOwner.GetPower<TemperaturePower>()?.Amount ?? 0;

    public static async Task<int> Shift(
        PlayerChoiceContext choiceContext,
        Creature indicatorOwner,
        int delta,
        Creature? applier,
        CardModel? cardSource)
    {
        TemperaturePower? temperature = indicatorOwner.GetPower<TemperaturePower>();
        int current = temperature?.Amount ?? 0;
        int next = Math.Clamp(current + delta, TemperaturePower.Minimum, TemperaturePower.Maximum);

        if (next == current)
            return current;

        if (next == 0)
        {
            if (temperature is not null)
                await PowerCmd.Remove(temperature);
            return 0;
        }

        if (temperature is null)
        {
            await PowerCmd.Apply<TemperaturePower>(
                choiceContext,
                indicatorOwner,
                next,
                applier,
                cardSource,
                false);
            return next;
        }

        await PowerCmd.ModifyAmount(
            choiceContext,
            temperature,
            next - current,
            applier,
            cardSource,
            false);
        return next;
    }
}
