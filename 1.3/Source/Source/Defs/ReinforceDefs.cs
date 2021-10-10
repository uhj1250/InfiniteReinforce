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
        public IntRange levelRange = new IntRange(1, 25);
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

        protected ReinforceWorker workercache;

    }

    public class ReinforceCostDef : Def
    {
        public List<ThingDefCountClass> costList;
    }

}
