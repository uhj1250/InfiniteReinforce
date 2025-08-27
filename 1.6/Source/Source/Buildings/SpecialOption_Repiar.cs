using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace InfiniteReinforce
{
    public class SpecialOption_Repiar : IReinforceSpecialOption
    {
        public bool Enable(ThingWithComps thing)
        {
            return thing.HitPoints < thing.MaxHitPoints;
        }

        public bool Appliable(ThingWithComps thing)
        {
            return thing.MaxHitPoints > 0 && !(thing is Pawn);
        }

        public Func<bool> Reinforce(ThingComp_Reinforce comp)
        {
            return delegate ()
            {
                comp.parent.HitPoints = comp.parent.MaxHitPoints;
                return true;
            };
        }

        public string LabelLeft(ThingComp_Reinforce comp)
        {
            return Keyed.Repair;
        }

        public string LabelRight(ThingComp_Reinforce comp)
        {
            return null;
        }

    }

}
