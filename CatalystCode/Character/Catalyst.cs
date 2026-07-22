using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Catalyst.CatalystCode.Cards.Attacks;
using Catalyst.CatalystCode.Cards.Basics;
using Catalyst.CatalystCode.Cards.PowerCards;
using Catalyst.CatalystCode.Cards.Skills;
using Catalyst.CatalystCode.Extensions;
using Catalyst.CatalystCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Catalyst.CatalystCode.Character;



public class Catalyst : PlaceholderCharacterModel
{
    public const string CharacterId = "Catalyst";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeCatalyst>(),
        ModelDb.Card<StrikeCatalyst>(),
        ModelDb.Card<StrikeCatalyst>(),
        ModelDb.Card<StrikeCatalyst>(),
        ModelDb.Card<StrikeCatalyst>(),
        ModelDb.Card<DefendCatalyst>(),
        ModelDb.Card<DefendCatalyst>(),
        ModelDb.Card<DefendCatalyst>(),
        ModelDb.Card<FoxTrinkets>(),
        ModelDb.Card<TheDoctorIsIn>(),
        ModelDb.Card<TailWhip>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BlueSleighBell>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<CatalystCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<CatalystRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<CatalystPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
