using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace InfiniteReinforce
{
    public class ReinforceDef : Def
    {
        public Type workerClass;
        public IntRange levelRange = new IntRange(1,25);
        public float offsetPerLevel;

        

        public ReinforceWorker Worker
        {
            get
            {
                if (workercache == null && workerClass != null)
                {
                    workercache = (ReinforceWorker)Activator.CreateInstance(workerClass);
                    workercache.def = this;
                }
                return workercache;
            }
        }

        public ReinforceWorker workercache;

    }


    public abstract class ReinforceWorker
    {
        public ReinforceDef def;

        public abstract bool Appliable(ThingWithComps thing);

        public abstract Func<bool> Reinforce(ThingComp_Reinforce comp, int level);

        public virtual string ResultString(int level)
        {
            return def.label + " +" + def.offsetPerLevel * level * 100 + "%";
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
