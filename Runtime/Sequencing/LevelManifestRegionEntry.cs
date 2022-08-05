using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class 
        LevelManifestRegionEntry : ScriptableObject {
        [SerializeField] private string _displayName;
        [SerializeField] private int _regionID;
        [SerializeField] private int _levelID;
        [SerializeField] private SceneRef _scene;
        [SerializeField] private bool _loadedByDefault;
        [SerializeField] private bool _alwaysLoaded;
        
        public string DisplayName => _displayName;
        public int LevelID => _levelID;
        public int RegionID => _regionID;
        public SceneRef Scene => _scene;
        public bool LoadedByDefault => _loadedByDefault;
        public bool AlwaysLoaded => _alwaysLoaded;
    }
}