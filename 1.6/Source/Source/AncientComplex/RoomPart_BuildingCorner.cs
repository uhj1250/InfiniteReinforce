using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace InfiniteReinforce
{
    public class RoomPart_BuildingCorner : RoomPartWorker
    {
        //public new RoomPart_ThingDef def => base.def as RoomPart_ThingDef;

        public RoomPart_BuildingCorner(RoomPartDef def) : base(def)
        {
        }

        public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
        {
            List<IntVec3> list = room.rects.SelectMany(x => x.ContractedBy(1).Corners).ToList();

            if (list.Count == 4)
            {
                var def = this.def as RoomPart_ThingDef;
                var Size = def.thingDef.Size;
                int corner = Rand.Range(0, 4);
                IntVec3 loc = list[corner];
                switch (corner)
                {
                    case 0:
                        loc = new IntVec3(loc.x + Size.x / 2, 0, loc.z + Size.z / 2);
                        break;
                    case 1:
                        loc = new IntVec3(loc.x + Size.x / 2, 0, loc.z - Size.z / 2);
                        break;
                    case 2:
                        loc = new IntVec3(loc.x - Size.x / 2, 0, loc.z - Size.z / 2);
                        break;
                    case 3:
                        loc = new IntVec3(loc.x - Size.x / 2, 0, loc.z + Size.z / 2);
                        break;
                }

                Thing thing = ThingMaker.MakeThing(def.thingDef);
                thing.SetFactionDirect(faction);
                
                GenSpawn.Spawn(thing, loc, map, new Rot4(corner));
            }

            //E S
            //N W

            //1 2
            //0 3

            //(+1,-1) (-1,-1) 
            //(+1,+1) (-1,+1)
        }


    }
}
