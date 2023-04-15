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
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class PersistedLevelRoot : LevelRoot {
        public LevelSaveData SaveData { get; private set; }
        public new static PersistedLevelRoot Current => LevelRoot.Current as PersistedLevelRoot;
        public bool ObjectsLoading { get; private set; }
        public bool ObjectsLoaded { get; private set; }
        
        private bool _dirty;
        private bool _delayLoadingRegions;

        private Dictionary<int, LoadedRegionInfo> _loadedRegionData = new Dictionary<int, LoadedRegionInfo>();

        public event Action WillSave;

        public override UniTask Initialize() {
            UniTask task = base.Initialize();
            SaveData = PersistenceManager.Instance.GetLevelData(ManifestEntry.LevelID);
            SaveData.StateChanged += SaveData_StateChanged;
            return task;
        }

        public virtual async UniTask LoadObjects() {
            if (ObjectsLoading) {
                Debug.LogError("Simultaneous calls to LoadObjects().");
            }
            
            ObjectsLoading = true;
            
            List<PersistedRegionRoot> persistedRegions = LoadedRegions.Values.OfType<PersistedRegionRoot>().ToList();
            _delayLoadingRegions = false;

            List<PersistedGameObject> gameObjects = new List<PersistedGameObject>();

            if (!ObjectsLoaded) {
                await PersistedGameObject.LoadDynamicObjects(SaveData.Objects, gameObject.scene, DynamicObjectRoot);
                PersistedGameObject.CollectGameObjects(gameObject.scene, gameObjects);
            }
            
            foreach (PersistedRegionRoot regionRoot in persistedRegions) {
                if (regionRoot.ObjectsLoaded) continue;
                
                await regionRoot.LoadDynamicObjects();
                PersistedGameObject.CollectGameObjects(regionRoot.gameObject.scene, gameObjects);
            }
            
            await PersistedGameObject.InitializeGameObjects(gameObjects);

            ObjectsLoading = false;
            ObjectsLoaded = true;
            
            foreach (PersistedRegionRoot regionRoot in persistedRegions) {
                if (regionRoot.ObjectsLoaded) continue;
                regionRoot.FinishedLoading();
            }
        }

        public override void Cleanup() {
            SaveData.StateChanged -= SaveData_StateChanged;
            base.Cleanup();
        }

        internal override async UniTask RegisterRegion(RegionRoot region) {
            if (ObjectsLoading) {
                await UniTask.WaitUntil(() => !ObjectsLoading);
            }
            
            await base.RegisterRegion(region);
            GetOrLoadRegionData(region.ManifestEntry).Loaded = true;

            if (ObjectsLoaded && !_delayLoadingRegions && region is PersistedRegionRoot root) {
                await root.LoadDynamicObjects();
                List<PersistedGameObject> gameObjects = new List<PersistedGameObject>();
                PersistedGameObject.CollectGameObjects(root.gameObject.scene, gameObjects);
                await PersistedGameObject.InitializeGameObjects(gameObjects);
                root.FinishedLoading();
            }
        }

        internal override void DeregisterRegion(RegionRoot region) {
            GetOrLoadRegionData(region.ManifestEntry).Loaded = false;
            base.DeregisterRegion(region);
        }

        public override IProgressSource UnloadRegion(LevelManifestRegionEntry regionEntry) {
            WillSave?.Invoke();
            return base.UnloadRegion(regionEntry);
        }

        private void SaveData_StateChanged() {
            _dirty = true;
            SaveData.StateChanged -= SaveData_StateChanged;
        }

        public void DelayRegionLoading() => _delayLoadingRegions = true;

        public RegionSaveData GetOrLoadRegionData(LevelManifestRegionEntry region) {
            if (!_loadedRegionData.TryGetValue(region.RegionID, out LoadedRegionInfo info)) {
                info = new LoadedRegionInfo {
                    Data = PersistenceManager.Instance.GetRegionData(LevelIndex, region.RegionID),
                };
                
                info.Data.StateChanged += info.SetDirty;
                _loadedRegionData[region.RegionID] = info;
            }

            return info.Data;
        }

        public void Save() {
            WillSave?.Invoke();
            foreach (int regionID in _loadedRegionData.Keys.ToList()) {
                LoadedRegionInfo info = _loadedRegionData[regionID];
                
                if (info.Dirty) {
                    info.Dirty = false;
                    info.Data.StateChanged += info.SetDirty;
                    PersistenceManager.Instance.SetRegionData(LevelIndex, regionID, info.Data);
                }

                if (!info.Data.Loaded) {
                    _loadedRegionData.Remove(regionID);
                }
            }
            
            if (_dirty) {
                _dirty = false;
                SaveData.StateChanged += SaveData_StateChanged;
                PersistenceManager.Instance.SetLevelData(LevelIndex, SaveData);
            }
        }
        
        private class LoadedRegionInfo {
            public RegionSaveData Data { get; set; }
            public bool Dirty { get; set; }

            public void SetDirty() {
                Dirty = true;
                Data.StateChanged -= SetDirty;
            }
        }
    }
}
