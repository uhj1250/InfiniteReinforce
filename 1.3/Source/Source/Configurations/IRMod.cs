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
    }

    public class IRConfig : ModSettings
    {
        public const int FailureResultCount = 5;

        public static readonly int[] DefaultWeights = new int[] { 50, 25, 10, 5, 1 };
        public static readonly int[] SuperWeenieWeights = new int[] { 50, 25, 11, 5, 0 };

        public static int[] BaseWeights
        {
            get
            {
                if (SuperWeenieMode) return SuperWeenieWeights;
                 return DefaultWeights;
            }
        }
        public static bool BabyMode = false;
        public static bool WeenieMode = false;
        public static bool SuperWeenieMode = false;
        public static float CostIncrementMultiplier = 1.0f;
        public static float FailureChanceMultiplier = 1.0f;



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


            listmain.CheckboxLabeled(Keyed.Config_Baby, ref IRConfig.BabyMode, Keyed.Config_BabyDesc);
            if (IRConfig.BabyMode)
            {
                listmain.Label(Keyed.Config_CostIncrement + String.Format(" {0:P2}",IRConfig.CostIncrementMultiplier));
                IRConfig.CostIncrementMultiplier = listmain.Slider(IRConfig.CostIncrementMultiplier, 0, 1.0f);
            }
            listmain.CheckboxLabeled(Keyed.Config_Weenie, ref IRConfig.WeenieMode, Keyed.Config_WeenieDesc);
            if (IRConfig.WeenieMode)
            {
                listmain.Label(Keyed.Config_FailureChance + String.Format(" {0:P2}", IRConfig.FailureChanceMultiplier));
                IRConfig.FailureChanceMultiplier = listmain.Slider(IRConfig.FailureChanceMultiplier, 0, 1.0f);

                listmain.CheckboxLabeled(Keyed.Config_SuperWeenie, ref IRConfig.SuperWeenieMode, Keyed.Config_SuperWeenieDesc);
            }

            listmain.End();
        }



    }
}
