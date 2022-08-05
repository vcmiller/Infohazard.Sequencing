using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace Infohazard.Sequencing {
    public abstract class Serializer : MonoBehaviour {
        public abstract void Write(Stream stream, object data);
        public abstract bool Read<T>(Stream stream, out T data);
        public abstract object ObjectToIntermediate(object data);
        public abstract bool IntermediateToObject<T>(object intermediate, out T data);
    }

    public abstract class SaveDataHandler : MonoBehaviour {
        public abstract GlobalSaveData GetGlobalSaveData(Serializer serializer);
        public abstract void SetGlobalSaveData(Serializer serializer, GlobalSaveData globalData);
        public abstract void ClearGlobalSaveData();
        
        public abstract IEnumerable<string> GetAvailableProfiles();
        public abstract ProfileSaveData GetProfileSaveData(Serializer serializer, string profile);
        public abstract void SetProfileSaveData(Serializer serializer, string profile, ProfileSaveData profileData);
        public abstract void ClearProfileSaveData(string profile);
        public abstract void CopyProfileSaveData(string sourceProfile, string destProfile);
        
        public abstract IEnumerable<string> GetAvailableStates(string profile);
        public abstract StateSaveData GetStateSaveData(Serializer serializer, string profile, string state);
        public abstract void SetStateSaveData(Serializer serializer, string profile, string state, StateSaveData stateData);
        public abstract void ClearStateSaveData(string profile, string state);
        public abstract void CopyStateSaveData(string profile, string sourceState, string destState);
        
        public abstract IEnumerable<int> GetAvailableLevels(string profile, string state);
        public abstract LevelSaveData GetLevelSaveData(Serializer serializer, string profile, string state, int level);
        public abstract void SetLevelSaveData(Serializer serializer, string profile, string state, int level, LevelSaveData levelData);
        public abstract void ClearLevelSaveData(string profile, string state, int level);
        
        public abstract IEnumerable<int> GetAvailableRegions(string profile, string state, int level);
        public abstract RegionSaveData GetRegionSaveData(Serializer serializer, string profile, string state, int level, int region);
        public abstract void SetRegionSaveData(Serializer serializer, string profile, string state, int level, int region, RegionSaveData regionData);
        public abstract void ClearRegionSaveData(string profile, string state, int level, int region);
    }
}

