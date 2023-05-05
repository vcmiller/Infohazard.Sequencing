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
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing {
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
        public event ChangeRegionDelegate CurrentRegionChanged;

        public RegionRoot CurrentRegion {
            get {
                if (_needsToInit) Initialize();
                return _region;
            }
            set {
                if (_region == value) return;
                RegionRoot oldRegion = _region;
                _region = value;
                CurrentRegionChanged?.Invoke(oldRegion, value);
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