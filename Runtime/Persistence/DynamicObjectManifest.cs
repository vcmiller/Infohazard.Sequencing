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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infohazard.Sequencing {
    public class DynamicObjectManifest : SingletonAsset<DynamicObjectManifest> {
        public override string ResourceFolderPath => "Infohazard.Core.Data/Resources";
        public override string ResourcePath => "DynamicObjectManifest.asset";

        [SerializeField] private List<DynamicPrefabEntry> _prefabs;

        private Dictionary<int, DynamicPrefabEntry> _entries;

        public DynamicPrefabEntry GetEntry(int prefabID) {
            if (Application.isPlaying) {
                if (_entries == null) _entries = new Dictionary<int, DynamicPrefabEntry>();
                if (_entries.TryGetValue(prefabID, out DynamicPrefabEntry entry)) return entry;
            } else {
                _entries = null;
            }

            if (_prefabs == null) return null;
            foreach (var prefab in _prefabs) {
                if (prefab.PrefabID != prefabID) continue;
                
                if (_entries != null) _entries[prefabID] = prefab;
                return prefab;
            }

            return null;
        }
        
#if UNITY_EDITOR
        public void AddObject(int id, string path) {
            Undo.RecordObject(this, "Add Object to Dynamic Manifest");
            _prefabs.Add(new DynamicPrefabEntry(path, id));
            _prefabs = _prefabs.Where(p => !string.IsNullOrEmpty(p.ResourcePath))
                               .OrderBy(p => p.ResourcePath).ToList();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        public void RemoveObject(int id) {
            Undo.RecordObject(this, "Remove Object from Dynamic Manifest");
            _prefabs.RemoveAll(p => p.PrefabID == id);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        [MenuItem("Tools/Infohazard/Select/Dynamic Object Manifest")]
        [MenuItem("Assets/Dynamic Object Manifest")]
        public static void SelectLevelManifest() {
            Selection.activeObject = Instance;
        }
#endif
        
        [Serializable]
        public class DynamicPrefabEntry {
            [SerializeField] private string _resourcePath;
            [SerializeField] private int _prefabID;
            public string ResourcePath => _resourcePath;
            public int PrefabID => _prefabID;

            public DynamicPrefabEntry() { }
            public DynamicPrefabEntry(string resourcePath, int prefabID) {
                _resourcePath = resourcePath;
                _prefabID = prefabID;
            }
        }
    }
}