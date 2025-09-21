using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace InfiniteReinforce
{
    public abstract class CompProperties_Upgrade : CompProperties
    {
        public UpgradeDef upgradeDef;
        public ResearchProjectDef prerequisite;
        public List<ThingDefCountClass> Cost => upgradeDef.CostList;
    }

    public class CompProperties_UpgradeThing : CompProperties_Upgrade
    {
        public ThingDef FrameDef
        {
            get
            {
                var frameDef = DefDatabase<ThingDef>.GetNamedSilentFail(FrameDefName);
                if (frameDef != null) return frameDef;
                else GenerateFrameDef();
                return DefDatabase<ThingDef>.GetNamed(FrameDefName);
            }
        }



        public string FrameDefName => "Upgrade" + ThingDefGenerator_Buildings.BuildingFrameDefNamePrefix + upgradeDef.resultThingDef.defName;

        private void GenerateFrameDef()
        {
            if (upgradeDef != null)
            {
                if (upgradeDef.blueprintDef == null)
                {
                    MethodInfo methodbp = typeof(ThingDefGenerator_Buildings).GetMethod("NewBlueprintDef_Thing", BindingFlags.Static | BindingFlags.NonPublic);
                    ThingDef blueprintDef = methodbp.Invoke(null, new object[] { upgradeDef, false, null, false }) as ThingDef;
                    blueprintDef.size = upgradeDef.Size;
                }

                if (DefDatabase<ThingDef>.GetNamedSilentFail(FrameDefName) == null)
                {
                    MethodInfo method = typeof(ThingDefGenerator_Buildings).GetMethod("NewFrameDef_Thing", BindingFlags.Static | BindingFlags.NonPublic);
                    ThingDef frameDef = method.Invoke(null, new object[] { upgradeDef, false }) as ThingDef;
                    frameDef.defName = FrameDefName;
                    frameDef.size = upgradeDef.Size;
                    DefDatabase<ThingDef>.Add(frameDef);
                }
            }
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            GenerateFrameDef();
        }


    }
    public abstract class ThingComp_Upgrade : ThingComp
    {
        public CompProperties_Upgrade Props => (CompProperties_Upgrade)props;

        protected abstract void TryUpgrade();
        protected abstract string UpgradeLabel { get; }
        protected abstract string UpgradeDesc { get; }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Props.prerequisite == null || Props.prerequisite.IsFinished)
                yield return CreateExtractItemGizmo();
        }

        public abstract bool Disabled(out string disabledReason);

        protected Gizmo CreateExtractItemGizmo()
        {
            Gizmo gizmo = new Command_Action
            {
                icon = IconCache.Upgrade,
                defaultLabel = UpgradeLabel,
                defaultDesc = UpgradeDesc,
                Disabled = Disabled(out string disabledReason),
                disabledReason = disabledReason,
                action = TryUpgrade
            };
            return gizmo;
        }
    }

    public class ThingComp_Upgrade_Reinforcer : ThingComp_Upgrade
    {
        public new CompProperties_UpgradeThing Props => (CompProperties_UpgradeThing)props;

        Building_Reinforcer Parent => parent as Building_Reinforcer;

        protected override string UpgradeLabel => Keyed.Upgrade;

        protected override string UpgradeDesc => Keyed.UpgradeDesc + "\n" + Keyed.Materials + ": " + String.Join("\n", Props.Cost.Select(x => x.Summary).ToList());

        protected override void TryUpgrade()
        {
            Upgrade();
        }

        private void Upgrade()
        {
            Frame frame = ThingMaker.MakeThing(Props.FrameDef) as Frame;
            frame.SetFaction(parent.Faction);
            GenSpawn.Spawn(frame, parent.Position, parent.Map, parent.Rotation);

            frame.resourceContainer.TryAddOrTransfer(parent.MakeMinified(), true);
        }

        public override bool Disabled(out string disabledReason)
        {
            if (Parent.HoldingThing != null)
            {
                disabledReason = "IR.MustEmpty".Translate();
                return true;
            }
            disabledReason = null;
            return false;
        }


    }

}
