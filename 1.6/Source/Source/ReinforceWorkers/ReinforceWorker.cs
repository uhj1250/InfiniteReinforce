using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace InfiniteReinforce
{


    public abstract class ReinforceWorker
    {
        public ReinforceDef def;

        public abstract bool Appliable(ThingWithComps thing);

        public abstract Func<bool> Reinforce(ThingComp_Reinforce comp, int level, float multiplier = 1.0f);

        public virtual string ResultString(int level)
        {
            return Keyed.ReinforceResult(def.label, def.offsetPerLevel * level);
        }

        public virtual string LeftLabel(ThingComp_Reinforce comp)
        {
            return def.label + " +" + comp.GetReinforcedCount(def);
        }
        public virtual string RightLabel(ThingComp_Reinforce comp)
        {
            return null;
        }
    }
}
