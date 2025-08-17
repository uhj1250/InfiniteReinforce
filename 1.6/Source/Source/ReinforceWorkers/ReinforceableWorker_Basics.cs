using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace InfiniteReinforce
{
    public class ReinforceableWorker_OnlyRangedWeapon : ReinforceableWorker
    {
        public override bool IsAppliable(ThingWithComps thing)
        {
            return thing.def.IsRangedWeapon;
        }
    }
    public class ReinforceableWorker_OnlyRangedWeapon_NotTurret : ReinforceableWorker
    {
        public override bool IsAppliable(ThingWithComps thing)
        {
            return thing.def.IsRangedWeapon && !thing.def.weaponTags.Contains("TurretGun");
        }
    }

    public class ReinforceableWorker_OnlyBurstWeapon : ReinforceableWorker
    {
        public override bool IsAppliable(ThingWithComps thing)  
        {
            return thing.def.Verbs.Exists(x => x.burstShotCount > 1);
        }
    }

    public class ReinforceableWorker_OnlyMeleeWeapon : ReinforceableWorker
    {
        public override bool IsAppliable(ThingWithComps thing)
        {
            return thing.def.IsMeleeWeapon;
        }
    }

    public class ReinforceableWorker_IsWeapon : ReinforceableWorker
    {
        public override bool IsAppliable(ThingWithComps thing)
        {
            return thing.def.IsWeapon;
        }
    }
    public class ReinforceableWorker_IsApparel : ReinforceableWorker
    {
        public override bool IsAppliable(ThingWithComps thing)
        {
            return thing.def.IsApparel;
        }
    }

}
