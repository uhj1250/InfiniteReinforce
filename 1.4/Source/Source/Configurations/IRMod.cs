using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace InfiniteReinforce
{
    public enum ReinforceFailureResult
    {
        None = 0,
        DamageLittle = 1,
        DamageLarge = 2,
        Explosion = 3,
        Destroy = 4
    }

    [Flags]
    public enum IRDifficultFlag
    {
        None = 0,
        Baby = 1,
        Weenie = 2,
        SuperWeenie = 4,
        Pro = 8,
        Badass = 16,
        Ironman = 32,
        

    }

    public class IRConfig : ModSettings
    {
        public const int FailureResultCount = 5;

        public static readonly int[] DefaultWeights = new int[] { 59, 25, 10, 5, 1 };
        public static readonly int[] SuperWeenieWeights = new int[] { 60, 25, 10, 5, 0 };
        public static readonly int[] IronmanWeights = new int[] { 0, 60, 25, 10, 5 };

        public static int[] BaseWeights
        {
            get
            {
                if (IronMode) return IronmanWeights;
                if (SuperWeenieMode) return SuperWeenieWeights;
                 return DefaultWeights;
            }
        }
        public static bool BabyMode = false;
        public static bool WeenieMode = false;
        public static bool SuperWeenieMode = false;
        public static bool ProMode = false;
        public static bool BadassMode = false;
        public static bool IronMode = false;
        public static bool InstantReinforce = false;
        public static float CostIncrementMultiplier = 1.0f;
        public static float FailureChanceMultiplier = 1.0f;
        public static QualityRange MaterialQualityRange = new QualityRange(QualityCategory.Awful,QualityCategory.Excellent);


        public void ResetDefault()
        {
            CostIncrementMultiplier = 1.0f;
            FailureChanceMultiplier = 1.0f;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref BabyMode, "BabyMode", false, true);
            Scribe_Values.Look(ref WeenieMode, "WeenieMode", false, true);
            Scribe_Values.Look(ref SuperWeenieMode, "SuperWeenieMode", false, true);
            Scribe_Values.Look(ref CostIncrementMultiplier, "CostIncrementMultiplier", 1.0f, true);
            Scribe_Values.Look(ref FailureChanceMultiplier, "FailureChanceMultiplier", 1.0f, true);
            Scribe_Values.Look(ref MaterialQualityRange, "MaterialQualityRange", new QualityRange(QualityCategory.Awful, QualityCategory.Excellent), true);
            Scribe_Values.Look(ref InstantReinforce, "InstantReinforce", false, true);


            base.ExposeData();
        }


    }


    public class IRMod : Mod
    {
        public IRMod(ModContentPack content) : base(content)
        {
            GetSettings<IRConfig>();
        }

        public override string SettingsCategory()
        {
            return Keyed.Title;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(inRect.ContractedBy(4f));

            Rect tmpRect = listmain.GetRect(24f);

            Widgets.CheckboxLabeled(tmpRect.LeftHalf() ,Keyed.Config_Baby, ref IRConfig.BabyMode);
            TooltipHandler.TipRegion(tmpRect.LeftHalf(), Keyed.Config_BabyDesc);

            if (IRConfig.BabyMode)
            {
                if (IRConfig.CostIncrementMultiplier > 1.0f) IRConfig.CostIncrementMultiplier = 1.0f;
                listmain.Label(Keyed.Config_CostIncrement + String.Format(" {0:P2}",IRConfig.CostIncrementMultiplier));
                IRConfig.CostIncrementMultiplier = listmain.Slider(IRConfig.CostIncrementMultiplier, 0, 1.0f);
                IRConfig.ProMode = false;
            }

            Widgets.CheckboxLabeled(tmpRect.RightHalf(), Keyed.Config_Pro, ref IRConfig.ProMode);
            TooltipHandler.TipRegion(tmpRect.RightHalf(), Keyed.Config_ProDesc);

            if (IRConfig.ProMode)
            {
                if (IRConfig.CostIncrementMultiplier < 1.0f) IRConfig.CostIncrementMultiplier = 1.0f;
                listmain.Label(Keyed.Config_CostIncrement + String.Format(" {0:P2}", IRConfig.CostIncrementMultiplier));
                IRConfig.CostIncrementMultiplier = listmain.Slider(IRConfig.CostIncrementMultiplier, 1.0f, 10.0f);
                IRConfig.BabyMode = false;
            }

            tmpRect = listmain.GetRect(24f);

            Widgets.CheckboxLabeled(tmpRect.LeftHalf(),Keyed.Config_Weenie, ref IRConfig.WeenieMode);
            TooltipHandler.TipRegion(tmpRect.LeftHalf(), Keyed.Config_WeenieDesc);


            if (IRConfig.WeenieMode)
            {
                IRConfig.BadassMode = false;
                IRConfig.IronMode = false;
                if (IRConfig.FailureChanceMultiplier > 1.0f) IRConfig.FailureChanceMultiplier = 1.0f;
                listmain.Label(Keyed.Config_FailureChance + String.Format(" {0:P2}", IRConfig.FailureChanceMultiplier));
                IRConfig.FailureChanceMultiplier = listmain.Slider(IRConfig.FailureChanceMultiplier, 0, 1.0f);

                listmain.CheckboxLabeled(Keyed.Config_SuperWeenie, ref IRConfig.SuperWeenieMode, Keyed.Config_SuperWeenieDesc);
            }

            Widgets.CheckboxLabeled(tmpRect.RightHalf(), Keyed.Config_Badass, ref IRConfig.BadassMode);
            TooltipHandler.TipRegion(tmpRect.RightHalf(), Keyed.Config_BadassDesc);

            if (IRConfig.BadassMode)
            {
                IRConfig.WeenieMode = false;
                IRConfig.SuperWeenieMode = false;
                if (IRConfig.FailureChanceMultiplier < 1.0f) IRConfig.FailureChanceMultiplier = 1.0f;
                listmain.Label(Keyed.Config_FailureChance + String.Format(" {0:P2}", IRConfig.FailureChanceMultiplier));
                IRConfig.FailureChanceMultiplier = listmain.Slider(IRConfig.FailureChanceMultiplier, 1.0f, 10.0f);

                listmain.CheckboxLabeled(Keyed.Config_Ironman, ref IRConfig.IronMode, Keyed.Config_IronmanDesc);
            }

            listmain.CheckboxLabeled(Keyed.Config_InstantReinforce, ref IRConfig.InstantReinforce, Keyed.Config_InstantReinforceDesc);


            listmain.End();
        }



    }
}
