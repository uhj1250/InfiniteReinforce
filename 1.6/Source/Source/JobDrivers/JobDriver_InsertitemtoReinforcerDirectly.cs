using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace InfiniteReinforce
{

    public class JobDriver_InsertItemtoReinforcerDirectly : JobDriver
    {
        public const int InsertTicks = 180;
        private const TargetIndex item = TargetIndex.A;
        private const TargetIndex reinforcer = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(job.GetTarget(item), job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(job.GetTarget(reinforcer), job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        public ThingWithComps ThingtoInsert => job.GetTarget(item).Thing as ThingWithComps;
        public Building_Reinforcer Reinforcer => job.GetTarget(reinforcer).Thing as Building_Reinforcer;

        public static Job MakeJob(ThingWithComps item, Building_Reinforcer reinforcer)
        {
            return new Job(ReinforceDefOf.InsertEquipmentToReinforcerDirectly, item, reinforcer);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(reinforcer, PathEndMode.InteractionCell).FailOn((Toil to) => ContainerFull());
            Toil toil = Toils_General.Wait(InsertTicks, reinforcer).WithProgressBarToilDelay(reinforcer).FailOnDespawnedOrNull(reinforcer)
                .FailOn((Toil to) => ContainerFull());
            toil.handlingFacing = true;
            yield return toil;
            yield return InsertItemDirectly();
            bool ContainerFull()
            {
                return pawn.jobs.curJob.GetTarget(reinforcer).Thing.TryGetComp<CompReinforcerContainer>()?.Full ?? true;
            }
        }

        protected Toil InsertItemDirectly()
        {
            Toil toil = ToilMaker.MakeToil("InfiniteReinforce.InsertItemDirectly");
            toil.FailOn((Toil to) => ThingtoInsert == null || Reinforcer == null);
            toil.initAction = delegate ()
            {
                if (ThingtoInsert != null)
                {
                    pawn.equipment.TryTransferEquipmentToContainer(ThingtoInsert, Reinforcer.ContainerComp.innerContainer);
                }
            };


            return toil;
        }

    }
}
