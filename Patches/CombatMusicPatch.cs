using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using BobbysMusicPlayer.Models;
using BobbysMusicPlayer.Utils;
using UnityEngine;

namespace BobbysMusicPlayer.Patches
{
    // The following patches each represent a different trigger for the combat state,
    // but they can never decrease the value of the combat timer.
    public class ShotAtPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(FlyingBulletSoundPlayer), nameof(FlyingBulletSoundPlayer.method_3));
        }

        [PatchPostfix]
        private static void Postfix()
        {
            if (AudioManager.combatTimer < SettingsModel.Instance.CombatAttackedEntryTime.Value)
            {
                AudioManager.combatTimer = SettingsModel.Instance.CombatAttackedEntryTime.Value;
                BobbysMusicPlayerPlugin.LogSource.LogInfo("Player shot at. Combat Timer set to " + AudioManager.combatTimer);
            }
            else
            {
                BobbysMusicPlayerPlugin.LogSource.LogInfo("Player shot at");
            }
        }
    }
    
    public class PlayerFiringPatch : ModulePatch
    {
        internal static bool playerFired = false;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnMakingShot));
        }

        [PatchPrefix]
        private static bool Prefix(Player __instance)
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            if (__instance != player)
            {
                return true;
            }
            playerFired = true;
            if (AudioManager.combatTimer < SettingsModel.Instance.CombatFireEntryTime.Value)
            {
                AudioManager.combatTimer = SettingsModel.Instance.CombatFireEntryTime.Value;
                BobbysMusicPlayerPlugin.LogSource.LogInfo("Player fired. Combat timer set to " + SettingsModel.Instance.CombatFireEntryTime.Value);
            }
            else
            {
                BobbysMusicPlayerPlugin.LogSource.LogInfo("Player fired");
            }
            return true;
        }
    }
    
    public class DamageTakenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ReceiveDamage));
        }
        
        [PatchPrefix]
        private static bool Prefix(Player __instance, EDamageType type)
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            if (__instance != player)
            {
                return true;
            }
            if (DamageTypeList.Contains(type.ToString()))
            {
                if (AudioManager.combatTimer < SettingsModel.Instance.CombatHitEntryTime.Value)
                {
                    AudioManager.combatTimer = SettingsModel.Instance.CombatHitEntryTime.Value;
                    BobbysMusicPlayerPlugin.LogSource.LogInfo("Player hit. Combat timer set to " + SettingsModel.Instance.CombatHitEntryTime.Value);
                }
                else
                {
                    BobbysMusicPlayerPlugin.LogSource.LogInfo("Player hit");
                }
            }
            return true;
        }
        
        private static List<string> DamageTypeList = new()
        {
            "Explosion", "Blunt", "Sniper", "Bullet", "Melee", "Landmine"
        };
    }
    
    public class ShotFiredNearPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WeaponSoundPlayer), nameof(WeaponSoundPlayer.FireBullet));
        }
        [PatchPrefix]
        private static bool Prefix(Vector3 shotPosition)
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            float distance = Vector3.Distance(player.PlayerBones.BodyTransform.position, shotPosition);
            if (distance < SettingsModel.Instance.ShotNearCutoff.Value)
            {
                if (PlayerFiringPatch.playerFired == true)
                {
                    PlayerFiringPatch.playerFired = false;
                    return true;
                }
                if (AudioManager.combatTimer < SettingsModel.Instance.CombatDangerEntryTime.Value)
                {
                    AudioManager.combatTimer = SettingsModel.Instance.CombatDangerEntryTime.Value;
                    BobbysMusicPlayerPlugin.LogSource.LogInfo("Player shot near. Combat Timer set to " + AudioManager.combatTimer);
                }
            }
            else
            {
                BobbysMusicPlayerPlugin.LogSource.LogInfo("Enemy shot fired past cutoff distance");
            }
            return true;
        }
    }

    public class GrenadePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Grenade), nameof(Grenade.Explosion));
        }
        [PatchPrefix]
        private static bool Prefix(Vector3 grenadePosition)
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            float distance = Vector3.Distance(player.PlayerBones.BodyTransform.position, grenadePosition);
            if (distance < SettingsModel.Instance.GrenadeNearCutoff.Value)
            {
                if (PlayerFiringPatch.playerFired == true)
                {
                    PlayerFiringPatch.playerFired = false;
                    return true;
                }
                if (AudioManager.combatTimer < SettingsModel.Instance.CombatGrenadeEntryTime.Value)
                {
                    AudioManager.combatTimer = SettingsModel.Instance.CombatGrenadeEntryTime.Value;
                    BobbysMusicPlayerPlugin.LogSource.LogInfo("Grenade explosion near. Combat Timer set to " + AudioManager.combatTimer);
                }
            }
            else
            {
                BobbysMusicPlayerPlugin.LogSource.LogInfo("Grenade explosion past cutoff distance");
            }
            return true;
        }
    }
}
