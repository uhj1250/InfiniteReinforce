using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace InfiniteReinforce
{
    public class StatPart_WarmUpMultiplier : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return Keyed.ReinforceStatPart + GetFactor(req) * 100 + "%";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            val /= GetFactor(req);
        }



        protected virtual float GetFactor(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn != null && pawn.equipment != null && pawn.equipment.Primary != null)
            {
                ThingWithComps weapon = pawn.equipment.Primary;
                if (weapon != null)
                {
                    return Mathf.Max(0.001f, weapon.GetReinforceStatFactor(StatDefOf.RangedWeapon_WarmupMultiplier));
                }
            }
            
            return 1.0f;
        }
    }
}
