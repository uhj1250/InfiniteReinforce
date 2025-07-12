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
    public class ReinforceWorker_Range : ReinforceWorker
    {
        public override bool Appliable(ThingWithComps thing)
        {
            return thing.def.IsRangedWeapon;

        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level)
        {
            return delegate
            {
                bool res = comp.ReinforceCustom(def, level);
                return res;
            };

        }
    }
    
    [HarmonyPatch(typeof(VerbProperties))]
    public static class Patch_AdjustedRange
    {
        [HarmonyPatch("AdjustedRange")]
        [HarmonyPostfix]
        public static void AdjustedRange(Verb ownerVerb, Pawn attacker, ref float __result)
        {
            ThingWithComps thing = ownerVerb.EquipmentSource;
            if (thing != null)
            {
                __result *= thing.GetReinforceCustomFactor(ReinforceDefOf.Reinforce_Range);
            }
    
            
        }
    
    }
    

}
