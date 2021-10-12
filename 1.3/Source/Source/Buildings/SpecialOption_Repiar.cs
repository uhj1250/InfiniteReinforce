using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
    public abstract class ReinforceSpecialOption
    {
        public abstract bool Enable(ThingWithComps thing);

        public abstract bool Appliable(ThingWithComps thing);

        public abstract Func<bool> Reinforce(ThingComp_Reinforce comp);

        public abstract string LabelLeft(ThingComp_Reinforce comp);
        public abstract string LabelRight(ThingComp_Reinforce comp);

    }

    public class SpecialOption_Repiar : ReinforceSpecialOption
    {
        public override bool Enable(ThingWithComps thing)
        {
            return thing.HitPoints < thing.MaxHitPoints;
        }

        public override bool Appliable(ThingWithComps thing)
        {
            return true;
        }

        public override Func<bool> Reinforce(ThingComp_Reinforce comp)
        {
            return delegate ()
            {
                comp.parent.HitPoints = comp.parent.MaxHitPoints;
                return true;
            };
        }

        public override string LabelLeft(ThingComp_Reinforce comp)
        {
            return Keyed.Repair;
        }

        public override string LabelRight(ThingComp_Reinforce comp)
        {
            return null;
        }

    }

}
