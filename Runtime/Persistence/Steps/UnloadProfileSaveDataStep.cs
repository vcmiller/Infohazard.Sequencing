using UnityEngine;

namespace Infohazard.Sequencing {
    public class UnloadProfileSaveDataStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            PersistenceManager.Instance.UnloadProfileData();
        }
    }
}