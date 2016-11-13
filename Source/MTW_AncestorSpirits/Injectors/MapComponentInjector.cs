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
        protected float reinjectTime = 0;

        public void OnLevelWasLoaded(int level)
        {
            Log.Message("Level was loaded; attempting to reinject!");
            reinjectNeeded = true;
            if (level >= 0)
            {
                reinjectTime = 1;
            }
            else
            {
                reinjectTime = 0;
            }
        }

        public void FixedUpdate()
        {
            if (reinjectNeeded)
            {
                reinjectTime -= Time.fixedDeltaTime;
                if (reinjectTime <= 0)
                {
                    reinjectNeeded = false;
                    reinjectTime = 0;
                    if (Current.Game != null && Find.Map != null && Find.Map.components != null)
                    {
                        if (Find.Map.components.FindAll(x => x.GetType().ToString() == mapComponentName).Count != 0)
                        {
                            Log.Message("MapComponentInjector: map already has " + mapComponentName + ".");
                            //Destroy(gameObject);
                        }
                        else
                        {
                            Log.Message("MapComponentInjector: adding " + mapComponentName + "...");
                            Find.Map.components.Add(mapComponent);
                            Log.Message("MapComponentInjector: success!");
                            //Destroy(gameObject);
                        }
                    }
                }
            }
        }

        public void Start()
        {
            OnLevelWasLoaded(-1);
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