using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System.Text;

namespace MTW_AncestorSpirits
{
    public class Building_Shrine : Building
    {
        public override void SpawnSetup()
        {
            Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_SpawnerCreated(this);
            base.SpawnSetup();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Find.Map.GetComponent<MapComponent_AncestorTicker>().Notify_SpawnerDestroyed(this);
            base.Destroy(mode);
        }

        public override string GetInspectString()
        {
            int magic = Find.Map.GetComponent<MapComponent_AncestorTicker>().CurrentMagic;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Magic: " + magic);
            builder.Append(base.GetInspectString());

            return builder.ToString();
        }
    }
}
