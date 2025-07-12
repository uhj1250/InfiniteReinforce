using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace InfiniteReinforce
{
    public class CompProperties_ReinforceFuel : CompProperties_Refuelable
    {
        public float fuelConsumptionPerReinforce = 1.0f;
        public List<IReinforceSpecialOption> SpecialOptions = new List<IReinforceSpecialOption>();
        public Type effectClass;

        public bool alwaysSuccess = false;
        public bool applyMultiplier = true;

        protected ReinforcerEffect effectcache;

        public CompProperties_ReinforceFuel()
        {
            compClass = typeof(CompReinforceFuel);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if (destroyOnNoFuel && initialFuelPercent <= 0f)
            {
                yield return "Refuelable component has destroyOnNoFuel, but initialFuelPercent <= 0";
            }
        }


        public ReinforcerEffect Effect
        {
            get
            {
                if (effectcache == null && effectClass != null)
                {
                    effectcache = (ReinforcerEffect)Activator.CreateInstance(effectClass);
                }
                return effectcache;
            }
        }


    }

    public class CompReinforceFuel : CompRefuelable
    {
        new public CompProperties_ReinforceFuel Props => (CompProperties_ReinforceFuel)props;
        public bool AlwaysSuccess
        {
            get
            {
                return Props.alwaysSuccess;
            }
        }
        public bool ApplyMultiplier
        {
            get
            {
                return Props.applyMultiplier;
            }
        }
        

        public virtual void ConsumeOnce()
        {
            ConsumeFuel(Props.fuelConsumptionPerReinforce);
        }

        public override string CompInspectStringExtra()
        {
            string text = Props.FuelLabel + ": " + Fuel.ToStringDecimalIfSmall() + " / " + Props.fuelCapacity.ToStringDecimalIfSmall();
            if (!HasFuel && !Props.outOfFuelMessage.NullOrEmpty())
            {
                text += $"\n{Props.outOfFuelMessage} ({GetFuelCountToFullyRefuel()}x {Props.fuelFilter.AnyAllowedDef.label})";
            }
            if (Props.targetFuelLevelConfigurable)
            {
                text += "\n" + "ConfiguredTargetFuelLevel".Translate(TargetFuelLevel.ToStringDecimalIfSmall());
            }
            return text;
        }
    }




}
