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
    public class JobDriver_InsertItemtoReinforcer : JobDriver
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

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn((Toil to) => ContainerFull());
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOn((Toil to) => ContainerFull());
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C).FailOn((Toil to) => ContainerFull());
            Toil toil = Toils_General.Wait(InsertTicks, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B)
                .FailOn((Toil to) => ContainerFull());
            toil.handlingFacing = true;
            yield return toil;
            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, delegate
            {
                job.GetTarget(TargetIndex.A).Thing.def.soundDrop.PlayOneShot(new TargetInfo(job.GetTarget(TargetIndex.B).Cell, pawn.Map));
                SoundDefOf.Relic_Installed.PlayOneShot(new TargetInfo(job.GetTarget(TargetIndex.B).Cell, pawn.Map));
            });
            bool ContainerFull()
            {
                return pawn.jobs.curJob.GetTarget(TargetIndex.B).Thing.TryGetComp<CompReinforcerContainer>()?.Full ?? true;
            }
        }



    }
}
