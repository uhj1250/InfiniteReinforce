using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;


namespace InfiniteReinforce
{
    public class CompReinforcerContainer : CompThingContainer
    {

        public override bool Accepts(Thing thing)
        {
            return !(thing is Building) && thing.IsReinforcable();
        }

        public override bool Accepts(ThingDef thingDef)
        {
            return !Empty && thingDef.IsReinforcable();
        }


    }
}
