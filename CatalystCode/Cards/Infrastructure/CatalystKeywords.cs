using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>
/// Custom player-facing tooltip and keyword entries generated and registered by BaseLib.
/// </summary>
public static class CatalystKeywords
{
    [CustomEnum("Concentrate")]
    public static StaticHoverTip Concentrate;

    [CustomEnum("Switch")] [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Switch;
    
    [CustomEnum("Mark")] [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Mark;
}
