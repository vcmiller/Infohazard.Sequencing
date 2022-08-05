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
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class PersistedSingleton<T, TS> : Singleton<T> where T : SingletonBase where TS : PersistedData, new() {
        [SerializeField] private PersistedSingletonScope _scope = PersistedSingletonScope.Profile;

        public TS State { get; private set; }

        protected virtual void OnEnable() {
            if (_scope == PersistedSingletonScope.Global) PersistenceManager.Instance.GlobalDataLoaded += UpdateState;
            if (_scope == PersistedSingletonScope.Profile) PersistenceManager.Instance.ProfileDataLoaded += UpdateState;
            if (_scope == PersistedSingletonScope.State) PersistenceManager.Instance.StateDataLoaded += UpdateState;
        }

        protected virtual void OnDisable() {
            PersistenceManager.Instance.GlobalDataLoaded -= UpdateState;
            PersistenceManager.Instance.ProfileDataLoaded -= UpdateState;
            PersistenceManager.Instance.StateDataLoaded -= UpdateState;
        }

        public override void Initialize() {
            base.Initialize();
            if (State == null) UpdateState();
        }

        protected virtual void UpdateState() {
            PersistedData data = _scope switch {
                PersistedSingletonScope.Global => PersistenceManager.Instance.LoadedGlobalData,
                PersistedSingletonScope.Profile => PersistenceManager.Instance.LoadedProfileData,
                PersistedSingletonScope.State => PersistenceManager.Instance.LoadedStateData,
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (data == null) {
                State = null;
                return;
            }

            PersistenceManager.Instance.GetCustomData(data, GetType().Name, out TS value);
            State = value;
            if (State == null) {
                Debug.LogError($"State could not be loaded for type {GetType().Name}.");
            } else if (State.Initialized) {
                LoadState();
            } else {
                LoadDefaultState();
            }
        }
        
        protected virtual void LoadState() { }
        protected virtual void LoadDefaultState() { }
        
        private enum PersistedSingletonScope {
            Global, Profile, State,
        }
    }
}