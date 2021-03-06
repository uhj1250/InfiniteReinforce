using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using InfiniteReinforce.UI;

namespace InfiniteReinforce
{
    public class Dialog_Reinforcer : Window
    {
        public enum CostMode
        {
            SameThing = 0,
            Material = 1,
            Fuel = 2
        }

        public const int CostModeCount = 3;
        public const float FontHeight = 22f;
        public const int BaseReinforceTicks = 120;

        public int ReinforceTicks
        {
            get
            {
                return BaseReinforceTicks * building.MaxHitPoints / building.HitPoints;
            }
        }

        protected Building_Reinforcer building;
        protected float progress = 0f;
        protected bool onprogress = false;
        protected bool? success = null;
        protected int? selectedindex = null;
        protected Func<bool> reinforceaction;
        protected List<StatDef> statlist = new List<StatDef>();
        protected IEnumerable<Thing> resourcethings;
        protected List<ThingDefCountClass>[] costlist = new List<ThingDefCountClass>[CostModeCount];
        protected List<ThingDefCountClass> thingcountcache = new List<ThingDefCountClass>();
        protected List<string> reinforcehistory = new List<string>();
        protected string reinforcehistorycache;
        protected CostMode costMode; 


        protected ThingWithComps thing => building?.HoldingItem;


        protected ThingComp_Reinforce comp
        {
            get
            {
                if (compcache == null)
                {
                    compcache = thing.GetReinforceComp();
                }
                return compcache;
            }
        }

        protected bool CanUse => building?.PowerOn ?? false;



        private ThingComp_Reinforce compcache;
        private static GUIStyleState fontstylestate = new GUIStyleState() { textColor = Color.white };
        private static GUIStyleState boxstylestate = GUI.skin.textArea.normal;
        private static GUIStyleState buttonstylestate = GUI.skin.button.normal;
        private static GUIStyle fontcenter = new GUIStyle() { alignment = TextAnchor.MiddleCenter, normal = fontstylestate };
        private static GUIStyle fontright = new GUIStyle() { alignment = TextAnchor.MiddleRight, normal = fontstylestate };
        private static GUIStyle fontleft = new GUIStyle() { alignment = TextAnchor.MiddleLeft, normal = fontstylestate };
        private static GUIStyle box = new GUIStyle(GUI.skin.textArea) { hover = boxstylestate, onHover = boxstylestate, onNormal = boxstylestate };
        private static GUIStyle button = new GUIStyle(GUI.skin.button) { hover = buttonstylestate, onHover = buttonstylestate, onNormal = buttonstylestate };

        public Dialog_Reinforcer(Building_Reinforcer building)
        {
            ChangeBuilding(building);
        }

        public void ChangeBuilding(Building_Reinforcer building)
        {
            this.building = building;
            ResetProgress();
            success = null;
            selectedindex = null;
            BuildStatList();
            if (thing != null)
            {
                BuildCostList();
                costMode = InitialCostMode();
                UpdateThingList();
            }
        }

        public void BuildStatList()
        {
            statlist.Clear();
            for (int i=0; i< ReinforceUtility.ReinforcableStats.Count; i++)
            {
                StatDef stat = ReinforceUtility.ReinforcableStats[i];
                if (stat.IsStatAppliable(thing))
                {
                    statlist.Add(stat);
                    continue;
                }
            }

        }

