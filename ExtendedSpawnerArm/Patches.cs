using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtendedSpawnerArm
{
    [HarmonyPatch(typeof(SpawnMenu))]
    public static class SpawnMenuPatch
    {
        private static bool init = false;
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void AwakePrefix(SpawnMenu __instance, SpawnableObjectsDatabase ___objects)
        {
            if (init) return;
            ___objects.enemies = ___objects.enemies.Concat(SpawnerInjector._enemies).ToArray();
            init = true;
        }
    }

    [HarmonyPatch(typeof(BossHealthBar))]
    public static class BossHealthBarPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(BossHealthBar __instance, EnemyIdentifier ___eid, GameObject ___bossBar)
        {
            var cust = __instance.gameObject.GetComponent<CustomHealthbarPos>();
            if (cust)
            {
                cust.barObj = ___bossBar;
                cust.enemy = ___eid.gameObject;
                cust.enabled = true;
            }
        }
    }

    [HarmonyPatch(typeof(MinosBoss))]
    public static class MinosPatch
    {
        static float minosHeight = 600, minosOffset = 200;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void StartPrefix(MinosBoss __instance)
        {
            Debug.Log("starting");
            var cust = __instance.GetComponentInChildren<CustomHealthbarPos>(true);
            if (cust)
            {
                Debug.Log("custom");
                cust.offset = Vector3.up * minosHeight * 1.5f;
                cust.size = 0.25f;
                var plr = MonoSingleton<NewMovement>.Instance.transform;
                __instance.transform.position = plr.position + Vector3.down * minosHeight;
                __instance.transform.position += plr.forward * minosOffset;
                __instance.transform.forward = Vector3.ProjectOnPlane((plr.position - __instance.transform.position).normalized, Vector3.up);
            }
        }
    }

    [HarmonyPatch(typeof(Wicked))]
    public static class WickedPatch
    {
        [HarmonyPatch("GetHit")]
        [HarmonyPrefix]
        public static bool GetHitPrefix(Wicked __instance)
        {
            if (__instance.patrolPoints[0] == null)
            {
                var oldAud = __instance.hitSound.GetComponent<AudioSource>();

                var newAudObj = new GameObject();
                newAudObj.transform.position = __instance.transform.position;

                var newAud = newAudObj.AddComponent<AudioSource>();
                newAud.playOnAwake = false;
                newAud.clip = oldAud.clip;
                newAud.volume = oldAud.volume;
                newAud.pitch = oldAud.pitch;
                newAud.spatialBlend = oldAud.spatialBlend;
                newAud.Play();

                GameObject.Destroy(newAudObj, newAud.clip.length);

                GameObject.Destroy(__instance.gameObject);
                return false;
            }

            return true;
        }
    }
}
