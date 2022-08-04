using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing.Runtime {
    [RequireComponent(typeof(PersistedGameObject))]
    public class CanMoveBetweenRegions : MonoBehaviour, IRegionAwareObject {
        private PersistedGameObject _pgo;

        [SerializeField] private bool _enableRegionTransition = true;

        private void Awake() {
            _pgo = GetComponent<PersistedGameObject>();
        }

        public RegionRoot CurrentRegion {
            get => _pgo.Region;
            set {
                if (value == _pgo.Region) return;
                if (_pgo.Initialized) {
                    _pgo.TransitionToRegion((PersistedRegionRoot)value);
                }

                Transform t = _pgo.transform;
                
                t.parent = null;
                SceneManager.MoveGameObjectToScene(_pgo.gameObject, value.gameObject.scene);
                t.parent = value.DynamicObjectRoot;
            }
        }
        
        public bool CanTransitionTo(RegionRoot region) {
            return region is PersistedRegionRoot persisted && _enableRegionTransition &&
                   _pgo.Region != null && _pgo.Region != region && _pgo.DynamicPrefabID != 0;
        }
    }
}
