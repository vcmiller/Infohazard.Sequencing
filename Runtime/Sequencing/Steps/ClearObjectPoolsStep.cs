using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class ClearObjectPoolsStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            PoolManager.Instance.ClearInactiveObjects();
        }
    }

}