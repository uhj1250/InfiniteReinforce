using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace InfiniteReinforce
{
    public class ReinforceWorker_MaxHitPoint : ReinforceWorker
    {
        public override bool Appliable(ThingWithComps thing)
        {
            return thing.def.IsApparel;
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level)
        {
            return delegate ()
            {
                float percent = (float)comp.parent.HitPoints / comp.parent.MaxHitPoints;
                bool res = comp.ReinforceCustom(def, level);
                comp.parent.HitPoints = (int)(comp.parent.MaxHitPoints * percent);
                return res;
            };
        }

    }

    [HarmonyPatch(typeof(Thing))]
    public static class Patch_ThingWithComps
    {
        [HarmonyPatch("get_MaxHitPoints")]
        [HarmonyPostfix]
        public static void MaxHitPoints(ref int __result, ref Thing __instance)
        {
            ThingWithComps thing = __instance as ThingWithComps;
            if (thing != null)
            {
                ThingComp_Reinforce comp = thing.GetReinforceComp();
                if (comp != null)
                {
                    __result = (int)(comp.GetCustomFactor(ReinforceDefOf.Reinforce_Hitpoint) * __result);
                }
            }
        }
    }



}
