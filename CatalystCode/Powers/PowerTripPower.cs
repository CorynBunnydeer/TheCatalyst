using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Catalyst.CatalystCode.Powers;

/// <summary>
/// Counts positive Grow applications received by this creature. The source of
/// Grow does not matter: another player, a card, Concentration, or a relic all
/// arrive through GrowPower's amount-change hook.
/// </summary>
public sealed class PowerTripPower : CatalystPower
{
    private int _streak;
    private CardModel? _activeCard;
    private bool _receivedGrowDuringActiveCard;
    private bool _activeCardBelongsToOwner;
    private bool _activeCardResolutionCompleted;
    private List<int> _pendingCardFeedback = [];
    private bool _isDoubling;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _activeCard = null;
        _receivedGrowDuringActiveCard = false;
        _activeCardBelongsToOwner = false;
        _activeCardResolutionCompleted = false;
        _pendingCardFeedback = [];
        _isDoubling = false;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // Track every resolving card so Grow received from another player's card
        // can still place its feedback on the card that caused it. Only cards
        // played by this power's owner participate in the non-Grow reset rule.
        _activeCard = cardPlay.Card;
        _receivedGrowDuringActiveCard = false;
        _activeCardBelongsToOwner = cardPlay.Card.Owner.Creature == Owner;
        _activeCardResolutionCompleted = false;
        _pendingCardFeedback.Clear();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!ReferenceEquals(cardPlay.Card, _activeCard))
            return Task.CompletedTask;

        // OnPlay, enchantment, and affliction resolution have completed before
        // this hook. Release all per-application counts together so a card that
        // applies Grow more than once still reports every step, including 3.
        _activeCardResolutionCompleted = true;
        bool reachedThirdApplication = _pendingCardFeedback.Contains(3);
        PowerTripCardFeedback.TryShow(cardPlay.Card, _pendingCardFeedback);
        if (reachedThirdApplication)
        {
            // The actual doubling already happened at the third Grow reception.
            // Flash Power Trip now so its feedback begins alongside the card's 3.
            Flash();
        }
        _pendingCardFeedback.Clear();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayedLate(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!ReferenceEquals(cardPlay.Card, _activeCard))
            return Task.CompletedTask;

        if (_activeCardBelongsToOwner && !_receivedGrowDuringActiveCard)
            _streak = 0;

        _activeCard = null;
        _receivedGrowDuringActiveCard = false;
        _activeCardBelongsToOwner = false;
        _activeCardResolutionCompleted = false;
        _pendingCardFeedback.Clear();
        return Task.CompletedTask;
    }

    public async Task OnGrowReceived(
        PlayerChoiceContext choiceContext,
        decimal amount)
    {
        if (amount <= 0M || _isDoubling)
            return;

        // This is intentionally based on the receiver: Grow applied by another
        // player still counts for this Power Trip.
        if (_activeCard is not null)
        {
            _receivedGrowDuringActiveCard = true;
        }

        _streak++;
        int feedbackCount = _streak;

        if (_activeCard is not null)
        {
            if (_activeCardResolutionCompleted)
            {
                // Another model's normal AfterCardPlayed callback applied Grow
                // after this power's callback. OnPlay is already complete, so it
                // is safe to display now rather than losing the attribution.
                PowerTripCardFeedback.TryShow(_activeCard, [feedbackCount]);
                if (feedbackCount == 3)
                    Flash();
            }
            else
            {
                _pendingCardFeedback.Add(feedbackCount);
            }
        }

        if (_streak < 3)
            return;

        _streak = 0;

        GrowPower? grow = Owner.GetPower<GrowPower>();
        if (grow is null || grow.Amount <= 0M)
            return;

        _isDoubling = true;
        try
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                grow,
                grow.Amount,
                Owner,
                null,
                true);
        }
        finally
        {
            _isDoubling = false;
        }
    }
}
