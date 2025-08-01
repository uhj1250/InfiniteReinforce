﻿using System;
using System.Collections;
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
        protected int discount = 0;
        protected Dictionary<StatDef, float> statboost = new Dictionary<StatDef, float>(new StatDefComparer());
        protected Dictionary<StatDef, int> reinforcedcount = new Dictionary<StatDef, int>(new StatDefComparer());
        protected Dictionary<ReinforceDef, float> custom = new Dictionary<ReinforceDef, float>();
        protected Dictionary<ReinforceDef, int> customcount = new Dictionary<ReinforceDef, int>();
        protected IRDifficultFlag difficult;

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
            float factor = 1.0f;
            if (IRConfig.BabyMode | IRConfig.ProMode) factor *= IRConfig.CostIncrementMultiplier;
            int qc = 0;
            if (parent.TryGetQuality(out QualityCategory quality)) qc = (int)quality - 2;

            return Math.Max(0, 1.0f + (reinforcedcount - discount + qc) * FactorPer*factor);
        }

        public float CostMultiplierOf(int start, int dest)
        {
            float factor = 1.0f;
            if (IRConfig.BabyMode | IRConfig.ProMode) factor *= IRConfig.CostIncrementMultiplier;
            int qc = 0;
            if (parent.TryGetQuality(out QualityCategory quality)) qc = (int)quality - 2 - discount;
            start += qc;
            dest += qc;
            return 0.5f*(1 + dest - start)*(FactorPer * factor * (start + dest) + 2);
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
            Scribe_Values.Look(ref discount, "discount", 0, true);
            Scribe_Values.Look(ref difficult, "difficult", IRDifficultFlag.None, true);
            Scribe_Collections.Look(ref statboost, "statboost", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref reinforcedcount, "reinforcedcount", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref custom, "custom", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref customcount, "customcount", LookMode.Def, LookMode.Value);
            if (statboost == null) statboost = new Dictionary<StatDef, float>(new StatDefComparer());
            if (reinforcedcount == null) reinforcedcount = new Dictionary<StatDef, int>(new StatDefComparer());
            if (custom == null) custom = new Dictionary<ReinforceDef, float>();
            if (customcount == null) customcount = new Dictionary<ReinforceDef, int>();
        }

        public override void DrawGUIOverlay()
        {
            if (Find.CameraDriver.CurrentZoom > CameraZoomRange.Close) return;
            QualityCategory cat;
            ThingWithComps thing = parent;
            int count = reinforced;
            if (thing is MinifiedThing)
            {
                thing = thing.GetInnerIfMinified() as ThingWithComps;
                count = thing.GetReinforcedCount();
            }

            if (parent.TryGetQuality(out cat))
            {
                GenMapUI.DrawThingLabel(thing, cat.GetLabelShort() + " +" + count);
            }
            else
            {
                GenMapUI.DrawThingLabel(thing, "+" + count);
            }
        }



        public override string TransformLabel(string label)
        {
            return reinforced > 0 ? label + " +" + reinforced : label;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Source, Keyed.ReinforceFlag, difficult.Translate(), Keyed.ReinforceFlagDesc, 0);


        }

        public new float GetStatFactor(StatDef def)
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
            Reinforced();
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
            Reinforced();
            return true;
        }

        protected void Reinforced()
        {
            if (IRConfig.WeenieMode && IRConfig.FailureChanceMultiplier < 1.0f) difficult |= IRDifficultFlag.Weenie;
            else if (IRConfig.BadassMode && IRConfig.FailureChanceMultiplier > 1.0f) difficult |= IRDifficultFlag.Badass;
            if (IRConfig.SuperWeenieMode) difficult |= IRDifficultFlag.SuperWeenie;
            else if (IRConfig.IronMode) difficult |= IRDifficultFlag.Ironman;
            if (IRConfig.BabyMode && IRConfig.CostIncrementMultiplier < 1.0f) difficult |= IRDifficultFlag.Baby;
            else if (IRConfig.ProMode && IRConfig.CostIncrementMultiplier > 1.0f) difficult |= IRDifficultFlag.Pro;
            reinforced++;
        }

        
        public void AddDiscount(int count)
        {
            discount += count;
        }

        public class StatDefComparer : IEqualityComparer<StatDef>
        {
            public bool Equals(StatDef x, StatDef y)
            {
                if (x != null && y != null)
                {
                    return x.defName.Equals(y.defName);
                }

                return false;
            }

            public int GetHashCode(StatDef obj)
            {
                return base.GetHashCode();
            }
        }

    }
}
