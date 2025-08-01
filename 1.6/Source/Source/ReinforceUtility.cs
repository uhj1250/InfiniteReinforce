﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI.Group;

namespace InfiniteReinforce
{
    public enum ReinforceType
    {
        None,
        Stat,
        Special,
        Custom
    }

    public enum CostMode
    {
        SameThing = 0,
        Material = 1,
        Fuel = 2
    }

    public static class ReinforceUtility
    {
        public static List<StatDef> ReinforcableStats;
        public static List<ReinforceableStatDef> WhiteList;
        public static List<ReinforceDef> ReinforceDefs = DefDatabase<ReinforceDef>.AllDefs.ToList();
        public static Dictionary<StatDef, bool> LowerBetter = null;
        

        public static ThingComp_Reinforce GetReinforceComp(this ThingWithComps thing)
        {
            return thing.TryGetComp<ThingComp_Reinforce>();
        }

        public static float GetReinforceStatFactor(this ThingWithComps thing, StatDef def)
        {
            ThingComp_Reinforce reinforce = thing.GetReinforceComp();
            if (reinforce != null)
            {
                return reinforce.GetStatFactor(def);
            }
            return 1.0f;
        }

        public static float GetReinforceCustomFactor(this ThingWithComps thing, ReinforceDef def)
        {
            ThingComp_Reinforce reinforce = thing.GetReinforceComp();
            if (reinforce != null)
            {
                return reinforce.GetCustomFactor(def);
            }
            return 1.0f;
        }

        public static void GetReinforceInfo(this ThingComp_Reinforce reinforce, StatDef def, out float factor, out int count)
        {
            float fres = 1.0f;
            int cres = 0;
            if (reinforce != null)
            {
                fres = reinforce.GetStatFactor(def);
                cres = reinforce.GetReinforcedCount(def);
            }
            factor = fres;
            count = cres;
        }

        public static int GetReinforcedCount(this ThingWithComps thing)
        {
            ThingComp_Reinforce reinforce = thing.GetReinforceComp();
            if (reinforce != null)
            {
                return reinforce.ReinforcedCount;
            }
            return 0;
        }


        public static int GetReinforcedCount(this Thing thing)
        {
            ThingWithComps thingwithcomp = thing as ThingWithComps;
            if (thingwithcomp != null) return thingwithcomp.GetReinforcedCount();
            return 0;
        }

        public static bool TryReinforce(this ThingWithComps thing, StatDef stat, int level = 1)
        {
            ThingComp_Reinforce comp = thing.GetReinforceComp();
            if (comp != null)
            {
                return comp.ReinforceStat(stat, level);
            }
            return false;
        }

        public static bool IsReinforcable(this Thing thing)
        {
            //MinifiedThing minified = thing as MinifiedThing; (minified !=null && minified.InnerThing.def.IsReinforcable()) || 
            return (thing is ThingWithComps && IsReinforcable(thing.def));
        }

        public static bool IsReinforcable(this ThingDef thingDef)
        {
            return thingDef.stackLimit <= 1 && (thingDef.IsApparel || thingDef.IsWeapon);
        }


        public static bool IsReinforcable(this StatCategoryDef category)
        {
            return category == StatCategoryDefOf.Weapon 
                || category == StatCategoryDefOf.Apparel 
                || category == StatCategoryDefOf.Weapon_Ranged 
                || category == StatCategoryDefOf.Weapon_Melee;
        }

        public static bool IsStatAppliable(this StatDef stat, ThingWithComps thing)
        {
            if (thing.GetStatValue(stat) == 0) return false;
            string deflower;

            ReinforceableStatDef def = WhiteList.FirstOrDefault(x => x.defName.Equals(stat.defName));
            if (def != null)
            {
                if (def.disable) return false;
                else if (def.isGeneric) return true;
                else if (def.workerClass != null) return def.Worker.IsAppliable(thing);
            }

            if (StatRequest.For(thing).StatBases.StatListContains(stat))
            {
                return true;
            }
            else if (stat.category.defName.ToLower().Contains("weapon"))
            {
                deflower = stat.defName.ToLower();
                if (thing.def.IsWeapon)
                {
                    if (thing.def.IsRangedWeapon && (stat.category == StatCategoryDefOf.Weapon_Ranged || deflower.Contains("range"))) return true;
                    else if (thing.def.IsMeleeWeapon && (stat.category == StatCategoryDefOf.Weapon_Melee || deflower.Contains("melee"))) return true;
                }
                //else if (thing is Building_Turret)
                //{
                //    if (deflower.Contains("range")) return true;
                //}
            }
            else if (stat.category == StatCategoryDefOf.Apparel && thing.def.IsApparel)
            {
                deflower = stat.defName.ToLower();
                if (thing.Stuff != null && deflower.Contains("stuff")) return true;
            }
            //else if (stat.category == StatCategoryDefOf.Building && thing.def.IsBuildingArtificial)
            //{
            //
            //}
            


            return false;
        }

        public static float GetOffsetPerLevel(this StatDef stat)
        {
            ReinforceableStatDef def = DefDatabase<ReinforceableStatDef>.GetNamedSilentFail(stat.defName);
            if (def != null) return def.offsetPerLevel;
            return ReinforceableStatDef.offsetPerLevelDefault;
        }

        public static void CountThingInCollection(this IEnumerable<Thing> things, ref List<ThingDefCountClass> list, ThingDef stuff = null)
        {
            if (!things.EnumerableNullOrEmpty()) foreach (Thing thing in things)
            {
                if (thing.CannotUseAsMaterial()) continue;
                for (int i = 0; i < list.Count; i++)
                {
                    if (stuff != null && thing.Stuff != null)
                        {
                            if (stuff == thing.Stuff) list[i].count += thing.stackCount;
                        }
                    else if (thing.def == list[i].thingDef) list[i].count += thing.stackCount;
                }
            }
        }

