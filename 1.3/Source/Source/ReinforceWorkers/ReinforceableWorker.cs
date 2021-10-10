using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace InfiniteReinforce
{
    public abstract class ReinforceableWorker
    {
        public ReinforceableStatDef def;

        public abstract bool IsAppliable(ThingWithComps thing);
    }
}
