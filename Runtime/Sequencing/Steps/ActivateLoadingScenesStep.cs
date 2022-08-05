using UnityEngine;

namespace Infohazard.Sequencing {
    public class ActivateLoadingScenesStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneGroup _groupToActivate;
        [SerializeField] private SceneLoadingType _typesToActivate = SceneLoadingType.All;
        [SerializeField] private bool _waitToFinish = false;
        
        public bool IsFinished => !_waitToFinish || !SceneLoadingManager.Instance.IsLoadingAnyScenes(_typesToActivate);
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            SceneLoadingManager.Instance.ActivateLoadingScenes(_groupToActivate, _typesToActivate);
        }
    }
}