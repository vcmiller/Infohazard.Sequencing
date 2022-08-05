// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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