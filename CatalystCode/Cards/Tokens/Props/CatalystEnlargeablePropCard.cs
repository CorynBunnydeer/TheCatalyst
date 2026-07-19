using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Catalyst.CatalystCode.Cards.Tokens.Props;

public abstract class CatalystEnlargeablePropCard<TEnlarged>(
    int cost,
    CardType type,
    TargetType target) : CatalystPropCard(cost, type, target), IEnlargeableProp
    where TEnlarged : CatalystPropCard
{
    public async Task Enlarge(CardPreviewStyle previewStyle)
    {
        ICombatState combatState = CombatState ??
            throw new InvalidOperationException("Props can only be enlarged during combat.");
        CardModel replacement = combatState.CreateCard(ModelDb.Card<TEnlarged>(), Owner);

        await CardCmd.Transform(this, replacement, previewStyle);
    }
}
