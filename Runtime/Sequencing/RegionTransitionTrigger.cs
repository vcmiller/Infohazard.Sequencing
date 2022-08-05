using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    [RequireComponent(typeof(TriggerVolume))]
    public class RegionTransitionTrigger : MonoBehaviour {
        [SerializeField] private RegionRoot _transitionTo;
        
        private TriggerVolume _volume;
        
        private void Awake() {
            _volume = GetComponent<TriggerVolume>();
            _volume.TriggerEntered += Volume_TriggerEntered;
        }

        private void Volume_TriggerEntered(GameObject obj) {
            if (!_transitionTo || !_transitionTo.Initialized) return;
            if (obj.TryGetComponentInParent(out IRegionAwareObject rao) && rao.CurrentRegion != _transitionTo && rao.CanTransitionTo(_transitionTo)) {
                rao.CurrentRegion = _transitionTo;
            }
        }
    }
}