using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;



namespace InfiniteReinforce
{
    public class StatPart_Reinforce : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return Keyed.ReinforceStatPart + GetFactor(req) * 100 + "%";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            val *= GetFactor(req);
        }



        protected virtual float GetFactor(StatRequest req)
        {
            ThingWithComps thing = req.Thing as ThingWithComps;
            if (thing != null)
            {
                return thing.GetReinforceStatFactor(parentStat);
            }
            else return 1.0f;
        }
        
    }

    public class StatPart_Reinforce_Reversal : StatPart_Reinforce
    {
        public override string ExplanationPart(StatRequest req)
        {
            return Keyed.ReinforceStatPartReversal + GetFactor(req) * 100 + "%";
        }
        public override void TransformValue(StatRequest req, ref float val)
        {
            val /= GetFactor(req);
        }
    }


    public class StatPart_ReinforceCount : StatPart_Reinforce
    {
        public float offsetPerCount;


        protected override float GetFactor(StatRequest req)
        {
            ThingWithComps thing = req.Thing as ThingWithComps;
            if (thing != null)
            {
                return 1.0f + 0.2f*thing.GetReinforcedCount();
            }
            else return 1.0f;
        }
    }

    public class StatPart_CustomReinforce : StatPart_Reinforce
    {
        public ReinforceDef reinforceDef;

        protected override float GetFactor(StatRequest req)
        {
            ThingWithComps thing = req.Thing as ThingWithComps;
            if (thing != null)
            {
                return thing.GetReinforceCustomFactor(reinforceDef);
            }
            else return 1.0f; 

        }


    }
    
}
