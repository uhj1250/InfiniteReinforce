using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
    public interface IReinforceSpecialOption
    {
        bool Enable(ThingWithComps thing);

        bool Appliable(ThingWithComps thing);

        Func<bool> Reinforce(ThingComp_Reinforce comp);

        string LabelLeft(ThingComp_Reinforce comp);
        string LabelRight(ThingComp_Reinforce comp);

    }

    public class SpecialOption_Repiar : IReinforceSpecialOption
    {
        public bool Enable(ThingWithComps thing)
        {
            return thing.HitPoints < thing.MaxHitPoints;
        }

        public bool Appliable(ThingWithComps thing)
        {
            return true;
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
