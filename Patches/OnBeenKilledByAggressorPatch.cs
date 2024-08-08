using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace ScavBirthdayParty
{
    public class OnBeenKilledByAggressorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocalPlayer).GetMethod(nameof(LocalPlayer.OnBeenKilledByAggressor));
        }

        [PatchPostfix]
        public static void Postfix(IPlayer aggressor, DamageInfo damageInfo, EBodyPart bodyPart, EDamageType lethalDamageType)
        {
            if (Random.Range(0, 100) > ScavBirthdayParty.confettiChance.Value) { return; }
            if (ScavBirthdayParty.headshotOnly.Value && bodyPart != EBodyPart.Head) { return; }

            CreateScavBirthdayEffect(damageInfo);
        }

        private static void CreateScavBirthdayEffect(DamageInfo damageInfo)
        {
            if (ScavBirthdayParty.enableConfetti.Value)
            {
                GameObject newConfettiEffect = GameObject.Instantiate(ScavBirthdayParty.confettiEffectPrefab);
                newConfettiEffect.transform.position = damageInfo.HitPoint;
            }
            
            if (ScavBirthdayParty.enableHooray.Value)
            {
                GameObject newHooraySound = GameObject.Instantiate(ScavBirthdayParty.hooraySoundPrefab);
                newHooraySound.transform.position = damageInfo.HitPoint;
            }
        }
    }
}
