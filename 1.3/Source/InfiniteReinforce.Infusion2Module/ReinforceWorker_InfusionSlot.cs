using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using InfiniteReinforce;
using Infusion;
using HarmonyLib;

namespace InfiniteReinforce.Infusion2Module
{
    public class ReinforceWorker_InfusionSlot : ReinforceWorker
    {
        public override bool Appliable(ThingWithComps thing)
        {
            return thing.TryGetComp<CompInfusion>() != null;
        }
        
        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level)
        {
            return delegate ()
            {
                bool res = comp.ReinforceCustom(def, level);
                CompInfusion infusion = comp.parent.TryGetComp<CompInfusion>();
                if (infusion != null)
                {
                    comp.parent.TryGetQuality(out QualityCategory qc);
                    infusion.SlotCount = infusion.CalculateSlotCountFor(qc) + (int)(comp.GetCustomFactor(InfusionDefOf.Reinforce_InfusionSlot) - 1);
                }
                return res;
            };
        }

        public override string LeftLabel(ThingComp_Reinforce comp)
        {
            return def.label + String.Format(" +{0:0.##}",(comp.GetCustomFactor(def) - 1));
        }

        public override string RightLabel(ThingComp_Reinforce comp)
        {
            return "Infusion2";
        }
    }

    //[HarmonyPatch(typeof(CompInfusion))]
    //public static class Patch_CompInfusion
    //{
    //    [HarmonyPatch("CalculateSlotCountFor")]
    //    [HarmonyPostfix]
    //    public static void CalculateSlotCountFor(QualityCategory qc, ref int __result, CompInfusion __instance)
    //    {
    //        ThingComp_Reinforce comp = __instance.parent.GetReinforceComp();
    //        if (comp != null)
    //        {
    //            __result += (int)Math.Round(comp.GetCustomFactor(InfusionDefOf.Reinforce_InfusionSlot) - 1);
    //        }
    //    }
    //
    //
    //
    //}


}
