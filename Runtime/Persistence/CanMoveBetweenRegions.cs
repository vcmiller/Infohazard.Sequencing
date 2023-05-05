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

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing {
    [RequireComponent(typeof(PersistedGameObject))]
    public class CanMoveBetweenRegions : MonoBehaviour, IRegionAwareObject {
        private PersistedGameObject _pgo;

        [SerializeField] private bool _enableRegionTransition = true;

        private void Awake() {
            _pgo = GetComponent<PersistedGameObject>();
        }

        public event ChangeRegionDelegate CurrentRegionChanged;

        public RegionRoot CurrentRegion {
            get => _pgo.Region;
            set {
                if (value == _pgo.Region) return;

                RegionRoot oldRegion = _pgo.Region;
                if (_pgo.Initialized) {
                    _pgo.TransitionToRegion((PersistedRegionRoot)value);
                }

                Transform t = _pgo.transform;
                
                t.parent = null;
                SceneManager.MoveGameObjectToScene(_pgo.gameObject, value.gameObject.scene);
                t.parent = value.DynamicObjectRoot;
                
                CurrentRegionChanged?.Invoke(oldRegion, value);
            }
        }
        
        public bool CanTransitionTo(RegionRoot region) {
            return region is PersistedRegionRoot persisted && _enableRegionTransition &&
                   _pgo.Region != null && _pgo.Region != region && _pgo.PrefabReference?.RuntimeKeyIsValid() == true;
        }
    }
}
