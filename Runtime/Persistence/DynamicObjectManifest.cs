using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.Core.Runtime;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infohazard.Sequencing.Runtime {
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