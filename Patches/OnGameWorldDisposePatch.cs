using System.Reflection;
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
            BobbysMusicPlayerPlugin.LogSource.LogWarning("GameWorld Dispose!");
            BobbysMusicPlayerPlugin.InRaid = false;
        }
    }
}