using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
    [HarmonyPatch(typeof(CompProjectileInterceptor))]
    public static class CompProjectileInterceptor_Patch
    {
        [HarmonyPatch("get_HitPointsMax")]
        [HarmonyPostfix]
        public static void get_HitPointsMax(ref int __result, ref CompProjectileInterceptor __instance)
        {
            var comp = __instance.parent.GetReinforceComp();
            if (comp != null)
            {
                __result = (int)(__result*comp.GetStatFactor(StatDefOf.EnergyShieldEnergyMax));
            }
        }

        [HarmonyPatch("get_HitPointsPerInterval")]
        [HarmonyPostfix]
        public static void get_HitPointsPerInterval(ref int __result, ref CompProjectileInterceptor __instance)
        {
            var comp = __instance.parent.GetReinforceComp();
            if (comp != null)
            {
                __result = (int)(__result*comp.GetStatFactor(StatDefOf.EnergyShieldRechargeRate));
            }
        }

    }
}
