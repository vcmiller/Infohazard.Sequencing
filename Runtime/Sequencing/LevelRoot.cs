using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class LevelRoot : MonoBehaviour {
        public static LevelRoot Current { get; private set; }
        
        [SerializeField] private LevelManifestLevelEntry _manifestEntry;
        [SerializeField] private Transform _dynamicObjectRoot;
        [SerializeField] private DebugTeleportPoint[] _debugTeleportPoints;
        
        private Dictionary<int, RegionRoot> _loadedRegions = new Dictionary<int, RegionRoot>();
        
        public int LevelIndex => _manifestEntry.LevelID;
        public LevelManifestLevelEntry ManifestEntry => _manifestEntry;
        public Transform DynamicObjectRoot => _dynamicObjectRoot;
        public IReadOnlyDictionary<int, RegionRoot> LoadedRegions => _loadedRegions;
        public IReadOnlyList<DebugTeleportPoint> TeleportPoints => _debugTeleportPoints;
        public bool Initialized { get; private set; }

        public Action Unloading;

        internal virtual void RegisterRegion(RegionRoot region) {
            _loadedRegions.Add(region.RegionIndex, region);
        }

        internal virtual void DeregisterRegion(RegionRoot region) {
            _loadedRegions.Remove(region.RegionIndex);
        }

        public virtual void Initialize() {
            Unloading?.Invoke();
            
            if (Current) {
                Debug.LogError("Trying to initialize a LevelRoot when one is already active!");
                return;
            }

            Current = this;
            Initialized = true;
        }

        public virtual void Cleanup() {
            Initialized = false;
            if (Current == this) {
                Current = null;
            }
        }

        public virtual bool LoadRegion(LevelManifestRegionEntry regionEntry, SceneGroup group = null) {
            if (regionEntry.AlwaysLoaded) {
                Debug.LogError($"Region {regionEntry.name} is always loaded, and cannot be passed as an argument to LoadRegion.");
                return false;
            }
            AsyncOperation op = SceneLoadingManager.Instance.LoadScene(regionEntry.Scene.Name, true, false, group);
            return op != null;
        }

        public virtual bool UnloadRegion(LevelManifestRegionEntry regionEntry) {
            if (regionEntry.AlwaysLoaded) {
                Debug.LogError($"Region {regionEntry.name} is always loaded, and cannot be passed as an argument to UnloadRegion.");
                return false;
            }
            return SceneLoadingManager.Instance.UnloadScene(regionEntry.Scene.Name);
        }
    }
}