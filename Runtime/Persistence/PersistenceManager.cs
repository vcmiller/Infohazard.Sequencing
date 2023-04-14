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

namespace Infohazard.Sequencing {
    public class PersistenceManager : Singleton<PersistenceManager> {
        [SerializeField] private Serializer _serializer;
        [SerializeField] private SaveDataHandler _dataHandler;

        private GlobalSaveData _loadedGlobalData = null;
        private bool _loadedGlobalDataDirty = false;
        public event Action GlobalDataLoaded;
        public GlobalSaveData LoadedGlobalData {
            get => _loadedGlobalData;
            private set {
                if (_loadedGlobalData == value) return;
                
                if (_loadedGlobalData != null) _loadedGlobalData.StateChanged -= LoadedGlobalData_StateChanged;
                _loadedGlobalData = value;
                if (_loadedGlobalData != null) _loadedGlobalData.StateChanged += LoadedGlobalData_StateChanged;
                
                _loadedGlobalDataDirty = false;
                GlobalDataLoaded?.Invoke();
            }
        }


        private ProfileSaveData _loadedProfileData = null;
        private bool _loadedProfileDataDirty = false;
        public event Action ProfileDataLoaded;
        public ProfileSaveData LoadedProfileData {
            get => _loadedProfileData;
            private set {
                if (_loadedProfileData == value) return;

                if (_loadedProfileData != null) _loadedProfileData.StateChanged -= LoadedProfileData_StateChanged;
                _loadedProfileData = value;
                if (_loadedProfileData != null) _loadedProfileData.StateChanged += LoadedProfileData_StateChanged;

                _loadedProfileDataDirty = false;
                ProfileDataLoaded?.Invoke();
            }
        }

        private StateSaveData _loadedStateData = null;
        private bool _loadedStateDataDirty = false;
        public event Action StateDataLoaded;
        public StateSaveData LoadedStateData {
            get => _loadedStateData;
            private set {
                if (_loadedStateData == value) return;

                if (_loadedStateData != null) _loadedStateData.StateChanged -= LoadedStateData_StateChanged;
                _loadedStateData = value;
                if (_loadedStateData != null) _loadedStateData.StateChanged += LoadedStateData_StateChanged;

                _loadedStateDataDirty = false;
                StateDataLoaded?.Invoke();
            }
        }

        private void LoadedGlobalData_StateChanged() {
            _loadedGlobalDataDirty = true;
        }

        private void LoadedProfileData_StateChanged() {
            _loadedProfileDataDirty = true;
        }

        private void LoadedStateData_StateChanged() {
            _loadedStateDataDirty = true;
        }
        
        public void LoadGlobalData() {
            LoadedGlobalData = _dataHandler.GetGlobalSaveData(_serializer);
            LoadedGlobalData.Initialized = true;
        }

        public void UnloadGlobalData() {
            LoadedGlobalData = null;
            _loadedGlobalDataDirty = false;
            LoadedProfileData = null;
            _loadedProfileDataDirty = false;
            LoadedStateData = null;
            _loadedStateDataDirty = false;
        }

        public void SaveGlobalData() {
            if (!_loadedGlobalDataDirty) return;
            _dataHandler.SetGlobalSaveData(_serializer, _loadedGlobalData);
            _loadedGlobalDataDirty = false;
        }

        public void DeleteGlobalData() {
            _dataHandler.ClearGlobalSaveData();
            UnloadGlobalData();
        }

        public IEnumerable<string> GetAvailableProfiles() {
            return _dataHandler.GetAvailableProfiles();
        }

        public ProfileSaveData GetProfileData(string profile) {
            if (LoadedGlobalData == null) {
                Debug.LogError("Trying to load profile data when global data is not loaded.");
                return null;
            }

            return _dataHandler.GetProfileSaveData(_serializer, profile);
        }

        public void SetProfileData(string profile, ProfileSaveData data) {
            if (LoadedGlobalData == null) {
                Debug.LogError("Trying to save profile data when global data is not loaded.");
                return;
            }

            _dataHandler.SetProfileSaveData(_serializer, profile, data);
        }
        
        public void LoadProfileData(string profile) {
            ProfileSaveData data = GetProfileData(profile);
            if (data == null) return;
            
            LoadedProfileData = _dataHandler.GetProfileSaveData(_serializer, profile);
            LoadedProfileData.Initialized = true;
        }

        public void UnloadProfileData() {
            LoadedProfileData = null;
            _loadedProfileDataDirty = false;
            LoadedStateData = null;
            _loadedStateDataDirty = false;
        }

        public void SaveProfileData() {
            if (!_loadedProfileDataDirty) return;
            _dataHandler.SetProfileSaveData(_serializer, LoadedProfileData.ProfileName, LoadedProfileData);
            _loadedProfileDataDirty = false;
            LoadedGlobalData.MostRecentProfile = LoadedProfileData.ProfileName;
        }

        public void DeleteProfileData(string profile = null) {
            if (string.IsNullOrEmpty(profile)) {
                if (LoadedProfileData != null) {
                    profile = LoadedProfileData.ProfileName;
                } else {
                    Debug.LogError("Trying to delete current profile data when no profile data is loaded.");
                    return;
                }
            }

            bool deletingCurrentProfile = LoadedProfileData != null && LoadedProfileData.ProfileName == profile;
            _dataHandler.ClearProfileSaveData(profile);
            if (deletingCurrentProfile) {
                UnloadProfileData();
            }
        }

