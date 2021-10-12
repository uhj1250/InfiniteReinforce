using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.SketchGen;
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
            var reinforcers = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Reinforcer>().Where(x => x.HoldingItem == null).Distinct(new ReinforcerComparer()).ToList();
            var refuelables = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Reinforcer>().Where(x => RefuelWorkGiverUtility.CanRefuel(pawn, x)).ToList();
            IEnumerable<LocalTargetInfo> targets = GenUI.TargetsAt(clickPos, OnlyItems);

            bool canInsert = !reinforcers.NullOrEmpty();
            bool canRefuel = !refuelables.NullOrEmpty();

            if (canInsert || canRefuel) foreach (LocalTargetInfo t in targets)
                {
                    bool isFuel = false;
                   
                    ThingWithComps thing = t.Thing as ThingWithComps;

                    if (thing != null)
                    {
                        if (canRefuel) for (int i = 0; i < refuelables.Count; i++)
                            {

                                if (thing is Building_Reinforcer && refuelables.Contains(thing))
                                {
                                    opts.AddDistinct(MakeReinforcerRefuelMenu(pawn, thing as Building_Reinforcer));
                                    isFuel = true;
                                    break;
                                }

                                if (refuelables[i].FuelThing?.Contains(thing.def) ?? false)
                                {
                                    opts.AddDistinct(MakeRefuelMenu(pawn, t, refuelables.FirstOrDefault()));
                                    isFuel = true;
                                    break;
                                }
                            }
                        if (isFuel) continue;

                        if (canInsert)
                        {
                            for (int i = 0; i < reinforcers.Count; i++)
                            {
                                if (thing != null)
                                {
                                    if (reinforcers[i].ContainerComp.Accepts(thing))
                                    {
                                        opts.AddDistinct(MakeReinforceMenu(pawn, thing, reinforcers[i]));
                                    }
                                }
                            }
                        }
                    }
                }

        }

        public static FloatMenuOption MakeReinforceMenu(Pawn pawn, LocalTargetInfo target, Building_Reinforcer reinforcer)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.InsertItem(target.Label, reinforcer.Label), delegate ()
            {
                Job job = new Job(ReinforceDefOf.InsertEquipmentToReinforcer, target, reinforcer, reinforcer.InteractionCell);
                job.count = 1;
                pawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Low), pawn, target);

            return option;
        }

        public static FloatMenuOption MakeRefuelMenu(Pawn pawn, LocalTargetInfo target, Building_Reinforcer reinforcer)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.InsertItem(target.Label, reinforcer.Label), delegate ()
            {
                Job job = RefuelWorkGiverUtility.RefuelJob(pawn, reinforcer, true);
                job.count = 1;
                pawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Low), pawn, target);

            return option;
        }

        public static FloatMenuOption MakeReinforcerRefuelMenu(Pawn pawn, Building_Reinforcer reinforcer)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.InsertFuel(reinforcer.Label), delegate ()
            {
                Job job = RefuelWorkGiverUtility.RefuelJob(pawn, reinforcer, true);
                job.count = 1;
                pawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Low), pawn, reinforcer);

            return option;
        }


    }




}
