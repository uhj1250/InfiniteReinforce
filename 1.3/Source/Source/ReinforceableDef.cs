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
        public float offsetPerLevel = offsetPerLevelDefault;
    }


}
