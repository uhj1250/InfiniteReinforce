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
                    return "None".Translate();
                case IRDifficultFlag.Baby:
                    return Baby;
                case IRDifficultFlag.Weenie:
                    return Weenie;
                case IRDifficultFlag.Baby | IRDifficultFlag.Weenie:
                    return WeenieBaby;
                case IRDifficultFlag.SuperWeenie:
                case IRDifficultFlag.Weenie | IRDifficultFlag.SuperWeenie:
                    return SuperWeenie;
                case IRDifficultFlag.Baby | IRDifficultFlag.SuperWeenie:
                case IRDifficultFlag.Baby | IRDifficultFlag.Weenie | IRDifficultFlag.SuperWeenie:
                    return SuperWeenieBaby;
                case IRDifficultFlag.Baby | IRDifficultFlag.Badass:
                    return Badassbaby;
                case IRDifficultFlag.Baby | IRDifficultFlag.Ironman:
                case IRDifficultFlag.Baby | IRDifficultFlag.Badass | IRDifficultFlag.Ironman:
                    return IronmanBaby;
                case IRDifficultFlag.Pro:
                    return Pro;
                case IRDifficultFlag.Pro | IRDifficultFlag.Weenie:
                    return ProWeenie;
                case IRDifficultFlag.Pro | IRDifficultFlag.SuperWeenie:
                case IRDifficultFlag.Pro | IRDifficultFlag.Weenie | IRDifficultFlag.SuperWeenie:
                    return ProSuperWeenie;
                case IRDifficultFlag.Pro | IRDifficultFlag.Badass:
                    return ProBadass;
                case IRDifficultFlag.Pro | IRDifficultFlag.Ironman:
                case IRDifficultFlag.Pro | IRDifficultFlag.Badass | IRDifficultFlag.Ironman:
                    return ProIronman;
                case IRDifficultFlag.Badass:
                    return Badass;
                case IRDifficultFlag.Ironman:
                case IRDifficultFlag.Badass | IRDifficultFlag.Ironman:
                    return Ironman;
                default:
                    return Cheater;
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
        public static readonly string Config_Pro = "IR.Config_Pro".Translate();
        public static readonly string Config_ProDesc = "IR.Config_ProDesc".Translate();
        public static readonly string Config_Badass = "IR.Config_Badass".Translate();
        public static readonly string Config_BadassDesc = "IR.Config_BadassDesc".Translate();
        public static readonly string Config_Ironman = "IR.Config_Ironman".Translate();
        public static readonly string Config_IronmanDesc = "IR.Config_IronmanDesc".Translate();
        public static readonly string Config_InstantReinforce = "IR.Config_InstantReinforce".Translate();
        public static readonly string Config_InstantReinforceDesc = "IR.Config_InstantReinforceDesc".Translate();
        public static readonly string Baby = "IR.Baby".Translate();
        public static readonly string Weenie = "IR.Weenie".Translate();
        public static readonly string WeenieBaby = "IR.WeenieBaby".Translate();
        public static readonly string SuperWeenie = "IR.SuperWeenie".Translate();
        public static readonly string SuperWeenieBaby = "IR.SuperWeenieBaby".Translate();
        public static readonly string Pro = "IR.Pro".Translate();
        public static readonly string ProWeenie = "IR.ProWeenie".Translate();
        public static readonly string ProSuperWeenie = "IR.ProSuperWeenie".Translate();
        public static readonly string ProBadass = "IR.ProBadass".Translate();
        public static readonly string ProIronman = "IR.ProIronman".Translate();
        public static readonly string Badass = "IR.Badass".Translate();
        public static readonly string Badassbaby = "IR.Badassbaby".Translate();
        public static readonly string Ironman = "IR.Ironman".Translate();
        public static readonly string IronmanBaby = "IR.IronmanBaby".Translate();
        public static readonly string Cheater = "IR.Cheater".Translate();
    }

}
