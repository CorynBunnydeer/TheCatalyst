using System.Linq;
using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace Catalyst.CatalystCode.Powers;

public enum ConcentrationEffectType
{
    GainGrow,
    FromTheEarth
}

public readonly record struct ConcentrationEffect(
    ConcentrationEffectType Type,
    int Amount,
    int SecondaryAmount = 0)
{
    public static ConcentrationEffect GainGrow(int amount) =>
        new(ConcentrationEffectType.GainGrow, amount);

    public static ConcentrationEffect FromTheEarth(int growAmount, int levitateAmount) =>
        new(ConcentrationEffectType.FromTheEarth, growAmount, levitateAmount);
}

/// <summary>
/// The shared delayed-effect queue for Concentrate.
/// The power is a singleton marker; Amount is deliberately not the payload count.
/// </summary>
public class ConcentrationPower : CatalystPower, IAddDumbVariablesToPowerDescription
{
    private const string PendingEffectsKey = "PendingEffects";

    private List<ConcentrationEffect> _pendingEffects = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public IReadOnlyList<ConcentrationEffect> PendingEffects => _pendingEffects;

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _pendingEffects = [.. _pendingEffects];
    }

    public void Queue(ConcentrationEffect effect)
    {
        AssertMutable();

        if (effect.Amount <= 0)
            return;

        _pendingEffects.Add(effect);
        InvokeDisplayAmountChanged();
    }

    public void AddDumbVariablesToPowerDescription(LocString description)
    {
        description.Add(PendingEffectsKey, DescribePendingEffects());
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner && delta < 0M && CombatState.CurrentSide == CombatSide.Enemy)
            await PowerCmd.Remove(this);
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Owner.Player != player || _pendingEffects.Count == 0)
            return;

        ConcentrationEffect[] effects = [.. _pendingEffects];

        // Remove before resolving so any Concentrate created by a resolving effect
        // belongs to the following cycle rather than this one.
        await PowerCmd.Remove(this);

        foreach (ConcentrationEffect effect in effects)
            await Resolve(effect, choiceContext);
    }

    private async Task Resolve(
        ConcentrationEffect effect,
        PlayerChoiceContext choiceContext)
    {
        switch (effect.Type)
        {
            case ConcentrationEffectType.GainGrow:
                await PowerCmd.Apply<GrowPower>(
                    choiceContext,
                    Owner,
                    effect.Amount,
                    Owner,
                    null,
                    false);
                break;

            case ConcentrationEffectType.FromTheEarth:
                await PowerCmd.Apply<GrowPower>(
                    choiceContext,
                    Owner,
                    effect.Amount,
                    Owner,
                    null,
                    false);

                foreach (Creature enemy in CombatState.Creatures.Where(creature => creature.Player is null))
                {
                    await PowerCmd.Apply<FloatingPower>(
                        choiceContext,
                        enemy,
                        effect.SecondaryAmount,
                        Owner,
                        null,
                        false);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private string DescribePendingEffects()
    {
        if (_pendingEffects.Count == 0)
            return string.Empty;

        return string.Join(
            "\n",
            _pendingEffects.Select(effect => effect.Type switch
            {
                ConcentrationEffectType.GainGrow =>
                    new LocString("powers", "CATALYST-CONCENTRATION_EFFECT_GROW")
                        .With("Amount", effect.Amount),
                ConcentrationEffectType.FromTheEarth =>
                    DescribeFromTheEarth(effect),
                _ => throw new ArgumentOutOfRangeException()
            }));
    }

    private static string DescribeFromTheEarth(ConcentrationEffect effect)
    {
        LocString description = new(
            "powers",
            "CATALYST-CONCENTRATION_EFFECT_FROM_THE_EARTH");
        description.Add("Amount", effect.Amount);
        description.Add("LevitateAmount", effect.SecondaryAmount);
        return description.GetFormattedText();
    }
}

internal static class LocStringExtensions
{
    public static string With(this LocString locString, string name, decimal value)
    {
        locString.Add(name, value);
        return locString.GetFormattedText();
    }
}
