using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class PlayerPrefsSaveDataHandler : SaveDataHandler {
        [SerializeField]
        private string _dataPrefName = "SaveData";

        private GlobalSaveDataPair _globalData;

        public override GlobalSaveData GetGlobalSaveData(Serializer serializer) {
            _globalData ??= GetGlobalDataPair();
            return _globalData.Data;
        }

        public override void SetGlobalSaveData(Serializer serializer, GlobalSaveData globalData) {
            _globalData ??= GetGlobalDataPair();
            _globalData.Data = globalData;
            Save();
        }

        public override void ClearGlobalSaveData() {
            _globalData = null;
            PlayerPrefs.DeleteKey(_dataPrefName);
        }

        public override IEnumerable<string> GetAvailableProfiles() {
            _globalData ??= GetGlobalDataPair();
            return _globalData.Profiles.Select(p => p.Data.ProfileName);
        }

        public override ProfileSaveData GetProfileSaveData(Serializer serializer, string profile) {
            return GetProfileSaveDataPair(profile).Data;
        }

        public override void SetProfileSaveData(Serializer serializer, string profile, ProfileSaveData profileData) {
            ProfileSaveDataPair pair = GetProfileSaveDataPair(profile);
            pair.Data = profileData;
            Save();
        }

        public override void ClearProfileSaveData(string profile) {
            _globalData ??= GetGlobalDataPair();
            int index = _globalData.Profiles.FindIndex(p => p.Data.ProfileName == profile);
            if (index < 0) return;

            _globalData.Profiles.RemoveAt(index);
            Save();
        }

        public override void CopyProfileSaveData(string sourceProfile, string destProfile) {
            ProfileSaveDataPair sourcePair = GetProfileSaveDataPair(sourceProfile);
            ProfileSaveDataPair copy = CopyData(sourcePair);
            copy.Data.ProfileName = destProfile;

            int index = _globalData.Profiles.FindIndex(p => p.Data.ProfileName == destProfile);
            if (index < 0) {
                _globalData.Profiles.Add(copy);
            } else {
                _globalData.Profiles[index] = copy;
            }

            Save();
        }

        public override IEnumerable<string> GetAvailableStates(string profile) {
            ProfileSaveDataPair pair = GetProfileSaveDataPair(profile);
            return pair.States.Select(s => s.Data.StateName);
        }

        public override StateSaveData GetStateSaveData(Serializer serializer, string profile, string state) {
            return GetStateSaveDataPair(profile, state).Data;
        }

        public override void SetStateSaveData(Serializer serializer, string profile, string state,
                                              StateSaveData stateData) {
            StateSaveDataPair pair = GetStateSaveDataPair(profile, state);
            pair.Data = stateData;
            Save();
        }

        public override void ClearStateSaveData(string profile, string state) {
            ProfileSaveDataPair profilePair = GetProfileSaveDataPair(profile);
            int index = profilePair.States.FindIndex(s => s.Data.StateName == state);
            if (index < 0) return;

            profilePair.States.RemoveAt(index);
            Save();
        }

        public override void CopyStateSaveData(string profile, string sourceState, string destState) {
            StateSaveDataPair sourcePair = GetStateSaveDataPair(profile, sourceState);
            StateSaveDataPair copy = CopyData(sourcePair);
            copy.Data.StateName = destState;

            ProfileSaveDataPair profilePair = GetProfileSaveDataPair(profile);
            int index = profilePair.States.FindIndex(s => s.Data.StateName == destState);
            if (index < 0) {
                profilePair.States.Add(copy);
            } else {
                profilePair.States[index] = copy;
            }

            Save();
        }

        public override IEnumerable<int> GetAvailableLevels(string profile, string state) {
            StateSaveDataPair pair = GetStateSaveDataPair(profile, state);
            return pair.Levels.Select(l => l.Data.LevelIndex);
        }

        public override LevelSaveData GetLevelSaveData(Serializer serializer, string profile, string state, int level) {
            return GetLevelSaveDataPair(profile, state, level).Data;
        }

        public override void SetLevelSaveData(Serializer serializer, string profile, string state, int level,
                                              LevelSaveData levelData) {
            LevelSaveDataPair pair = GetLevelSaveDataPair(profile, state, level);
            pair.Data = levelData;
            Save();
        }

        public override void ClearLevelSaveData(string profile, string state, int level) {
            StateSaveDataPair statePair = GetStateSaveDataPair(profile, state);
            int index = statePair.Levels.FindIndex(l => l.Data.LevelIndex == level);
            if (index < 0) return;

            statePair.Levels.RemoveAt(index);
            Save();
        }

        public override IEnumerable<int> GetAvailableRegions(string profile, string state, int level) {
            LevelSaveDataPair pair = GetLevelSaveDataPair(profile, state, level);
            return pair.Regions.Select(r => r.RegionIndex);
        }

        public override RegionSaveData GetRegionSaveData(Serializer serializer, string profile, string state, int level,
                                                         int region) {
            LevelSaveDataPair pair = GetLevelSaveDataPair(profile, state, level);
            return pair.Regions.FirstOrDefault(r => r.RegionIndex == region);
        }

        public override void SetRegionSaveData(Serializer serializer, string profile, string state, int level,
                                               int region,
                                               RegionSaveData regionData) {
            LevelSaveDataPair levelPair = GetLevelSaveDataPair(profile, state, level);
            int index = levelPair.Regions.FindIndex(r => r.RegionIndex == region);
            if (index < 0) {
                levelPair.Regions.Add(regionData);
            } else {
                levelPair.Regions[index] = regionData;
            }
            Save();
        }

        public override void ClearRegionSaveData(string profile, string state, int level, int region) {
            LevelSaveDataPair levelPair = GetLevelSaveDataPair(profile, state, level);
            int index = levelPair.Regions.FindIndex(r => r.RegionIndex == region);
            if (index < 0) return;

            levelPair.Regions.RemoveAt(index);
            Save();
        }

        #region Private Methods

        private T CopyData<T>(T data) {
            string json = JsonConvert.SerializeObject(data);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private void Save() {
            string json = JsonConvert.SerializeObject(_globalData);
            PlayerPrefs.SetString(_dataPrefName, json);
        }

        private GlobalSaveDataPair GetGlobalDataPair() {
            string json = PlayerPrefs.GetString(_dataPrefName, null);
            if (string.IsNullOrEmpty(json)) {
                return new GlobalSaveDataPair {
                    Data = new GlobalSaveData(),
                    Profiles = new List<ProfileSaveDataPair>(),
                };
            }

            return JsonConvert.DeserializeObject<GlobalSaveDataPair>(json);
        }

        private ProfileSaveDataPair GetProfileSaveDataPair(string profile) {
            _globalData ??= GetGlobalDataPair();

            ProfileSaveDataPair profileData = _globalData.Profiles.FirstOrDefault(p => p.Data.ProfileName == profile);
            if (profileData != null) {
                return profileData;
            }

            profileData = new ProfileSaveDataPair {
                Data = new ProfileSaveData {
                    ProfileName = profile,
                },
                States = new List<StateSaveDataPair>(),
            };
            _globalData.Profiles.Add(profileData);

            return profileData;
        }

        private StateSaveDataPair GetStateSaveDataPair(string profile, string state) {
            ProfileSaveDataPair profileData = GetProfileSaveDataPair(profile);

            StateSaveDataPair stateData = profileData.States.FirstOrDefault(s => s.Data.StateName == state);
            if (stateData != null) {
                return stateData;
            }

            stateData = new StateSaveDataPair {
                Data = new StateSaveData {
                    StateName = state,
                },
                Levels = new List<LevelSaveDataPair>(),
            };
            profileData.States.Add(stateData);

            return stateData;
        }

        private LevelSaveDataPair GetLevelSaveDataPair(string profile, string state, int level) {
            StateSaveDataPair stateData = GetStateSaveDataPair(profile, state);

            LevelSaveDataPair levelData = stateData.Levels.FirstOrDefault(l => l.Data.LevelIndex == level);
            if (levelData != null) {
                return levelData;
            }

            levelData = new LevelSaveDataPair {
                Data = new LevelSaveData {
                    LevelIndex = level,
                },
                Regions = new List<RegionSaveData>(),
            };
            stateData.Levels.Add(levelData);

            return levelData;
        }

        private RegionSaveData GetRegionSaveDataPair(string profile, string state, int level, int region) {
            LevelSaveDataPair levelData = GetLevelSaveDataPair(profile, state, level);

            RegionSaveData regionData = levelData.Regions.FirstOrDefault(r => r.RegionIndex == region);
            if (regionData != null) {
                return regionData;
            }

            regionData = new RegionSaveData {
                RegionIndex = region,
            };
            levelData.Regions.Add(regionData);
            return regionData;
        }

        private class GlobalSaveDataPair {
            public GlobalSaveData Data { get; set; }
            public List<ProfileSaveDataPair> Profiles { get; set; }
        }

        private class ProfileSaveDataPair {
            public ProfileSaveData Data { get; set; }
            public List<StateSaveDataPair> States { get; set; }
        }

        private class StateSaveDataPair {
            public StateSaveData Data { get; set; }
            public List<LevelSaveDataPair> Levels { get; set; }
        }

        private class LevelSaveDataPair {
            public LevelSaveData Data { get; set; }
            public List<RegionSaveData> Regions { get; set; }
        }

        #endregion
    }
}
