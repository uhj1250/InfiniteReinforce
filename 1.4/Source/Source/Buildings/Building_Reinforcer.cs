using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public const int BaseReinforceTicks = 720;
        
        protected static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);
        protected static readonly Color BarColor = new Color(0.0f, 0.80f, 0.80f);
        protected static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
        protected CompPowerTrader power = null;
        protected CompThingContainer container = null;
        protected CompReinforceFuel fuel = null;
        private ReinforceInstance instance = null;
        protected bool onprogress = false;
        private Material barFilledCachedMat;
        private List<Thing> insertedmaterials = new List<Thing>();
        protected Sustainer sustainer = null;
        public int ReinforceTicks => BaseReinforceTicks + (ItemReinforceComp?.ReinforcedCount ?? 0) * 30;
        
        public virtual bool PowerOn
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
        public ReinforceInstance Instance
        {
            get
            {
                if (instance != null) return instance;
                else
                {
                    instance = new ReinforceInstance(this);
                    return instance;
                }
            }
        }
        public float Progress
        {
            get
            {
                return Instance.Progress / ReinforceTicks;
            }
        }
        public ThingWithComps HoldingItem => ContainerComp?.ContainedThing as ThingWithComps;
        public virtual float Fuel => FuelComp?.Fuel ?? -1f;
        public virtual bool AlwaysSuccess => FuelComp?.AlwaysSuccess ?? false;
        public List<IReinforceSpecialOption> SpecialOptions => FuelComp?.Props?.SpecialOptions;
        public virtual bool ApplyMultiplier => FuelComp?.ApplyMultiplier ?? true;
        public IEnumerable<ThingDef> FuelThing => FuelComp?.Props.fuelFilter.AllowedThingDefs;
        public bool OnProgress => onprogress;

        protected Material BarFilledMat
        {
            get
            {
                if (barFilledCachedMat == null)
                {
                    barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(BarColor, false);
                }
                return barFilledCachedMat;
            }
        }

        protected float ProgressPerTick
        {
            get
            {
                return HitPoints / def.BaseMaxHitPoints;
            }
        }

        public ThingComp_Reinforce ItemReinforceComp
        {
            get
            {
                return HoldingItem?.GetReinforceComp();
            }
        }

        public CompThingContainer ContainerComp
        {
            get
            {
                if (container == null) container = GetComp<CompThingContainer>();
                return container;
            }
        }


        public virtual CompReinforceFuel FuelComp
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
            Scribe_Deep.Look(ref instance, "instance");
            Scribe_Values.Look(ref onprogress, "onprogress", false, true);
            Scribe_Collections.Look(ref insertedmaterials, true, "insertedmaterials", LookMode.Reference);
        }

        public override void Tick()
        {
            base.Tick();
            if (onprogress)
            {
                Instance.Tick(ProgressPerTick);
            }
        }
        public override void Draw()
        {
            base.Draw();
            if (onprogress)
            {
                Vector3 drawPos = DrawPos;
                drawPos.y += 0.04054054f;
                drawPos.z += 0.25f;
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = BarSize,
                    fillPercent = Progress,
                    filledMat = BarFilledMat,
                    unfilledMat = BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });
            }
        }

        public virtual void InsertItem(ThingWithComps item)
        {
            ContainerComp.innerContainer.Take(item);
        }

        public void InsertMaterials(List<Thing> things)
        {
            if (!things.NullOrEmpty()) for(int i=0; i<things.Count; i++)
            {
                if (things[i].Spawned) things[i].DeSpawn(DestroyMode.Vanish);
            }

            insertedmaterials.AddRange(things);
        }

        public void ExtractAllMaterials()
        {
            if(!insertedmaterials.NullOrEmpty()) for(int i=0; i<insertedmaterials.Count; i++)
            {
                GenPlace.TryPlaceThing(insertedmaterials[i], Position, Map, ThingPlaceMode.Near);
            }
            insertedmaterials.Clear();
        }

        protected void ConsumeMaterials()
        {
            insertedmaterials.Clear();
        }

        public virtual void ExtractItem()
        {
            
            if (ContainerComp.innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near))
            {
                ExtractAllMaterials();
                Instance.CleanUp();
            }
            else
            {
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
            }
        }

        public virtual void SetFuelRandom()
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

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            sustainer?.End();
            ExtractAllMaterials();
            base.Destroy(mode);
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
                    SoundDefOf.DropPod_Open.PlayOneShot(this);
                }
            };
            return gizmo;
        }

        public ReinforceFailureResult FailureEffect(float totalweight, int[] weights)
        {
            float sum = 0;
            float rand = Rand.Range(0, totalweight);

            for (int i = 0; i < weights.Length; i++)
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
                    ReinforceDefOf.Reinforce_FailedMinor.PlayOneShot(this);
                    break;
                case ReinforceFailureResult.DamageLittle:
                    DamageThing(Rand.Range(1, 10));
                    if (!HoldingItem?.Destroyed ?? false) Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDamaged(HoldingItem.Label), LetterDefOf.NegativeEvent, this);
                    break;
                case ReinforceFailureResult.DamageLarge:
                    DamageThing(Rand.Range(10, 60));
                    if (!HoldingItem?.Destroyed ?? false) Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDamaged(HoldingItem.Label), LetterDefOf.NegativeEvent, this);
                    break;
                case ReinforceFailureResult.Explosion:
                    Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedExplosion(Label), LetterDefOf.NegativeEvent, this);
                    Explosion();
                    break;
                case ReinforceFailureResult.Destroy:
                    Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDestroy(HoldingItem.Label), LetterDefOf.Death, this);
                    HoldingItem.Destroy(DestroyMode.Vanish);
                    ReinforceDefOf.Reinforce_FailedCritical.PlayOneShot(this);
                    break;
            }
        }

        public void DamageThing(float damage)
        {
            if (HoldingItem.HitPoints <= damage)
            {
                Find.LetterStack.ReceiveLetter(Keyed.FailedLetter, Keyed.FailedDestroy(HoldingItem.Label), LetterDefOf.Death, this);
                ReinforceDefOf.Reinforce_FailedCritical.PlayOneShot(this);
                HoldingItem.Destroy(DestroyMode.Vanish);
            }
            else
            {
                if (damage > 10) ReinforceDefOf.Reinforce_FailedNormal.PlayOneShot(this);
                else ReinforceDefOf.Reinforce_FailedMinor.PlayOneShot(this);
                HoldingItem.HitPoints -= (int)damage;
            }
        }

        public void Explosion()
        {
            DamageThing(Rand.Range(30, 80));
            HitPoints = Math.Max((int)(HitPoints * Rand.Range(0.65f, 0.90f)), 1);
            GenExplosion.DoExplosion(Position, Map, Rand.Range(1, 3), DamageDefOf.Bomb, this, Rand.Range(50, 120));
        }


        public class ReinforceInstance : IExposable
        {
            private float progress;
            private ReinforceType type;
            private Building_Reinforcer parent;
            private Def reinforcedef;
            //private IReinforceSpecialOption option;
            private Type optiontype;
            private bool? successed = null;
            private bool alwaysSuccess = false;
            private ThingComp_Reinforce compcache;
            protected List<string> reinforcehistory = new List<string>();
            

            public bool? Succession => successed; 
            protected ThingComp_Reinforce Comp
            {
                get
                {
                    if (compcache == null) compcache = parent.HoldingItem.TryGetComp<ThingComp_Reinforce>();
                    return compcache;
                }
            }


            public void ExposeData()
            {
                Scribe_Values.Look(ref progress, "progress", 0, true);
                Scribe_Values.Look(ref type, "type", ReinforceType.None, true);
                Scribe_Values.Look(ref alwaysSuccess, "alwaysSuccess", false, true);
                switch (type)
                {
                    case ReinforceType.Stat:
                        StatDef statdef = (StatDef)reinforcedef;
                        Scribe_Defs.Look(ref statdef, "reinforcedef");
                        if (Scribe.mode == LoadSaveMode.LoadingVars) reinforcedef = statdef;
                        break;
                    case ReinforceType.Custom:
                        ReinforceDef customdef = (ReinforceDef)reinforcedef;
                        Scribe_Defs.Look(ref customdef, "reinforcedef");
                        if (Scribe.mode == LoadSaveMode.LoadingVars) reinforcedef = customdef;
                        break;
                    case ReinforceType.Special:
                        Scribe_Values.Look(ref optiontype, "optiontype");
                        break;
                    default:
                        break;
                }
                Scribe_References.Look(ref parent, "parent", true);
                Scribe_Collections.Look(ref reinforcehistory, "reinforcehistory", LookMode.Value);

            }

            public ReadOnlyCollection<string> History => reinforcehistory.AsReadOnly();


            public ReinforceInstance() { }
            public ReinforceInstance(Building_Reinforcer parent)
            {
                this.parent = parent;
            }

            public float Progress
            {
                get
                {
                    return progress;
                }
            }
            
            protected bool Init()
            {
                progress = 0f;
                successed = null;
                return true;
            }

            protected bool SetUp()
            {
                parent.onprogress = true;
                if (parent.sustainer == null || parent.sustainer.Ended) parent.sustainer = ReinforceDefOf.Reinforce_Progress.TrySpawnSustainer(parent);
                return true;
            }

            public void Reset()
            {
                Init();
                type = ReinforceType.None;
                reinforcedef = null;
                optiontype = null;
            }

            public bool CleanUp()
            {
                progress = 0f;
                optiontype = null;
                reinforcedef = null;
                parent.onprogress = false;
                parent.sustainer?.End();
                StatsReportUtility.Notify_QuickSearchChanged();
                alwaysSuccess = false;
                return true;
            }


            public bool TryReinforce(StatDef def, bool alwaysSuccess = false)
            {
                Init();
                type = ReinforceType.Stat;
                reinforcedef = def;
                this.alwaysSuccess = alwaysSuccess;
                SetUp();
                return true;
            }
            
            public bool TryReinforce(ReinforceDef def, bool alwaysSuccess = false)
            {
                Init();
                type = ReinforceType.Custom;
                reinforcedef = def;
                this.alwaysSuccess = alwaysSuccess;
                SetUp();
                return true;
            }

            public bool TryReinforce<T>(T option, bool alwaysSuccess = false) where T : IReinforceSpecialOption
            {
                Init();
                type = ReinforceType.Special;
                optiontype = option.GetType();
                this.alwaysSuccess = alwaysSuccess;
                SetUp();
                return true;
            }


            public bool CompleteReinforce()
            {
                int level;

                if (parent.HoldingItem != null)
                {
                    int[] weights = Comp.GetFailureWeights(out int totalweight);
                    successed = alwaysSuccess || !Comp.RollFailure(out float rolled, totalweight, parent.MaxHitPoints / parent.HitPoints);
                    parent.ConsumeMaterials();
                    if (reinforcehistory.Count > 30)
                    {
                        reinforcehistory.RemoveAt(0);
                    }
                    if (successed ?? false)
                    {
                        switch (type)
                        {
                            case ReinforceType.Stat:
                                level = Rand.Range(1, 25);
                                StatDef stat = (StatDef)reinforcedef;
                                parent.ItemReinforceComp.ReinforceStat(stat, level);
                                reinforcehistory.Add(stat.label + " +" + level * stat.GetOffsetPerLevel() * 100 + "%");
                                break;
                            case ReinforceType.Special:
                                IReinforceSpecialOption option = (IReinforceSpecialOption)Activator.CreateInstance(optiontype);
                                option.Reinforce(parent.ItemReinforceComp)();
                                reinforcehistory.Add(option.LabelLeft(parent.ItemReinforceComp));
                                break;
                            case ReinforceType.Custom:
                                ReinforceDef def = ((ReinforceDef)reinforcedef);
                                level = Rand.Range(def.levelRange.min, def.levelRange.max);
                                def.Worker.Reinforce(parent.ItemReinforceComp, level)();
                                reinforcehistory.Add(def.Worker.ResultString(level));
                                break;
                            default:
                                CleanUp();
                                return false;
                        }
                        CleanUp();
                        if (reinforcehistory.Count > 30) reinforcehistory.RemoveAt(0);
                        ReinforceDefOf.Reinforce_Success.PlayOneShot(parent);
                        return true;
                    }
                    else
                    {
                        CleanUp();
                        ReinforceFailureResult effect = parent.FailureEffect(totalweight, weights);
                        reinforcehistory.Add(Keyed.Failed.CapitalizeFirst() + " - " + effect.Translate());
                        return false;
                    }
                }
                CleanUp();
                Log.Error(parent + parent.Label + " is empty.");
                return false;
            }

            
            public void Tick(float progression)
            {
                progress += progression;
                if (IRConfig.InstantReinforce || progress > parent.ReinforceTicks)
                {
                    successed = CompleteReinforce();
                }
            }

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
