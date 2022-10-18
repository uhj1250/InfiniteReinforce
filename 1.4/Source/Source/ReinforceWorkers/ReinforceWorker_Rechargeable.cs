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
            return thing.TryGetComp<CompReloadable>() != null;
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level)
        {
            return delegate ()
            {
                bool res = comp.ReinforceCustom(def, level);
                CompReloadable reloadcomp = comp.parent.TryGetComp<CompReloadable>();
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


    [HarmonyPatch(typeof(CompReloadable))]
    public static class Patch_CompReloadable
    {
        [HarmonyPatch("get_MaxCharges")]
        [HarmonyPostfix]
        public static void MaxCharges(ref int __result, ref CompReloadable __instance)
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
