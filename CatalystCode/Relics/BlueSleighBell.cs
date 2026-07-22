using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using BaseLib.Utils;
using Catalyst.CatalystCode.Powers;

namespace Catalyst.CatalystCode.Relics;

/// <summary>
/// Enables Catalyst's escalating Mass Differential rules for Grow and Shrink applied
/// by the player carrying this relic.
/// </summary>
public class BlueSleighBell : CatalystRelic
{
    // Kept as a named rule so a balance cap can be introduced without changing the
    // card's Fatal reward or the saved-counter representation.
    public const int MaximumStacks = int.MaxValue;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<GrowPower>()];

    public override bool ShowCounter => true;

    public override int DisplayAmount => Stacks;

    [SavedProperty]
    public int Stacks
    {
        get => _stacks;
        private set
        {
            AssertMutable();
            _stacks = Math.Clamp(value, 0, MaximumStacks);
            InvokeDisplayAmountChanged();
        }
    }

    public void AddStack(int amount = 1)
    {
        if (amount <= 0 || Stacks >= MaximumStacks)
            return;

        Stacks += Math.Min(amount, MaximumStacks - Stacks);
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom || Stacks <= 0)
            return;

        Flash();
        await PowerCmd.Apply<GrowPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            Stacks,
            Owner.Creature,
            null,
            false);
    }

    private int _stacks;
}
