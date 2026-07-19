using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using CatalystModel = Catalyst.CatalystCode.Character.Catalyst;

namespace Catalyst.CatalystCode;

// ((REFERENCE)) STS2: ModInitializer marks the static method the game's mod loader
// invokes after this assembly is loaded.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Catalyst"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        // ((REFERENCE)) BaseLib: CustomCharacterUtils.TryOrderCustomCharacters registers
        // the custom character in character-select ordering before play begins.
        CustomCharacterUtils.TryOrderCustomCharacters([typeof(CatalystModel)]);

        // ((REFERENCE)) Harmony: PatchAll scans this assembly for [HarmonyPatch]
        // classes, including ShrinkPowerPatches.
        Harmony harmony = new(ModId);

        harmony.PatchAll();
    }
}
