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

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        public ThingWithComps ThingtoInsert => job.GetTarget(TargetIndex.A).Thing as ThingWithComps;
        public Building_Reinforcer Reinforcer => job.GetTarget(TargetIndex.B).Thing as Building_Reinforcer;


        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell).FailOn((Toil to) => ContainerFull());
            Toil toil = Toils_General.Wait(InsertTicks, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B)
                .FailOn((Toil to) => ContainerFull());
            toil.handlingFacing = true;
            yield return toil;
            yield return InsertItemDirectly();
            bool ContainerFull()
            {
                return pawn.jobs.curJob.GetTarget(TargetIndex.B).Thing.TryGetComp<CompReinforcerContainer>()?.Full ?? true;
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
