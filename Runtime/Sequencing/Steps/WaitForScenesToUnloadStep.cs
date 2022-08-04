using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class WaitForScenesToUnloadStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => !SceneLoadingManager.Instance.IsUnloadingAnyScenes;
        public void ExecuteForward(ExecutionStepArguments arguments) { }
    }
}