using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;


namespace InfiniteReinforce
{
    [StaticConstructorOnStartup]
    static class Init
    {
        static Init()
        {
            Harmony harmony = new Harmony("InfiniteReinforce");
            harmony.PatchAll();
        }
    }
}
