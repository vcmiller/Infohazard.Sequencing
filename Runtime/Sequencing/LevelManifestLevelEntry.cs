using System.Collections.Generic;
using System.Linq;
using Infohazard.Core.Runtime;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class LevelManifestLevelEntry : ScriptableObject {
        [SerializeField] private string _displayName;
        [SerializeField] private int _levelID;
        [SerializeField] private SceneRef _scene;

        [SerializeField, Expandable]
        private LevelManifestRegionEntry[] _regions;

        public string DisplayName => _displayName;
        public int LevelID => _levelID;
        public SceneRef Scene => _scene;
        public IReadOnlyList<LevelManifestRegionEntry> Regions => _regions;

        public LevelManifestRegionEntry GetRegionWithID(int id) {
            return _regions.FirstOrDefault(r => r.RegionID == id);
        }

        public LevelManifestRegionEntry GetRegionWithSceneName(string sceneName) {
            return _regions.FirstOrDefault(r => r.Scene.Name == sceneName);
        }
    }
}