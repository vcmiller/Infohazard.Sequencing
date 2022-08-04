using Infohazard.Core.Runtime;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class ClearObjectPoolsStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            PoolManager.Instance.ClearInactiveObjects();
        }
    }

}