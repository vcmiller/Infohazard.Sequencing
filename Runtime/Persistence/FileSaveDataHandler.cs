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
using System.IO;
using System.Linq;

using UnityEngine;

namespace Infohazard.Sequencing {
    public class FileSaveDataHandler : SaveDataHandler {
        [SerializeField] private string _globalDataFolder = "SaveData/";
        [SerializeField] private string _globalDataFile = "Global.json";
        [SerializeField] private string _profileDataFile = "Profile.json";
        [SerializeField] private string _stateDataFile = "State.json";
        [SerializeField] private string _levelDataFile = "Level.json";

        public string GlobalDataFolder => Path.Combine(Application.persistentDataPath, _globalDataFolder);
        public string GlobalDataFile => Path.Combine(GlobalDataFolder, _globalDataFile);

        public string GetProfileDataFolder(string profile) =>
            Path.Combine(GlobalDataFolder, $"Profile_{profile}");

        public string GetProfileDataFile(string profile) =>
            Path.Combine(GetProfileDataFolder(profile), _profileDataFile);

        public string GetStateDataFolder(string profile, string state) =>
            Path.Combine(GetProfileDataFolder(profile), $"State_{state}");

        public string GetStateDataFile(string profile, string state) =>
            Path.Combine(GetStateDataFolder(profile, state), _stateDataFile);

        public string GetLevelDataFolder(string profile, string state, int level) =>
            Path.Combine(GetStateDataFolder(profile, state), $"Level_{level}");

        public string GetLevelDataFile(string profile, string state, int level) =>
            Path.Combine(GetLevelDataFolder(profile, state, level), _levelDataFile);

        public string GetRegionDataFile(string profile, string state, int level, int region) =>
            Path.Combine(GetLevelDataFolder(profile, state, level), $"Region_{region}.json");

        public override GlobalSaveData GetGlobalSaveData(Serializer serializer) {
            string path = GlobalDataFile;
            return LoadDataFromFile<GlobalSaveData>(serializer, path) ?? new GlobalSaveData();
        }

        public override void SetGlobalSaveData(Serializer serializer, GlobalSaveData globalData) {
            string path = GlobalDataFile;
            SaveDataToFile(serializer, path, globalData);
        }

        public override void ClearGlobalSaveData() {
            DeleteFileOrFolder(GlobalDataFolder);
        }

        public override IEnumerable<string> GetAvailableProfiles() {
            return GetFoldersInDirectoryContainingFile(GlobalDataFolder, _profileDataFile);
        }

        public override ProfileSaveData GetProfileSaveData(Serializer serializer, string profile) {
            string path = GetProfileDataFile(profile);
            return LoadDataFromFile<ProfileSaveData>(serializer, path) ?? new ProfileSaveData {
                ProfileName = profile,
            };
        }

        public override void SetProfileSaveData(Serializer serializer, string profile, ProfileSaveData profileData) {
            string path = GetProfileDataFile(profile);
            SaveDataToFile(serializer, path, profileData);
        }

        public override void ClearProfileSaveData(string profile) {
            DeleteFileOrFolder(GetProfileDataFolder(profile));
        }

        public override void CopyProfileSaveData(string sourceProfile, string destProfile) {
            ClearProfileSaveData(destProfile);
            CopyDirectory(GetProfileDataFolder(sourceProfile), GetProfileDataFolder(destProfile));
        }

        public override IEnumerable<string> GetAvailableStates(string profile) {
            return GetFoldersInDirectoryContainingFile(GetProfileDataFolder(profile), _stateDataFile);
        }

        public override StateSaveData GetStateSaveData(Serializer serializer, string profile, string state) {
            string path = GetStateDataFile(profile, state);
            return LoadDataFromFile<StateSaveData>(serializer, path) ?? new StateSaveData {
                StateName = state,
            };
        }

        public override void SetStateSaveData(Serializer serializer, string profile, string state, StateSaveData stateData) {
            string path = GetStateDataFile(profile, state);
            SaveDataToFile(serializer, path, stateData);
        }

        public override void ClearStateSaveData(string profile, string state) {
            DeleteFileOrFolder(GetStateDataFolder(profile, state));
        }

