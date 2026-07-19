using Catalyst.CatalystCode.Cards.Attacks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>
/// Gives Rainbow Claws a rich-text title without changing STS2's shared card scene.
/// The base %TitleLabel must remain a MegaLabel because NCard binds it by that exact
/// type in _Ready(). This patch adds a sibling MegaRichTextLabel for Rainbow Claws,
/// hides the base label only while the custom title is active, and restores the base
/// label before a pooled NCard can be reused for another model.
/// </summary>
internal static class RainbowClawsTitlePatches
{
    private const string RichTitleNodeName = "CatalystRainbowClawsTitle";

    [HarmonyPatch(typeof(NCard), "Reload")]
    private static class ReloadPatch
    {
        [HarmonyPostfix]
        private static void RefreshAfterReload(NCard __instance)
        {
            RefreshTitle(__instance);
        }
    }

    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
    private static class UpdateVisualsPatch
    {
        [HarmonyPostfix]
        private static void RefreshAfterVisualUpdate(NCard __instance)
        {
            RefreshTitle(__instance);
        }
    }

    [HarmonyPatch(typeof(NCard), nameof(NCard.OnFreedToPool))]
    private static class FreedToPoolPatch
    {
        [HarmonyPostfix]
        private static void CleanupBeforeReuse(NCard __instance)
        {
            RemoveRichTitle(__instance);
        }
    }

    private static void RefreshTitle(NCard cardNode)
    {
        MegaLabel? baseTitle = cardNode.GetNodeOrNull<MegaLabel>("%TitleLabel");
        if (baseTitle is null)
        {
            return;
        }

        RainbowClaws? model = cardNode.Model as RainbowClaws;
        if (model is null || cardNode.Visibility != ModelVisibility.Visible)
        {
            RemoveRichTitle(cardNode, baseTitle);
            return;
        }

        Node parent = baseTitle.GetParent();
        MegaRichTextLabel richTitle =
            parent.GetNodeOrNull<MegaRichTextLabel>(RichTitleNodeName) ??
            CreateRichTitle(parent, baseTitle);

        SyncLayoutAndTheme(baseTitle, richTitle);
        richTitle.SetTextAutoSize($"[center]{model.Title}[/center]");
        richTitle.Visible = true;
        baseTitle.Visible = false;
    }

    private static MegaRichTextLabel CreateRichTitle(Node parent, MegaLabel baseTitle)
    {
        Font titleFont = baseTitle.GetThemeFont(ThemeConstants.Label.Font, "Label");
        MegaRichTextLabel richTitle = new()
        {
            Name = RichTitleNodeName,
            BbcodeEnabled = true,
            ScrollActive = false,
            ClipContents = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutoSizeEnabled = true,
            IsHorizontallyBound = true,
            IsVerticallyBound = true,
            MinFontSize = baseTitle.MinFontSize,
            MaxFontSize = baseTitle.MaxFontSize
        };

        // MegaRichTextLabel._Ready() requires an explicit normal-font override.
        // Reusing the title's resolved FontVariation preserves STS2's title face and
        // locale substitution while changing only the renderer type.
        richTitle.AddThemeFontOverride(ThemeConstants.RichTextLabel.NormalFont, titleFont);
        richTitle.AddThemeFontOverride(ThemeConstants.RichTextLabel.BoldFont, titleFont);
        richTitle.AddThemeFontOverride(ThemeConstants.RichTextLabel.ItalicsFont, titleFont);

        SyncLayoutAndTheme(baseTitle, richTitle);
        parent.AddChild(richTitle);
        parent.MoveChild(richTitle, baseTitle.GetIndex() + 1);
        return richTitle;
    }

    private static void SyncLayoutAndTheme(
        MegaLabel baseTitle,
        MegaRichTextLabel richTitle)
    {
        richTitle.Position = baseTitle.Position;
        richTitle.Size = baseTitle.Size;
        richTitle.PivotOffset = baseTitle.PivotOffset;
        richTitle.Rotation = baseTitle.Rotation;
        richTitle.Scale = baseTitle.Scale;
        richTitle.ZIndex = baseTitle.ZIndex;
        richTitle.MinFontSize = baseTitle.MinFontSize;
        richTitle.MaxFontSize = baseTitle.MaxFontSize;

        richTitle.AddThemeColorOverride(
            ThemeConstants.RichTextLabel.DefaultColor,
            baseTitle.GetThemeColor(ThemeConstants.Label.FontColor, "Label"));
        richTitle.AddThemeColorOverride(
            ThemeConstants.RichTextLabel.FontOutlineColor,
            baseTitle.GetThemeColor(ThemeConstants.Label.FontOutlineColor, "Label"));
        richTitle.AddThemeColorOverride(
            ThemeConstants.RichTextLabel.FontShadowColor,
            baseTitle.GetThemeColor(ThemeConstants.Label.FontShadowColor, "Label"));

        CopyThemeConstant(baseTitle, richTitle, "outline_size");
        CopyThemeConstant(baseTitle, richTitle, "shadow_offset_x");
        CopyThemeConstant(baseTitle, richTitle, "shadow_offset_y");
        CopyThemeConstant(baseTitle, richTitle, "shadow_outline_size");
    }

    private static void CopyThemeConstant(
        MegaLabel baseTitle,
        MegaRichTextLabel richTitle,
        StringName constantName)
    {
        richTitle.AddThemeConstantOverride(
            constantName,
            baseTitle.GetThemeConstant(constantName, "Label"));
    }

    private static void RemoveRichTitle(NCard cardNode, MegaLabel? knownBaseTitle = null)
    {
        MegaLabel? baseTitle =
            knownBaseTitle ?? cardNode.GetNodeOrNull<MegaLabel>("%TitleLabel");
        if (baseTitle is null)
        {
            return;
        }

        baseTitle.Visible = true;
        Node? richTitle = baseTitle.GetParent().GetNodeOrNull(RichTitleNodeName);
        if (richTitle is null)
        {
            return;
        }

        richTitle.GetParent()?.RemoveChild(richTitle);
        richTitle.QueueFree();
    }
}
