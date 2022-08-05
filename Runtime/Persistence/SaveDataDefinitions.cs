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
using Infohazard.Core;
using UnityEngine;

using Random = System.Random;

namespace Infohazard.Sequencing {
    [Serializable]
    public class PersistedData {
        private bool _initialized;
        public bool Initialized {
            get => _initialized;
            set {
                if (_initialized == value) return;
                _initialized = value;
                NotifyStateChanged();
            }
        }


        [NonSerialized] private bool _subscribed;
        protected bool Subscribed {
            get => _subscribed;
            private set {
                if (value == _subscribed) return;
                _subscribed = value;
                if (value) Subscribe();
                else Unsubscribe();
            }
        }

        private Dictionary<string, object> _customData;
        public IReadOnlyDictionary<string, object> CustomData {
            get => _customData;
            set {
                if (Equals(_customData, value)) return;
                bool b = Subscribed;
                Subscribed = false;
                _customData = value.ToDictionary(pair => pair.Key, pair => pair.Value);
                Subscribed = b;
            }
        }

        [NonSerialized] private Action _stateChanged;
        public bool HasStateChangedListeners => _stateChanged != null;
        public event Action StateChanged {
            add {
                _stateChanged += value;
                Subscribed = HasStateChangedListeners;
            }
            remove {
                _stateChanged -= value;
                Subscribed = HasStateChangedListeners;
            }
        }

        public void NotifyStateChanged() => _stateChanged?.Invoke();

        protected virtual void Subscribe() {
            if (_customData == null) return;
            foreach (object obj in _customData.Values) {
                if (!(obj is PersistedData data)) continue;
                data.StateChanged += CustomData_StateChanged;
            }
        }

        protected virtual void Unsubscribe() {
            if (_customData == null) return;
            foreach (object obj in _customData.Values) {
                if (!(obj is PersistedData data)) continue;
                data.StateChanged -= CustomData_StateChanged;
            }
        }

        public object GetCustomData(string key) {
            _customData ??= new Dictionary<string, object>();
            return _customData.TryGetValue(key, out object value) ? value : null;
        }

        public void SetCustomData(string key, object value) {
            _customData ??= new Dictionary<string, object>();

            if (_customData.TryGetValue(key, out object prevValue) && prevValue is PersistedData prevData) {
                prevData.StateChanged -= CustomData_StateChanged;
            }
            
            _customData[key] = value;
            
            if (value is PersistedData data) {
                data.StateChanged += CustomData_StateChanged;
            }
        }

        private void CustomData_StateChanged() {
            NotifyStateChanged();
        }
    }
    
    [Serializable]
    public class GlobalSaveData : PersistedData {
        private string _mostRecentProfile;

