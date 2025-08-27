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
        public bool? Succession => Instance.Succession;

        protected Building_Reinforcer building;
        protected Building_Reinforcer.ReinforceInstance Instance => building.Instance;
        protected float Progress => building.Progress;
        //protected int? selectedindex = null;
        //protected Func<bool> reinforceaction;
        protected List<StatDef> statlist = new List<StatDef>();
        protected List<ReinforceDef> customlist = new List<ReinforceDef>();
        protected IEnumerable<Thing> resourcethings;
        protected List<ThingDefCountClass>[] costlist = new List<ThingDefCountClass>[CostModeCount];
        protected List<ThingDefCountClass> thingcountcache = new List<ThingDefCountClass>();
        protected CostMode costMode;

        private Vector2 optionscroll;


        protected ThingWithComps thing => building?.TargetThing;

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
        private static bool blockstatinfo = false;

        public Dialog_Reinforcer(Building_Reinforcer building)
        {
            ChangeBuilding(building);
        }

        private void ItemDestroyed(object sender, EventArgs e)
        {
            if (sender is Building_Reinforcer)
            {
                var b = sender as Building_Reinforcer;
                if (b == building)
                {
                    //Reset Dialog
                    ChangeBuilding(building);
                }
            }
        }

        private void ReinforceCompleted(object sender, EventArgs e)
        {
            UpdateThingList();
        }

        public void ChangeBuilding(Building_Reinforcer building)
        {
            if (this.building != null)
            {
                this.building.ItemDestroyed -= ItemDestroyed;
                this.building.ReinforceCompleted -= ReinforceCompleted;
            }
            this.building = building;
            building.ItemDestroyed += ItemDestroyed;
            building.ReinforceCompleted += ReinforceCompleted;
            compcache = null;
            BuildStatList();
            BuildCustomList();
            if (thing != null)
            {
                BuildCostList();
                costMode = InitialCostMode();
                UpdateThingList();
            }
            StatsReportUtility.Notify_QuickSearchChanged();
        }

        public bool BuildStatList()
        {
            return thing.GetStatList(out statlist);
        }

        public void BuildCostList()
        {

            //Build same thing
            costlist[(int)CostMode.SameThing] = Instance.BuildCostList(CostMode.SameThing);

            //Build material cost
            costlist[(int)CostMode.Material] = Instance.BuildCostList(CostMode.Material);

            //Build fuel cost
            costlist[(int)CostMode.Fuel] = Instance.BuildCostList(CostMode.Fuel);
        }

        public bool BuildCustomList()
        {
            return thing.GetAppliableCustom(out customlist);
        }

        public void UpdateThingList()
        {
            thingcountcache.Clear();
            if (costMode == CostMode.Fuel && building.FuelComp != null)
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

                if (building.Map.GetThingsNearBeacon(out List<Thing> things))
                {
                    resourcethings = things.Where(x => costlist[(int)costMode].Exists(y => y.thingDef == x.def));
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
                else
                {
                    foreach (ThingDefCountClass cost in costlist[(int)costMode])
                    {
                        thingcountcache.Add(new ThingDefCountClass(cost.thingDef, 0));
                    }
                }
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
            return Instance.CostOf(costlist[(int)costMode], index, costMode);
        }


        public bool CheckIngredients()
        {
            if (DebugSettings.godMode) return true;
            for (int i=0; i<costlist[(int)costMode].Count; i++)
            {
                int count = thingcountcache.FirstOrDefault(x => x.thingDef == costlist[(int)costMode][i].thingDef)?.count ?? 0;
                if (count < CostOf(i)) return false;
            }
            return true;
        }

        public void Reinforce(Func<bool> action, int index)
        {
            if (CheckIngredients())
            {
                action();
                UpdateThingList();
            }
            else 
            {
                Messages.Message(Keyed.NotEnough, MessageTypeDefOf.RejectInput, false);
            }
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

            Widgets.FillableBar(progressRect, Progress);
            
            if (building.Instance.QueueCount > 0)
            {
                GUI.Label(progressRect, String.Format("{0:0.00 %}", Progress), fontcenter);
            }
            else if (Succession == true)
            {
                progressRect.DrawBinkTexture(Color.cyan, Color.black);
                GUI.Label(progressRect, Keyed.Success, fontcenter);
            }
            else if (Succession == false)
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
                        costMode = (CostMode)i;
                        BuildCostList();
                        UpdateThingList();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                    if (i == (int)costMode) Widgets.DrawHighlight(costModeRect);
                }


                costModeRect.x -= FontHeight;
            }



            ResourceInfo(listmain.GetRect(FontHeight * 8 + 8f));

            GUI.Label(listmain.GetRect(FontHeight), Keyed.Queue, fontleft);
            QueueInfo(listmain.GetRect(FontHeight * 5 + 8f));

            //GUI.Label(listmain.GetRect(FontHeight), Keyed.FailureOutcome, fontleft);
            //FailureInfo(listmain.GetRect(FontHeight * 5 + 8f));
            
            listmain.End();

        }
        
        protected void RightSection(Rect rect)
        {
            Listing_Standard listmain = new Listing_Standard();
            Rect outRect = new Rect(rect.x, rect.y + FontHeight, rect.width, rect.height - FontHeight);
            Rect inRect = new Rect(rect.x, rect.y + FontHeight, rect.width - 20f, FontHeight * (statlist.Count + ReinforceUtility.ReinforceDefs.Count(x => x.Worker.Appliable(thing)) + (building?.FuelComp?.Props?.SpecialOptions?.Count ?? 0)));
            Rect failureRect = new Rect(rect.x, rect.y, rect.width, FontHeight);

            float chance = (costMode == CostMode.Fuel && building.AlwaysSuccess) ? 0 : comp.GetFailureChance((float)building.MaxHitPoints / (building.HitPoints*building.Instance.ProgressMultiplier));
            GUI.color = Color.Lerp(Color.green, Color.red, chance / 50f);
            GUI.Box(failureRect, "");
            GUI.Label(failureRect, String.Format(Keyed.FailureChance + " {0:0.00}%", chance), fontright);
            
            GUI.color = Color.white;
            TooltipHandler.TipRegion(failureRect, FailureInfo());

            Rect btnRect = new Rect(failureRect.x, failureRect.y, FontHeight, FontHeight);
            if (Widgets.ButtonImage(btnRect, IconCache.Plus, true))
            {
                Instance.ProgressMultiplier = Mathf.Clamp(Instance.ProgressMultiplier + 0.25f, 1.0f, 10.0f);
            }
            btnRect.x += FontHeight;
            if (Widgets.ButtonImage(btnRect, IconCache.Minus, true))
            {
                Instance.ProgressMultiplier = Mathf.Clamp(Instance.ProgressMultiplier - 0.25f, 1.0f, 10.0f);
            }
            failureRect.Set(rect.x + FontHeight*2, rect.y, rect.width - FontHeight*2, FontHeight);
            GUI.Label(failureRect, String.Format(" " + Keyed.ProcTime + " x{0:0.00}", Instance.ProgressMultiplier), fontleft);



            Widgets.BeginScrollView(outRect, ref optionscroll, inRect);
            listmain.Begin(inRect);


            if (costMode == CostMode.Fuel && building.FuelComp != null)
            {
                if (!building.FuelComp.Props.SpecialOptions.NullOrEmpty())
                {
                    List<IReinforceSpecialOption> options = building.FuelComp.Props.SpecialOptions;
                    for (int i=0; i<options.Count; i++)
                    {
                        if (options[i].Appliable(thing))
                            SpecialOption(listmain.GetRect(FontHeight), options[i],i);
                    }
                }
            }

            for (int i = 0; i < customlist.Count; i++)
            {
                ReinforceDef def = customlist[i];
                CustomOption(listmain.GetRect(FontHeight), def, i);
            }

            for (int i=0; i<statlist.Count; i++)
            {
                StatOption(listmain.GetRect(FontHeight), statlist[i], i);
            }
            

            listmain.End();
            Widgets.EndScrollView();
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
            Rect switchRect = new Rect(rect.x,rect.y,rect.height/4, rect.height/4);
            
            GUI.Box(rect, "", box);
            GUI.Box(iconRect, "", box);

            GUI.color = thing.DrawColor;
            Widgets.DrawTextureFitted(iconRect, thing.def.uiIcon, 1.0f);
            GUI.color = Color.white;

            GUI.Label(labelRect, " " + thing.Label.CapitalizeFirst(), fontleft);
            GUI.Label(labelRect2, " " + StatDefOf.MarketValue.label.CapitalizeFirst() + ": " + thing.GetStatValue(StatDefOf.MarketValue), fontleft);
            if (thing is Pawn)
            {
                Pawn pawn = thing as Pawn;
                Widgets.FillableBar(labelRect3.ContractedBy(2f), pawn.health.summaryHealth.SummaryHealthPercent, Texture2D.linearGrayTexture);
                GUI.Label(labelRect3, " " + "HitPoints".Translate(pawn.health.summaryHealth.SummaryHealthPercent * 100f + "%").CapitalizeFirst(), fontleft);
            }
            else
            {
                Widgets.FillableBar(labelRect3.ContractedBy(2f), (float)thing.HitPoints / thing.MaxHitPoints, Texture2D.linearGrayTexture);
                GUI.Label(labelRect3, " " + "HitPoints".Translate("").CapitalizeFirst() + String.Format(" {0} / {1}", thing.HitPoints, thing.MaxHitPoints), fontleft);
            }

            if (building.HoldingThing is Pawn && Widgets.ButtonImage(switchRect, ContentFinder<Texture2D>.Get("UI/ReinforceSwitch", false)))
            {
                SwitchTarget();
            }
        }

        private void SwitchTarget()
        {
            if (building.OnProgress) SoundDefOf.ClickReject.PlayOneShotOnCamera();
            else
            {
                switch (building.Target)
                {
                    case Building_Reinforcer.ReinforceTarget.Equipment:
                    default:
                        building.Target = Building_Reinforcer.ReinforceTarget.Mechanoid;
                        break;
                    case Building_Reinforcer.ReinforceTarget.Mechanoid:
                        building.Target = Building_Reinforcer.ReinforceTarget.Equipment;
                        break;
                }
                ChangeBuilding(building);
            }
        }

        protected void ReinforceHistory(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listmain = new Listing_Standard();
            
            listmain.Begin(rect.ContractedBy(4f));
            
            for (int i = Instance.History.Count - 1; i >= 0; i--)
            {
                listmain.Label(Instance.History[i]);
            }

            listmain.End();
        }

        protected void StatInfo(Rect rect)
        {
            GUI.Box(rect, "", box);
            Rect statRect = rect.ContractedBy(4f);
            if (!blockstatinfo)
                try
                {
                    StatsReportUtility.DrawStatsReport(statRect, thing);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to load status informations for " + thing.Label + ": " + e.Message);
                    blockstatinfo = true;
                }
            else
            {
                Widgets.Label(statRect, "Failed to load status informations");
            }
        }

        protected void ResourceInfo(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(rect.ContractedBy(4f));
            if (thingcountcache.Exists(x => x.thingDef.IsWeapon || x.thingDef.IsApparel))
            {
                QualityRange temp = IRConfig.MaterialQualityRange;
                Widgets.QualityRange(listmain.GetRect(FontHeight), 1, ref IRConfig.MaterialQualityRange);
                FloatRange temp2 = IRConfig.DurabilityRange;
                Widgets.FloatRange(listmain.GetRect(FontHeight), 2, ref IRConfig.DurabilityRange, 0f, 1f, "HitPoints",ToStringStyle.PercentZero);
                if (temp != IRConfig.MaterialQualityRange || temp2 != IRConfig.DurabilityRange)
                {
                    UpdateThingList();
                }
            }
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

        protected string FailureInfo()
        {
            int[] weights = comp.GetFailureWeights(out int totalweight);
            return String.Format(Keyed.Failure + (float)weights[0] / totalweight * 100 + "%" + "\n" +
                Keyed.MinorDamage + (float)weights[1] / totalweight * 100 + "%" + "\n" +
                Keyed.MajorDamage + (float)weights[2] / totalweight * 100 + "%" + "\n" +
                Keyed.Explosion + (float)weights[3] / totalweight * 100 + "%" + "\n" +
                Keyed.Destruction + (float)weights[4] / totalweight * 100 + "%" + "");
            
            //Widgets.DrawMenuSection(rect);
            //Listing_Standard listmain = new Listing_Standard();
            //listmain.Begin(rect.ContractedBy(4f));
            //Rect temp = listmain.GetRect(FontHeight);
            //GUI.Label(temp, Keyed.Failure, fontleft);
            //GUI.Label(temp, (float)weights[0]/totalweight*100 + "%", fontright);
            //
            //temp = listmain.GetRect(FontHeight);
            //GUI.Label(temp, Keyed.MinorDamage, fontleft);
            //GUI.Label(temp, (float)weights[1] / totalweight*100 + "%", fontright);
            //
            //temp = listmain.GetRect(FontHeight);
            //GUI.Label(temp, Keyed.MajorDamage, fontleft);
            //GUI.Label(temp, (float)weights[2] / totalweight*100 + "%", fontright);
            //
            //temp = listmain.GetRect(FontHeight);
            //GUI.Label(temp, Keyed.Explosion, fontleft);
            //GUI.Label(temp, (float)weights[3] / totalweight*100 + "%", fontright);
            //
            //temp = listmain.GetRect(FontHeight);
            //GUI.Label(temp, Keyed.Destruction, fontleft);
            //GUI.Label(temp, (float)weights[4] / totalweight*100 + "%", fontright);
            //listmain.End();
        }

        protected void QueueInfo(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(rect.ContractedBy(4f));
            List<string> queue = building.Instance.QueuedReinforcements;
            if (!queue.NullOrEmpty()) for(int i=0; i< queue.Count; i++)
            {
                Rect temp = listmain.GetRect(FontHeight);
                GUI.Label(temp, queue[i], fontleft);
            }


            listmain.End();
        }

        protected void SpecialOption(Rect rect, IReinforceSpecialOption option, int index)
        {
            string left = option.LabelLeft(comp);
            bool alwayssuccess = costMode == CostMode.Fuel && building.AlwaysSuccess;
            CostMode costmode = costMode;
            OptionRow(rect, delegate { Reinforce(delegate { return Instance.TryReinforce(option, costmode, alwayssuccess); }, index); }, left, option.LabelRight(comp), !option.Enable(thing));
        }

        protected void StatOption(Rect rect, StatDef stat, int index)
        {
            bool alwayssuccess = costMode == CostMode.Fuel && building.AlwaysSuccess;
            CostMode costmode = costMode;
            OptionRow(rect, 
                delegate
                {
                    Reinforce(delegate { return Instance.TryReinforce(stat,costmode, alwayssuccess); }, index);
                }
                , stat.label + " +" + comp.GetReinforcedCount(stat), null , comp.NotUpgradable(stat));
        }

        protected void CustomOption(Rect rect, ReinforceDef def, int index)
        {
            bool alwayssuccess = costMode == CostMode.Fuel && building.AlwaysSuccess;
            CostMode costmode = costMode;
            int level = Rand.Range(def.levelRange.min, def.levelRange.max);
            OptionRow(rect, delegate { Reinforce(delegate { return Instance.TryReinforce(def,costmode, alwayssuccess); }, index); }, def.Worker.LeftLabel(comp), def.Worker.RightLabel(comp));
        }

        
        protected void OptionRow(Rect rect, Action action, string leftlabel, string rightlabel = null, bool disable = false)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            Rect labelRect = new Rect(rect.x + iconRect.width, rect.y, rect.width - iconRect.width, rect.height);

            GUI.Box(labelRect, "", box);
            GUI.Label(labelRect, "  " + leftlabel.CapitalizeFirst(), fontleft);
            if (rightlabel != null) GUI.Label(labelRect, rightlabel.CapitalizeFirst() + "  ", fontright);

            UpgradeButton(iconRect, action, disable);
        }

        protected void UpgradeButton(Rect rect, Action action, bool disable = false)
        {   

            if (!disable && building.Instance.QueueCount < 5)
            {
                if (Widgets.ButtonImage(rect, IconCache.Upgrade, Color.white, Color.gray))
                {
                    action();
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
