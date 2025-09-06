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

    public class JobDriver_InsertItemtoReinforcerDirectly : JobDriver
    {
        public const int InsertTicks = 180;
        private const TargetIndex itemidx = TargetIndex.A;
        private const TargetIndex reinforceridx = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(job.GetTarget(itemidx), job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(job.GetTarget(reinforceridx), job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        public ThingWithComps ThingtoInsert => job.GetTarget(itemidx).Thing as ThingWithComps;
        public Building_Reinforcer Reinforcer => job.GetTarget(reinforceridx).Thing as Building_Reinforcer;

        public static Job MakeJob(ThingWithComps item, Building_Reinforcer reinforcer)
        {
            return new Job(ReinforceDefOf.InsertEquipmentToReinforcerDirectly, item, reinforcer);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(reinforceridx, PathEndMode.InteractionCell).FailOn((Toil to) => ContainerFull());
            Toil toil = Toils_General.Wait(InsertTicks, reinforceridx).WithProgressBarToilDelay(reinforceridx).FailOnDespawnedOrNull(reinforceridx)
                .FailOn((Toil to) => ContainerFull());
            toil.handlingFacing = true;
            yield return toil;
            yield return InsertItemDirectly();
            bool ContainerFull()
            {
                return pawn.jobs.curJob.GetTarget(reinforceridx).Thing.TryGetComp<CompReinforcerContainer>()?.Full ?? true;
            }
        }

        protected Toil InsertItemDirectly()
        {
            Building_Reinforcer reinforcer = job.GetTarget(reinforceridx).Thing as Building_Reinforcer;

            Toil toil = ToilMaker.MakeToil("InfiniteReinforce.InsertItemDirectly");
            toil.FailOn((Toil to) => ThingtoInsert == null || Reinforcer == null);
            toil.initAction = delegate ()
            {
                if (ThingtoInsert != null)
                {
                    reinforcer.InsertedEquipment();
                    pawn.equipment.TryTransferEquipmentToContainer(ThingtoInsert, Reinforcer.ContainerComp.innerContainer);
                }
            };


            return toil;
        }

    }
}
