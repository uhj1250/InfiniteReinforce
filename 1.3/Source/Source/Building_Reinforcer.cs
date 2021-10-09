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

        public CompThingContainer ContainerComp
        {
            get
            {
                if (container == null) container = GetComp<CompThingContainer>();
                return container;
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
}
