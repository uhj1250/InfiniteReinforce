using System;
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

}
