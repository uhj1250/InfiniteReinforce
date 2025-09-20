using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace InfiniteReinforce
{
    public class UpgradeDef : ThingDef
    {
        public ThingDef resultThingDef;
        public override IntVec2 Size => resultThingDef.Size;
    }


    public abstract class UpgradeThing : ThingWithComps
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Upgrade();
        }

        protected abstract void Upgrade();

    }

    public class UpgradeThing_Building : UpgradeThing
    {
        protected override void Upgrade()
        {
            var upgradeDef = def as UpgradeDef;
            if (upgradeDef != null && upgradeDef.resultThingDef != null)
            {
                var newBuilding = (Building)ThingMaker.MakeThing(upgradeDef.resultThingDef);
                newBuilding.SetFaction(Faction);
                GenSpawn.Spawn(newBuilding, Position, Map, Rotation);
                if (!Destroyed) Destroy();
            }
        }

        
    }



}
