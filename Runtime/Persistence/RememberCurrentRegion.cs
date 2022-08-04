using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing.Runtime {
    public class RememberCurrentRegion : MonoBehaviour, IRegionAwareObject {
        [SerializeField] private bool _onlyPersistedRegions = false;

        private bool _needsToInit = false;
        
        private LevelRoot _level;
        public LevelRoot Level {
            get {
                if (_needsToInit) Initialize();
                return _level;
            }
            private set => _level = value;
        }
        
        private RegionRoot _region;
        public event Action RegionChanged;
        public RegionRoot CurrentRegion {
            get {
                if (_needsToInit) Initialize();
                return _region;
            }
            set {
                if (_region == value) return;
                _region = value;
                RegionChanged?.Invoke();
            }
        }

        public PersistedObjectCollection Container =>
            CurrentRegion is PersistedRegionRoot pRegion
                ? pRegion.SaveData.Objects
                : Level is PersistedLevelRoot pLevel
                    ? pLevel.SaveData.Objects
                    : null;
        
        public Scene RegionScene =>
            CurrentRegion is PersistedRegionRoot
                ? CurrentRegion.gameObject.scene
                : Level is PersistedLevelRoot
                    ? Level.gameObject.scene
                    : default;

        private void OnEnable() {
            _needsToInit = true;
        }

        private void OnDisable() {
            _needsToInit = false;
            CurrentRegion = null;
            Level = null;
        }

        private void Update() {
            if (_needsToInit) Initialize();
        }

        private void Initialize() {
            _needsToInit = false;
            SceneLoadingManager.Instance.GetSceneLoadedState(gameObject.scene.name, out _, out RegionRoot region);
            CurrentRegion = (!_onlyPersistedRegions || region is PersistedRegionRoot) ? region : null;
            Level = LevelRoot.Current;
        }

        public bool CanTransitionTo(RegionRoot region) {
            return !_onlyPersistedRegions || region is PersistedRegionRoot;
        }
    }

}