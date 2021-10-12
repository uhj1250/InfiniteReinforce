using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;


namespace InfiniteReinforce
{
    public class Building_Reinforcer : Building
    {
        protected CompPowerTrader power = null;
        protected CompThingContainer container = null;
        protected CompReinforceFuel fuel = null;

        public bool PowerOn
        {
            get
            {
                if (power == null)
                {
                    power = this.GetComp<CompPowerTrader>();
                }
                return power?.PowerOn ?? false;
            }
        }
        public ThingWithComps HoldingItem => ContainerComp?.ContainedThing as ThingWithComps;
        public float Fuel => FuelComp?.Fuel ?? -1f;
        public bool AlwaysSuccess => FuelComp?.AlwaysSuccess ?? false;
        public List<ReinforceSpecialOption> SpecialOptions => FuelComp?.Props?.SpecialOptions;
        public bool ApplyMultiplier => FuelComp?.ApplyMultiplier ?? true;
        public IEnumerable<ThingDef> FuelThing => FuelComp?.Props.fuelFilter.AllowedThingDefs;

        public CompThingContainer ContainerComp
        {
            get
            {
                if (container == null) container = GetComp<CompThingContainer>();
                return container;
            }
        }


        public CompReinforceFuel FuelComp
        {
            get
            {
                if (fuel == null) fuel = this.TryGetComp<CompReinforceFuel>();
                return fuel;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();


        }


        public void InsertItem(ThingWithComps item)
        {
            ContainerComp.innerContainer.Take(item);
        }

        public void ExtractItem()
        {
            
            if (ContainerComp.innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near))
            {

            }
            else
            {
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
            }
        }

        public void SetFuelRandom()
        {
            if (FuelComp != null)
            {
                FuelComp.Refuel(Rand.Range(0, FuelComp.GetFuelCountToFullyRefuel()));
            }
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = base.GetGizmos().ToList();
            gizmos.Add(CreateReinforceGizmo());
            gizmos.Add(CreateExtractItemGizmo());
            return gizmos;
        }

        protected Gizmo CreateReinforceGizmo()
        {
            Gizmo gizmo = new Command_Action
            {
                icon = IconCache.EquipmentReinforce,
                defaultLabel = Keyed.Reinforce,
                defaultDesc = Keyed.ReinforceDesc,
                disabled = !PowerOn || HoldingItem == null,
                //disabledReason = "PowerNotConnected".Translate(),
                action = delegate
                {
                    Dialog_Reinforcer.ToggleWindow(this);
                }
            };
            return gizmo;
        }

        protected Gizmo CreateExtractItemGizmo()
        {
            Gizmo gizmo = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Open", false),
                defaultLabel = Keyed.TakeOut,
                defaultDesc = Keyed.TakeOutDesc,
                disabled = HoldingItem == null,
                disabledReason = Keyed.Empty,
                action = delegate
                {
                    ExtractItem();
                    SoundDefOf.DropPod_Open.PlayOneShot(SoundInfo.InMap(this));
                }
            };
            return gizmo;
        }


    }

    public class ReinforcerComparer : IEqualityComparer<Building_Reinforcer>
    {
        public bool Equals(Building_Reinforcer x, Building_Reinforcer y)
        {
            return x.def == y.def;
        }

        public int GetHashCode(Building_Reinforcer obj)
        {
            return base.GetHashCode();
        }

        
    }

}
