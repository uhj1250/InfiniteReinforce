using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace InfiniteReinforce
{
    public class ThingComp_Reinforce : ThingComp
    {
        public const float FactorPer = 0.60f;

        protected int reinforced = 0;
        protected Dictionary<StatDef, float> statboost = new Dictionary<StatDef, float>();
        protected Dictionary<StatDef, int> reinforcedcount = new Dictionary<StatDef, int>();
        protected Dictionary<ReinforceDef, float> custom = new Dictionary<ReinforceDef, float>();
        protected Dictionary<ReinforceDef, int> customcount = new Dictionary<ReinforceDef, int>();


        public int ReinforcedCount
        {
            get
            {
                return reinforced;
            }
        }

        public float CostMultiplier
        {
            get
            {
                return CostMultiplierOf(reinforced);
            }
        }

        public float CostMultiplierOf(int reinforcedcount)
        {

            int qc = 0;
            if (parent.TryGetQuality(out QualityCategory quality)) qc = (int)quality - 2;

            return 1.0f + (reinforcedcount + qc) * FactorPer;
        }

        public float CostMultiplierOf(int start, int dest)
        {
            int qc = 0;
            if (parent.TryGetQuality(out QualityCategory quality)) qc = (int)quality - 2;
            start += qc;
            dest += qc;
            return 0.5f*(1 + dest - start)*(FactorPer * (start + dest) + 2);
        }

        
        public List<KeyValuePair<StatDef,float>> StatBoosts
        {
            get
            {
                return statboost.ToList();
            }
        }

        public List<KeyValuePair<StatDef, int>> ReinforcedCounts
        {
            get
            {
                return reinforcedcount.ToList();
            }
        }

        public override bool AllowStackWith(Thing other)
        {
            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref reinforced, "reinforced", 0, true);
            Scribe_Collections.Look(ref statboost, "statboost", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref reinforcedcount, "reinforcedcount", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref custom, "custom", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref customcount, "customcount", LookMode.Def, LookMode.Value);
            if (statboost == null) statboost = new Dictionary<StatDef, float>();
            if (reinforcedcount == null) reinforcedcount = new Dictionary<StatDef, int>();
            if (custom == null) custom = new Dictionary<ReinforceDef, float>();
            if (customcount == null) customcount = new Dictionary<ReinforceDef, int>();
        }

        public override void DrawGUIOverlay()
        {
            QualityCategory cat;
            if (parent.TryGetQuality(out cat))
            {
                GenMapUI.DrawThingLabel(parent, cat.GetLabelShort() + " +" + reinforced);
            }
            else
            {
                GenMapUI.DrawThingLabel(parent, "+" + reinforced);
            }
        }



        public override string TransformLabel(string label)
        {
            return reinforced > 0 ? label + " +" + reinforced : label;
        }

        public float GetStatFactor(StatDef def)
        {
            if (def != null)
            {
                return statboost.TryGetValue(def, 1.0f);
            }
            return 1.0f;
        }

        public float GetCustomFactor(ReinforceDef def)
        {
            if (def != null)
            {
                return custom.TryGetValue(def, 1.0f);
            }
            return 1.0f;
        }

        public int GetReinforcedCount(StatDef def)
        {
            if (def != null)
            {
                return reinforcedcount.TryGetValue(def, 0);
            }
            return 0;
        }

        public int GetReinforcedCount(ReinforceDef def)
        {
            if (def != null)
            {
                return customcount.TryGetValue(def, 0);
            }
            return 0;
        }

        public bool ReinforceStat(StatDef stat, int level)
        {
            if (!statboost.ContainsKey(stat))
            {
                statboost.Add(stat, 1.0f);
                reinforcedcount.Add(stat, 0);
            }

            statboost[stat] += stat.GetOffsetPerLevel()*level;
            reinforcedcount[stat] ++;
            reinforced++;
            return true;
        }

        public bool ReinforceCustom(ReinforceDef def, int level)
        {
            if (!custom.ContainsKey(def))
            {
                custom.Add(def, 1.0f);
                customcount.Add(def, 0);
            }

            custom[def] += def.offsetPerLevel * level;
            customcount[def]++;
            reinforced++;
            return true;
        }

    }
}
