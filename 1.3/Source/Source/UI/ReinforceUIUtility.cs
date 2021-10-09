using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;


namespace InfiniteReinforce.UI
{
    public static class ReinforceUIUtility
    {
        public static void DrawBinkTexture(this Rect rect, Color color1, Color Color2)
        {
            Color color = Color.Lerp(color1, Color2, 0.5f + Mathf.Cos(Time.unscaledTime*3f)/2f );
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = Color.white;
        }


    }
}