        public IEnumerable<string> GetAvailableStates(string profile = null) {
            if (string.IsNullOrEmpty(profile)) {
                if (LoadedProfileData != null) {
                    profile = LoadedProfileData.ProfileName;
                } else {
                    Debug.LogError("Trying to get available states no profile data is loaded.");
                    return Enumerable.Empty<string>();
                }
            }

            return _dataHandler.GetAvailableStates(profile);
        }

        public StateSaveData GetStateData(string stateName) {
            if (LoadedProfileData == null) {
                Debug.LogError("Trying to load state data when no profile data is loaded.");
                return null;
            }

            return _dataHandler.GetStateSaveData(_serializer, LoadedProfileData.ProfileName, stateName);
        }

        public void SetStateData(string stateName, StateSaveData state) {
            if (LoadedProfileData == null) {
                Debug.LogError("Trying to save state data when no profile data is loaded.");
                return;
            }

            state.SaveTime = DateTime.Now;
            _dataHandler.SetStateSaveData(_serializer, LoadedProfileData.ProfileName, stateName, state);
        }
        
        public void LoadStateData(string stateName) {
            StateSaveData data = GetStateData(stateName);
            if (data == null) return;

            LoadedStateData = data;
            LoadedStateData.Initialized = true;
        }

        public void LoadEmptyStateData() {
            StateSaveData data = new StateSaveData();

            LoadedStateData = data;
            data.Initialized = true;
        }

        public void UnloadStateData() {
            LoadedStateData = null;
            _loadedStateDataDirty = false;
        }

        public void SaveStateData() {
            if (!_loadedStateDataDirty || string.IsNullOrEmpty(LoadedStateData.StateName)) return;
            SaveStateDataAs(LoadedStateData.StateName);
        }

        public void SaveStateDataAs(string newStateName) {
            if (string.IsNullOrEmpty(newStateName)) {
                Debug.LogError("A state cannot be saved without a name.");
                return;
            }
            
            if (newStateName != LoadedStateData.StateName && !string.IsNullOrEmpty(LoadedStateData.StateName)) {
                _dataHandler.CopyStateSaveData(LoadedProfileData.ProfileName, LoadedStateData.StateName, newStateName);
            }
            
            LoadedStateData.StateName = newStateName;
            LoadedProfileData.MostRecentState = LoadedStateData.StateName;
            LoadedStateData.SaveTime = DateTime.Now;
            
            if (!_loadedStateDataDirty) return;
            _dataHandler.SetStateSaveData(_serializer, LoadedProfileData.ProfileName, newStateName,
                                          LoadedStateData);
            _loadedStateDataDirty = false;
        }

        public void DeleteStateData(string profile = null, string state = null) {
            if (string.IsNullOrEmpty(profile)) {
                if (string.IsNullOrEmpty(state)) {
                    Debug.LogError("Trying to delete current state on a specified profile. This is not allowed.");
                    return;
                } else if (LoadedProfileData == null) {
                    Debug.LogError("Trying to delete current profile state data when no profile data is loaded.");
                    return;
                } else {
                    profile = LoadedProfileData.ProfileName;
                } 
            }

            if (string.IsNullOrEmpty(state)) {
                if (LoadedStateData == null) {
                    Debug.LogError("Trying to delete current profile state data when no state data is loaded.");
                } else {
                    state = LoadedStateData.StateName;
                }
            }
            
            bool deletingCurrentState = LoadedProfileData != null && LoadedProfileData.ProfileName == profile &&
                                        LoadedStateData != null && LoadedStateData.StateName == state;
            if (deletingCurrentState) {
                UnloadStateData();
            }
        }

        public LevelSaveData GetLevelData(int levelIndex) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to load level data when no state data is loaded.");
                return null;
            }

            return _dataHandler.GetLevelSaveData(_serializer, LoadedProfileData.ProfileName, LoadedStateData.StateName,
                                                 levelIndex);
        }

        public void SetLevelData(int levelIndex, LevelSaveData level) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to save level data when no state data is loaded.");
                return;
            }

            _dataHandler.SetLevelSaveData(_serializer, LoadedProfileData.ProfileName, LoadedStateData.StateName,
                                          levelIndex, level);
        }

        public RegionSaveData GetRegionData(int levelIndex, int regionIndex) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to load region data when no state data is loaded.");
                return null;
            }

            return _dataHandler.GetRegionSaveData(_serializer, LoadedProfileData.ProfileName,
                                                  LoadedStateData.StateName,
                                                  levelIndex, regionIndex);
        }

        public void SetRegionData(int levelIndex, int regionIndex, RegionSaveData regionData) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to save region data when no state data is loaded.");
                return;
            }

            _dataHandler.SetRegionSaveData(_serializer, LoadedProfileData.ProfileName,
                                           LoadedStateData.StateName,
                                           levelIndex, regionIndex, regionData);
        }

        public bool GetCustomData<T>(PersistedData parent, string key, out T result) where T : new() {
            object current = parent.GetCustomData(key);
            if (current == null) {
                result = new T();
                parent.SetCustomData(key, result);
                return true;
            } else if (current is T t) {
                result = t;
                return true;
            } else if (_serializer.IntermediateToObject(current, out result)) {
                parent.SetCustomData(key, result);
                return true;
            }

            result = default;
            return false;
        }
    }
}
