using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;



namespace InfiniteReinforce
{
    [StaticConstructorOnStartup]
    public static class DefInjection
    {
        static DefInjection()
        {

            CompProperties prop = new CompProperties(typeof(ThingComp_Reinforce));
            //StatPart_Reinforce part = new StatPart_Reinforce();
            //StatPart_Reinforce_Reversal partrev = new StatPart_Reinforce_Reversal();
            List<ReinforceableStatDef> whitelist = DefDatabase<ReinforceableStatDef>.AllDefs.ToList();
            List<ThingDef> things = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsReinforcable()).ToList();
            List<StatDef> stats = DefDatabase<StatDef>.AllDefs.Where(x => whitelist.Exists(y => y.defName.Equals(x.defName)) || (x.category.IsReinforcable() && !ExcludedStatDefs(x))).ToList();
            for (int i=0; i<things.Count; i++)
            {
                things[i].comps.Add(prop);
                things[i].drawGUIOverlayQuality = false;
            }
            for (int i=0; i<stats.Count; i++)
            {
                if (stats[i].parts == null)
                {
                    stats[i].parts = new List<StatPart>();
                }
                string deflower = stats[i].defName.ToLower();
                ReinforceableStatDef def = DefDatabase<ReinforceableStatDef>.GetNamedSilentFail(stats[i].defName);
                if (def != null)
                {
                    if (def.disable) continue;
                    if (!def.reversal)
                    {
                        StatPart_Reinforce statpart = new StatPart_Reinforce();
                        statpart.parentStat = stats[i];
                        stats[i].parts.Add(statpart);
                    }
                    else
                    {
                        StatPart_Reinforce statpart = new StatPart_Reinforce_Reversal();
                        statpart.parentStat = stats[i];
                        stats[i].parts.Add(statpart);
                    }
                }
                else if (deflower.Contains("cooldown"))
                {
                    StatPart_Reinforce statpart = new StatPart_Reinforce_Reversal();
                    statpart.parentStat = stats[i];
                    stats[i].parts.Add(statpart);
                }
                else if (deflower.Contains("delay"))
                {
                    StatPart_Reinforce statpart = new StatPart_Reinforce_Reversal();
                    statpart.parentStat = stats[i];
                    stats[i].parts.Add(statpart);
                }
                else
                {
                    StatPart_Reinforce statpart = new StatPart_Reinforce();
                    statpart.parentStat = stats[i];
                    stats[i].parts.Add(statpart);
                }
            }
            ReinforceUtility.ReinforcableStats = stats;
            ReinforceUtility.WhiteList = whitelist;
        }

        public static bool ExcludedStatDefs(StatDef stat)
        {
            string deflower = stat.defName.ToLower();
            return deflower.Contains("dps") || deflower.Contains("insulation");
        }

        

    }
}
