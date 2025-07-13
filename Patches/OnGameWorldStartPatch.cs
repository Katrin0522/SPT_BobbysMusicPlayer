using System.Reflection;
using BobbysMusicPlayer.Utils;
using EFT;
using SPT.Reflection.Patching;

namespace BobbysMusicPlayer.Patches
{
    public class OnGameWorldStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GameWorld).GetMethod(
                "OnGameStarted",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static void PostFix()
        {
#if DEBUG
            OverlayDebug.Instance.Enable();
#endif
            Plugin.InRaid = true;
        }
    }
}