        public override void CopyStateSaveData(string profile, string sourceState, string destState) {
            ClearStateSaveData(profile, destState);
            CopyDirectory(GetStateDataFolder(profile, sourceState), GetStateDataFolder(profile, destState));
        }

        public override IEnumerable<int> GetAvailableLevels(string profile, string state) {
            return ElementsAsInts(
                GetFoldersInDirectoryContainingFile(GetStateDataFolder(profile, state), _levelDataFile));
        }

        public override LevelSaveData GetLevelSaveData(Serializer serializer, string profile, string state, int level) {
            string path = GetLevelDataFile(profile, state, level);
            return LoadDataFromFile<LevelSaveData>(serializer, path) ?? new LevelSaveData {
                LevelIndex = level,
            };
        }

        public override void SetLevelSaveData(Serializer serializer, string profile, string state, int level, LevelSaveData levelData) {
            string path = GetLevelDataFile(profile, state, level);
            SaveDataToFile(serializer, path, levelData);
        }

        public override void ClearLevelSaveData(string profile, string state, int level) {
            DeleteFileOrFolder(GetLevelDataFolder(profile, state, level));
        }

        public override IEnumerable<int> GetAvailableRegions(string profile, string state, int level) {
            return ElementsAsInts(GetFilesInDirectory(GetLevelDataFolder(profile, state, level))
                                      .Select(Path.GetFileNameWithoutExtension));
        }

        public override RegionSaveData GetRegionSaveData(Serializer serializer, string profile, string state, int level, int region) {
            string path = GetRegionDataFile(profile, state, level, region);
            return LoadDataFromFile<RegionSaveData>(serializer, path) ?? new RegionSaveData {
                RegionIndex = region,
            };
        }

        public override void SetRegionSaveData(Serializer serializer, string profile, string state, int level, int region,
                                               RegionSaveData regionData) {
            string path = GetRegionDataFile(profile, state, level, region);
            SaveDataToFile(serializer, path, regionData);
        }

        public override void ClearRegionSaveData(string profile, string state, int level, int region) {
            DeleteFileOrFolder(GetRegionDataFile(profile, state, level, region));
        }

#region Private Methods
        
        private static IEnumerable<string> GetFoldersInDirectoryContainingFile(string path, string file) {
            if (!Directory.Exists(path)) yield break;

            foreach (DirectoryInfo directoryInfo in new DirectoryInfo(path).EnumerateDirectories()) {
                foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles()) {
                    if (fileInfo.Name != file) continue;
                    
                    string name = directoryInfo.Name;
                    int underscore = name.IndexOf('_');
                    if (underscore >= 0) {
                        name = name.Substring(underscore + 1);
                    }

                    yield return name;
                    break;
                }
            }
        }

        private static IEnumerable<string> GetFilesInDirectory(string path) {
            if (!Directory.Exists(path)) return Enumerable.Empty<string>();
            return new DirectoryInfo(path).EnumerateFiles().Select(t => t.Name);
        }

        private static IEnumerable<int> ElementsAsInts(IEnumerable<string> input) {
            return input.Select(n => {
                int lastUnderscore = n.LastIndexOf('_');
                if (lastUnderscore >= 0 && lastUnderscore < n.Length - 1) {
                    n = n.Substring(lastUnderscore + 1);
                }
                if (int.TryParse(n, out int result)) return (int?) result;
                return null;
            }).Where(t => t.HasValue).Select(t => t.Value);
        }

        private static T LoadDataFromFile<T>(Serializer serializer, string path) {
            try {
                using Stream stream = new FileStream(path, FileMode.Open);
                if (serializer.Read(stream, out T data)) {
                    return data;
                }
            } catch (FileNotFoundException) {
                // Ignore, we just return null
            } catch (DirectoryNotFoundException) {
                // Ignore, we just return null
            } catch (IOException ex) {
                Debug.LogException(ex);
            }

            return default;
        }

        private static void SaveDataToFile(Serializer serializer, string path, object data) {
            try {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                
                using Stream stream = new FileStream(path, FileMode.Create);
                serializer.Write(stream, data);
            } catch (IOException ex) {
                Debug.LogException(ex);
            }
        }

        private static void DeleteFileOrFolder(string path) {
            if (File.Exists(path)) {
                File.Delete(path);
            }

            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
        }
        
        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                  + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
        
            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);        

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, tempPath);
            }
        }
#endregion
    }
}