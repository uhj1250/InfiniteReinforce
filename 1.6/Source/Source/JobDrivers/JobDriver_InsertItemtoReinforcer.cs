using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

namespace InfiniteReinforce
{
    public class JobDriver_InsertItemtoReinforcer : JobDriver
    {
        public const int InsertTicks = 180;

        private const TargetIndex thing = TargetIndex.A;
        private const TargetIndex reinforcer = TargetIndex.B;
        private const TargetIndex cell = TargetIndex.C;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        public static Job MakeJob(ThingWithComps thing, Building_Reinforcer reinforcer)
        {
            return new Job(ReinforceDefOf.InsertEquipmentToReinforcer, thing, reinforcer, reinforcer.InteractionCell);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(thing, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(thing).FailOn((Toil to) => ContainerFull());
            yield return Toils_Haul.StartCarryThing(thing, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOn((Toil to) => ContainerFull());
            yield return Toils_Haul.CarryHauledThingToCell(cell).FailOn((Toil to) => ContainerFull());
            Toil toil = Toils_General.Wait(InsertTicks, reinforcer).WithProgressBarToilDelay(reinforcer).FailOnDespawnedOrNull(reinforcer)
                .FailOn((Toil to) => ContainerFull());
            toil.handlingFacing = true;
            yield return toil;
            yield return Toils_Haul.DepositHauledThingInContainer(reinforcer, thing, delegate
            {
                job.GetTarget(thing).Thing.def.soundDrop.PlayOneShot(new TargetInfo(job.GetTarget(reinforcer).Cell, pawn.Map));
                SoundDefOf.Relic_Installed.PlayOneShot(new TargetInfo(job.GetTarget(reinforcer).Cell, pawn.Map));
            });
            bool ContainerFull()
            {
                return pawn.jobs.curJob.GetTarget(reinforcer).Thing.TryGetComp<CompReinforcerContainer>()?.Full ?? true;
            }
        }



    }
}
