using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>
/// Renders Power Trip's transient streak count on the card node that caused
/// Grow. Gameplay state remains in <see cref="Powers.PowerTripPower"/>.
/// </summary>
internal static class PowerTripCardFeedback
{
    private static readonly Color NormalColor = new(0.72f, 0.95f, 0.42f);
    private static readonly Color ThirdColor = new(1f, 0.84f, 0.24f);
    private static readonly Color OutlineColor = new(0.035f, 0.055f, 0.11f);

    private const float FeedbackWidth = 100f;
    private const float FeedbackHeight = 180f;
    private const float FeedbackTop = -100f;
    private const float MaximumSequenceSpread = 180f;
    private const float MaximumSequenceStep = 65f;
    private const float ApplicationStagger = 0.06f;

    /// <summary>
    /// Attempts to show one feedback value per Grow application. The executing
    /// card may already have moved or been freed, in which case presentation is
    /// skipped without affecting gameplay.
    /// </summary>
    public static bool TryShow(CardModel card, IReadOnlyList<int> counts)
    {
        if (counts.Count == 0)
            return false;

        NCard? cardNode = NCard.FindOnTable(card);
        if (cardNode is null ||
            !GodotObject.IsInstanceValid(cardNode) ||
            !cardNode.IsInsideTree() ||
            !ReferenceEquals(cardNode.Model, card))
        {
            return false;
        }

        if (cardNode.CardVfxContainer is not Control container ||
            !GodotObject.IsInstanceValid(container) ||
            !container.IsInsideTree())
        {
            return false;
        }

        List<int> validCounts = [];
        foreach (int count in counts)
        {
            if (count is >= 1 and <= 3)
                validCounts.Add(count);
        }

        if (validCounts.Count == 0)
            return false;

        float sequenceStep = validCounts.Count > 1
            ? Math.Min(
                MaximumSequenceStep,
                MaximumSequenceSpread / (validCounts.Count - 1))
            : 0f;
        float sequenceCenter = (validCounts.Count - 1) * 0.5f;

        for (int index = 0; index < validCounts.Count; index++)
        {
            float horizontalOffset = (index - sequenceCenter) * sequenceStep;
            ShowSingle(
                container,
                validCounts[index],
                index * ApplicationStagger,
                horizontalOffset);
        }

        return true;
    }

    private static void ShowSingle(
        Control container,
        int count,
        float delay,
        float horizontalOffset)
    {
        bool isThird = count == 3;
        var label = new Label
        {
            Text = count.ToString(),
            Position = new Vector2(
                -FeedbackWidth * 0.5f + horizontalOffset,
                FeedbackTop),
            Size = new Vector2(FeedbackWidth, FeedbackHeight),
            PivotOffset = new Vector2(FeedbackWidth * 0.5f, FeedbackHeight * 0.5f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZIndex = 200,
            Modulate = new Color(1f, 1f, 1f, 0f),
            Scale = Vector2.One * 0.55f,
        };

        label.AddThemeFontSizeOverride("font_size", isThird ? 112 : 96);
        label.AddThemeColorOverride("font_color", isThird ? ThirdColor : NormalColor);
        label.AddThemeColorOverride("font_outline_color", OutlineColor);
        label.AddThemeConstantOverride("outline_size", isThird ? 14 : 11);
        container.AddChildSafely(label);

        if (!GodotObject.IsInstanceValid(label) || !label.IsInsideTree())
            return;

        float peakScale = isThird ? 1.2f : 1f;
        float holdDuration = isThird ? 0.55f : 0.4f;
        Tween tween = label.CreateTween();
        if (delay > 0f)
            tween.TweenInterval(delay);

        tween.TweenProperty(label, "modulate:a", 1f, 0.08f);
        tween.TweenProperty(label, "scale", Vector2.One * peakScale, 0.14f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
        tween.TweenInterval(holdDuration);
        tween.TweenProperty(
                label,
                "position:y",
                label.Position.Y - 36f,
                0.28f)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Parallel().TweenProperty(label, "modulate:a", 0f, 0.28f);
        tween.TweenCallback(Callable.From(label.QueueFreeSafely));
    }
}
