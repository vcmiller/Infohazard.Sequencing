using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class UnloadStateSaveDataStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            PersistenceManager.Instance.UnloadStateData();
        }
    }
}