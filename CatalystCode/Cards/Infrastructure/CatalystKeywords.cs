using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Catalyst.CatalystCode.Cards.Infrastructure;

/// <summary>Custom player-facing card keywords generated and registered by BaseLib.</summary>


public static class CatalystKeywords
{
    [CustomEnum("Switch")] [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Switch;
    
    [CustomEnum("Mark")] [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Mark;
}
