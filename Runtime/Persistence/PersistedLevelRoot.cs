using System;
using System.Collections.Generic;
using System.Linq;

namespace Infohazard.Sequencing.Runtime {
    public class PersistedLevelRoot : LevelRoot {
        public LevelSaveData SaveData { get; private set; }
        public new static PersistedLevelRoot Current => LevelRoot.Current as PersistedLevelRoot;
        public bool ObjectsLoaded { get; private set; }
        
        private bool _dirty;

        private Dictionary<int, LoadedRegionInfo> _loadedRegionData = new Dictionary<int, LoadedRegionInfo>();

        public event Action WillSave;

        public override void Initialize() {
            base.Initialize();
            SaveData = PersistenceManager.Instance.GetLevelData(ManifestEntry.LevelID);
            SaveData.StateChanged += SaveData_StateChanged;
        }

        public virtual void LoadObjects() {
            List<PersistedRegionRoot> persistedRegions = LoadedRegions.Values.OfType<PersistedRegionRoot>().ToList();

            List<PersistedGameObject> gameObjects = new List<PersistedGameObject>();
            
            PersistedGameObject.LoadDynamicObjects(SaveData.Objects, gameObject.scene, DynamicObjectRoot);
            PersistedGameObject.CollectGameObjects(gameObject.scene, gameObjects);
            
            foreach (PersistedRegionRoot regionRoot in persistedRegions) {
                regionRoot.LoadDynamicObjects();
                PersistedGameObject.CollectGameObjects(regionRoot.gameObject.scene, gameObjects);
            }
            
            PersistedGameObject.InitializeGameObjects(gameObjects);

            ObjectsLoaded = true;
        }

        public override void Cleanup() {
            SaveData.StateChanged -= SaveData_StateChanged;
            base.Cleanup();
        }

        internal override void RegisterRegion(RegionRoot region) {
            base.RegisterRegion(region);
            GetOrLoadRegionData(region.ManifestEntry).Loaded = true;

            if (ObjectsLoaded && region is PersistedRegionRoot root) {
                root.LoadDynamicObjects();
                List<PersistedGameObject> gameObjects = new List<PersistedGameObject>();
                PersistedGameObject.CollectGameObjects(root.gameObject.scene, gameObjects);
                PersistedGameObject.InitializeGameObjects(gameObjects);
            }
        }

        internal override void DeregisterRegion(RegionRoot region) {
            GetOrLoadRegionData(region.ManifestEntry).Loaded = false;
            base.DeregisterRegion(region);
        }

        public override bool UnloadRegion(LevelManifestRegionEntry regionEntry) {
            WillSave?.Invoke();
            return base.UnloadRegion(regionEntry);
        }

        private void SaveData_StateChanged() {
            _dirty = true;
            SaveData.StateChanged -= SaveData_StateChanged;
        }

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
