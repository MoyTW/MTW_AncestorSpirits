using System;
using Verse;
using UnityEngine;



namespace MTW_AncestorSpirits
{
    // Copied from RT's Mad Skills    
    // This code is mostly borrowed from Pawn State Icons mod by Dan Sadler, which has open source and no license I could find, so...
    [StaticConstructorOnStartup]
    public class MapComponentInjectorBehavior : MonoBehaviour
    {
        public static readonly string mapComponentName = "MTW_AncestorSpirits.MapComponent_AncestorTicker";       // Ditto.
        private static readonly MapComponent_AncestorTicker mapComponent = new MapComponent_AncestorTicker();       // Ditto.

        #region No editing required
        protected bool reinjectNeeded = false;

        private bool MapComponentsExist()
        {
            try
            {
                var canInject = Current.Game != null && Find.Map != null && Find.Map.components != null;
                Log.Message("MTW_AncestorSpirits.MapComponentInjectorBehavior.MapComponentsExist: canInject=" +
                    canInject);
                return canInject;
            }
            catch (Exception e)
            {
                Log.Message("MTW_AncestorSpirits' MapComponentInject could not find the map to inject!");
                Log.Notify_Exception(e);
                return false;
            }
        }

        public void FixedUpdate()
        {
            if (reinjectNeeded && this.MapComponentsExist())
            {
                reinjectNeeded = false;

                if (Find.Map.components.FindAll(x => x.GetType().ToString() == mapComponentName).Count != 0)
                {
                    Log.Message("MTW_AncestorSpirits.MapComponentInjector: map already has " +
                        mapComponentName + ".");
                    //Destroy(gameObject);
                }
                else
                {
                    Log.Message("MTW_AncestorSpirits.MapComponentInjector: adding " + mapComponentName +
                        "...");
                    Find.Map.components.Add(mapComponent);
                    Log.Message("MTW_AncestorSpirits.MapComponentInjector: success!");
                    //Destroy(gameObject);
                }
            }
        }

        public void Start()
        {
            Log.Message("MTW_AncestorSpirits: Level was loaded; attempting to reinject!");
            reinjectNeeded = true;
        }
    }

    class MapComponentInjector : ITab
    {
        protected UnityEngine.GameObject initializer;

        public MapComponentInjector()
        {
            Log.Message("MapComponentInjector: initializing for " + MapComponentInjectorBehavior.mapComponentName);
            initializer = new UnityEngine.GameObject("MapComponentInjector");
            initializer.AddComponent<MapComponentInjectorBehavior>();
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)initializer);
        }

        protected override void FillTab()
        {

        }
    }
}
#endregion