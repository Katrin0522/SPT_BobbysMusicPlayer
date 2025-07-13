using System.Reflection;
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
            Plugin.InRaid = true;
        }
    }
}