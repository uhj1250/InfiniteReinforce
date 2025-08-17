using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
    public class ReinforceableWorker_Mechanoid : ReinforceableWorker
    {
        public override bool IsAppliable(ThingWithComps thing)
        {
            return thing.HasComp<CompMechanoid>();
        }

    }
}
