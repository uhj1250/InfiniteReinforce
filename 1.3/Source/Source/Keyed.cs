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
        public static string InsertItem(string item) => "IR.InsertItem".Translate(item).CapitalizeFirst();
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

    }
}
