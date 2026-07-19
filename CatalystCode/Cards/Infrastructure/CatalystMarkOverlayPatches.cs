using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>
/// Shows the Marked scratch scene on regular card renderers whenever the card has
/// Catalyst's Mark keyword.
/// </summary>
internal static class CatalystMarkOverlayPatches
{
    private const string MarkOverlayNodeName = "CatalystMarkOverlay";
    private const string MarkDepthLabelNodeName = "DepthNumber";
    private const string MarkOverlayScenePath =
        "res://Catalyst/scenes/cards/overlays/marked.tscn";
    private const string MarkDepthFontPath = "res://Catalyst/fonts/Shojumaru-Regular.ttf";
    private static readonly Vector2 MarkDepthAnchorCenter = new(74f, -58f);
    private static readonly Color MarkDepthTextColor = new(0.960784f, 0.309804f, 0.815686f, 0.68f);
    private static readonly Color MarkDepthOutlineColor = new(0.294118f, 0.0313726f, 0.239216f, 0.52f);
    private static readonly Color MarkDepthShadowColor = new(1f, 0.180392f, 0.717647f, 0.24f);
    private static FontFile? cachedMarkDepthFont;

    [HarmonyPatch(typeof(NCard), "ReloadOverlay")]
    private static class ReloadOverlayPatch
    {
        [HarmonyPostfix]
        private static void RefreshMarkedOverlay(NCard __instance)
        {
            RefreshMarkOverlay(__instance);
        }
    }

    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
    private static class UpdateVisualsPatch
    {
        [HarmonyPostfix]
        private static void RefreshMarkedOverlayAfterVisualUpdate(NCard __instance)
        {
            RefreshMarkOverlay(__instance);
        }
    }

    [HarmonyPatch(typeof(NCard), nameof(NCard.OnFreedToPool))]
    private static class FreedToPoolPatch
    {
        [HarmonyPostfix]
        private static void CleanupMarkedOverlay(NCard __instance)
        {
            RemoveMarkOverlay(__instance);
        }
    }

    private static void RefreshMarkOverlay(NCard cardNode)
    {
        RemoveMarkOverlay(cardNode);

        CardModel? card = cardNode.Model;
        if (card is null || !card.Keywords.Contains(CatalystKeywords.Mark))
        {
            return;
        }

        if (!ResourceLoader.Exists(MarkOverlayScenePath))
        {
            MainFile.Logger.Info("Could not find Mark overlay scene: " + MarkOverlayScenePath);
            return;
        }

        Control overlay = PreloadManager.Cache
            .GetScene(MarkOverlayScenePath)
            .Instantiate<Control>(PackedScene.GenEditState.Disabled);
        overlay.Name = MarkOverlayNodeName;
        ConfigureDepthLabel(overlay, cardNode, card);
        cardNode.OverlayContainer.AddChild(overlay);
        MainFile.Logger.Info("Added Mark overlay to card: " + card.Id);
    }

    private static void ConfigureDepthLabel(Control overlay, NCard cardNode, CardModel card)
    {
        Label depthLabel = overlay.GetNodeOrNull<Label>(MarkDepthLabelNodeName) ?? CreateDepthLabel();
        if (depthLabel.GetParent() is null)
        {
            overlay.AddChild(depthLabel);
            overlay.MoveChild(depthLabel, 0);
        }

        bool isDrawViewerCard = cardNode.DisplayingPile == PileType.Draw;
        int? drawDepth = isDrawViewerCard ? CatalystMarkSystem.GetDrawDepth(card) : null;
        depthLabel.Visible = drawDepth.HasValue;
        depthLabel.Text = drawDepth?.ToString() ?? string.Empty;
        ApplyDepthLabelSizing(depthLabel, drawDepth);
    }

    private static Label CreateDepthLabel()
    {
        Label depthLabel = new()
        {
            Name = MarkDepthLabelNodeName,
            Visible = false,
            Position = GetDepthLabelTopLeft(new Vector2(138f, 92f)),
            Size = new Vector2(138f, 92f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Rotation = -0.244346f,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        FontFile? markDepthFont = GetMarkDepthFont();
        if (markDepthFont is not null)
        {
            depthLabel.AddThemeFontOverride("font", markDepthFont);
        }

        depthLabel.AddThemeColorOverride("font_color", MarkDepthTextColor);
        depthLabel.AddThemeColorOverride("font_outline_color", MarkDepthOutlineColor);
        depthLabel.AddThemeColorOverride("font_shadow_color", MarkDepthShadowColor);
        depthLabel.AddThemeConstantOverride("outline_size", 8);
        depthLabel.AddThemeConstantOverride("shadow_offset_x", 0);
        depthLabel.AddThemeConstantOverride("shadow_offset_y", 0);
        depthLabel.AddThemeFontSizeOverride("font_size", 64);
        return depthLabel;
    }

    private static void ApplyDepthLabelSizing(Label depthLabel, int? drawDepth)
    {
        int digits = drawDepth?.ToString().Length ?? 1;
        (int fontSize, Vector2 size) = digits switch
        {
            >= 3 => (44, new Vector2(172f, 86f)),
            2 => (54, new Vector2(156f, 90f)),
            _ => (64, new Vector2(138f, 92f))
        };

        depthLabel.Position = GetDepthLabelTopLeft(size);
        depthLabel.Size = size;
        depthLabel.AddThemeFontSizeOverride("font_size", fontSize);
    }

    private static Vector2 GetDepthLabelTopLeft(Vector2 size)
    {
        return MarkDepthAnchorCenter - (size * 0.5f);
    }

    private static FontFile? GetMarkDepthFont()
    {
        if (cachedMarkDepthFont is not null)
        {
            return cachedMarkDepthFont;
        }

        if (!ResourceLoader.Exists(MarkDepthFontPath))
        {
            return null;
        }

        cachedMarkDepthFont = ResourceLoader.Load<FontFile>(MarkDepthFontPath);
        return cachedMarkDepthFont;
    }

    private static void RemoveMarkOverlay(NCard cardNode)
    {
        Node? overlay = cardNode.OverlayContainer.GetNodeOrNull(MarkOverlayNodeName);
        if (overlay is null)
        {
            return;
        }

        overlay.GetParent()?.RemoveChild(overlay);
        overlay.QueueFree();
    }
}
