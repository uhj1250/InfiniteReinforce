using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;


namespace InfiniteReinforce
{
    public class ITab_Reinforce : ITab
    {
        private const float ROWHEIGHT = 20f;

        public struct ReinforceInfo
        {
            public ReinforceInfo(string label, int count, float factor)
            {
                this.label = label;
                this.count = count;
                this.factor = factor;
            }

            public string label;
            public int count;
            public float factor;
        }


        private static readonly Vector2 winsize = new Vector2(400f, 300f);
        private static List<ReinforceInfo> infolist = new List<ReinforceInfo>();
        private static Vector2 scrollPos;
        private static ThingComp_Reinforce compcache;
        private static int rowcount;

        protected static int RowCount
        {
            get => rowcount;
            set => rowcount = Math.Max(rowcount, value);
        }

        public static List<ReinforceInfo> InfoList => infolist;

        public ITab_Reinforce()
        {
            size = winsize;
            labelKey = "IR.Reinforcement";
            tutorTag = "Reinforce";
        }

        private ThingComp_Reinforce SelectedComp
        {
            get
            {
                Thing thing = Find.Selector.SingleSelectedThing;
                if (thing is MinifiedThing minifiedThing) thing = minifiedThing.InnerThing;
                return thing?.TryGetComp<ThingComp_Reinforce>();
            }
        }

        private static ThingComp_Reinforce Comp => compcache;

        public override void OnOpen()
        {
            base.OnOpen();
            rowcount = 0;
            Update();
        }

        protected override void CloseTab()
        {
            base.CloseTab();
            infolist.Clear();
        }

        private void Update()
        {
            ThingComp_Reinforce comp = SelectedComp;
            compcache = comp;
            GetReinforceInfo(comp.parent, out infolist);
            rowcount = infolist.Count + 2;

        }

        public static bool GetReinforceInfo(ThingWithComps thing, out List<ReinforceInfo> infolist)
        {
            ThingComp_Reinforce comp = thing.TryGetComp<ThingComp_Reinforce>();
            if (comp != null)
            {
                List<StatDef> reinforcedStats = comp.StatReinforceList;
                List<ReinforceDef> reinforceDefs = comp.CustomReinforceList;

                infolist = new List<ReinforceInfo>();
                foreach (ReinforceDef def in reinforceDefs)
                {
                    infolist.Add(new ReinforceInfo(def.label, comp.GetReinforcedCount(def), comp.GetCustomFactor(def)));
                }

                foreach (StatDef stat in reinforcedStats)
                {
                    infolist.Add(new ReinforceInfo(stat.LabelCap, comp.GetReinforcedCount(stat), comp.GetStatFactor(stat)));
                }

                return true;
            }
            infolist = null;
            return false;
        }


        public override bool IsVisible => SelectedComp != null && SelectedComp.ReinforcedCount > 0;

        protected override void FillTab()
        {
            int currowcount = 0;
            if (compcache != SelectedComp) Update();
            Rect rect = new Rect(0f, 0f, winsize.x, winsize.y);

            Rect viewRect = new Rect(10 , 10, rect.width - 20f, ROWHEIGHT * RowCount + 40f).ContractedBy(20f);
            Widgets.BeginScrollView(rect.ContractedBy(20f), ref scrollPos, viewRect);
            Rect row = new Rect(viewRect.x,viewRect.y,viewRect.width, ROWHEIGHT);

            DrawThingLabel(ref row, Comp.parent);
            DrawInfoList(ref row, InfoList);

            currowcount += InfoList.Count() + 2;

            Pawn pawn = Comp.parent as Pawn;
            if (pawn != null)
            {
                if (pawn.equipment?.Primary != null)
                {
                    ThingWithComps thing = pawn.equipment.Primary;
                    if (GetReinforceInfo(thing,out List<ReinforceInfo> weaponinfo))
                    {
                        row.y += ROWHEIGHT;
                        DrawThingLabel(ref row, thing);
                        DrawInfoList(ref row, weaponinfo);
                        currowcount += weaponinfo.Count + 2;
                    }
                }

                CompTurretGun turret = pawn.TryGetComp<CompTurretGun>();
                if (turret != null)
                {
                    ThingWithComps thing = turret.gun as ThingWithComps;
                    if (GetReinforceInfo(thing, out List<ReinforceInfo> turretinfo) )
                    {
                        row.y += ROWHEIGHT;
                        DrawThingLabel(ref row, thing);
                        DrawInfoList(ref row, turretinfo);
                        currowcount += turretinfo.Count + 2;
                    }
                }
            }

            


            Widgets.EndScrollView();
            RowCount = currowcount;
            Log.Message(RowCount);
        }

        private static void DrawThingLabel(ref Rect row,Thing thing)
        {
            if (thing is Pawn) Widgets.Label(row, String.Format("{0} +{1}", thing.LabelCap, Comp.ReinforcedCount));
            else Widgets.Label(row, String.Format("{0}", thing.LabelCap));
            row.y += ROWHEIGHT;
            Widgets.DrawLineHorizontal(row.x, row.y, row.width);
        }

        private static void DrawInfoList(ref Rect row, List<ReinforceInfo> infoList)
        {
            for (int i = 0; i < infoList.Count; i++)
            {
                ReinforceInfo info = infoList[i];

                if (i % 2 == 0) GUI.DrawTexture(row, Texture2D.grayTexture, ScaleMode.StretchToFill);
                Widgets.Label(row, String.Format("{0}:", info.label));
                Widgets.Label(new Rect(row.x + row.width * 0.66f, row.y, row.width * 0.33f, ROWHEIGHT), String.Format("{0,5} | {1,8:P2}", "+" + info.count, info.factor));
                row.y += ROWHEIGHT;

            }
        }



    }
}
