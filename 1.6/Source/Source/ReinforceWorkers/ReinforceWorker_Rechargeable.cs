using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace InfiniteReinforce
{
    public class ReinforceWorker_Reloadable : ReinforceWorker
    {
        public override bool Appliable(ThingWithComps thing)
        {
            
            return thing.TryGetComp<CompApparelReloadable>() != null || thing.TryGetComp<CompEquippableAbilityReloadable>() != null;
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level, float multiplier = 1.0f)
        {
            return delegate ()
            {
                bool res = comp.ReinforceCustom(def, level, multiplier);
                float offset = def.offsetPerLevel * level * multiplier;
                if (TryApparelReloadable(comp, offset)) { return res; }
                else if (TryEquippableReloadable(comp, offset)) { return res; }
                
                return res;
            };
        }

        private bool TryApparelReloadable(ThingComp_Reinforce comp, float offset)
        {
            CompApparelReloadable reloadcomp = comp.parent.TryGetComp<CompApparelReloadable>();
            if (reloadcomp != null)
            {
                int charges = (int)reloadcomp.GetMemberValue("remainingCharges");
                reloadcomp.SetMemberValue("remainingCharges", charges + (int)offset);
                return true;
            }
            return false;
        }

        private bool TryEquippableReloadable(ThingComp_Reinforce comp, float offset)
        {
            CompEquippableAbilityReloadable reloadcomp = comp.parent.TryGetComp<CompEquippableAbilityReloadable>();
            if (reloadcomp != null)
            {
                reloadcomp.AbilityForReading.RemainingCharges += (int)offset;
                return true;
            }
            return false;

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

    [HarmonyPatch(typeof(CompEquippableAbilityReloadable))]
    public static class Patch_CompEquippableAbilityReloadable
    {
        [HarmonyPatch("get_MaxCharges")]
        [HarmonyPostfix]
        public static void get_MaxCharges(ref int __result, ref CompEquippableAbilityReloadable __instance)
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
