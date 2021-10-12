using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace InfiniteReinforce
{
    public static class Keyed
    {
        public static string InsertItem(string item, string reinforcer) => "IR.InsertItem".Translate(item, reinforcer).CapitalizeFirst();
        public static string InsertFuel(string reinforcer) => "IR.InsertFuel".Translate(reinforcer).CapitalizeFirst();
        public static string ReinforcedCount(int count) => "IR.ReinforcedCount".Translate(count).CapitalizeFirst();
        public static string FailedDamaged(string thing) => "IR.FailedDamaged".Translate(thing).CapitalizeFirst();
        public static string FailedExplosion(string building) => "IR.FailedExplosion".Translate(building).CapitalizeFirst();
        public static string FailedDestroy(string thing) => "IR.FailedDestroy".Translate(thing).CapitalizeFirst();

        public static string Translate(this ReinforceFailureResult result)
        {
            switch (result)
            {
                case ReinforceFailureResult.None:
                default:
                    return Failure;
                case ReinforceFailureResult.DamageLittle:
                    return MinorDamage;
                case ReinforceFailureResult.DamageLarge:
                    return MajorDamage;
                case ReinforceFailureResult.Explosion:
                    return Explosion;
                case ReinforceFailureResult.Destroy:
                    return Explosion;
            }
        }

        public static string Translate(this IRDifficultFlag flag)
        {
            switch (flag)
            {
                case IRDifficultFlag.None:
                default:
                    return "None";
                case IRDifficultFlag.Baby:
                    return "Baby";
                case IRDifficultFlag.Weenie:
                    return "Weenie";
                case IRDifficultFlag.SuperWeenie:
                case IRDifficultFlag.Weenie | IRDifficultFlag.SuperWeenie:
                    return "SuperWeenie";
            }
        }

        public static readonly string Title = "IR.Title".Translate();
        public static readonly string Reinforce = "IR.Reinforce".Translate();
        public static readonly string ReinforceDesc = "IR.ReinforceDesc".Translate();
        public static readonly string TakeOut = "IR.TakeOut".Translate();
        public static readonly string TakeOutDesc = "IR.TakeOutDesc".Translate();
        public static readonly string Empty = "IR.Empty".Translate();
        public static readonly string NotEnough = "IR.NotEnough".Translate();
        public static readonly string Success = "IR.Success".Translate();
        public static readonly string Failed = "IR.Failed".Translate();
        public static readonly string History = "IR.History".Translate();
        public static readonly string ReinforceStatPart = "IR.ReinforceStatPart".Translate();
        public static readonly string Materials = "IR.Materials".Translate();
        public static readonly string FailedLetter = "IR.FailedLetter".Translate();
        public static readonly string FailureChance = "IR.FailureChance".Translate();
        public static readonly string FailureOutcome = "IR.FailureOutcome".Translate();
        public static readonly string Failure = "IR.Failure".Translate();
        public static readonly string MinorDamage = "IR.MinorDamage".Translate();
        public static readonly string MajorDamage = "IR.MajorDamage".Translate();
        public static readonly string Explosion = "IR.Explosion".Translate();
        public static readonly string Destruction = "IR.Destruction".Translate();
        public static readonly string Repair = "IR.Repair".Translate();
        public static readonly string ReinforceFlag = "IR.ReinforceFlag".Translate();
        public static readonly string ReinforceFlagDesc = "IR.ReinforceFlagDesc".Translate();

        public static readonly string Config_Baby = "IR.Config_Baby".Translate();
        public static readonly string Config_BabyDesc = "IR.Config_BabyDesc".Translate();
        public static readonly string Config_CostIncrement = "IR.Config_CostIncrement".Translate();
        public static readonly string Config_Weenie = "IR.Config_Weenie".Translate();
        public static readonly string Config_WeenieDesc = "IR.Config_WeenieDesc".Translate();
        public static readonly string Config_FailureChance = "IR.Config_FailureChance".Translate();
        public static readonly string Config_SuperWeenie = "IR.Config_SuperWeenie".Translate();
        public static readonly string Config_SuperWeenieDesc = "IR.Config_SuperWeenieDesc".Translate();

    }

}
