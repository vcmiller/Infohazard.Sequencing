using System;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class RegionRoot : MonoBehaviour {
        [SerializeField] private LevelManifestRegionEntry _manifestEntry;
        [SerializeField] private Transform _dynamicObjectRoot;

        public LevelManifestRegionEntry ManifestEntry => _manifestEntry;
        
        public Transform DynamicObjectRoot => _dynamicObjectRoot;

        public int RegionIndex => _manifestEntry.RegionID;
        
        public bool Initialized { get; private set; }

        public event Action Unloading;
        
        public virtual void Initialize() {
            LevelRoot.Current.RegisterRegion(this);
            Initialized = true;
        }

        public virtual void Cleanup() {
            Unloading?.Invoke();
            Initialized = false;
            // It's ok if the level is unloaded before its regions (as long as it's in the same frame)
            if (LevelRoot.Current) {
                LevelRoot.Current.DeregisterRegion(this);
            }
        }
    }
}