using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.BaseGen;

namespace InfiniteReinforce
{


    [HarmonyPatch(typeof(SymbolResolver_Interior_AncientTemple))]
    public static class AncientTempleInterior_Patch
    {
        public const float ANCIENT_REINFORCER_CHANCE = 0.45f;


        [HarmonyPatch("Resolve")]
        [HarmonyPostfix]
        public static void Resolve(ResolveParams rp)
        {
            if (Rand.Chance(ANCIENT_REINFORCER_CHANCE))
            {
                ResolveParams resolveparams = rp;
                Building_Reinforcer reinforcer = ThingMaker.MakeThing(ReinforceDefOf.AncientReinforcer) as Building_Reinforcer;
                reinforcer.SetFuelRandom();
                resolveparams.singleThingToSpawn = reinforcer;
                
                BaseGen.symbolStack.Push("thing", resolveparams);
            }
        }





    }
}
