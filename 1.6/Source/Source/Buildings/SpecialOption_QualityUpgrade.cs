using System;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
    public class SpecialOption_QualityUpgrade : IReinforceSpecialOption
    {
        public bool Appliable(ThingWithComps thing)
        {
            return thing.compQuality != null;
        }

        public bool Enable(ThingWithComps thing)
        {
            return thing.compQuality?.Quality < QualityCategory.Legendary;
        }

        public string LabelLeft(ThingComp_Reinforce comp)
        {
            return comp.parent.compQuality?.CompInspectStringExtra();
        }

        public string LabelRight(ThingComp_Reinforce comp)
        {
            return null;
        }

        public Func<bool> Reinforce(ThingComp_Reinforce comp)
        {
            return delegate ()
            {
                var cq = comp.parent.compQuality;
                if (cq != null && cq.Quality < QualityCategory.Legendary)
                {
                    cq.SetQuality(cq.Quality + 1, ArtGenerationContext.Outsider);
                    return true;
                }
                else return false;
            };
        }
    }

}
