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

        private const TargetIndex thingidx = TargetIndex.A;
        private const TargetIndex reinforceridx = TargetIndex.B;
        private const TargetIndex cellidx = TargetIndex.C;

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
            Thing thing = job.GetTarget(thingidx).Thing;
            Building_Reinforcer reinforcer = job.GetTarget(reinforceridx).Thing as Building_Reinforcer;

            yield return Toils_Goto.GotoThing(thingidx, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(thingidx).FailOn((Toil to) => ContainerFull());
            yield return Toils_Haul.StartCarryThing(thingidx, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOn((Toil to) => ContainerFull());
            yield return Toils_Haul.CarryHauledThingToCell(cellidx).FailOn((Toil to) => ContainerFull());
            Toil toil = Toils_General.Wait(InsertTicks, reinforceridx).WithProgressBarToilDelay(reinforceridx).FailOnDespawnedOrNull(reinforceridx)
                .FailOn((Toil to) => ContainerFull());
            toil.handlingFacing = true;
            yield return toil;
            yield return Toils_Haul.DepositHauledThingInContainer(reinforceridx, thingidx, delegate
            {
                if (thing.IsEquipment()) reinforcer.InsertedEquipment();

                thing.def.soundDrop.PlayOneShot(new TargetInfo(job.GetTarget(reinforceridx).Cell, pawn.Map));
                SoundDefOf.Relic_Installed.PlayOneShot(new TargetInfo(job.GetTarget(reinforceridx).Cell, pawn.Map));
            });
            bool ContainerFull()
            {
                return pawn.jobs.curJob.GetTarget(reinforceridx).Thing.TryGetComp<CompReinforcerContainer>()?.Full ?? true;
            }
        }



    }
}