        public void BuildCostList()
        {

            //Build same thing
            if (costlist[(int)CostMode.SameThing] == null) costlist[(int)CostMode.SameThing] = new List<ThingDefCountClass>();
            costlist[(int)CostMode.SameThing].Clear();
            costlist[(int)CostMode.SameThing].Add(new ThingDefCountClass(thing.def, 1));

            //Build material cost
            ReinforceCostDef costDef = DefDatabase<ReinforceCostDef>.GetNamedSilentFail(thing.def.defName);
            if (costlist[(int)CostMode.Material] == null) costlist[(int)CostMode.Material] = new List<ThingDefCountClass>();
            costlist[(int)CostMode.Material].Clear();
            if (costDef != null)
            {
                if (!costDef.costList.NullOrEmpty()) costlist[(int)CostMode.Material].AddRange(costDef.costList);
            }
            else
            {
                if (!thing.def.costList.NullOrEmpty()) costlist[(int)CostMode.Material].AddRange(thing.def.CostList);
                if (thing.Stuff != null)
                {
                    ThingDefCountClass stuff = costlist[(int)CostMode.Material].FirstOrDefault(x => x.thingDef == thing.Stuff);
                    if (stuff != null)
                    {
                        stuff.count += thing.def.costStuffCount;
                    }
                    else
                    {
                        costlist[(int)CostMode.Material].Add(new ThingDefCountClass(thing.Stuff, thing.def.CostStuffCount));
                    }
                }
            }

            //Build fuel cost
            if (costlist[(int)CostMode.Fuel] == null) costlist[(int)CostMode.Fuel] = new List<ThingDefCountClass>();
            costlist[(int)CostMode.Fuel].Clear();
            if (building.Fuel > 0)
            {
                costlist[(int)CostMode.Fuel].Add(new ThingDefCountClass(building.FuelThing.FirstOrDefault(), 1));
            }
        }

        public void UpdateThingList()
        {
            thingcountcache.Clear();
            if (costMode == CostMode.Fuel && building.Fuel > 0)
            {
                IEnumerable<ThingDef> fuelthings = building.FuelThing;
                if (!fuelthings.EnumerableNullOrEmpty()) foreach(ThingDef def in fuelthings)
                    { 
                        thingcountcache.Add(new ThingDefCountClass(def, (int)building.Fuel));
                    }
            }
            else
            {
                if (costMode == CostMode.Fuel) costMode = InitialCostMode();
                resourcethings = ReinforceUtility.AllThingsNearBeacon(building.Map).Where(x => costlist[(int)costMode].Exists(y => y.thingDef == x.def));//TradeUtility.AllLaunchableThingsForTrade(building.Map).Where(x => costlist.Exists(y => y.thingDef == x.def));
                if (!costlist[(int)costMode].NullOrEmpty())
                {
                    foreach (ThingDefCountClass cost in costlist[(int)costMode])
                    {
                        thingcountcache.Add(new ThingDefCountClass(cost.thingDef, 0));
                    }
                }

                if (costMode == CostMode.SameThing) resourcethings.CountThingInCollection(ref thingcountcache, thing.Stuff);
                else resourcethings.CountThingInCollection(ref thingcountcache);
            }
        }

        public CostMode InitialCostMode()
        {
            if (building.Fuel > 0) return CostMode.Fuel;
            if (costlist[(int)CostMode.Material].NullOrEmpty()) return CostMode.SameThing;
            return CostMode.Material;
        }

        public int CostOf(int index)
        {
            if (costMode == CostMode.Fuel && !building.ApplyMultiplier) return costlist[(int)costMode][index].count;
            return (int)(costlist[(int)costMode][index].count * comp.CostMultiplier);
        }

        public void ResetProgress()
        {
            progress = 0f;
            onprogress = false;
            StatsReportUtility.Notify_QuickSearchChanged();
        }

        public void RemoveIngredients()
        {
            if (costMode == CostMode.Fuel && building.Fuel > 0)
            {
                building.FuelComp.ConsumeOnce();
                if (building.Fuel <= 0) BuildCostList();
                ReinforcerEffect effect = building.FuelComp.Props.Effect;
                if (effect != null && effect.Apply(comp)) effect.DoEffect(building, comp);
            }
            else
            {
                for (int i = 0; i < costlist[(int)costMode].Count; i++)
                {
                    if (costMode == CostMode.SameThing) resourcethings.EliminateThingOfType(costlist[(int)costMode][i].thingDef, CostOf(i), thing.Stuff);
                    else resourcethings.EliminateThingOfType(costlist[(int)costMode][i].thingDef, CostOf(i));
                }
            }
            UpdateThingList();
        }

        public bool CheckIngredients()
        {
            for(int i=0; i<costlist[(int)costMode].Count; i++)
            {
                int count = thingcountcache.FirstOrDefault(x => x.thingDef == costlist[(int)costMode][i].thingDef)?.count ?? 0;
                if (count < CostOf(i)) return false;
            }
            return true;
        }

