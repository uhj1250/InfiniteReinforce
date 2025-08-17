using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
    public class ReinforceWorker_Stats : ReinforceWorker
    {
        private StatDef stat;

        protected StatDef statDef
        {
            get
            {
                if (stat == null) stat = DefDatabase<StatDef>.GetNamed(def.defName);
                return stat;
            }
        } 

        public override bool Appliable(ThingWithComps thing)
        {
            return true;
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp, int level, float multiplier = 1)
        {
            return delegate { return comp.ReinforceStat(statDef, level, multiplier); };
        }

        public override string ResultString(int level)
        {
            return statDef.label + " +" + def.offsetPerLevel * level * 100 + "%";
        }

        public override string LeftLabel(ThingComp_Reinforce comp)
        {
            return statDef.label + " +" + comp.GetReinforcedCount(statDef);
        }
    }

    public class ReinforceWorker_EquipmentStats : ReinforceWorker_Stats
    {
        public override bool Appliable(ThingWithComps thing)
        {
            return thing.def.IsApparel || thing.def.IsWeapon;
        }
    }


}
