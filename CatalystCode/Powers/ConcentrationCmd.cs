using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Powers;

public static class ConcentrationCmd
{
    public static async Task Queue(
        PlayerChoiceContext choiceContext,
        Creature owner,
        ConcentrationEffect effect,
        Creature? applier = null,
        CardModel? cardSource = null,
        bool silent = false)
    {
        if (effect.Amount <= 0)
            return;

        ConcentrationPower? power = owner.GetPower<ConcentrationPower>();

        if (power is null)
        {
            power = await PowerCmd.Apply<ConcentrationPower>(
                choiceContext,
                owner,
                1M,
                applier,
                cardSource,
                silent);
        }

        power?.Queue(effect);
    }

    public static Task QueueGrow(
        PlayerChoiceContext choiceContext,
        Creature owner,
        decimal amount,
        Creature? applier = null,
        CardModel? cardSource = null,
        bool silent = false) =>
        Queue(
            choiceContext,
            owner,
            ConcentrationEffect.GainGrow((int)amount),
            applier,
            cardSource,
            silent);
}
