using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;


namespace InfiniteReinforce
{
    public static class IconCache
    {
        public static readonly Texture2D Upgrade = ContentFinder<Texture2D>.Get("UI/Upgrade");
        public static readonly Texture2D EquipmentReinforce = ContentFinder<Texture2D>.Get("UI/EquipmentReinforce");
    }
}
