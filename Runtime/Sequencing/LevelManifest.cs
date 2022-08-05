using System.Collections.Generic;
using System.Linq;
using Infohazard.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infohazard.Sequencing {
    public class LevelManifest : SingletonAsset<LevelManifest> {
        [SerializeField] [Expandable(false)]
        private LevelManifestLevelEntry[] _levels;

        public override string ResourceFolderPath => "Infohazard.Core.Data/Resources";
        public override string ResourcePath => "LevelManifest.asset";
        
        public IReadOnlyList<LevelManifestLevelEntry> Levels => _levels;

        public LevelManifestLevelEntry GetLevelWithID(int id) {
            return _levels.FirstOrDefault(l => l.LevelID == id);
        }

        public LevelManifestLevelEntry GetLevelWithSceneName(string sceneName) {
            return _levels.FirstOrDefault(l => l.Scene.Name == sceneName);
        }

        public LevelManifestLevelEntry GetLevelWithName(string levelName) {
            return _levels.FirstOrDefault(l => l.name == levelName);
        }
        
#if UNITY_EDITOR
        [MenuItem("Assets/Level Manifest")]
        public static void SelectLevelManifest() {
            Selection.activeObject = Instance;
        }
#endif
    }
}