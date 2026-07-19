using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;

using Catalyst.CatalystCode.Cards.Infrastructure;

namespace Catalyst.CatalystCode.Cards.Debug;

/// <summary>Base class for spawnable prototypes excluded from normal card pools.</summary>
[Pool(typeof(CatalystDebugCardPool))]
public abstract class CatalystDebugCard(
    int cost,
    CardType type,
    CardRarity rarity,
    TargetType target) : CatalystCard(cost, type, rarity, target)
{
}
