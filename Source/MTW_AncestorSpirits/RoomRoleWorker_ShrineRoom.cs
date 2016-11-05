using Verse;

using System.Linq;

namespace MTW_AncestorSpirits
{
    class RoomRoleWorker_ShrineRoom : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            // I don't know if there should be some "If it's full of joy objects/work benches, *don't* classify it as
            // a Shrine - if you Shrine room wall gets broken down by a bug, this will probably push all the attached
            // rooms into the "Shrine Room" - is that a desired behaviour?

            var shrine = Find.Map.GetComponent<MapComponent_AncestorTicker>().CurrentSpawner;
            if (shrine != null && room.AllContainedThings.Contains<Thing>(shrine))
            {
                return 9999.0f;
            }
            else
            {
                return 0.0f;
            }
        }
    }
}
