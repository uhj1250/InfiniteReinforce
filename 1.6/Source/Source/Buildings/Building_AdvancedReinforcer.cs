using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;


namespace InfiniteReinforce
{
    public class Building_AdvancedReinforcer : Building_Reinforcer
    {
        private int reliableadj;

        protected override float FuelConsumtionMultiplier => Mathf.Min(base.FuelConsumtionMultiplier * Mathf.Pow((1 + Reliable),2) * (1 + TargetThing.GetReinforceComp()?.ReinforcedCount ?? 1), 10000f);

        public float Reliableadj { get => reliableadj * 0.1f; }
        protected int Reliable => reliableadj;

        protected override float GetFailureMultiplier(ReinforceInstance.Reinforcement reinforcement)
        {
            if (FuelComp.CanConsumeOnce(FuelConsumtionMultiplier)) return base.GetFailureMultiplier(reinforcement)*0.8f;
            return base.GetFailureMultiplier(reinforcement);
        }

        protected override int RollReinforceLevel(int min, int max)
        {
            if (FuelComp.CanConsumeOnce(FuelConsumtionMultiplier)) min = (int)Mathf.Lerp(min, max, Reliableadj);
            return base.RollReinforceLevel(min, max);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref reliableadj, "reliableadj", 0, true);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = base.GetGizmos().ToList();
            gizmos.Add(new Gizmo_IntSlider(SetReliableAdj, reliableadj, 0, 9, Keyed.ReliableAdjConfig, Keyed.ReliableAdjConfigDesc, ReliableAdjInspectStringTop, ReliableAdjInspectStringBottom, ReliableAdjLabeInBar));
            return gizmos;
        }

        protected virtual void SetReliableAdj(int adj)
        {
            reliableadj = adj;
        }
        
        protected virtual string ReliableAdjInspectStringTop()
        {
            return String.Format(Keyed.CoreConsumtion + ": {0}", FuelConsumtionMultiplier);
        }
        protected virtual string ReliableAdjInspectStringBottom()
        {
            return String.Format(Keyed.AdditionalCoreConsumtion + ": {0}", FuelConsumtionMultiplier - base.FuelConsumtionMultiplier * (1 + TargetThing.GetReinforceComp()?.ReinforcedCount ?? 1));
        }
        protected virtual string ReliableAdjLabeInBar()
        {
            return String.Format("{0,0:P}", Reliableadj);
        }

    }

    public class Gizmo_IntSlider : Gizmo
    {
        public const float Width = 212f;
        public const float AdditionalHeight = 8f;

        private Action<int> action;
        private Func<string> inspectstringtop;
        private Func<string> inspectstringbottom;
        private Func<string> labelinbar;
        private int min;
        private int max;
        private int curval;
        private string label;
        private string description;

        protected static bool clicked = false;

        public string Label { get => label; set => label = value; }
        public string Description { get => description; set => description = value; }
        public int Min { get => min; set => min = value; }
        public int Max { get => max; set => max = value; }

        public Gizmo_IntSlider(Action<int> changevalue, int value ,int min, int max, string label, string description, Func<string> inspectstringtop, Func<string> inspectstringbottom, Func<string> labelinbar)
        {
            this.Order = -100;
            action = changevalue;
            this.Min = min;
            this.Max = max;
            curval = value;
            this.label = label;
            this.description = description;
            this.inspectstringtop = inspectstringtop;
            this.inspectstringbottom = inspectstringbottom;
            this.labelinbar = labelinbar;

        }

        public override bool GroupsWith(Gizmo other)
        {
            return other is Gizmo_IntSlider;
        }

        public override void MergeWith(Gizmo other)
        {
            if (other is Gizmo_IntSlider)
            {
                Gizmo_IntSlider o = other as Gizmo_IntSlider;
            }
        }

        public override float GetWidth(float maxWidth)
        {
            return Width;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect outRect = new Rect(topLeft.x, topLeft.y - AdditionalHeight, GetWidth(maxWidth), Height + AdditionalHeight);
            Rect inRect = outRect.ContractedBy(3f);
            Rect row1 = new Rect(inRect.x, inRect.y, inRect.width, inRect.height / 4);
            Rect row2 = new Rect(inRect.x, row1.y + row1.height, inRect.width, inRect.height / 4);
            Rect row3 = new Rect(inRect.x, row2.y + row2.height, inRect.width, inRect.height / 4);
            Rect row4 = new Rect(inRect.x, row3.y + row3.height, inRect.width, inRect.height / 4);

            Widgets.DrawWindowBackground(outRect);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(row1, Label);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(row2, inspectstringtop());
            Widgets.Label(row3, inspectstringbottom());
            Text.Font = GameFont.Small;
            int originval = (int)curval;
            var value = DrawAdjustableBar(row4, curval, Min, Max, labelinbar(), description);
            curval = (int)value;

            if (curval != originval)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                action((int)curval);
            }

            return new GizmoResult(GizmoState.Clear);
        }

        protected float DrawAdjustableBar(Rect rect, float val, float min, float max, string label = null, string tooltip = null)
        {
            Rect nutritionRect = rect.ContractedBy(2f);
            Rect barRect = rect.ContractedBy(4f);


            if (!clicked && Mouse.IsOver(rect))
            {
                if (Input.GetMouseButton(0))
                {
                    clicked = true;
                }
                else if (Event.current.type == EventType.ScrollWheel)
                {
                    float delta = Input.mouseScrollDelta.y;
                    float adjust = 1f;
                    if (Input.GetKey(KeyCode.LeftShift)) adjust = 10f;
                    else if (Input.GetKey(KeyCode.LeftControl)) adjust = 5f;
                    if (delta > 0) val = Mathf.Clamp(val + adjust, min, max);
                    else if (delta < 0) val = Mathf.Clamp(val - adjust, min, max);
                    Event.current.Use();

                }
            }
            
            if (clicked)
            {
                float posnormalized = Mathf.Clamp01(Input.mousePosition.x.Normalize(barRect.x, barRect.xMax));
                val = Mathf.Floor(posnormalized.DeNormalize(min, max) * 100f) / 100f;
                if (!Input.GetMouseButton(0))
                {

                    clicked = false;
                }
            }

            val = Mathf.RoundToInt(val);
            Widgets.FillableBar(barRect, val.Normalize(min, max));

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.DrawHighlightIfMouseover(rect);
            if (tooltip != null) TooltipHandler.TipRegion(rect, tooltip);

            return val;
        }
    }
}
