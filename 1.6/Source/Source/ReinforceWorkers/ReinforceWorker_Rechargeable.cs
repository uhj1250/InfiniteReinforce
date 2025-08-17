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
    public class ReinforceWorker_Reloadable : ReinforceWorker
    {
        public override bool Appliable(ThingWithComps thing)
        {
            
            return thing.TryGetComp<CompApparelReloadable>() != null;
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level, float multiplier = 1.0f)
        {
            return delegate ()
            {
                bool res = comp.ReinforceCustom(def, level);
                CompApparelReloadable reloadcomp = comp.parent.TryGetComp<CompApparelReloadable>();
                int charges = (int)reloadcomp.GetMemberValue("remainingCharges");
                reloadcomp.SetMemberValue("remainingCharges", charges + 1);
                return res;
            };
        }

        public override string ResultString(int level)
        {
            return def.label + " +" + level;
        }
    }


    [HarmonyPatch(typeof(CompApparelVerbOwner_Charged))]
    public static class Patch_CompApparelVerbOwner_Charged
    {
        [HarmonyPatch("get_MaxCharges")]
        [HarmonyPostfix]
        public static void get_MaxCharges(ref int __result, ref CompApparelVerbOwner_Charged __instance)
        {
            ThingWithComps thing = __instance.parent;
            if (thing != null)
            {
                ThingComp_Reinforce comp = thing.GetReinforceComp();
                if (comp != null)
                {
                    __result += (int)comp.GetCustomFactor(ReinforceDefOf.Reinforce_Reloadable) - 1;
                }
            }
        }
    }


}
