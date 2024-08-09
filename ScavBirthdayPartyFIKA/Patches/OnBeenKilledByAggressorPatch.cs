using Comfort.Common;
using EFT;
using Fika.Core.Networking;
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

            //Playing as server just spawn the effect.
            if (Singleton<FikaServer>.Instantiated)
            {
                BirthdayPartyPacket packet = new BirthdayPartyPacket { position = damageInfo.HitPoint };
                Singleton<FikaServer>.Instance.SendDataToAll(ScavBirthdayParty.writer, ref packet, LiteNetLib.DeliveryMethod.Unreliable);
                CreateScavBirthdayEffect(damageInfo.HitPoint);
            }

            //Playing as client send packet to server.
            if (Singleton<FikaClient>.Instantiated)
            {
                BirthdayPartyPacket packet = new BirthdayPartyPacket { position = damageInfo.HitPoint };
                Singleton<FikaClient>.Instance.SendData(ScavBirthdayParty.writer, ref packet, LiteNetLib.DeliveryMethod.Unreliable);
            }
        }

        public static void CreateScavBirthdayEffect(Vector3 position)
        {
            if (ScavBirthdayParty.enableConfetti.Value)
            {
                GameObject newConfettiEffect = GameObject.Instantiate(ScavBirthdayParty.confettiEffectPrefab);
                newConfettiEffect.transform.position = position;
            }
            
            if (ScavBirthdayParty.enableHooray.Value)
            {
                GameObject newHooraySound = GameObject.Instantiate(ScavBirthdayParty.hooraySoundPrefab);
                newHooraySound.transform.position = position;
            }
        }
    }
}
