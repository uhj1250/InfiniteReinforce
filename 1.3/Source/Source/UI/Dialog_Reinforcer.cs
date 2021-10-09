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
        protected List<ThingDefCountClass> costlist;
        protected List<ThingDefCountClass> thingcountcache = new List<ThingDefCountClass>();
        protected List<string> reinforcehistory = new List<string>();
        protected string reinforcehistorycache;

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
            costlist = thing.def.CostList;
            if (costlist == null) costlist = new List<ThingDefCountClass>();
            if (thing.Stuff != null)
            {
                ThingDefCountClass stuff = costlist.FirstOrDefault(x => x.thingDef == thing.Stuff);
                if (stuff != null)
                {
                    stuff.count += thing.def.costStuffCount;
                }
                else
                {
                    costlist.Add(new ThingDefCountClass(thing.Stuff, thing.def.CostStuffCount));
                }
            }
            if (costlist.NullOrEmpty())
            {
                costlist.Add(new ThingDefCountClass(thing.def, 1));
            }

        }

        public void UpdateThingList()
        {
            resourcethings = TradeUtility.AllLaunchableThingsForTrade(building.Map).Where(x => costlist.Exists(y => y.thingDef == x.def));
            thingcountcache.Clear();
            if (!costlist.NullOrEmpty())
            {
                foreach (ThingDefCountClass cost in costlist)
                {

                    thingcountcache.Add(new ThingDefCountClass(cost.thingDef, 0));
                }
            }

            resourcethings.CountThingInCollection(ref thingcountcache);
        }

        public int CostOf(int index)
        {
            return (int)(costlist[index].count * comp.CostMultiplier);
        }

        public void ResetProgress()
        {
            progress = 0f;
            onprogress = false;
            StatsReportUtility.Notify_QuickSearchChanged();
        }

        public void RemoveIngredients()
        {
            for(int i=0; i<costlist.Count; i++)
            {
                resourcethings.EliminateThingOfType(costlist[i].thingDef, CostOf(i));
            }
            UpdateThingList();
        }

        public bool CheckIngredients()
        {
            for(int i=0; i<costlist.Count; i++)
            {
                int count = thingcountcache.FirstOrDefault(x => x.thingDef == costlist[i].thingDef)?.count ?? 0;
                if (count < CostOf(i)) return false;
            }
            return true;
        }

        public void Reinforce(Func<bool> action, int index, string resultstring)
        {
            if (CheckIngredients())
            {
                onprogress = true;
                selectedindex = index;
                reinforceaction = action;
                reinforcehistorycache = resultstring;
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
                RemoveIngredients();
                int[] weights = comp.GetFailureWeights(out int totalweight);
                success = !comp.RollFailure(out float rolled, totalweight, building.MaxHitPoints / building.HitPoints);
                if (success ?? false)
                {
                    reinforceaction();
                    reinforcehistory.Add(reinforcehistorycache);
                    ReinforceDefOf.Reinforce_Success.PlayOneShotOnCamera();
                    return true;
                }
                else
                {
                    ReinforceFailureResult effect = FailureEffect(totalweight, weights);
                    reinforcehistory.Add(Keyed.Failed + " - " + effect.Translate());
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
            GenExplosion.DoExplosion(building.Position, building.Map, Rand.Range(1, 3), DamageDefOf.Bomb ,building, Rand.Range(50, 120));
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

            GUI.Label(listmain.GetRect(FontHeight), Keyed.Materials, fontleft);
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

            float chance = comp.GetFailureChance((float)building.MaxHitPoints / building.HitPoints);
            GUI.color = Color.Lerp(Color.green,Color.red, chance/50f);
            GUI.Box(failureRect, "");
            GUI.Label(failureRect, " " + Keyed.FailureChance, fontleft);
            GUI.Label(failureRect, String.Format(" {0:0.00}%", chance), fontright);
            GUI.color = Color.white;

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

            GUI.Label(labelRect, " " + thing.Label, fontleft);
            GUI.Label(labelRect2, " " + StatDefOf.MarketValue.label + ": " + thing.GetStatValue(StatDefOf.MarketValue), fontleft);
            Widgets.FillableBar(labelRect3.ContractedBy(2f), (float)thing.HitPoints/thing.MaxHitPoints, Texture2D.linearGrayTexture);
            GUI.Label(labelRect3, " " + StatDefOf.MaxHitPoints.label + " " + String.Format("{0} / {1}", thing.HitPoints, thing.MaxHitPoints), fontleft);
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
            GUI.Label(rowleft, stackcount + "", fontright);
            GUI.Label(rowRight, "Required".Translate() + ": " + cost + " ", fontright);

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

        protected void OptionRow(Rect rect, Func<bool> action, int index, string resultstring, string leftlabel, string rightlabel = null)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            Rect labelRect = new Rect(rect.x + iconRect.width, rect.y, rect.width - iconRect.width, rect.height);

            GUI.Box(labelRect, "", box);
            GUI.Label(labelRect, "  " + leftlabel.CapitalizeFirst(), fontleft);
            if (rightlabel != null) GUI.Label(labelRect, rightlabel.CapitalizeFirst() + "  ", fontright);

            UpgradeButton(iconRect, action, index, resultstring);
        }

        protected void UpgradeButton(Rect rect, Func<bool> action, int index, string resultstring)
        {

            if (!onprogress)
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
