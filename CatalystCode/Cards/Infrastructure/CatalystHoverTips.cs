using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>Shared non-keyword hover tips for named Catalyst mechanic terms.</summary>
public static class CatalystHoverTips
{
    private static readonly HoverTip ConcentrateTip = new(
        new LocString("static_hover_tips", "CATALYST-CONCENTRATE.title"),
        new LocString("static_hover_tips", "CATALYST-CONCENTRATE.description"));

    public static IHoverTip Concentrate => ConcentrateTip;
}
