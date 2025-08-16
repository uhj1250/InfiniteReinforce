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

namespace InfiniteReinforce
{
    public class JobDriver_InsertSelftoReinforcer : JobDriver
    {
        public const int InsertTicks = 180;

        private const TargetIndex reinforcer = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
            {
                return true;
            }
            return false;
        }

        public static Job MakeJob(ThingWithComps thing, Building_Reinforcer reinforcer)
        {
            return new Job(ReinforceDefOf.InsertEquipmentToReinforcer, thing, reinforcer, reinforcer.InteractionCell);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(reinforcer, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(reinforcer).FailOn((Toil to) => ContainerFull());
            Toil toil = Toils_General.Wait(InsertTicks, reinforcer).WithProgressBarToilDelay(reinforcer).FailOnDespawnedOrNull(reinforcer)
                .FailOn((Toil to) => ContainerFull());
            toil.handlingFacing = true;
            yield return toil;
            yield return InsertSelf();

            bool ContainerFull()
            {
                return pawn.jobs.curJob.GetTarget(reinforcer).Thing.TryGetComp<CompReinforcerContainer>()?.Full ?? true;
            }
        }

        protected Toil InsertSelf()
        {
            Toil toil = ToilMaker.MakeToil("InfiniteReinforce.InsertSelf");
            toil.initAction = delegate ()
            {
                Building_Reinforcer building = job.GetTarget(reinforcer).Thing as Building_Reinforcer;
                SoundDefOf.Relic_Installed.PlayOneShot(new TargetInfo(job.GetTarget(reinforcer).Cell, pawn.Map));
                building.InsertPawn(pawn);
            };
            
            return toil;
        }


    }
}
