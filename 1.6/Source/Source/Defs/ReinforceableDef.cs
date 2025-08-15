using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;



namespace InfiniteReinforce
{
    public class ReinforceableStatDef : Def
    {
        public const float offsetPerLevelDefault = 0.01f;
        
        public bool isGeneric = true;
        public bool reversal = false;
        public bool disable = false;
        public float offsetPerLevel = offsetPerLevelDefault;
        public Type workerClass = null;

        public ReinforceableWorker Worker
        {
            get
            {
                if (workercache == null && workerClass != null)
                {
                    workercache = (ReinforceableWorker)Activator.CreateInstance(workerClass);
                    workercache.def = this;
                }
                return workercache;
            }
        }

        protected ReinforceableWorker workercache;


    }


}
