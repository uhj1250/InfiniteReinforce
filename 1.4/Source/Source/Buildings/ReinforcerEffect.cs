using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace InfiniteReinforce
{
    public abstract class ReinforcerEffect
    {
        public abstract bool Apply(ThingComp_Reinforce comp);

        public abstract void DoEffect(Building_Reinforcer reinforcer, ThingComp_Reinforce comp);
    }

    public class ReinforcerEffect_Discount : ReinforcerEffect
    {
        public override bool Apply(ThingComp_Reinforce comp)
        {
            return true;
        }

        public override void DoEffect(Building_Reinforcer reinforcer, ThingComp_Reinforce comp)
        {
            comp.AddDiscount(2);
        }

    }

}
