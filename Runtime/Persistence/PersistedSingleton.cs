using System;
using Infohazard.Core.Runtime;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
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