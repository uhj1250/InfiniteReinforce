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
                MethodInfo methodbp = typeof(ThingDefGenerator_Buildings).GetMethod("NewBlueprintDef_Thing", BindingFlags.Static | BindingFlags.NonPublic);
                ThingDef blueprintDef = methodbp.Invoke(null, new object[] { upgradeDef, false, null, false}) as ThingDef;
                blueprintDef.size = upgradeDef.Size;

                MethodInfo method = typeof(ThingDefGenerator_Buildings).GetMethod("NewFrameDef_Thing", BindingFlags.Static | BindingFlags.NonPublic);
                ThingDef frameDef = method.Invoke(null, new object[] { upgradeDef, false }) as ThingDef;
                frameDef.defName = FrameDefName;
                frameDef.size = upgradeDef.Size;
                DefDatabase<ThingDef>.Add(frameDef);
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
        protected abstract bool Disable { get; }
        protected abstract string DisabledReason { get; }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return CreateExtractItemGizmo();
        }


        protected Gizmo CreateExtractItemGizmo()
        {
            Gizmo gizmo = new Command_Action
            {
                icon = IconCache.Upgrade,
                defaultLabel = UpgradeLabel,
                defaultDesc = UpgradeDesc,
                Disabled = Disable,
                disabledReason = DisabledReason,
                action = TryUpgrade
            };
            return gizmo;
        }
    }

    public class ThingComp_Upgrade_Reinforcer : ThingComp_Upgrade
    {
        public new CompProperties_UpgradeThing Props => (CompProperties_UpgradeThing)props;

        Building_Reinforcer Parent => parent as Building_Reinforcer;

        protected override bool Disable { get => Parent.HoldingThing != null; }

        protected override string DisabledReason => "The reinforcer must be empty";

        protected override string UpgradeLabel => Keyed.Upgrade;

        protected override string UpgradeDesc => Keyed.UpgradeDesc + "\n" + Keyed.Materials + ": " + Props.Cost[0].thingDef.label + " x" + Props.Cost[0].count;

        protected override void TryUpgrade()
        {
            Upgrade();


            //if (parent.Map.GetThingsNearBeacon(out List<Thing> things) && things.CountThingInCollection(Props.Cost[0].thingDef) >= Props.Cost[0].count)
            //{
            //    List<Thing> materials = things.GetThingsOfType(Props.Cost[0].thingDef, Props.Cost[0].count);
            //    //foreach (var thing in materials) { thing.Destroy(); }
            //    Upgrade(materials);
            //}
            //else
            //{
            //    SoundDefOf.ClickReject.PlayOneShotOnCamera();
            //    Messages.Message(Keyed.NotEnough + ": " + Props.Cost[0].thingDef.label, parent, MessageTypeDefOf.RejectInput);
            //}

        }

        private void Upgrade()
        {
            Frame frame = ThingMaker.MakeThing(Props.FrameDef) as Frame;
            frame.SetFaction(parent.Faction);
            GenSpawn.Spawn(frame, parent.Position, parent.Map, parent.Rotation);

            frame.resourceContainer.TryAddOrTransfer(parent.MakeMinified(), true);
            //Thing thing = ThingMaker.MakeThing(Props.thingDef);
            //if (thing.HasComp<CompRefuelable>()) thing.TryGetComp<CompRefuelable>().Refuel(Props.cost[0].count);
            //thing.SetFaction(parent.Faction);
            //GenSpawn.Spawn(thing, parent.Position, parent.Map, parent.Rotation);
            //ReinforceDefOf.Reinforce_Success.PlayOneShot(thing);
        }

        

    }

}
