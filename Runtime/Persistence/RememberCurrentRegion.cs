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
    public class RememberCurrentRegion : PersistedComponent<RememberCurrentRegion.StateInfo>, IRegionAwareObject {
        [SerializeField] private bool _onlyPersistedRegions = false;
        [SerializeField] private LevelManifestRegionEntry _initialRegion;

        private RegionRoot _region;
        public event ChangeRegionDelegate CurrentRegionChanged;

        public RegionRoot CurrentRegion {
            get => _region;
            
            set {
                if (_region == value) return;
                RegionRoot oldRegion = _region;
                _region = value;
                CurrentRegionChanged?.Invoke(oldRegion, value);
                State.RegionID = _region.RegionIndex;
            }
        }

        public override void LoadDefaultState() {
            if (_initialRegion != null &&
                LevelRoot.Current.LoadedRegions.TryGetValue(_initialRegion.RegionID, out RegionRoot region) &&
                CanTransitionTo(region)) {
                CurrentRegion = region;
            } else {
                SceneLoadingManager.Instance.GetSceneLoadedState(gameObject.scene.name, out _, out region);
                CurrentRegion = CanTransitionTo(region) ? region : null;
            }
        }

        public override void LoadState() {
            if (State.RegionID >= 0 &&
                LevelRoot.Current.LoadedRegions.TryGetValue(State.RegionID, out RegionRoot region) &&
                CanTransitionTo(region)) {
                CurrentRegion = region;
            }
        }

        public bool CanTransitionTo(RegionRoot region) {
            return !_onlyPersistedRegions || region is PersistedRegionRoot;
        }
        
        [Serializable]
        public class StateInfo : PersistedData {
            [SerializeField]
            private int _regionID = -1;

            public int RegionID {
                get => _regionID;
                set {
                    if (_regionID == value) return;
                    _regionID = value;
                    NotifyStateChanged();
                }
            }
        }
    }
}