        public static int CountThingInCollection(this IEnumerable<Thing> things, ThingDef def, ThingDef stuff = null)
        {
            int res = 0;
            if (!things.EnumerableNullOrEmpty()) foreach (Thing thing in things)
                {
                    if (thing.CannotUseAsMaterial()) continue;
                    if (stuff != null && thing.Stuff != null)
                    {
                        if (stuff == thing.Stuff) res += thing.stackCount;
                    }
                    else if (thing.def == def) res += thing.stackCount;
                }

            return res;
        }



        public static void EliminateThingsOfType(this Map map, ThingDef def, int cost)
        {
            while (cost > 0)
            {
                Thing thing = null;
                foreach (Building_OrbitalTradeBeacon building_OrbitalTradeBeacon in Building_OrbitalTradeBeacon.AllPowered(map))
                {
                    foreach (IntVec3 c in building_OrbitalTradeBeacon.TradeableCells)
                    {
                        foreach (Thing thing2 in map.thingGrid.ThingsAt(c))
                        {
                            if (thing2.def == def)
                            {
                                thing = thing2;
                                break;
                            }
                        }
                    }
                }
                if (thing == null)
                {
                    Log.Error("Could not find any " + def + " to remove.");
                    return;
                }
                int num = Math.Min(cost, thing.stackCount);
                thing.SplitOff(num).Destroy(DestroyMode.Vanish);
                cost -= num;
            }
        }

        public static List<Thing> GetThingsOfType(this IEnumerable<Thing> things, ThingDef def, int cost, ThingDef stuff = null)
        {
            List<Thing> result = new List<Thing>();
            while (cost > 0)
            {
                Thing thing = null;
                foreach (Thing t in things)
                {
                    if (t.Spawned && t.def == def && !t.CannotUseAsMaterial())
                    {
                        if (stuff != null && stuff != t.Stuff)
                        {
                            continue;
                        }
                        thing = t;
                        break;
                    }
                }
                if (thing == null)
                {
                    Log.Error("Could not find any " + def + " to remove.");
                    return null;
                }
                int num = Math.Min(cost, thing.stackCount);
                result.Add(thing.SplitOff(num));
                cost -= num;
            }
            return result;
        }

        public static float GetFailureChance(this ThingComp_Reinforce comp, float multiply)
        {
            if (IRConfig.WeenieMode | IRConfig.BadassMode) multiply *= IRConfig.FailureChanceMultiplier;
            return Mathf.Min(99.99f , Mathf.Min(50f, comp.ReinforcedCount) * multiply);
        }

        public static bool RollFailure(this ThingComp_Reinforce comp, out float rolled, out float chance, int totalweight, float multiply)
        {
            chance = totalweight * comp.GetFailureChance(multiply)/100;
            rolled = Rand.Range(0f, totalweight);
            if (DebugSettings.godMode) return false;
            return chance > rolled;
        }

        public static int[] GetFailureWeights(this ThingComp_Reinforce comp, out int totalweight)
        {
            totalweight = IRConfig.BaseWeights.Sum();
            return IRConfig.BaseWeights;
        }

        public static bool LowerIsBetter(this StatDef stat)
        {
            if (LowerBetter == null)
            {
                LowerBetter = new Dictionary<StatDef, bool>();
                List<StatDef> statlist = DefDatabase<StatDef>.AllDefsListForReading;
                for(int i=0; i<statlist.Count; i++)
                {
                    LowerBetter.Add(statlist[i], LowerIsBetter(statlist[i].defName));
                }
            }

            return LowerBetter.TryGetValue(stat, false);
        }

        public static bool LowerIsBetter(string defName)
        {
            ReinforceableStatDef def = DefDatabase<ReinforceableStatDef>.GetNamedSilentFail(defName);
            if (def != null && def.reversal) return true;

            string deflower = defName.ToLower();
            
            if (deflower.Contains("delay")
                || deflower.Contains("flammability")
                || deflower.Contains("cooldown")) return true;
            return false;
        }

        public static IEnumerable<Thing> AllThingsNearBeacon(Map map, Func<Thing, bool> predicate = null)
        {
            HashSet<Thing> yieldedThings = new HashSet<Thing>();
            if (predicate == null) predicate = delegate { return true; };
            foreach (Building_OrbitalTradeBeacon item in Building_OrbitalTradeBeacon.AllPowered(map))
            {
                foreach (IntVec3 tradeableCell in item.TradeableCells)
                {
                    IEnumerable<Thing> thingList = tradeableCell.GetThingList(map).Where(predicate);
                    yieldedThings.AddRange(thingList);
                }
            }
            return yieldedThings;
        }

        public static bool CannotUseAsMaterial(this Thing thing)
        {
            if (thing.GetReinforcedCount() > 0) return true;
            if (thing.def.IsWeapon || thing.def.IsApparel)
            {
                if (thing.TryGetQuality(out QualityCategory qc))
                {
                    float hp = (float)thing.HitPoints / thing.MaxHitPoints;
                    return !IRConfig.MaterialQualityRange.Includes(qc) || !IRConfig.DurabilityRange.Includes(hp);
                }
                else
                {
                    float hp = (float)thing.HitPoints / thing.MaxHitPoints;
                    return !IRConfig.DurabilityRange.Includes(hp);
                }
            }
            return false;
        }

        public static string GetStatParts(this StatDef stat)
        {
            if (stat.parts == null || stat.parts.Count == 0) return "None";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stat.parts.Count; i++)
            {
                sb.Append(stat.parts[i].GetType().Name + "\n");
                if (i < stat.parts.Count - 1) sb.Append(", ");
            }
            return sb.ToString();
        }

    }

}
