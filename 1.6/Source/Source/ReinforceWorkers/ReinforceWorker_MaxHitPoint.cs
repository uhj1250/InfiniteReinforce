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
            return thing.HitPoints > 0 && (thing.def.IsApparel || thing.def.IsWeapon);
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level, float multiplier = 1.0f)
        {
            return delegate ()
            {
                int delta = (int)comp.parent.GetStatValue(StatDefOf.MaxHitPoints);
                bool res = comp.ReinforceStat(StatDefOf.MaxHitPoints, level, multiplier);
                delta = (int)comp.parent.GetStatValue(StatDefOf.MaxHitPoints) - delta; 
                comp.parent.HitPoints += delta;
                
                return res;
            };
        }

        public override string ResultString(int level)
        {
            return Keyed.ReinforceResult(StatDefOf.MaxHitPoints.label, def.offsetPerLevel * level);
        }

        public override string LeftLabel(ThingComp_Reinforce comp)
        {
            return StatDefOf.MaxHitPoints.label + " +" + comp.GetReinforcedCount(StatDefOf.MaxHitPoints);
        }

    }
    
    //[HarmonyPatch(typeof(Thing))]
    //public static class Patch_ThingWithComps
    //{
    //    [HarmonyPatch("get_MaxHitPoints")]
    //    [HarmonyPostfix]
    //    public static void MaxHitPoints(ref int __result, ref Thing __instance)
    //    {
    //        ThingWithComps thing = __instance as ThingWithComps;
    //        if (thing != null)
    //        {
    //            ThingComp_Reinforce comp = thing.GetReinforceComp();
    //            if (comp != null)
    //            {
    //                __result = (int)(comp.GetCustomFactor(ReinforceDefOf.Reinforce_Hitpoint) * __result);
    //            }
    //        }
    //    }
    //}



}
