using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class UnloadScenesStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneGroup _groupToUnload;
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            SceneLoadingManager.Instance.UnloadScenes(_groupToUnload);
        }
    }
}