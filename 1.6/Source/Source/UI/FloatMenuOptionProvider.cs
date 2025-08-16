using LudeonTK;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace InfiniteReinforce.UI
{
    public class FloatMenuOptionProvider_Reinforcer : FloatMenuOptionProvider
    {

        public static readonly TargetingParameters OnlyItems = new TargetingParameters
        {
            canTargetItems = true,
            canTargetPawns = false,
            canTargetBuildings = false,
            canTargetAnimals = false,
            canTargetHumans = false,
            canTargetMechs = false,
            mapObjectTargetsMustBeAutoAttackable = false
        };

        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool MechanoidCanDo => true;

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            Pawn pawn = context.FirstSelectedPawn;
            Vector3 clickPos = context.clickPosition;
            var reinforcers = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Reinforcer>().Where(x => x.HoldingThing == null).Distinct(new ReinforcerComparer()).ToList();
            var refuelables = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Reinforcer>().Where(x => RefuelWorkGiverUtility.CanRefuel(pawn, x)).ToList();
            //IEnumerable<LocalTargetInfo> targets = GenUI.TargetsAt(clickPos, OnlyItems);
            
            bool canInsert = !reinforcers.NullOrEmpty();
            bool canRefuel = !refuelables.NullOrEmpty();

            if (canInsert || canRefuel) //foreach (LocalTargetInfo t in targets)
            {
                bool isFuel = false;

                ThingWithComps thing = clickedThing as ThingWithComps;
                if (thing != null)
                {
                    if (thing is Building_Reinforcer)
                    {
                        Building_Reinforcer reinforcer = thing as Building_Reinforcer;
                        if (!pawn.HasComp<CompMechanoid>())
                        {
                            if (reinforcer.HoldingThing == null)
                            {
                                List<ThingWithComps> equipments = pawn.equipment.AllEquipmentListForReading;
                                if (!equipments.NullOrEmpty()) for (int i = 0; i < equipments.Count; i++)
                                    {
                                        if (equipments[i].IsReinforcable())
                                        {
                                            yield return MakeInsertItemDirectlyMenu(pawn, equipments[i], reinforcer);
                                        }
                                    }
                            }
                            if (RefuelWorkGiverUtility.CanRefuel(pawn, reinforcer))
                            {
                                yield return MakeReinforcerRefuelMenu(pawn, reinforcer);
                            }
                        }
                        else
                        {
                            if (reinforcer.HoldingThing == null)
                            {
                                yield return MakeInsertSelfMenu(pawn, reinforcer);
                            }
                        }
                    }
                    else
                    {
                        if (canRefuel) for (int i = 0; i < refuelables.Count; i++)
                            {

                                //if (thing is Building_Reinforcer && refuelables.Contains(thing))
                                //{
                                //    opts.AddDistinct(MakeReinforcerRefuelMenu(pawn, thing as Building_Reinforcer));
                                //    isFuel = true;
                                //    break;
                                //}

                                if (refuelables[i].FuelThing?.Contains(thing.def) ?? false)
                                {
                                    yield return MakeRefuelMenu(pawn, thing, refuelables.FirstOrDefault());
                                    isFuel = true;
                                    break;
                                }
                            }
                        if (!isFuel)
                        {
                            if (canInsert)
                            {
                                for (int i = 0; i < reinforcers.Count; i++)
                                {
                                    if (thing != null)
                                    {
                                        if (reinforcers[i].ContainerComp.Accepts(thing))
                                        {
                                            yield return MakeReinforceMenu(pawn, thing, reinforcers[i]);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }



        public static FloatMenuOption MakeReinforceMenu(Pawn pawn, ThingWithComps thing, Building_Reinforcer reinforcer)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.InsertItem(thing.Label, reinforcer.Label), delegate ()
            {
                Job job = JobDriver_InsertItemtoReinforcer.MakeJob(thing, reinforcer);
                job.count = 1;
                pawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Low), pawn, thing);

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

        public static FloatMenuOption MakeInsertItemDirectlyMenu(Pawn pawn, ThingWithComps item, Building_Reinforcer reinforcer)
        {

            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.InsertItem(item.Label, reinforcer.Label), delegate ()
            {
                Job job = JobDriver_InsertItemtoReinforcerDirectly.MakeJob(item, reinforcer);
                job.count = 1;
                pawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.High), pawn, reinforcer);
            return option;
        }

        public static FloatMenuOption MakeInsertSelfMenu(Pawn pawn, Building_Reinforcer reinforcer)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.InsertItem(pawn.Label, reinforcer.Label), delegate ()
            {
                Job job = JobMaker.MakeJob(ReinforceDefOf.InsertSelfToReinforcer, reinforcer);
                pawn.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.High), pawn, reinforcer);
            return option;
        }


    }
}