        public void Reinforce(Func<bool> action, int index, string resultstring)
        {
            UpdateThingList();
            if (CheckIngredients())
            {
                onprogress = true;
                selectedindex = index;
                reinforceaction = action;
                reinforcehistorycache = resultstring;
                ReinforceDefOf.Reinforce_Progress.PlayOneShotOnCamera();
            }
            else 
            {
                Messages.Message(Keyed.NotEnough, MessageTypeDefOf.RejectInput, false);
            }
        }

        public bool Reinforced()
        {
            ResetProgress();
            if (thing != null)
            {
                int[] weights = comp.GetFailureWeights(out int totalweight);
                success = (costMode == CostMode.Fuel && building.AlwaysSuccess) || !comp.RollFailure(out float rolled, totalweight, building.MaxHitPoints / building.HitPoints);
                RemoveIngredients();
                if (success ?? false)
                {
                    reinforceaction();
                    reinforcehistory.Add(reinforcehistorycache.CapitalizeFirst());
                    ReinforceDefOf.Reinforce_Success.PlayOneShotOnCamera();
                    return true;
                }
                else
                {
                    ReinforceFailureResult effect = FailureEffect(totalweight, weights);
                    reinforcehistory.Add(Keyed.Failed.CapitalizeFirst() + " - " + effect.Translate());
                    return false;
                }
            }
            success = null;
            return false;
        }

        public ReinforceFailureResult FailureEffect(float totalweight, int[] weights)
        {
            float sum = 0;
            float rand = Rand.Range(0, totalweight);

            for (int i=0; i<weights.Length; i++)
            {
                sum += weights[i];
                if (rand < sum)
                {
                    DoFailureEffect((ReinforceFailureResult)i);
                    return (ReinforceFailureResult)i;
                }
            }
            return ReinforceFailureResult.None;
        }

