using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class ClearUnusedResourcesStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            Resources.UnloadUnusedAssets();
        }
    }

}