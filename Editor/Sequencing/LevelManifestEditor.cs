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
using Infohazard.Core.Editor;
using Infohazard.Sequencing;
using UnityEditor;

using UnityEngine;

namespace Infohazard.Sequencing.Editor {
    [CustomEditor(typeof(LevelManifest))]
    public class LevelManifestEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var action = ExpandableAttributeDrawer.SaveAction;
            ExpandableAttributeDrawer.SaveAction = SaveAction;

            DrawDefaultInspector();

            ExpandableAttributeDrawer.SaveAction = action;
            
            SetRegionLevelIDs();
            ClearDuplicateEntries();

            if (GUILayout.Button("Cleanup Unused Objects")) {
                CleanupUnusedObjects();
            }
        }

        private void SetRegionLevelIDs() {
            var manifest = (LevelManifest) target;
            if (manifest.Levels == null) return;

            foreach (LevelManifestLevelEntry level in manifest.Levels) {
                if (!level || level.Regions == null) continue;
                foreach (LevelManifestRegionEntry region in level.Regions) {
                    if (!region || region.LevelID == level.LevelID) continue;

                    SerializedObject so = new SerializedObject(region);
                    SerializedProperty prop = so.FindProperty("_levelID");
                    prop.intValue = level.LevelID;
                    so.ApplyModifiedProperties();
                }
            }
        }

        private void ClearDuplicateEntries() {
            var manifest = (LevelManifest) target;
            if (manifest.Levels == null) return;

            HashSet<LevelManifestLevelEntry> seenLevels = new HashSet<LevelManifestLevelEntry>();
            HashSet<LevelManifestRegionEntry> seenRegions = new HashSet<LevelManifestRegionEntry>();

            serializedObject.Update();
            SerializedProperty levelsProp = serializedObject.FindProperty("_levels");

            for (int levelIndex = 0; levelIndex < manifest.Levels.Count; levelIndex++) {
                LevelManifestLevelEntry level = manifest.Levels[levelIndex];
                if (!level) continue;
                if (!seenLevels.Add(level)) {
                    levelsProp.GetArrayElementAtIndex(levelIndex).objectReferenceValue = null;
                    continue;
                }
                
                if (level.Regions == null) continue;
                SerializedObject so = new SerializedObject(level);
                SerializedProperty regionsProperty = so.FindProperty("_regions");
                for (int regionIndex = 0; regionIndex < level.Regions.Count; regionIndex++) {
                    LevelManifestRegionEntry region = level.Regions[regionIndex];
                    if (!region || seenRegions.Add(region)) continue;

                    regionsProperty.GetArrayElementAtIndex(regionIndex).objectReferenceValue = null;
                }

                so.ApplyModifiedProperties();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static HashSet<Object> _validChildObjects = new HashSet<Object>();
        private void CleanupUnusedObjects() {
            _validChildObjects.Clear();
            var manifest = (LevelManifest) target;
            if (manifest.Levels == null) return;
            
            foreach (LevelManifestLevelEntry level in manifest.Levels) {
                if (!level) continue;
                _validChildObjects.Add(level);
                if (level.Regions == null) continue;
                foreach (LevelManifestRegionEntry region in level.Regions) {
                    if (!region) continue;
                    _validChildObjects.Add(region);
                }
            }

            bool modified = false;
            string path = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(path)) {
                foreach (Object childAsset in AssetDatabase.LoadAllAssetsAtPath(path)) {
                    if (childAsset == target || _validChildObjects.Contains(childAsset)) continue;
                    Undo.DestroyObjectImmediate(childAsset);
                    modified = true;
                }

                if (modified) {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                }
            }
        }

        private void SaveAction(ScriptableObject asset, string path) {
            AssetDatabase.AddObjectToAsset(asset, target);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
        }
    }
}