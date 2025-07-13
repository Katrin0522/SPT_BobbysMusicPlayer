using System.Reflection;
using BobbysMusicPlayer.Utils;
using EFT;
using SPT.Reflection.Patching;

namespace BobbysMusicPlayer.Patches
{
    public class OnGameWorldDisposePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GameWorld).GetMethod(
                "Dispose",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static void Prefix()
        {
#if DEBUG
            OverlayDebug.Instance.Disable();
#endif
            
            Plugin.InRaid = false;
        }
    }
}