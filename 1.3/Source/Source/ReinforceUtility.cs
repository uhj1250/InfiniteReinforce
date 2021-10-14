using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;


namespace InfiniteReinforce
{

    public static class ReinforceUtility
    {
        public static List<StatDef> ReinforcableStats;
        public static List<ReinforceableStatDef> WhiteList;
        public static List<ReinforceDef> ReinforceDefs = DefDatabase<ReinforceDef>.AllDefs.ToList();


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
            return category == StatCategoryDefOf.Weapon || category == StatCategoryDefOf.Apparel;
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
            else if (stat.category == StatCategoryDefOf.Weapon)
            {
                deflower = stat.defName.ToLower();
                if (thing.def.IsWeapon)
                {
                    if (thing.def.IsRangedWeapon && deflower.Contains("range")) return true;
                    else if (thing.def.IsMeleeWeapon && deflower.Contains("melee")) return true;
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

        public static void CountThingInCollection(this IEnumerable<Thing> things, ref List<ThingDefCountClass> list)
        {
            if (!things.EnumerableNullOrEmpty()) foreach (Thing thing in things)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (thing.def == list[i].thingDef) list[i].count += thing.stackCount;
                }
            }
        }

        public static void ElminateThingsOfType(this Map map, ThingDef def, int cost)
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

        public static void EliminateThingOfType(this IEnumerable<Thing> things, ThingDef def, int cost)
        {
            while (cost > 0)
            {
                Thing thing = null;
                foreach (Thing t in things)
                {
                    if (t.Spawned && t.def == def)
                    {
                        thing = t;
                        break;
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

        public static float GetFailureChance(this ThingComp_Reinforce comp, float multiply)
        {
            if (IRConfig.WeenieMode) multiply *= IRConfig.FailureChanceMultiplier;
            return Mathf.Min(50f, comp.ReinforcedCount) * multiply;
        }

        public static bool RollFailure(this ThingComp_Reinforce comp, out float rolled, int totalweight, float multiply)
        {
            float chance = totalweight * comp.GetFailureChance(multiply)/100;
            rolled = Rand.Range(0f, totalweight);
            return chance > rolled;
        }

        public static int[] GetFailureWeights(this ThingComp_Reinforce comp, out int totalweight)
        {
            totalweight = 100;
            return IRConfig.BaseWeights;
        }

        public static bool LowerIsBetter(this StatDef stat)
        {
            string deflower = stat.defName.ToLower();

            if (deflower.Contains("delay")
                || deflower.Contains("flammability")
                || deflower.Contains("cooldown")) return true;

            return false;

        }

        public static IEnumerable<Thing> AllThingsNearBeacon(Map map)
        {
            HashSet<Thing> yieldedThings = new HashSet<Thing>();
            foreach (Building_OrbitalTradeBeacon item in Building_OrbitalTradeBeacon.AllPowered(map))
            {
                foreach (IntVec3 tradeableCell in item.TradeableCells)
                {
                    List<Thing> thingList = tradeableCell.GetThingList(map);
                    yieldedThings.AddRange(thingList);
                    //for (int i = 0; i < thingList.Count; i++)
                    //{
                    //    Thing thing = thingList[i];
                    //    if (thing.def.category == ThingCategory.Item && PlayerSellableNow(thing, trader) && !yieldedThings.Contains(thing))
                    //    {
                    //        yieldedThings.Add(thing);
                    //        yield return thing;
                    //    }
                    //}
                }
            }
            return yieldedThings;
        }


    }
}
