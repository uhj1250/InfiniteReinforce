using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace InfiniteReinforce.Infusion2Module
{
    [StaticConstructorOnStartup]
    static class Init
    {
        static Init()
        {
            Harmony harmony = new Harmony("InfiniteReinforce.Infusion2Module");
            harmony.PatchAll();
        }
    }
}
