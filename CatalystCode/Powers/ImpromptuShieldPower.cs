using BaseLib.Utils;
using Catalyst.CatalystCode.Cards.Tokens.Props;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;


namespace Catalyst.CatalystCode.Powers;

/// <summary>Uses the ability to enlarge objects to enlarge a prop as a shield</summary>
public class ImpromptuShieldPower : CatalystPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];
    
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // base.AfterCardPlayed(choiceContext, cardPlay); //I was affraid of overriding standard behaviour but
        // research indicates that this has no standard behaviour, so I will leave it commented here as a 
        // reminder in case we need it later

        // Conditional for only this player + only props
        if (cardPlay.Card.Owner.Creature != this.Owner || cardPlay.Card is not CatalystPropCard)
        {
            return;
        }

        await CreatureCmd.GainBlock(this.Owner, this.Amount, ValueProp.Unpowered, null, true);



    }

   
        
}