using BaseLib.Abstracts;
using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Infrastructure;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Catalyst.CatalystCode.Cards.Tokens.Props;

[Pool(typeof(TokenCardPool))]
public abstract class CatalystPropCard(
    int cost,
    CardType type,
    TargetType target) : CatalystCard(cost, type, CardRarity.Token, target)
{
    public virtual PropStage Stage => PropStage.Base;

    public virtual int PropSize => 0;
}
