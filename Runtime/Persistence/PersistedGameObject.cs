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

using System.Collections.Generic;
using System.Linq;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Infohazard.Sequencing {
    [ExecuteAlways]
    public class PersistedGameObject : MonoBehaviour, IPersistedInstance {
        [SerializeField] private int _dynamicPrefabID;
        [SerializeField] private ulong _instanceID;
        [SerializeField] private bool _immediatelyConvert = false;

        private bool _hasCheckedId;

        public ObjectSaveData SaveData { get; private set; }
        public PersistedLevelRoot Level { get; private set; }
        public PersistedRegionRoot Region { get; private set; }

        public int DynamicPrefabID => _dynamicPrefabID;

        public bool Initialized { get; private set; }
        
        public bool Initializing { get; private set; }

        public bool IsDynamicInstance { get; private set; }

        public ulong InstanceID => _instanceID;

        private IPersistedComponent[] _components;
        private bool _needsToInitialize = false;

        private static Dictionary<ulong, PersistedGameObject> _objects = new Dictionary<ulong, PersistedGameObject>();
        public static IReadOnlyDictionary<ulong, PersistedGameObject> Objects => _objects;

        private void Awake() {
            if (!Application.isPlaying) return;
            _components = GetComponentsInChildren<IPersistedComponent>();
            OnSpawned();
        }

        private void OnDestroy() {
            OnDespawned();
        }

        private PersistedObjectCollection Container {
            get {
                if (Region) return Region.SaveData.Objects;
                if (Level) return Level.SaveData.Objects;
                return null;
            }
        }

        private void OnSpawned() {
            if (!Application.isPlaying) return;
            _needsToInitialize = true;
            IsDynamicInstance = false;
        }

        private void OnDespawned() {
            if (!Application.isPlaying) return;
            _needsToInitialize = false;
            IsDynamicInstance = false;
            Initialized = false;
            if (Level) Level.WillSave -= LevelRoot_WillSave;
            if (Region) Region.Unloading -= RegionRoot_Unloading;
            _objects.Remove(_instanceID);
        }

#if UNITY_EDITOR
        private void UpdateUniqueID() {
            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(this);
            ulong newID = id.targetObjectId ^ id.targetPrefabId;
            _hasCheckedId = true;
            if (newID == _instanceID) return;
            Undo.RecordObject(this, "Set Unique ID");
            _instanceID = newID;
        }

        private bool IsPrefab() {
            var stage = PrefabStageUtility.GetPrefabStage(gameObject);
            var type = PrefabUtility.GetPrefabAssetType(gameObject);
            var status = PrefabUtility.GetPrefabInstanceStatus(gameObject);


            return (stage != null && transform.parent == null) ||
                   ((type == PrefabAssetType.Regular || type == PrefabAssetType.Variant) &&
                    status == PrefabInstanceStatus.NotAPrefab);
        }
#endif

        private void Update() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                if ((_instanceID == 0 || !_hasCheckedId) && !IsPrefab()) {
                    UpdateUniqueID();
                }
                return;
            }
#endif

            if (_needsToInitialize) {
                CheckDynamicRegister();
            }
        }

        private void CheckDynamicRegister() {
            var level = PersistedLevelRoot.Current;
            if (level == null || !level.ObjectsLoaded) return;

            SceneLoadingManager.Instance.GetSceneLoadedState(gameObject.scene.name, out _, out RegionRoot region);
            if (region is PersistedRegionRoot {ObjectsLoaded: false}) return;

            Initialize();
            InitializeComponents();
            PostLoad();
        }

        public void SetupDynamicInstance(ulong instanceID) {
            if (!_needsToInitialize) {
                Debug.LogError("SetupDynamicInstance can only be called before a spawned object is initialized.");
            }

            _instanceID = instanceID;
            IsDynamicInstance = true;
        }

        private void Initialize() {
            if (!_needsToInitialize) return;
            _needsToInitialize = false;
            Initializing = true;

            Level = PersistedLevelRoot.Current;
            if (Level) Level.WillSave += LevelRoot_WillSave;

            Scene scene = gameObject.scene;
            SceneLoadingManager.Instance.GetSceneLoadedState(scene.name, out _, out RegionRoot region);

            if (region is PersistedRegionRoot pRegion) Region = pRegion;
            if (Region) Region.Unloading += RegionRoot_Unloading;

            PersistedObjectCollection container = Container;
            SaveData = container != null
                ? _instanceID == 0
                    ? container.RegisterNewDynamicObject(_dynamicPrefabID)
                    : container.RegisterExistingObject(_instanceID, IsDynamicInstance)
                : new ObjectSaveData(_instanceID, false, -1);
            _instanceID = SaveData.InstanceID;
            if (_objects.ContainsKey(_instanceID)) {
                Debug.LogError($"Object with instanceID already exists, trying to replace with {name}.", this);
            }
            _objects[_instanceID] = this;

            if (SaveData.Destroyed) {
                Spawnable.Despawn(gameObject);
            } else {
                if (!IsDynamicInstance && _immediatelyConvert) {
                    ConvertToDynamicInstance();
                }
            }
        }

        private void InitializeComponents() {
            foreach (IPersistedComponent component in _components) {
                component.Initialize(this, SaveData, new ComponentID(transform, (Component) component).ToString());
            }
        }

        private void PostLoad() {
            foreach (IPersistedComponent component in _components) {
                component.PostLoad();
            }

            Initialized = true;
            Initializing = false;
        }

        private void ConvertToDynamicInstance() {
            if (IsDynamicInstance) {
                Debug.LogError($"Trying to convert object {name} with ID {_instanceID} to dynamic, but it already is.");
                return;
            }

            PersistedObjectCollection container = Container;
            if (container == null) return;
            SaveData = container.ConvertStaticObjectToDynamic(_instanceID, _dynamicPrefabID);

            _objects.Remove(_instanceID);
            _instanceID = SaveData.InstanceID;
            _objects.Add(_instanceID, this);
        }

        public void RegisterDestroyed() {
            Container?.RegisterObjectDestroyed(_instanceID);
        }

        public void TransitionToRegion(PersistedRegionRoot newRoot) {
            if (DynamicPrefabID == 0) {
                Debug.LogError($"Trying to transition object {name}, which doesn't have a dynamic prefab ID.");
                return;
            }
            WriteState();
            PersistedObjectCollection container = Container;
            if (container == null) return;
            container.RegisterObjectDestroyed(_instanceID);
            Region = newRoot;
            
            // Update container after changing region.
            container = Container;

            var data = container.RegisterNewDynamicObject(DynamicPrefabID, _instanceID);
            data.CustomData = SaveData.CustomData;
            data.Destroyed = false;
            data.Initialized = true;

            SaveData = data;
            IsDynamicInstance = true;
            if (SaveData.InstanceID != _instanceID) {
                _objects.Remove(_instanceID);
                _instanceID = SaveData.InstanceID;
                _objects.Add(_instanceID, this);
            }
            InitializeComponents();
        }

        private void LevelRoot_WillSave() {
            WriteState();
        }

        private void RegionRoot_Unloading() {
            WriteState();
        }

        public void WriteState() {
            foreach (IPersistedComponent component in _components) {
                component.WriteState();
            }
        }

        public static bool TryGetObjectWithID(ulong id, out PersistedGameObject obj) {
            return _objects.TryGetValue(id, out obj);
        }

        public static PersistedGameObject GetObjectWithID(ulong id) {
            return _objects.TryGetValue(id, out PersistedGameObject obj) ? obj : null;
        }

        public static IEnumerable<PersistedGameObject> GetObjectsWithIDs(IEnumerable<ulong> ids) {
            return ids?.Select(GetObjectWithID).Where(obj => obj) ?? Enumerable.Empty<PersistedGameObject>();
        }

        public static void LoadDynamicObjects(PersistedObjectCollection data, Scene scene, Transform parent) {
            foreach (var objectData in data.Objects.ToList()) {
                if (!objectData.IsDynamicInstance) continue;
                string path = DynamicObjectManifest.Instance.GetEntry(objectData.DynamicPrefabID)?.ResourcePath;
                if (string.IsNullOrEmpty(path)) {
                    Debug.LogError($"Could not find prefab path for prefab with ID {objectData.DynamicPrefabID}.");
                    continue;
                }

                PersistedGameObject prefab = Resources.Load<PersistedGameObject>(path);
                if (prefab == null) {
                    Debug.LogError($"Could not load prefab at path {path}.");
                    continue;
                }

                Spawnable.Spawn(prefab, parent: parent, persistedInstanceID: objectData.InstanceID, scene: scene);
            }
        }

        public static void InitializeGameObjects(List<PersistedGameObject> list) {
            foreach (PersistedGameObject obj in list) {
                obj.Initialize();
            }

            foreach (PersistedGameObject obj in list) {
                obj.InitializeComponents();
            }

            foreach (PersistedGameObject obj in list) {
                obj.PostLoad();
            }
        }

        private static Stack<Transform> _transforms = new Stack<Transform>();
        private static List<GameObject> _rootGameObjects = new List<GameObject>();
        public static void CollectGameObjects(Scene scene, List<PersistedGameObject> list) {
            _transforms.Clear();
            _rootGameObjects.Clear();
            scene.GetRootGameObjects(_rootGameObjects);
            foreach (GameObject rootGameObject in _rootGameObjects) {
                _transforms.Push(rootGameObject.transform);
            }

            while (_transforms.Count > 0) {
                Transform current = _transforms.Pop();
                if (current.gameObject.activeSelf == false) continue;
                if (current.TryGetComponent(out PersistedGameObject obj)) {
                    list.Add(obj);
                    continue; // We don't support nested PersistedGameObjects.
                }

                for (int i = 0; i < current.childCount; i++) {
                    _transforms.Push(current.GetChild(i));
                }
            }
        }
    }
}
