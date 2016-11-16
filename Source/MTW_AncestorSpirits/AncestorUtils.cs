using Verse;
using RimWorld;
using UnityEngine;

namespace MTW_AncestorSpirits
{
    public static class AncestorUtils
    {
        public static readonly Color spiritColor = new Color(0.95f, 0.87f, 0.93f, .2f);

        public const int TicksPerInterval = GenDate.TicksPerHour / 10;
        public const int IntervalsPerDay = GenDate.TicksPerDay / TicksPerInterval;
        public const int IntervalsPerSeason = GenDate.TicksPerSeason / TicksPerInterval;

        public static bool IsIntervalTick()
        {
            return (Find.TickManager.TicksGame % TicksPerInterval == 0);
        }

        public static float DayValueToIntervalValue(float dayValue)
        {
            return dayValue / IntervalsPerDay;
        }

        public static float SeasonValueToIntervalValue(float seasonValue)
        {
            return seasonValue / IntervalsPerSeason;
        }

        public static int DaysToTicks(float days)
        {
            return Mathf.RoundToInt(days * GenDate.TicksPerDay);
        }

        public static int HoursToTicks(float hours)
        {
            return Mathf.RoundToInt(hours * GenDate.TicksPerHour);
        }

        public static long EstStartOfSeasonAt(long ticks)
        {
            var currentDayTicks = (int)(GenDate.CurrentDayPercent * GenDate.TicksPerDay);
            var dayOfSeason = GenDate.DayOfSeasonZeroBasedAt(ticks);
            var currentSeasonDayTicks = DaysToTicks(dayOfSeason);

            return ticks - currentDayTicks - currentSeasonDayTicks;
        }

        public static bool IsAncestor(Pawn p)
        {
            return p.def.defName == "Spirit";
        }

        public static bool SetAncestorGraphics(Pawn pawn)
        {
            if (pawn.Drawer == null || pawn.Drawer.renderer == null || pawn.Drawer.renderer.graphics == null)
            {
                return false;
            }

            if (!pawn.Drawer.renderer.graphics.AllResolved)
            {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }

            if (pawn.Drawer.renderer.graphics.headGraphic == null ||
                pawn.Drawer.renderer.graphics.nakedGraphic == null ||
                pawn.Drawer.renderer.graphics.headGraphic.path == null ||
                pawn.Drawer.renderer.graphics.nakedGraphic.path == null)
            {
                return false;
            }

            Graphic nakedBodyGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.BodyType, ShaderDatabase.CutoutSkin, AncestorUtils.spiritColor);
            Graphic headGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.HeadGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, AncestorUtils.spiritColor);
            pawn.Drawer.renderer.graphics.headGraphic = headGraphic;
            pawn.Drawer.renderer.graphics.nakedGraphic = nakedBodyGraphic;

            return true;
        }

        internal static MapComponent_AncestorTicker GetMapComponent()
        {
            var ticker = Find.Map.GetComponent<MapComponent_AncestorTicker>();
            if (ticker == null)
            {
                Find.Map.components.Add(new MapComponent_AncestorTicker());
                Log.Message("Injected MTW_AncestorSpirits.MapComponent_AncestorTicker!");
                return Find.Map.GetComponent<MapComponent_AncestorTicker>();
            }
            else
            {
                return ticker;
            }
        }
    }
}