        public void DoFailureEffect(ReinforceFailureResult res)
        {
            switch (res)
            {
                case ReinforceFailureResult.None:
                default:
                    ReinforceDefOf.Reinforce_FailedMinor.PlayOneShotOnCamera();
                    break;
                case ReinforceFailureResult.DamageLittle:
                    DamageThing(Rand.Range(1, 10));
                    if (!thing?.Destroyed ?? false) Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDamaged(thing.Label), LetterDefOf.NegativeEvent, building);
                    break;
                case ReinforceFailureResult.DamageLarge:
                    DamageThing(Rand.Range(10, 60));
                    if (!thing?.Destroyed ?? false) Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDamaged(thing.Label), LetterDefOf.NegativeEvent, building);
                    break;
                case ReinforceFailureResult.Explosion:
                    Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedExplosion(building.Label), LetterDefOf.NegativeEvent, building);
                    Explosion();
                    break;
                case ReinforceFailureResult.Destroy:
                    Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDestroy(thing.Label), LetterDefOf.Death, building);
                    thing.Destroy(DestroyMode.Vanish);
                    ReinforceDefOf.Reinforce_FailedCritical.PlayOneShotOnCamera();
                    break;
            }


        }

        public void DamageThing(float damage)
        {
            if (thing.HitPoints <= damage)
            {
                Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDestroy(thing.Label), LetterDefOf.Death, building);
                ReinforceDefOf.Reinforce_FailedCritical.PlayOneShotOnCamera();
                thing.Destroy(DestroyMode.Vanish);
            }
            else
            {
                if (damage > 10) ReinforceDefOf.Reinforce_FailedNormal.PlayOneShotOnCamera();
                else ReinforceDefOf.Reinforce_FailedMinor.PlayOneShotOnCamera();
                thing.HitPoints -= (int)damage;
            }
        }

        public void Explosion()
        { 
            DamageThing(Rand.Range(30, 80));
            building.HitPoints = Math.Max((int)(building.HitPoints * Rand.Range(0.65f,0.90f)), 1);
            GenExplosion.DoExplosion(building.Position, building.Map, Rand.Range(1, 3), DamageDefOf.Bomb, building, Rand.Range(50, 120));
        }

        public static void ToggleWindow(Building_Reinforcer building)
        {
            Dialog_Reinforcer window = (Dialog_Reinforcer)Find.WindowStack.Windows.FirstOrDefault(x => x is Dialog_Reinforcer);
            if (window != null)
            {
                if (window.building != building)
                {
                    SoundDefOf.TabOpen.PlayOneShotOnCamera(building.Map);
                    window.ChangeBuilding(building);
                }
            }
            else
            {
                Find.WindowStack.Add(new Dialog_Reinforcer(building));
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                float width = 900f;
                float height = 600f;
                soundClose = SoundDefOf.CommsWindow_Close;
                absorbInputAroundWindow = false;
                forcePause = false;
                preventCameraMotion = false;
                draggable = true;
                doCloseX = true;
                forcePause = true;
                return new Vector2(width, height);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// From here, UI stuffs
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

        public override void DoWindowContents(Rect inRect)
        {
            if (!building.Spawned || thing == null) Close(true);
            MainContents(inRect.ContractedBy(4f));
        }
        
        protected void MainContents(Rect rect)
        {
            float sectionWidth = rect.width / 3;
            float sectionHeight = rect.height - FontHeight;
            float statHeight = FontHeight * 8 + 8f;

            Rect leftRect = new Rect(rect.x, rect.y, sectionWidth, sectionHeight);
            Rect centerRect = new Rect(rect.x + sectionWidth, rect.y + statHeight, sectionWidth, sectionHeight - statHeight);
            Rect rightRect = new Rect(rect.x + sectionWidth*2, rect.y + statHeight, sectionWidth, sectionHeight - statHeight);
            Rect progressRect = new Rect(rect.x, rect.yMax - FontHeight, rect.width, FontHeight);
            Rect statRect = new Rect(centerRect.x, rect.y, sectionWidth*2, statHeight);
            
            if (thing != null)
            {
                LeftSection(leftRect);
                StatInfo(statRect);
                CenterSection(centerRect);
                RightSection(rightRect);
            }

            if (onprogress) progress++;
            Widgets.FillableBar(progressRect, progress / ReinforceTicks);
            if (progress > ReinforceTicks)
            {
                success = Reinforced();
            }
            if (success == true)
            {
                progressRect.DrawBinkTexture(Color.cyan, Color.black);
                GUI.Label(progressRect, Keyed.Success, fontcenter);
            }
            else if (success == false)
            {
                progressRect.DrawBinkTexture(Color.red, Color.black);
                GUI.Label(progressRect, Keyed.Failed, fontcenter);
            }


        }
        
        protected void LeftSection(Rect rect)
        {
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(rect);
            Rect thingRect = listmain.GetRect(FontHeight*3);

            ThingInfo(thingRect);

            GUI.Label(listmain.GetRect(FontHeight), Keyed.History, fontleft);
            ReinforceHistory(listmain.GetRect(rect.height - thingRect.height - 26f));

            listmain.End();

        }

        protected void CenterSection(Rect rect)
        {
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(rect);

            Rect resourceLabelRect = listmain.GetRect(FontHeight);
            Rect costModeRect = new Rect(resourceLabelRect.xMax - FontHeight, resourceLabelRect.y, FontHeight, FontHeight);
            GUI.Label(resourceLabelRect, Keyed.Materials, fontleft);
            for (int i=0; i< CostModeCount; i++)
            {
                if (!costlist[i].NullOrEmpty())
                {
                    ThingDef firstdef = costlist[i].FirstOrDefault().thingDef;
                    if (Widgets.ButtonImage(costModeRect, firstdef.uiIcon, true))
                    {
                        if (onprogress)
                        {
                            SoundDefOf.ClickReject.PlayOneShotOnCamera();
                        }
                        else
                        {
                            costMode = (CostMode)i;
                            UpdateThingList();
                            SoundDefOf.Click.PlayOneShotOnCamera();
                        }
                    }
                    if (i == (int)costMode) Widgets.DrawHighlight(costModeRect);
                }


                costModeRect.x -= FontHeight;
            }



            ResourceInfo(listmain.GetRect(FontHeight * 8 + 8f));

            GUI.Label(listmain.GetRect(FontHeight), Keyed.FailureOutcome, fontleft);
            FailureInfo(listmain.GetRect(FontHeight * 5 + 8f));

            listmain.End();

        }
        
        protected void RightSection(Rect rect)
        {
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(rect);

            Rect failureRect = listmain.GetRect(FontHeight);

            float chance = (costMode == CostMode.Fuel && building.AlwaysSuccess) ? 0 : comp.GetFailureChance((float)building.MaxHitPoints / building.HitPoints);
            GUI.color = Color.Lerp(Color.green,Color.red, chance/50f);
            GUI.Box(failureRect, "");
            GUI.Label(failureRect, " " + Keyed.FailureChance, fontleft);
            GUI.Label(failureRect, String.Format(" {0:0.00}%", chance), fontright);
            GUI.color = Color.white;

            if (costMode == CostMode.Fuel && building.Fuel > 0)
            {
                if (!building.FuelComp.Props.SpecialOptions.NullOrEmpty())
                {
                    List<ReinforceSpecialOption> options = building.FuelComp.Props.SpecialOptions;
                    for (int i=0; i<options.Count; i++)
                    {
                        SpecialOption(listmain.GetRect(FontHeight), options[i],i);
                    }
                }
            }

            for(int i=0; i<statlist.Count; i++)
            {
                StatOption(listmain.GetRect(FontHeight), statlist[i], i);
            }
            for (int i=0; i<ReinforceUtility.ReinforceDefs.Count; i++)
            {
                ReinforceDef def = ReinforceUtility.ReinforceDefs[i];
                if (def.Worker.Appliable(thing)) CustomOption(listmain.GetRect(FontHeight), def, i);
            }
            
            
            listmain.End();
        }
        

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /// UI components
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        protected void ThingInfo(Rect rect)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            Rect labelRect = new Rect(rect.x + iconRect.width, rect.y, rect.width - iconRect.width, rect.height/3);
            Rect labelRect2 = new Rect(rect.x + iconRect.width, labelRect.y + rect.height/3, rect.width - iconRect.width, rect.height/3);
            Rect labelRect3 = new Rect(rect.x + iconRect.width, labelRect2.y + rect.height / 3, rect.width - iconRect.width, rect.height/3);


            GUI.Box(rect, "", box);
            GUI.Box(iconRect, "", box);

            GUI.color = thing.DrawColor;
            Widgets.DrawTextureFitted(iconRect, thing.def.uiIcon, 1.0f);
            GUI.color = Color.white;

            GUI.Label(labelRect, " " + thing.Label.CapitalizeFirst(), fontleft);
            GUI.Label(labelRect2, " " + StatDefOf.MarketValue.label.CapitalizeFirst() + ": " + thing.GetStatValue(StatDefOf.MarketValue), fontleft);
            Widgets.FillableBar(labelRect3.ContractedBy(2f), (float)thing.HitPoints/thing.MaxHitPoints, Texture2D.linearGrayTexture);
            GUI.Label(labelRect3, " " + StatDefOf.MaxHitPoints.label.CapitalizeFirst() + " " + String.Format("{0} / {1}", thing.HitPoints, thing.MaxHitPoints), fontleft);
        }

        protected void ReinforceHistory(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listmain = new Listing_Standard();

            listmain.Begin(rect.ContractedBy(4f));

            for (int i = reinforcehistory.Count - 1; i >= 0; i--)
            {
                listmain.Label(reinforcehistory[i]);
            }

            listmain.End();
        }

        protected void StatInfo(Rect rect)
        {
            GUI.Box(rect, "", box);
            Rect statRect = rect.ContractedBy(4f);
            StatsReportUtility.DrawStatsReport(statRect, thing);
        }

        protected void ResourceInfo(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(rect.ContractedBy(4f));

            for (int i = 0; i < thingcountcache.Count; i++)
            {
                Rect row = listmain.GetRect(FontHeight);
                ResourceRow(row, thingcountcache[i].thingDef, thingcountcache[i].count, CostOf(i));
            }
            listmain.End();
        }

        protected void ResourceRow(Rect rect, ThingDef def, int stackcount, int cost)
        {
            Rect rowleft = rect.LeftHalf();
            Rect rowRight = rect.RightHalf();
            Color costColor = stackcount < cost ? Color.red : Color.green;
            Widgets.DefLabelWithIcon(rect, def);
            GUI.Label(rowRight, stackcount + ", " + "Required".Translate().CapitalizeFirst() + ": " + cost + " ", fontright);

            GUI.color = costColor;
            Widgets.DrawHighlight(rect);
            GUI.color = Color.white;
        }

        protected void FailureInfo(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(rect.ContractedBy(4f));
            int[] weights = comp.GetFailureWeights(out int totalweight);
            Rect temp = listmain.GetRect(FontHeight);
            GUI.Label(temp, Keyed.Failure, fontleft);
            GUI.Label(temp, (float)weights[0]/totalweight*100 + "%", fontright);

            temp = listmain.GetRect(FontHeight);
            GUI.Label(temp, Keyed.MinorDamage, fontleft);
            GUI.Label(temp, (float)weights[1] / totalweight*100 + "%", fontright);

            temp = listmain.GetRect(FontHeight);
            GUI.Label(temp, Keyed.MajorDamage, fontleft);
            GUI.Label(temp, (float)weights[2] / totalweight*100 + "%", fontright);

            temp = listmain.GetRect(FontHeight);
            GUI.Label(temp, Keyed.Explosion, fontleft);
            GUI.Label(temp, (float)weights[3] / totalweight*100 + "%", fontright);

            temp = listmain.GetRect(FontHeight);
            GUI.Label(temp, Keyed.Destruction, fontleft);
            GUI.Label(temp, (float)weights[4] / totalweight*100 + "%", fontright);
            listmain.End();
        }

        protected void SpecialOption(Rect rect, ReinforceSpecialOption option, int index)
        {
            string left = option.LabelLeft(comp);
            OptionRow(rect, option.Reinforce(comp), index, left, left, option.LabelRight(comp), !option.Enable(thing));
        }

        protected void StatOption(Rect rect, StatDef stat, int index)
        {
            int level = Rand.Range(1, 25);
            OptionRow(rect, 
                delegate
                {
                    return comp.ReinforceStat(stat, level); 
                }
                , index, stat.label + " +" + level * stat.GetOffsetPerLevel()*100 + "%", stat.label + " +" + comp.GetReinforcedCount(stat));
        }

        protected void CustomOption(Rect rect, ReinforceDef def, int index)
        {
            int level = Rand.Range(def.levelRange.min, def.levelRange.max);
            OptionRow(rect, def.Worker.Reinforce(comp, level), index, def.Worker.ResultString(level), def.Worker.LeftLabel(comp), def.Worker.RightLabel(comp));
        }

        
        protected void OptionRow(Rect rect, Func<bool> action, int index, string resultstring, string leftlabel, string rightlabel = null, bool disable = false)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            Rect labelRect = new Rect(rect.x + iconRect.width, rect.y, rect.width - iconRect.width, rect.height);

            GUI.Box(labelRect, "", box);
            GUI.Label(labelRect, "  " + leftlabel.CapitalizeFirst(), fontleft);
            if (rightlabel != null) GUI.Label(labelRect, rightlabel.CapitalizeFirst() + "  ", fontright);

            UpgradeButton(iconRect, action, index, resultstring, disable);
        }

        protected void UpgradeButton(Rect rect, Func<bool> action, int index, string resultstring, bool disable = false)
        {

            if (!disable && !onprogress)
            {
                if (Widgets.ButtonImage(rect, IconCache.Upgrade, Color.white, Color.gray))
                {
                    success = null;
                    Reinforce(action, index, resultstring);
                }
            }
            else
            {
                if (Widgets.ButtonImage(rect, IconCache.Upgrade, Color.gray, Color.gray))
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
                GUI.color = Color.white;
            }
        }

    }

}