        public string MostRecentProfile {
            get => _mostRecentProfile;
            set {
                if (_mostRecentProfile == value) return;
                _mostRecentProfile = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class ProfileSaveData : PersistedData {
        private string _profileName;
        private string _mostRecentState;

        public string ProfileName {
            get => _profileName;
            set {
                if (_profileName == value) return;
                _profileName = value;
                NotifyStateChanged();
            }
        }

        public string MostRecentState {
            get => _mostRecentState;
            set {
                if (_mostRecentState == value) return;
                _mostRecentState = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class StateSaveData : PersistedData {
        private string _stateName;
        private string _currentScene;
        private DateTime _saveTime;

        public string StateName {
            get => _stateName;
            set {
                if (_stateName == value) return;
                _stateName = value;
                NotifyStateChanged();
            }
        }

        public string CurrentScene {
            get => _currentScene;
            set {
                if (_currentScene == value) return;
                _currentScene = value;
                NotifyStateChanged();
            }
        }

        public DateTime SaveTime {
            get => _saveTime;
            set {
                if (_saveTime == value) return;
                _saveTime = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class ObjectContainerSaveData : PersistedData {
        private PersistedObjectCollection _objects = new PersistedObjectCollection();
        public PersistedObjectCollection Objects => _objects;

        protected override void Subscribe() {
            base.Subscribe();
            Objects.StateChanged += Objects_StateChanged;
        }

        protected override void Unsubscribe() {
            base.Subscribe();
            Objects.StateChanged -= Objects_StateChanged;
        }

        private void Objects_StateChanged() {
            NotifyStateChanged();
        }
    }

    [Serializable]
    public class LevelSaveData : ObjectContainerSaveData {
        private int _levelIndex;

        public int LevelIndex {
            get => _levelIndex;
            set {
                if (_levelIndex == value) return;
                _levelIndex = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class RegionSaveData : ObjectContainerSaveData {
        private int _regionIndex;
        private bool _loaded;
        
        public int RegionIndex {
            get => _regionIndex;
            set {
                if (_regionIndex == value) return;
                _regionIndex = value;
                NotifyStateChanged();
            }
        }

        public bool Loaded {
            get => _loaded;
            set {
                if (_loaded == value) return;
                _loaded = value;
                NotifyStateChanged();
            }
        }
    }
    
    [Serializable]
    public class PersistedObjectCollection : PersistedData {
        private Dictionary<ulong, ObjectSaveData> _objects = new Dictionary<ulong, ObjectSaveData>();

        private static readonly Random LongRandomizer = new Random();

        public IEnumerable<ObjectSaveData> Objects => _objects.Values;


        private ulong GetUnusedInstanceID() {
            ulong id;
            do {
                id = LongRandomizer.NextUlong();
            } while (id == 0 || _objects.ContainsKey(id));

            return id;
        }

        public ObjectSaveData RegisterExistingObject(ulong instanceID, bool isDynamic) {
            if (!_objects.TryGetValue(instanceID, out ObjectSaveData data)) {
                data = new ObjectSaveData(instanceID, isDynamic, -1);
                _objects[instanceID] = data;
                Subscribe(data);
                NotifyStateChanged();
            }

            return data;
        }

        public ObjectSaveData RegisterNewDynamicObject(int prefabID, ulong? instanceID = null) {
            if (!instanceID.HasValue || _objects.ContainsKey(instanceID.Value)) {
                instanceID = GetUnusedInstanceID();
            }
            
            ObjectSaveData data = new ObjectSaveData(instanceID.Value, true, prefabID);
            _objects[instanceID.Value] = data;
            Subscribe(data);
            NotifyStateChanged();
            return data;
        }

        public ObjectSaveData ConvertStaticObjectToDynamic(ulong instanceID, int prefabID) {
            ObjectSaveData staticData = RegisterExistingObject(instanceID, false);
            staticData.Destroyed = true;
            ulong newInstanceID = GetUnusedInstanceID();
            ObjectSaveData instanceData = new ObjectSaveData(newInstanceID, true, prefabID) {
                CustomData = staticData.CustomData,
                Destroyed = false,
                Initialized = true,
            };
            _objects[newInstanceID] = instanceData;
            Subscribe(instanceData);
            NotifyStateChanged();
            return instanceData;
        }

        public void RegisterObjectDestroyed(ulong instanceID) {
            if (!_objects.TryGetValue(instanceID, out ObjectSaveData data)) {
                Debug.LogError($"Invalid instanceID {instanceID} passed to RegisterObjectDestroyed.");
                return;
            }

            if (data.IsDynamicInstance) {
                _objects.Remove(instanceID);
                Unsubscribe(data);
            } else {
                data.Destroyed = true;
            }
            NotifyStateChanged();
        }

        protected override void Subscribe() {
            base.Subscribe();
            foreach (var data in _objects.Values) {
                Subscribe(data);
            }
        }

        protected override void Unsubscribe() {
            base.Unsubscribe();
            foreach (var data in _objects.Values) {
                Unsubscribe(data);
            }
        }

        protected void Subscribe(ObjectSaveData data) {
            if (Subscribed) data.StateChanged += ObjectData_StateChanged;
        }

        private void Unsubscribe(ObjectSaveData data) {
            data.StateChanged -= ObjectData_StateChanged;
        }

        private void ObjectData_StateChanged() {
            NotifyStateChanged();
        }
    }

    [Serializable]
    public class ObjectSaveData : PersistedData {
        private ulong _instanceID;
        private bool _isDynamicInstance;
        private int _dynamicPrefabID;
        private bool _destroyed;

        public ulong InstanceID => _instanceID;

        public bool IsDynamicInstance => _isDynamicInstance;

        public int DynamicPrefabID => _dynamicPrefabID;
        
        public ObjectSaveData() {}

        public ObjectSaveData(ulong instanceID, bool isDynamicInstance, int dynamicPrefabID) {
            _instanceID = instanceID;
            _isDynamicInstance = isDynamicInstance;
            _dynamicPrefabID = dynamicPrefabID;
        }

        public bool Destroyed {
            get => _destroyed;
            set {
                if (_destroyed == value) return;
                _destroyed = value;
                NotifyStateChanged();
            }
        }
    }
}
