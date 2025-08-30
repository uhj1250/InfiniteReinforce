using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace InfiniteReinforce
{
    public class CompProperties_Upgrade : CompProperties
    {
        public List<ThingDefCountClass> cost;
    }

    public class CompProperties_UpgradeThing : CompProperties_Upgrade
    {
        public ThingDef thingDef;
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

        protected override string UpgradeDesc => Keyed.UpgradeDesc + "\n" + Keyed.Materials + ": " + Props.cost[0].thingDef.label + " x" + Props.cost[0].count;

        protected override void TryUpgrade()
        {
            if (parent.Map.GetThingsNearBeacon(out List<Thing> things) && things.CountThingInCollection(Props.cost[0].thingDef) >= Props.cost[0].count)
            {
                var materials = things.GetThingsOfType(Props.cost[0].thingDef, Props.cost[0].count);
                foreach (var thing in materials) { thing.Destroy(); }
                Upgrade();
            }
            else
            {
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
                Messages.Message(Keyed.NotEnough + ": " + Props.cost[0].thingDef.label, parent, MessageTypeDefOf.RejectInput);
            }

        }

        private void Upgrade()
        {
            Thing thing = ThingMaker.MakeThing(Props.thingDef);
            if (thing.HasComp<CompRefuelable>()) thing.TryGetComp<CompRefuelable>().Refuel(Props.cost[0].count);
            thing.SetFaction(parent.Faction);
            GenSpawn.Spawn(thing, parent.Position, parent.Map, parent.Rotation);
            ReinforceDefOf.Reinforce_Success.PlayOneShot(thing);
        }

        

    }

}
