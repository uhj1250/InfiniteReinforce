using System.Linq;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace InfiniteReinforce
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class Rimworld_Patch_AddHumanlikeOrders
    {
        public static readonly TargetingParameters OnlyItems = new TargetingParameters {
            canTargetItems = true,
            canTargetPawns = false,
            canTargetBuildings = false,
            canTargetAnimals = false,
            canTargetHumans = false,
            canTargetMechs = false,
            mapObjectTargetsMustBeAutoAttackable = false
        };


        public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            Building_Reinforcer reinforcer = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Reinforcer>().FirstOrDefault(x => x.HoldingItem == null);
            
            IEnumerable<LocalTargetInfo> targets = GenUI.TargetsAt(clickPos, OnlyItems);

            if (reinforcer != null)
            {
                foreach (LocalTargetInfo t in targets)
                {
                    ThingWithComps thing = t.Thing as ThingWithComps;
                    if (thing != null && reinforcer.ContainerComp.Accepts(thing))
                    {
                        opts.AddDistinct(MakeMenu(pawn,thing,reinforcer));
                    }                    
                }
            }
        }

        public static FloatMenuOption MakeMenu(Pawn pawn, LocalTargetInfo target, Building_Reinforcer reinforcer)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.InsertItem(target.Label), delegate ()
            {
                Job job = new Job(ReinforceDefOf.InsertEquipmentToReinforcer, target, reinforcer, reinforcer.InteractionCell);
                job.count = 1;
                pawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Low), pawn, target);

            return option;
        }
    }





}
