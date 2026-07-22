using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Catalyst.CatalystCode.Cards.PowerCards;
using MegaCrit.Sts2.Core.Models;

namespace Catalyst.CatalystCode.Powers;

/// <summary>Renews From the Earth's breakable Concentrate payload each turn.</summary>
public class FromTheEarthPower : CatalystPower
{
    public const string GrowPerStackKey = "GrowPerStack";
    public const string LevitatePerStackKey = "LevitatePerStack";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(GrowPerStackKey, 1M),
        new DynamicVar(LevitatePerStackKey, 2M)
    ];

    public override Task AfterApplied(
        MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier,
        CardModel? cardSource)
    {
        if (cardSource is FromTheEarth)
        {
            DynamicVars[GrowPerStackKey].BaseValue =
                cardSource.DynamicVars.Power<GrowPower>().BaseValue;
            DynamicVars[LevitatePerStackKey].BaseValue =
                cardSource.DynamicVars.Power<FloatingPower>().BaseValue;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartLate(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner.Player != player || Amount <= 0)
            return;

        await ConcentrationCmd.Queue(
            choiceContext,
            Owner,
            ConcentrationEffect.FromTheEarth(
                (int)(Amount * DynamicVars[GrowPerStackKey].BaseValue),
                (int)(Amount * DynamicVars[LevitatePerStackKey].BaseValue)),
            Owner,
            null,
            false);
    }
}
