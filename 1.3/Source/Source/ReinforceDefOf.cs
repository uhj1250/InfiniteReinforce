using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace InfiniteReinforce
{
    [DefOf]
    public static class ReinforceDefOf
    {
        public static readonly JobDef InsertEquipmentToReinforcer;
        public static readonly SoundDef Reinforce_Success;
        public static readonly SoundDef Reinforce_FailedCritical;
        public static readonly SoundDef Reinforce_FailedNormal;
        public static readonly SoundDef Reinforce_FailedMinor;

        public static readonly ReinforceDef Reinforce_Hitpoint;
        public static readonly ReinforceDef Reinforce_Reloadable;



    }
}
