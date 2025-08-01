﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using System.Runtime.CompilerServices;

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
                return Instance.Progress / Instance.ProgressionTicks;
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
            Scribe_Collections.Look(ref insertedmaterials, "insertedmaterials", true, LookMode.Reference);
        }

        protected override void Tick()
        {
            base.Tick();
            if (PowerOn && onprogress)
            {
                Instance.Tick(ProgressPerTick);
            }
        }


        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
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

        public override string GetInspectStringLowPriority()
        {
            string res = base.GetInspectStringLowPriority();
            if (OnProgress) res += "DurationLeft".Translate(((int)((Instance.ProgressionTicks - Instance.Progress)/ProgressPerTick)).ToStringTicksToPeriod(allowSeconds: true, shortForm: true));
            return res;
            
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
                Instance.Reset();
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
            if (OnProgress) gizmos.Add(CreateCancelGizmo());
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
                Disabled = !PowerOn || HoldingItem == null,
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
                Disabled = HoldingItem == null,
                disabledReason = Keyed.Empty,
                action = delegate
                {
                    ExtractItem();
                    SoundDefOf.DropElement.PlayOneShot(this);
                }
            };
            return gizmo;
        }

        protected Gizmo CreateCancelGizmo()
        {
            Gizmo gizmo = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", false),
                defaultLabel = "Cancel".Translate(),
                defaultDesc = "Cancel".Translate(),
                action = delegate
                {
                    Instance.Reset();
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
            public struct Reinforcement : IExposable
            {
                public ReinforceType type;
                public Type optiontype;
                public Def reinforcedef;
                public bool alwaysSuccess;
                public float progressmultiplier;
                public CostMode costMode;
                public bool instant;

                public float ProgressMultiplier
                {
                    get
                    {
                        if (IRConfig.InstantReinforce) return 1.0f;
                        return progressmultiplier;
                    }
                }

                public void ExposeData()
                {
                    Scribe_Values.Look(ref type, "type", ReinforceType.None, true);
                    Scribe_Values.Look(ref alwaysSuccess, "alwaysSuccess", false, true);
                    Scribe_Values.Look(ref progressmultiplier, "progressmultiplier", 1.0f, true);
                    Scribe_Values.Look(ref costMode, "costMode", CostMode.SameThing, true);
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
                }
            }

            private float progress;
            private float progressmultiplier = 1.0f;
            private Building_Reinforcer parent;
            private bool? successed = null;
            protected List<string> reinforcehistory = new List<string>();
            protected Queue<Reinforcement> reinforcementqueue = new Queue<Reinforcement>();
            
            public int QueueCount
            {
                get
                {
                    return reinforcementqueue.Count;
                }
            }
            public float ProgressMultiplier
            {
                get
                {
                    if (IRConfig.InstantReinforce) return 1.0f;
                    return progressmultiplier;
                }
                set
                {
                    progressmultiplier = value;
                }
            }
            public float ProgressionTicks
            {
                get
                {
                    Reinforcement reinforcement = reinforcementqueue.FirstOrDefault();
                    float multiply = reinforcement.alwaysSuccess ? 1.0f : reinforcement.progressmultiplier;
                    return parent.ReinforceTicks * multiply;
                }
            }
            public bool? Succession => successed; 
            protected ThingComp_Reinforce Comp
            {
                get
                {
                    return parent.ItemReinforceComp;
                }
            }


            public void ExposeData()
            {
                Scribe_Values.Look(ref progress, "progress", 0, true);
                Scribe_Values.Look(ref progressmultiplier, "progressmultiplier", 1.0f, true);
                Scribe_References.Look(ref parent, "parent", true);
                Scribe_Collections.Look(ref reinforcehistory, "reinforcehistory", LookMode.Value);
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    List<Reinforcement> queuetemp = new List<Reinforcement>();
                    Scribe_Collections.Look(ref queuetemp, "reinforcementqueue", LookMode.Deep);
                    if (!queuetemp.NullOrEmpty()) reinforcementqueue.Concat(queuetemp);
                }
                else if (Scribe.mode == LoadSaveMode.Saving)
                {
                    List<Reinforcement> queuetemp = reinforcementqueue.ToList();
                    Scribe_Collections.Look(ref queuetemp, "reinforcementqueue", LookMode.Deep);
                }
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
            
            public List<string> QueuedReinforcements
            {
                get
                {
                    if (reinforcementqueue.EnumerableNullOrEmpty())
                    {
                        return null;
                    }
                    List<string> res = new List<string>();
                    foreach(Reinforcement r in reinforcementqueue)
                    {
                        switch (r.type)
                        {
                            case ReinforceType.Stat:
                            case ReinforceType.Custom:
                                res.Add(r.reinforcedef.label);
                                break;
                            case ReinforceType.Special:
                                res.Add(parent.SpecialOptions?.FirstOrDefault(x => x.GetType().Equals(r.optiontype))?.LabelLeft(null) ?? "Error");
                                break;
                            default:
                                res.Add("Error");
                                break;
                        }
                    }

                    return res;
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
                if (!parent.onprogress)
                {
                    parent.onprogress = true;
                    if (parent.sustainer == null || parent.sustainer.Ended) parent.sustainer = ReinforceDefOf.Reinforce_Progress.TrySpawnSustainer(parent);
                    return InsertMaterials(reinforcementqueue.First());
                }
                else
                {
                    return true;
                }
            }

            public void Reset()
            {
                Init();
                reinforcementqueue.Clear();
                parent.ExtractAllMaterials();
                CleanUp();
            }

            public bool CleanUp()
            {
                progress = 0f;

                if (reinforcementqueue.Count > 0)
                {
                    return InsertMaterials(reinforcementqueue.First());
                }
                else
                {
                    parent.onprogress = false;
                    parent.sustainer?.End();
                    StatsReportUtility.Reset();
                    StatsReportUtility.Notify_QuickSearchChanged();
                    return true;
                }
            }

            public bool TryReinforce(StatDef def, CostMode costmode, bool alwaysSuccess = false)
            {
                reinforcementqueue.Enqueue(new Reinforcement
                {
                    type = ReinforceType.Stat,
                    reinforcedef = def,
                    alwaysSuccess = alwaysSuccess,
                    costMode = costmode,
                    progressmultiplier = ProgressMultiplier
                    
                });
                return SetUp();
            }
            
            public bool TryReinforce(ReinforceDef def, CostMode costmode, bool alwaysSuccess = false)
            {
                reinforcementqueue.Enqueue(new Reinforcement
                {
                    type = ReinforceType.Custom,
                    reinforcedef = def,
                    alwaysSuccess = alwaysSuccess,
                    costMode = costmode,
                    progressmultiplier = ProgressMultiplier
                });
                return SetUp();
            }

            public bool TryReinforce<T>(T option, CostMode costmode, bool alwaysSuccess = false) where T : IReinforceSpecialOption
            {
                reinforcementqueue.Enqueue(new Reinforcement
                {
                    type = ReinforceType.Special,
                    optiontype = option.GetType(),
                    alwaysSuccess = alwaysSuccess,
                    costMode = costmode,
                    progressmultiplier = ProgressMultiplier
                });
                return SetUp();
                
            }
            
            


            public bool CompleteReinforce()
            {
                int level;
                if (reinforcementqueue.EnumerableNullOrEmpty())
                {
                    Log.Error(parent.Label + ": Reinforcement queue is empty");
                    return false;
                }
                Reinforcement reinforcement = reinforcementqueue.Dequeue();

                if (parent.HoldingItem != null)
                {
                    float rolled = 100f; float chance = 0f;
                    int[] weights = Comp.GetFailureWeights(out int totalweight);
                    successed = reinforcement.alwaysSuccess || !Comp.RollFailure(out rolled,out chance, totalweight, parent.MaxHitPoints / (parent.HitPoints*reinforcement.ProgressMultiplier));
                    string chancestring = String.Format("{0:0.0}/{1:0.0}", rolled,chance);
                    parent.ConsumeMaterials();
                    if (reinforcehistory.Count > 30)
                    {
                        reinforcehistory.RemoveAt(0);
                    }
                    if (successed ?? false)
                    {
                        switch (reinforcement.type)
                        {
                            case ReinforceType.Stat:
                                level = Rand.Range(1, 25);
                                StatDef stat = (StatDef)reinforcement.reinforcedef;
                                parent.ItemReinforceComp.ReinforceStat(stat, level);
                                reinforcehistory.Add(stat.label + " " + (level * stat.GetOffsetPerLevel() * 100).ToString("+#;-#;0") + "%  " + chancestring);
                                stat.Worker.TryClearCache();
                                break;
                            case ReinforceType.Special:
                                IReinforceSpecialOption option = (IReinforceSpecialOption)Activator.CreateInstance(reinforcement.optiontype);
                                option.Reinforce(parent.ItemReinforceComp)();
                                reinforcehistory.Add(option.LabelLeft(parent.ItemReinforceComp) + "  " + chancestring);
                                break;
                            case ReinforceType.Custom:
                                ReinforceDef def = ((ReinforceDef)reinforcement.reinforcedef);
                                level = Rand.Range(def.levelRange.min, def.levelRange.max);
                                def.Worker.Reinforce(parent.ItemReinforceComp, level)();
                                reinforcehistory.Add(def.Worker.ResultString(level) + "  " + chancestring);
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
                        reinforcehistory.Add(Keyed.Failed.CapitalizeFirst() + " - " + effect.Translate() + "  " + chancestring);
                        if (reinforcehistory.Count > 30) reinforcehistory.RemoveAt(0);
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
                if (IRConfig.InstantReinforce || progress > ProgressionTicks || DebugSettings.godMode)
                {
                    successed = CompleteReinforce();
                }
            }

            protected bool InsertMaterials(Reinforcement reinforcement)
            {
                if (DebugSettings.godMode) return true;
                List<ThingDefCountClass> costlist = BuildCostList(reinforcement.costMode);
                if (CheckAndInsertMaterials(costlist, reinforcement.costMode))
                {
                    return true;
                }
                else
                {
                    Messages.Message(Keyed.NotEnough, parent, MessageTypeDefOf.RejectInput);
                    Reset();
                    return false;
                }

            }

            protected bool CheckAndInsertMaterials(List<ThingDefCountClass> costlist, CostMode costMode)
            {
                if (costlist.NullOrEmpty()) return false;

                if (costMode == CostMode.Fuel)
                {
                    if (parent.Fuel > 0)
                    {
                        parent.FuelComp.ConsumeOnce();
                        ReinforcerEffect effect = parent.FuelComp.Props.Effect;
                        if (effect != null && effect.Apply(parent.ItemReinforceComp)) effect.DoEffect(parent, parent.ItemReinforceComp);
                        return true;
                    }
                    return false;
                }
                else
                {
                    IEnumerable<Thing> materials = ReinforceUtility.AllThingsNearBeacon(parent.Map, x => costlist.Exists(y => y.thingDef == x.def));
                    List<Thing>[] thingtoinsert = new List<Thing>[costlist.Count];


                    ThingDef stuff = costMode == CostMode.SameThing ? parent.HoldingItem.Stuff : null;


                    for (int i=0; i < costlist.Count; i++)
                    {
                        if (materials.CountThingInCollection(costlist[i].thingDef, stuff) < CostOf(costlist, i, costMode)) return false;
                    }
                         
                    for (int i = 0; i < costlist.Count; i++)
                    {
                        thingtoinsert[i] = materials.GetThingsOfType(costlist[i].thingDef, CostOf(costlist, i, costMode), stuff);
                        if (thingtoinsert[i] == null) return false;
                    }

                    for (int i = 0; i < costlist.Count; i++)
                    {
                        parent.InsertMaterials(thingtoinsert[i]);
                    }

                    return true;
                }
            }
            
            protected int CostOf(List<ThingDefCountClass> costlist, int index, CostMode costMode)
            {
                if (costMode == CostMode.Fuel && !parent.ApplyMultiplier) return costlist[index].count;
                return (int)(costlist[index].count * parent.ItemReinforceComp.CostMultiplier);
            }

            protected List<ThingDefCountClass> BuildCostList(CostMode costmode)
            {
                
                List<ThingDefCountClass> costlist = new List<ThingDefCountClass>();

                switch (costmode)
                {
                    case CostMode.SameThing:
                        costlist.Add(new ThingDefCountClass(parent.HoldingItem.def, 1));
                        break;
                    case CostMode.Material:
                        ReinforceCostDef costDef = DefDatabase<ReinforceCostDef>.GetNamedSilentFail(parent.HoldingItem.def.defName);
                        if (costDef != null)
                        {
                            if (!costDef.costList.NullOrEmpty()) costlist.AddRange(costDef.costList);
                        }
                        else
                        {
                            if (!parent.HoldingItem.def.costList.NullOrEmpty()) costlist.AddRange(parent.HoldingItem.def.CostList);
                            if (parent.HoldingItem.Stuff != null)
                            {
                                ThingDefCountClass stuff = costlist.FirstOrDefault(x => x.thingDef == parent.HoldingItem.Stuff);
                                if (stuff != null)
                                {
                                    stuff.count += parent.HoldingItem.def.costStuffCount;
                                }
                                else
                                {
                                    costlist.Add(new ThingDefCountClass(parent.HoldingItem.Stuff, parent.HoldingItem.def.CostStuffCount));
                                }
                            }
                        }
                        break;
                    case CostMode.Fuel:
                        if (parent.FuelComp != null)
                        {
                            costlist.Add(new ThingDefCountClass(parent.FuelThing.FirstOrDefault(), 1));
                        }
                        break;
                    default:
                        Log.Error(parent.Label + parent.HoldingItem.Label + ": Wrong cost mode");
                        return null;

                }
                return costlist;
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


    public class ReinDeer : IDisposable
    {
        public void Dispose()
        {
            
        }
        


    }




}
