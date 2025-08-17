using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace InfiniteReinforce
{
    public class ReinforceWorker_EquippedStatOffsets : ReinforceWorker
    {
        public override bool Appliable(ThingWithComps thing)
        {
            return !thing.def.equippedStatOffsets.NullOrEmpty();
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level, float multiplier = 1.0f)
        {
            return delegate ()
            {
                bool res = comp.ReinforceCustom(def, level);
                return res;
            };
        }
    }

    [HarmonyPatch(typeof(StatWorker))]
    public static class Patch_StatWorker
    {
        [HarmonyPatch("StatOffsetFromGear")]
        [HarmonyPostfix]
        public static void StatOffsetFromGear(Thing gear, StatDef stat, ref float __result)
        {
            ThingWithComps thing = gear as ThingWithComps;
            if (thing != null)
            {
                ThingComp_Reinforce comp = thing.GetReinforceComp();
                if (comp != null)
                {
                    float factor = comp.GetCustomFactor(ReinforceDefOf.Reinforce_EquippedStatOffset);
                    if (factor > 1.0f && !stat.LowerIsBetter())
                    {
                        if (__result < 0) __result /= factor;
                        else __result *= factor;
                    }
                    else
                    {
                        if (__result < 0) __result *= factor;
                        else __result /= factor;
                    }
    
                }
            }
        }
    }


}
