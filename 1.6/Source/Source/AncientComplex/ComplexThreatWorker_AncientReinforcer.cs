using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
	public class ComplexThreatWorker_AncientReinforcer : ComplexThreatWorker
	{

		protected override void ResolveInt(ComplexResolveParams parms, ref float threatPointsUsed, List<Thing> spawnedThings)
		{
			
			ComplexUtility.TryFindRandomSpawnCell(ReinforceDefOf.AncientReinforcer, parms.room, parms.map, out IntVec3 loc, 1, null);
			Building_Reinforcer ancient_reinforcer = (Building_Reinforcer)GenSpawn.Spawn(ReinforceDefOf.AncientReinforcer, loc, parms.map, WipeMode.Vanish);
			ancient_reinforcer.SetFuelRandom();

			spawnedThings.Add(ancient_reinforcer);
			parms.spawnedThings.Add(ancient_reinforcer);
		}
	}
}